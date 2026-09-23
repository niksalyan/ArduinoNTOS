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
                debugOutput.Text += args[0] + "\n";
                // OnDebugOutput?.Invoke(args[0]);
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

        public void Execute(string name, string source)
        {

            bytecodeGrid.DataSource = null;
            variablesGrid.DataSource = null;

            app.CompileSource(name, source);

            bytecodeGrid.DataSource = app.Instructions;
            variablesGrid.DataSource = app.Variables;


            try
            {
                vm.Execute(app.GetBytecode(name));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("VM Error");
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
