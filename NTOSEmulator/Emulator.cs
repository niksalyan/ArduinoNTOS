using NTOSCompiler;
using NTOSCompiler.Compiler;
using NTOSEmulator.Libs;
using NTOSEmulator.Models;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

namespace NTOSEmulator
{
    public static class Emulator
    {
        // Central static emulator state and helpers. No direct UI access here; views should bind to the BindingLists
        // or subscribe to events below. Initialize() should be called from UI thread before usage so
        // SynchronizationContext is captured for marshaling if available.

        public static VMFunctions VMFunctions { get; private set; } = new VMFunctions();
        public static VirtualMachine VM { get; private set; }
        public static Compiler Compiler { get; private set; }
        internal static ScreenBuffer Screen { get; private set; }

        // Public state
        public static string AppPath { get; private set; } = string.Empty;
        public static string CurrentFile = string.Empty;



        public static Dictionary<int, object> EEPROM { get; } = new Dictionary<int, object>();

        // Input state
        public static byte CurrentKey { get; set; } = 0;
        public static int CurrentNumber { get; set; } = -1;
        public static Dictionary<byte, bool> KeyStates { get; } = new Dictionary<byte, bool>();

        // Bindable collections for views
        public static BindingList<DebugLine> DebugLines { get; } = new BindingList<DebugLine>();
        public static BindingList<Instruction> BytecodeList { get; } = new BindingList<Instruction>();
        public static BindingList<Variable> VariablesList { get; } = new BindingList<Variable>();

        // Events
        public static event Action<string> DebugOutputAdded;
        public static event Action<string> DebugErrorAdded;
        public static event Action ScreenInvalidated;

        // internal
        private static SynchronizationContext uiContext;
    

        static Emulator()
        {
            uiContext = SynchronizationContext.Current;

            // Setup VM functions (mirror EmulatorControl behavior)
            RegisterFunctions();

            Screen = new ScreenBuffer(VMFunctions);

            var constants = Screen.GetConstants();
            constants["NONE"] = (byte)0;
            constants["EMPTY"] = (byte)0;
            constants["LOW"] = (byte)0;
            constants["HIGH"] = (byte)0x1;
            constants["PI"] = (float)Math.PI;

            for(int r = 0; r < 8; r++)
            {
                for (int g = 0; g < 8; g++)
                {
                    for (int b = 0; b < 8; b++)
                    {
                        Color c = Color.FromArgb(255, r * 36, g * 36, b * 36);
                        constants["C" + r + g + b] = ScreenBuffer.GetColor332(c);
                    }
                }
            }

            


            Compiler = new Compiler(VMFunctions, constants);
            VM = new VirtualMachine(3072, VMFunctions);

            Screen.OnInvalidate += () => ScreenInvalidated?.Invoke();
        }

        private static void RegisterFunctions()
        {
            // Clear any previous functions by replacing the object if needed
            VMFunctions = VMFunctions ?? new VMFunctions();

            VMFunctions.AddFunction(0, "debug", 1, VariableType.None, async args =>
            {
                string FormatDebugValue(object? value)
                {
                    if (value == null)
                        return "null";

                    return value switch
                    {
                        string s => $"\"{s}\"",

                        float f => f.ToString("0.0###############", CultureInfo.InvariantCulture),
                        double d => d.ToString("0.0###############", CultureInfo.InvariantCulture),
                        decimal m => m.ToString("0.0###############", CultureInfo.InvariantCulture),

                        int i => i.ToString(CultureInfo.InvariantCulture),
                        long l => l.ToString(CultureInfo.InvariantCulture),
                        short s => s.ToString(CultureInfo.InvariantCulture),

                        bool b => b ? "true" : "false",

                        byte b => $"byte({b})",
                        sbyte b => $"sbyte({b})",
                        ushort s => $"ushort({s})",
                        uint i => $"uint({i})",
                        ulong l => $"ulong({l})",

                        char c => $"char('{c}')",

                        _ => $"{value.GetType().Name}({value})"
                    };
                }

                AppendDebug(FormatDebugValue(args[0]));
                return null;
            });

            VMFunctions.AddFunction(1, "load", 1, VariableType.None, async (args) =>
            {
                if (args.Length > 0)
                {
                    string file = AppPath + args[0]?.ToString() + ".js";
                    // execute but do not touch UI here
                    _ = ExecuteFile(file, true);
                }
                return null;
            });

            VMFunctions.AddFunction(2, "exit", 0, VariableType.None, async (args) =>
            {
                VM?.Stop();
                AppendDebug("Execution finished.");
                return null;
            });

            VMFunctions.AddFunction(3, "delay", 1, VariableType.None, async (args) =>
            {
                int d = (int)args[0];
                await Task.Delay(d);
                VM?.ResetStopwatch();
                return null;
            });

            VMFunctions.AddFunction(4, "getKey", 0, VariableType.Byte, async (args) =>
            {
                byte key = CurrentKey;
                CurrentKey = 0;
                return key;
            });

            VMFunctions.AddFunction(5, "getNumericKey", 0, VariableType.Int, async (args) =>
            {
                int number = CurrentNumber;
                CurrentNumber = -1;
                return number >= 0 && number <= 9 ? number : -1;
            });

            VMFunctions.AddFunction(6, "getKeyPressed", 1, VariableType.Bool, async (args) =>
            {
                byte key = (byte)args[0];
                return KeyStates.ContainsKey(key) ? KeyStates[key] : false;
            });

            VMFunctions.AddFunction(7, "sync", 1, VariableType.None, async (args) =>
            {
                int d = (int)args[0];
                await Task.Delay(d);
                VM?.ResetStopwatch();
                return null;
            });

            VMFunctions.AddFunction(31, "alert", 2, VariableType.Bool, async args =>
            {
                // Dialogs are UI; keep calling existing helper so host can show UI
                NTOSEmulator.Libs.Dialogs.Alert(args[0].ToString() ?? "", args[1].ToString() ?? "");
                VM?.ResetStopwatch();
                return null;
            });

            VMFunctions.AddFunction(32, "confirm", 1, VariableType.Bool, async args =>
            {
                var r = NTOSEmulator.Libs.Dialogs.Confirm(args[0].ToString() ?? "");
                VM?.ResetStopwatch();
                return r;
            });

            VMFunctions.AddFunction(33, "confirmNumber", 4, VariableType.Int, async args =>
            {
                var r = NTOSEmulator.Libs.Dialogs.ConfirmNumber(args[0].ToString() ?? "", args[1].ToString() ?? "", (int)args[2], (int)args[3]);
                VM?.ResetStopwatch();
                return r;
            });

            VMFunctions.AddFunction(34, "editText", 3, VariableType.Bool, async args =>
            {
                int memAddr = (int)args[1];
                int maxStringSize = (int)args[2];
                var text = VM?.GetMemoryString((ushort)memAddr);
                var r = NTOSEmulator.Libs.Dialogs.EditText(args[0].ToString() ?? "", text, maxStringSize);
                if (r != null)
                {
                    VM?.SetMemoryString((ushort)memAddr, r);
                }
                VM?.ResetStopwatch();
                return r != null;
            });

            VMFunctions.AddFunction(39, "addr", 3, VariableType.Int, async args =>
            {
                return (int)args[0] + (int)args[1] * (int)args[2];
            });

            VMFunctions.AddFunction(40, "loadInt", 2, VariableType.Int, async args =>
            {
                int addr = (int)args[0];
                return EEPROM.ContainsKey(addr) ? EEPROM[addr] : (int)args[1];
            });

            VMFunctions.AddFunction(41, "saveInt", 2, VariableType.None, async args =>
            {
                EEPROM[(int)args[0]] = (int)args[1];
                return null;
            });

            VMFunctions.AddFunction(42, "loadFloat", 2, VariableType.Float, async args =>
            {
                int addr = (int)args[0];
                return EEPROM.ContainsKey(addr) ? EEPROM[addr] : (int)args[1];
            });

            VMFunctions.AddFunction(43, "saveFloat", 2, VariableType.None, async args =>
            {
                EEPROM[(int)args[0]] = (int)args[1];
                return null;
            });

            VMFunctions.AddFunction(44, "loadStr", 4, VariableType.None, async args =>
            {
                int eepromAddr = (int)args[0];
                int memAddr = (int)args[1];
                string defaultValue = (string)args[2];
                int maxStringSize = (int)args[3];

                string value = EEPROM.ContainsKey(eepromAddr)
                    ? EEPROM[eepromAddr]?.ToString() ?? defaultValue
                    : defaultValue;

                if (value.Length > maxStringSize)
                    value = value[..maxStringSize];

                VM?.SetMemoryString((ushort)memAddr, value);

                return null;
            });

            VMFunctions.AddFunction(45, "saveStr", 3, VariableType.None, async args =>
            {
                EEPROM[(int)args[0]] = (string)args[1];
                return null;
            });

            VMFunctions.AddFunction(129, "abs", 1, VariableType.Float, async args =>
            {
                return Math.Abs(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(130, "min", 2, VariableType.Float, async args =>
            {
                return Math.Min(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]));
            });

            VMFunctions.AddFunction(131, "max", 2, VariableType.Float, async args =>
            {
                return Math.Max(Convert.ToDouble(args[0]), Convert.ToDouble(args[1]));
            });

            VMFunctions.AddFunction(132, "clamp", 3, VariableType.Float, async args =>
            {
                double value = Convert.ToDouble(args[0]);
                double min = Convert.ToDouble(args[1]);
                double max = Convert.ToDouble(args[2]);

                return Math.Clamp(value, min, max);
            });

            VMFunctions.AddFunction(133, "sign", 1, VariableType.Int, async args =>
            {
                double value = Convert.ToDouble(args[0]);
                return Math.Sign(value);
            });

            VMFunctions.AddFunction(134, "sqrt", 1, VariableType.Float, async args =>
            {
                return Math.Sqrt(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(135, "pow", 2, VariableType.Float, async args =>
            {
                return Math.Pow(
                    Convert.ToDouble(args[0]),
                    Convert.ToDouble(args[1]));
            });

            VMFunctions.AddFunction(136, "hypot", 2, VariableType.Float, async args =>
            {
                return Math.Sqrt(
                    Math.Pow(Convert.ToDouble(args[0]), 2) +
                    Math.Pow(Convert.ToDouble(args[1]), 2));
            });

            VMFunctions.AddFunction(137, "sin", 1, VariableType.Float, async args =>
            {
                return Math.Sin(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(138, "cos", 1, VariableType.Float, async args =>
            {
                return Math.Cos(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(139, "tan", 1, VariableType.Float, async args =>
            {
                return Math.Tan(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(140, "asin", 1, VariableType.Float, async args =>
            {
                return Math.Asin(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(141, "acos", 1, VariableType.Float, async args =>
            {
                return Math.Acos(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(142, "atan", 1, VariableType.Float, async args =>
            {
                return Math.Atan(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(143, "atan2", 2, VariableType.Float, async args =>
            {
                return Math.Atan2(
                    Convert.ToDouble(args[0]),
                    Convert.ToDouble(args[1]));
            });

            VMFunctions.AddFunction(144, "floor", 1, VariableType.Float, async args =>
            {
                return Math.Floor(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(145, "ceil", 1, VariableType.Float, async args =>
            {
                return Math.Ceiling(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(146, "round", 1, VariableType.Float, async args =>
            {
                return Math.Round(Convert.ToDouble(args[0]));
            });

            VMFunctions.AddFunction(147, "fmod", 2, VariableType.Float, async args =>
            {
                return Convert.ToDouble(args[0]) %
                       Convert.ToDouble(args[1]);
            });

            VMFunctions.AddFunction(148, "lerp", 3, VariableType.Float, async args =>
            {
                double a = Convert.ToDouble(args[0]);
                double b = Convert.ToDouble(args[1]);
                double t = Convert.ToDouble(args[2]);

                return a + (b - a) * t;
            });

            VMFunctions.AddFunction(149, "map", 5, VariableType.Float, async args =>
            {
                double value = Convert.ToDouble(args[0]);
                double inMin = Convert.ToDouble(args[1]);
                double inMax = Convert.ToDouble(args[2]);
                double outMin = Convert.ToDouble(args[3]);
                double outMax = Convert.ToDouble(args[4]);

                if (inMax == inMin)
                    return outMin;

                return outMin +
                    (value - inMin) *
                    (outMax - outMin) /
                    (inMax - inMin);
            });

            VMFunctions.AddFunction(150, "rnd", 0, VariableType.Float, async args =>
            {
                return (float)Random.Shared.Next(10000000) / 10000000.0f;
            });
            VMFunctions.AddFunction(151, "scroll", 1, VariableType.None, async args =>
            {
                Screen.tftScroll = Convert.ToInt32(args[0]);
                ScreenInvalidated?.Invoke();
                return null;
            });
        }

        public static void AppendDebug(string line, bool error= false)
        {
            if (uiContext != null)
            {
                uiContext.Post(_ =>
                {
                    DebugLines.Add(new DebugLine(line, error));
                    if (error)
                    {
                        DebugErrorAdded?.Invoke(line);
                    } else
                    {
                        DebugOutputAdded?.Invoke(line);
                    }
                }, null);
            }
            else
            {
                DebugLines.Add(new DebugLine(line, error));
                if (error)
                {
                    DebugErrorAdded?.Invoke(line);
                }
                else
                {
                    DebugOutputAdded?.Invoke(line);
                }
            }
        }

        public static void ClearDebug()
        {
            if (uiContext != null)
            {
                uiContext.Post(_ => DebugLines.Clear(), null);
            }
            else
            {
                DebugLines.Clear();
            }
        }

        public static async Task ExecuteFile(string file, bool initialized = false)
        {
            if (!File.Exists(file))
            {
                AppendDebug("File not found: " + file);
                return;
            }

            AppPath = Path.GetDirectoryName(file) + "\\";
            CurrentFile = file;
            string name = Path.GetFileNameWithoutExtension(file);
            string source = File.ReadAllText(file);



            ClearDebug();
            BytecodeList.Clear();
            VariablesList.Clear();

            byte[] bytecode = null;
            try
            {
                bytecode = Compiler.CompileSingle(source);
                if (bytecode.Length > 3072)
                {
                    AppendDebug($"Compiled {bytecode.Length}/3072b - too large", true);
                }
                else
                {
                    AppendDebug($"Compiled {bytecode.Length}/3072b");
                }
            }
            catch (Exception ex)
            {
                AppendDebug(ex.Message, true);
                return;
            }

            // populate bindable lists
            foreach (var instr in Compiler.Instructions)
                BytecodeList.Add(instr);
            foreach (var v in Compiler.Variables)
                VariablesList.Add(v);

            if (bytecode != null)
            {
                try
                {
                    CurrentKey = 0;
                    CurrentNumber = -1;
                    VM.Initialized = initialized;
                    if (name == "main" && !initialized)
                    {
                        VM.ClearMemory();
                    }
                    await VM.Execute(bytecode);
                }
                catch (Exception ex)
                {
                    AppendDebug(ex.Message, true);
                }
            }
        }

        public static void BuildAll(bool silent = false)
        {
            if (!silent) {
                AppendDebug("STARTS BUILDING");
            }
            
            Compiler.ClearVariables();
            try
            {
                if (string.IsNullOrEmpty(AppPath))
                {
                    AppendDebug("AppPath not set", true);
                    return;
                }

                Directory.CreateDirectory(Path.Combine(AppPath, "build"));

                string[] files = Directory.GetFiles(AppPath, "*.js");

                var fileSources = new Dictionary<string, string>();
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        string source = File.ReadAllText(file);
                        fileSources[name] = source;
                    }
                }

                var compiled = Compiler.CompileMultiple(fileSources);

                foreach (var c in compiled)
                {
                    var bytecode = c.PrepareBytecode();
                    File.WriteAllBytes(Path.Combine(AppPath, "build", c.Name + ".ntx"), bytecode);
                    if (!silent)
                    {
                        AppendDebug($"Build: {c.Name} OK ({bytecode.Length}b) !");
                    }
                }

            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    AppendDebug(ex.Message, true);
                }
            }
        }

        public static void CopyNtiFiles(bool silent = false)
        {
            if (!silent)
            {
                AppendDebug("COPYING IMAGES");
            }

            try
            {
                string buildPath = Path.Combine(AppPath, "build");

                Directory.CreateDirectory(buildPath);

                string[] files = Directory.GetFiles(AppPath, "*.nti");

                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        string name = Path.GetFileName(file);
                        string destination = Path.Combine(buildPath, name);

                        File.Copy(file, destination, true);
                        if (!silent)
                        {
                            AppendDebug($"Copy: {name} OK !");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    AppendDebug("Copy NTI: " + ex.Message, true);
                }
            }
        }
    }
}
