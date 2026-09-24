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

        private Dictionary<string, byte> colors = new Dictionary<string, byte>
        {
            ["BLACK"] = 0x00,
            ["WHITE"] = 0xFF,

            ["RED"] = 0xE0,
            ["GREEN"] = 0x1C,
            ["BLUE"] = 0x03,

            ["YELLOW"] = 0xFC,
            ["CYAN"] = 0x1F,
            ["MAGENTA"] = 0xE3,

            ["ORANGE"] = 0xE8,
            ["PURPLE"] = 0x83,
            ["PINK"] = 0xE6,

            ["GRAY"] = 0x92,
            ["DARKGRAY"] = 0x49,
            ["LIGHTGRAY"] = 0xDB,

            ["BROWN"] = 0xA0,
            ["DARKRED"] = 0x80,
            ["DARKGREEN"] = 0x10,
            ["DARKBLUE"] = 0x02,

            ["LIME"] = 0x3C,
            ["NAVY"] = 0x02,
            ["TEAL"] = 0x12,
            ["OLIVE"] = 0xB0
        };

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
                using var g = Graphics.FromImage(screenBuffer);
                using var pen = new Pen(GetColor332((int)args[4]));
                g.DrawRectangle(pen, new Rectangle((int)args[0], (int)args[1], (int)args[2], (int)args[3]));
                screenContainer.Invalidate();
                return null;
            });

            app = new BytecodeApp(vmFunctions, colors);
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

        public static Color GetColor332(int colorInt)
        {
            byte color = (byte)(colorInt % 256);
            int r = (color >> 5) & 0b111;
            int g = (color >> 2) & 0b111;
            int b = color & 0b11;

            int red = r * 255 / 7;
            int green = g * 255 / 7;
            int blue = b * 255 / 3;

            return Color.FromArgb(red, green, blue);
        }

    }
}
