using NTOSCompiler;
using NTOSCompiler.Compiler;
using NTOSEmulator.Libs;
using System.Text;

namespace NTOSEmulator
{
    public partial class Emulator : UserControl
    {
        public VMFunctions vmFunctions { get; private set; } = new VMFunctions();
        private VirtualMachine vm;
        private BytecodeApp app;

        private ScreenBuffer screen;

        

        // public Action<object> OnCompilerError;

        public Emulator()
        {
            InitializeComponent();

            vmFunctions.AddFunction(0, "debug", (args) =>
            {
                if (args.Length > 0) {
                    DebugOutput(args[0]?.ToString() ?? "");
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
        }

        public void ClearDebug()
        {
            debugOutput.ForeColor = SystemColors.WindowText;
            debugOutput.Text = "";
        }

        public void DebugOutput(string output)
        {
            debugOutput.ForeColor = SystemColors.WindowText;
            debugOutput.Text += output + Environment.NewLine;

        }
        public void DebugError(string error)
        {
            debugOutput.ForeColor = Color.Red;
            debugOutput.Text = error;
            tabControl.SelectTab(1);

        }


        public void Execute(string name, string source)
        {
            ClearDebug();
            bytecodeGrid.DataSource = null;
            variablesGrid.DataSource = null;
            bytecodeOutput.Text = "";

            string error = app.CompileSource(name, source);

            

            bytecodeGrid.DataSource = app.Instructions;
            variablesGrid.DataSource = app.Variables;
            bytecodeOutput.Text = ToArduinoArray(app.GetBytecode(name));

            if (error != null)
            {
                DebugError(error);
                return;
            }

            try
            {
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

       
        public static string ToArduinoArray(byte[] bytecode, int columns = 8)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bytecode.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                }

                sb.Append($"0x{bytecode[i]:X2}");

                if (i < bytecode.Length - 1)
                {
                    sb.Append(',');
                }

                if ((i + 1) % columns == 0)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

    }
}
