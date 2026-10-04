namespace NTOSEmulator
{
    public partial class EmulatorControl : UserControl
    {


        public EmulatorControl()
        {
            InitializeComponent();

            typeof(Panel)
            .GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(screenContainer, true);


            numpadControl.KeyPressed += NumpadControl_KeyPressed;
            numpadControl.KeyReleased += NumpadControl_KeyReleased;

            Emulator.ScreenInvalidated += () => screenContainer.Invalidate();
        }



        private void Emulator_Load(object sender, EventArgs e)
        {

        }

        private void NumpadControl_KeyPressed(object? sender, char key)
        {
            Emulator.CurrentKey = (byte)key;
            Emulator.CurrentNumber = Emulator.CurrentKey - '0';
            Emulator.KeyStates[(byte)key] = true;
        }

        private void NumpadControl_KeyReleased(object? sender, char key)
        {
            Emulator.KeyStates[(byte)key] = false;
        }

        private void Emulator_Resize(object sender, EventArgs e)
        {
            screenContainer.Height = (int)Math.Round(Width * 0.666f);
            screenContainer.Invalidate();
        }

        private void screenContainer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Emulator.Screen.GetBuffer(), new Rectangle(0, 0, screenContainer.Width, screenContainer.Height));
        }

    }
}
