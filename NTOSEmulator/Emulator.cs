using NTOSCompiler;
using NTOSCompiler.Compiler;
using NTOSEmulator.Libs;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;

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

        public Emulator()
        {
            InitializeComponent();

            vmFunctions.AddFunction(0, "debug", (args) =>
            {
                if (args.Length > 0)
                {
                    DebugOutput(args[0]?.ToString() ?? "");
                }
                return null;
            });

            vmFunctions.AddFunction(1, "load", (args) =>
            {
                if (args.Length > 0)
                {
                    string file = appPath + args[0]?.ToString() + ".ntos";
                    Debug.WriteLine(file);
                    Execute(file, true);
                }
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

            app.Variables.Add(keyboardInput);

            numpadControl.KeyPressed += NumpadControl_KeyPressed;
            serial.DataReceived += Serial_DataReceived;

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

        public void ClearDebug()
        {
            debugOutput.ForeColor = SystemColors.WindowText;
            debugOutput.Text = "";
        }

        public void DebugStart(string output)
        {
            ClearDebug();
            tabControl.SelectTab(1);
            debugOutput.Text = output + Environment.NewLine;
        }

        public void DebugOutput(string output)
        {
            debugOutput.ForeColor = SystemColors.WindowText;
            debugOutput.Text += output + Environment.NewLine;

        }
        public void DebugError(string error)
        {
            DebugStart(error);
            debugOutput.ForeColor = Color.Red;
        }

        public void BuildAll()
        {
            DebugStart("STARTS BUILDING");
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
                            DebugOutput("Build: " + name + " OK !");
                            byte[] bytecode = app.GetBytecode(name);
                            File.WriteAllBytes(appPath + "build\\" + name + ".ntx", bytecode);
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


        public void Execute(string file, bool initialized = false)
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
            uploadButton.Enabled = serial.IsOpen;
            comPortsList.Enabled = !serial.IsOpen;
            connectButton.Text = serial.IsOpen ? "Disconnect" : "Connect";
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

                        BeginInvoke(() =>
                        {
                            DebugOutput("[SERIAL] " + line);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                BeginInvoke(() =>
                {
                    DebugError("Serial receive error: " + ex.Message);
                });
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

        private void uploadButton_Click(object sender, EventArgs e)
        {
            
            try
            {
                string appName = new DirectoryInfo(appPath).Name;
                string[] files = Directory.GetFiles(appPath + "build", "*.ntx");
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

                        var sbHex = sb.ToString();

                        string cmd1 = "U " + appName + " " + name;
                        serial.WriteLine(cmd1);


                        string cmd2 = "<" + sbHex + ">";
                        serial.WriteLine(cmd2);

                        DebugOutput(cmd1);


                    }
                }


            } catch (Exception ex)
            {

            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ClearDebug();
        }
    }
}
