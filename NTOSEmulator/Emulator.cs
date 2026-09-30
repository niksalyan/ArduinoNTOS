using NTOSCompiler;
using NTOSCompiler.Compiler;
using NTOSEmulator.Libs;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace NTOSEmulator
{
    public partial class Emulator : UserControl
    {
        public VMFunctions vmFunctions { get; private set; } = new VMFunctions();
        private VirtualMachine vm;
        private Compiler compiler;

        private ScreenBuffer screen;


        private string appPath;
        private string lastBytecode;

        private SerialPort serial = new SerialPort();
        private readonly StringBuilder serialReceiveBuffer = new StringBuilder();

        private bool isUploading = false;
        private byte currentKey = 0;
        private int currentNumber = -1;

        private Dictionary<int, object> EEPROM = new Dictionary<int, object>();

        public Emulator()
        {
            InitializeComponent();

            vmFunctions.AddFunction(0, "debug", 1, VariableType.None, async (args) =>
            {
                if (args.Length > 0)
                {
                    DebugOutput(args[0]?.ToString() ?? "");
                }
                return null;
            });

            vmFunctions.AddFunction(1, "load", 1, VariableType.None, async (args) =>
            {
                if (args.Length > 0)
                {
                    string file = appPath + args[0]?.ToString() + ".js";
                    Debug.WriteLine(file);
                    Execute(file, true);
                }
                return null;
            });

            

            vmFunctions.AddFunction(2, "exit", 0, VariableType.None, async (args) =>
            {
                vm?.Stop();
                DebugOutput("Execution finished.");               
                return null;
            });

            vmFunctions.AddFunction(3, "delay", 1, VariableType.None, async (args) =>
            {
                int d = (int)args[0];
                await Task.Delay((int)d);
                vm?.ResetStopwatch();
                return null;
            });

            vmFunctions.AddFunction(4, "getKey", 0, VariableType.Byte, async (args) =>
            {
                byte key = currentKey;
                currentKey = 0;
                return key;
            });

            vmFunctions.AddFunction(5, "getNumericKey", 0, VariableType.Int, async (args) =>
            {
                int number = currentNumber;
                currentNumber = -1;
                return number >= 0 && number <= 9 ? number : -1;
            });

            vmFunctions.AddFunction(31, "alert", 2, VariableType.Bool, async args =>
            {
                Dialogs.Alert(args[0].ToString() ?? "", args[1].ToString() ?? "");
                vm?.ResetStopwatch();
                return null;
            });

            vmFunctions.AddFunction(32, "confirm", 1, VariableType.Bool, async args =>
            {
                var r =  Dialogs.Confirm(args[0].ToString() ?? "");
                vm?.ResetStopwatch();
                return r;
            });

            vmFunctions.AddFunction(33, "confirmNumber", 4, VariableType.Int, async args =>
            {
                var r = Dialogs.ConfirmNumber(args[0].ToString() ?? "", args[1].ToString() ?? "", (int)args[2], (int)args[3]);
                vm?.ResetStopwatch();
                return r;
            });

            vmFunctions.AddFunction(34, "editText", 3, VariableType.Bool, async args =>
            {
                int memAddr = (int)args[1];
                int maxStringSize = (int)args[2];
                var text = vm.GetMemoryString((ushort)memAddr);
                var r = Dialogs.EditText(args[0].ToString() ?? "", text, maxStringSize);
                if (r != null) {
                    vm.SetMemoryString((ushort)memAddr, r);
                }
                vm?.ResetStopwatch();
                return r != null;
            });

            vmFunctions.AddFunction(39, "addr", 3, VariableType.Int, async args =>
            {
                return (int)args[0] + (int)args[1] * (int)args[2];
            });

            vmFunctions.AddFunction(40, "loadInt", 2, VariableType.Int, async args =>
            {
                int addr = (int)args[0];
                return EEPROM.ContainsKey(addr) ? EEPROM[addr] : (int)args[1];
            });

            vmFunctions.AddFunction(41, "saveInt", 2, VariableType.None, async args =>
            {
                EEPROM[(int)args[0]] = (int)args[1];
                return null;
            });

            vmFunctions.AddFunction(42, "loadFloat", 2, VariableType.Float, async args =>
            {
                int addr = (int)args[0];
                return EEPROM.ContainsKey(addr) ? EEPROM[addr] : (int)args[1];
            });

            vmFunctions.AddFunction(43, "saveFloat", 2, VariableType.None, async args =>
            {
                EEPROM[(int)args[0]] = (int)args[1];
                return null;
            });

            vmFunctions.AddFunction(44, "loadStr", 4, VariableType.None, async args =>
            {
                Debug.WriteLine("loadStr");
                int eepromAddr = (int)args[0];
                int memAddr = (int)args[1];
                string defaultValue = (string)args[2];
                int maxStringSize = (int)args[3];

                string value = EEPROM.ContainsKey(eepromAddr)
                    ? EEPROM[eepromAddr]?.ToString() ?? defaultValue
                    : defaultValue;

                if (value.Length > maxStringSize)
                    value = value[..maxStringSize];


                Debug.WriteLine("WRITING INTO MEMORY: " + memAddr + " = " + value);
                vm?.SetMemoryString((ushort)memAddr, value);

                return null;
            });

            vmFunctions.AddFunction(45, "saveStr", 3, VariableType.None, async args =>
            {
                EEPROM[(int)args[0]] = (string)args[1];
                return null;
            });


            screen = new ScreenBuffer(vmFunctions);

            compiler = new Compiler(vmFunctions, screen.colors);
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

            compiler.ClearVariables();

            

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
            currentKey = (byte)key;
            currentNumber = currentKey - '0';
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
            compiler.ClearVariables();
            try
            {
                PrepareDirectory(appPath + "build");
                string[] files = Directory.GetFiles(appPath, "*.js")
                    .OrderBy(f => Path.GetFileName(f).Equals("main.js", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ToArray();
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {

                        string name = Path.GetFileNameWithoutExtension(file);
                        string source = File.ReadAllText(file);
                        byte[] bytecode;
                        try
                        {
                            bytecode = compiler.Compile(source);
                            File.WriteAllBytes(appPath + "build\\" + name + ".ntx", bytecode);
                            DebugOutput("Build: " + name + " OK (" + bytecode.Length + "b) !");
                        } catch (Exception ex)
                        {
                            bytecode = null;
                            DebugOutput("Build: " + name + " : " + ex.Message);
                        }
                        

                    }
                }
            }
            catch (Exception ex)
            {
                DebugError(ex.Message);
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

            
            byte[] bytecode = null;
            try
            {
                bytecode = compiler.Compile(source);
                DebugOutput("Compiled");
            } catch (Exception ex)
            {
                bytecode = null;
                DebugError(ex.Message);
            }

            BeginInvoke(() =>
            {
                bytecodeGrid.DataSource = compiler.Instructions;
                variablesGrid.DataSource = compiler.Variables;
            });

            //bytecodeOutput.Text = app.ToArduinoArray(name);
            /* // TODO
                        if (error != null)
                        {
                            DebugError(error);
                            return;
                        }
            */

            if (bytecode != null) {
                try
                {
                    currentKey = 0;
                    currentNumber = -1;
                    vm.Initialized = initialized;
                    await vm.Execute(bytecode); // TODO
                }
                catch (Exception ex)
                {
                    DebugError(ex.Message);
                }
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
            // DebugStart(app.ToArduinoArray(lastBytecode));
        }

        private async void uploadButton_Click(object sender, EventArgs e)
        {

            Task.Run(DoUpload);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ClearDebug();
        }

        static void PrepareDirectory(string path)
        {
            Directory.CreateDirectory(path);

            foreach (string file in Directory.GetFiles(path))
                File.Delete(file);

            foreach (string directory in Directory.GetDirectories(path))
                Directory.Delete(directory, recursive: true);
        }
    }
}
