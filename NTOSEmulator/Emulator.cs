using NTOSCompiler;
using NTOSCompiler.Compiler;

namespace NTOSEmulator
{
    public partial class Emulator : Form
    {
        private VMFunctions vmFunctions = new VMFunctions();
        private VirtualMachine vm;

        private Bitmap screenBuffer = new Bitmap(480, 320);
        public Emulator()
        {
            InitializeComponent();
            vm = new VirtualMachine(4096, vmFunctions);
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
