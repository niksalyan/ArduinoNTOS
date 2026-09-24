using NTOSCompiler;
using NTOSCompiler.Compiler;
using System.Diagnostics;

namespace NTOSEmulator
{
    public partial class Emulator : UserControl
    {
        public VMFunctions vmFunctions { get; private set; } = new VMFunctions();
        private VirtualMachine vm;
        private BytecodeApp app;

        private Bitmap screenBuffer = new Bitmap(480, 320, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

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

            vmFunctions.AddFunction(1, "cls", (args) =>
            {
                using (var g = Graphics.FromImage(screenBuffer))
                {
                    g.Clear(Color.Black);
                }
                screenContainer.Invalidate();
                return null;
            });

            vmFunctions.AddFunction(10, "drawBox", (args) =>
            {
                using (var g = Graphics.FromImage(screenBuffer))
                {
                    g.DrawRectangle(Pens.Red, new Rectangle((int)args[0], (int)args[1], (int)args[2], (int)args[3]));
                }
                screenContainer.Invalidate();
                return null;
            });

            app = new BytecodeApp(vmFunctions);
            vm = new VirtualMachine(4096, vmFunctions);
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

            string error = app.CompileSource(name, source);

            if (error != null)
            {
                DebugError(error);
                return;
            }

            bytecodeGrid.DataSource = app.Instructions;
            variablesGrid.DataSource = app.Variables;


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
            e.Graphics.DrawImage(screenBuffer, new Rectangle(0, 0, screenContainer.Width, screenContainer.Height));
        }

    }
}
