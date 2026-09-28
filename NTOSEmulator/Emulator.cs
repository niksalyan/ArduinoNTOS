using NTOSCompiler;
using NTOSCompiler.Compiler;
using NTOSEmulator.Libs;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Xml.Linq;

namespace NTOSEmulator
{
    public partial class Emulator : UserControl
    {
        public VMFunctions vmFunctions { get; private set; } = new VMFunctions();
        private VirtualMachine vm;
        private BytecodeApp app;

        private ScreenBuffer screen;

        private Variable keyboardInput = new Variable("keyInput", VariableType.Byte);

        private string appPath;
        private string lastBytecode;

        private SerialPort serial = new SerialPort();
        private readonly StringBuilder serialReceiveBuffer = new StringBuilder();

        // public Action<object> OnCompilerError;

        private bool isUploading = false;

        public Emulator()
        {
            InitializeComponent();

            vmFunctions.AddFunction(0, "debug", VariableType.None, async (args) =>
            {
                if (args.Length > 0)
                {
                    DebugOutput(args[0]?.ToString() ?? "");
                }
                return null;
            });

            vmFunctions.AddFunction(1, "load", VariableType.None, async (args) =>
            {
                if (args.Length > 0)
                {
                    string file = appPath + args[0]?.ToString() + ".ntos";
                    Debug.WriteLine(file);
                    Execute(file, true);
                }
                return null;
            });

            

            vmFunctions.AddFunction(2, "exit", VariableType.None, async (args) =>
            {
                vm?.Stop();
                DebugOutput("Execution finished.");               
                return null;
            });

            vmFunctions.AddFunction(3, "delay", VariableType.None, async (args) =>
            {
                int d = (int)args[0];
                await Task.Delay((int)d);
                vm?.ResetStopwatch();
                return null;
            });

            screen = new ScreenBuffer(vmFunctions);

            app = new BytecodeApp(vmFunctions, screen.colors);
            vm = new VirtualMachine(4096, vmFunctions);

            screen.OnInvalidate += () =>
            {
                screenContainer.Invalidate();
            };

            typeof(Panel)
            .GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(screenContainer, true);

            

            numpadControl.KeyPressed += NumpadControl_KeyPressed;
            serial.DataReceived += Serial_DataReceived;

            ClearMemory();

        }




        private void Emulator_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comPortsList.Items.Clear();
            foreach (var port in ports)
            {
                comPortsList.Items.Add(port);
            }
            UpdateConsole();
        }

        private void NumpadControl_KeyPressed(object? sender, char key)
        {
            vm.WriteByteToMemory(keyboardInput.Address, (byte)key);
        }

        public void ClearMemory()
        {
            app.Reset();
            app.Variables.Add(keyboardInput);
        }

        public void ClearDebug()
        {
            BeginInvoke(() =>
            {
                debugOutput.ForeColor = SystemColors.WindowText;
                debugOutput.Text = "";
            });
            
        }

        public void DebugStart(string output)
        {
            ClearDebug();
            BeginInvoke(() =>
            {
                tabControl.SelectTab(1);
                debugOutput.Text = output + Environment.NewLine;
            });
            
        }

        public void DebugOutput(string output)
        {
            BeginInvoke(() => {
                debugOutput.ForeColor = SystemColors.WindowText;
                debugOutput.Text += output + Environment.NewLine;
            });
            

        }
        public void DebugError(string error)
        {
            DebugStart(error);
            BeginInvoke(() =>
            {
                debugOutput.ForeColor = Color.Red;
            });
        }

        public void BuildAll()
        {
            DebugStart("STARTS BUILDING");
            ClearMemory();
            try
            {
                Directory.CreateDirectory(appPath + "build");
                string[] files = Directory.GetFiles(appPath, "*.ntos");
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {

                        string name = Path.GetFileNameWithoutExtension(file);
                        string source = File.ReadAllText(file);
                        string error = app.CompileSource(name, source);
                        if (error == null)
                        {
                            byte[] bytecode = app.GetBytecode(name);
                            File.WriteAllBytes(appPath + "build\\" + name + ".ntx", bytecode);
                            DebugOutput("Build: " + name + " OK (" + bytecode.Length + "b) !");
                        }
                        else
                        {
                            DebugOutput("Build: " + name + " : " + error);
                        }


                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void CopyNtiFiles()
        {
            DebugStart("COPYING IMAGES");

            try
            {
                string buildPath = Path.Combine(appPath, "build");

                Directory.CreateDirectory(buildPath);

                string[] files = Directory.GetFiles(appPath, "*.nti");

                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        string name = Path.GetFileName(file);
                        string destination = Path.Combine(buildPath, name);

                        File.Copy(file, destination, true);

                        DebugOutput("Copy: " + name + " OK !");
                    }
                }
            }
            catch (Exception ex)
            {
                DebugOutput("Copy NTI: " + ex.Message);
            }
        }


        public async void Execute(string file, bool initialized = false)
        {
            appPath = Path.GetDirectoryName(file) + "\\";
            string name = Path.GetFileNameWithoutExtension(file);
            string source = File.ReadAllText(file);

            lastBytecode = name;

            ClearDebug();
            bytecodeGrid.DataSource = null;
            variablesGrid.DataSource = null;
            //bytecodeOutput.Text = "";

            string error = app.CompileSource(name, source);



            bytecodeGrid.DataSource = app.Instructions;
            variablesGrid.DataSource = app.Variables;
            //bytecodeOutput.Text = app.ToArduinoArray(name);

            if (error != null)
            {
                DebugError(error);
                return;
            }

            try
            {
                vm.Initialized = initialized;
                vm.Execute(app.GetBytecode(name));
            }
            catch (Exception ex)
            {
                DebugError(ex.Message);
            }
        }


        private void Emulator_Resize(object sender, EventArgs e)
        {
            screenContainer.Height = (int)Math.Round(Width * 0.666f);
            screenContainer.Invalidate();
        }

        private void screenContainer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(screen.GetBuffer(), new Rectangle(0, 0, screenContainer.Width, screenContainer.Height));
        }

        private void UpdateConsole()
        {
            BeginInvoke(() =>
            {
                uploadButton.Enabled = serial.IsOpen;
                comPortsList.Enabled = !serial.IsOpen;
                connectButton.Text = serial.IsOpen ? "Disconnect" : "Connect";
                uploadButton.Enabled = !isUploading;
            });
            
        }

        private async Task DoUpload()
        {
            if (isUploading) return;

            isUploading = true;
            UpdateConsole();

            try
            {
                BuildAll();
                CopyNtiFiles();
                string appName = new DirectoryInfo(appPath).Name;

                string[] files =
                    Directory.GetFiles(appPath + "build")
                        .Where(f =>
                            f.EndsWith(".ntx", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".nti", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                DebugStart("UPLOADING: " + appName);
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {

                        string name = Path.GetFileName(file);
                        byte[] bytecode = File.ReadAllBytes(file);
                        var sb = new StringBuilder();

                        for (int i = 0; i < bytecode.Length; i++)
                        {
                            sb.Append($"{bytecode[i]:X2}");
                        }

                        DebugOutput("-> " + name + " (" + bytecode.Length + "b)");

                        var sbHex = sb.ToString();

                        string cmd1 = "U " + appName + " " + name;
                        serial.WriteLine(cmd1);

                        await Task.Delay(1000);


                        string cmd2 = "<" + sbHex + ">";
                        serial.WriteLine(cmd2);

                        await Task.Delay(2000);




                    }
                }

                await Task.Delay(2000);
                DebugOutput("DONE");


            }
            catch (Exception ex)
            {
                DebugOutput("Upload Error");
            }
            finally
            {
                isUploading = false;
                UpdateConsole();
            }
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serial.ReadExisting();

                lock (serialReceiveBuffer)
                {
                    serialReceiveBuffer.Append(data);

                    while (true)
                    {
                        string buffer = serialReceiveBuffer.ToString();

                        int newlineIndex = buffer.IndexOf('\n');

                        if (newlineIndex < 0)
                            break;

                        string line = buffer[..newlineIndex];

                        // Remove the processed line including \n
                        serialReceiveBuffer.Remove(0, newlineIndex + 1);

                        // Handle possible \r\n
                        line = line.TrimEnd('\r');

                        DebugOutput("[SERIAL] " + line);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugError("Serial receive error: " + ex.Message);
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serial.IsOpen)
                {
                    serial.PortName = comPortsList.Text;
                    serial.BaudRate = 4800;
                    serial.Open();
                    DebugOutput("Connected to: " + comPortsList.Text);
                } else
                {
                    serial.Close();
                }
                
            } catch (Exception ex)
            {
                DebugError(ex.Message);
            }
            UpdateConsole();
        }

        private void showBytecode_Click(object sender, EventArgs e)
        {
            DebugStart(app.ToArduinoArray(lastBytecode));
        }

        private async void uploadButton_Click(object sender, EventArgs e)
        {

            Task.Run(DoUpload);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ClearDebug();
        }
    }
}
