using NTOSCompiler;

namespace NTOSEmulator
{
    public partial class Emulator : Form
    {
        private FunctionRegistry functionRegistry = new FunctionRegistry();
        private VirtualMachine vm;

        private Bitmap screenBuffer = new Bitmap(480, 320);
        public Emulator()
        {
            InitializeComponent();
            vm = new VirtualMachine(functionRegistry);
        }

        public void Execute(byte[] bytecode)
        {
            vm.Execute(bytecode);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawImage(screenBuffer, ClientRectangle);
        }
    }
}
