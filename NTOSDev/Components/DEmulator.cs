using NTOSDev.Controls;
using NTOSEmulator;
using System.Diagnostics;
using WeifenLuo.WinFormsUI.Docking;

namespace NTOSDev.Components
{
    public partial class DEmulator : DockContent
    {

        private static DEmulator instance;

        public DEmulator()
        {
            InitializeComponent();
            Text = "Emulator";
            HideOnClose = true;

            instance = this;
            InitEmulator();
        }

        public static void RefreshEmulator()
        {
            instance?.InitEmulator();
        }

        public void BuildAll()
        {
            Emulator.BuildAll();
        }

        public void InitEmulator()
        {
            try
            {
                if (File.Exists(CodeEditor.currentFile))
                {
                    Emulator.ExecuteFile(CodeEditor.currentFile);

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
    }
}
