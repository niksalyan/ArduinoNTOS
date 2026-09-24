using NTOSCompiler;
using NTOSCompiler.Compiler;
using NTOSDev.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

        public static void RefreshEmulator ()
        {
            instance?.InitEmulator();
        }

        public void InitEmulator()
        {
            try
            {
                if (File.Exists(CodeEditor.currentFile))
                {
                    emulator.Execute(CodeEditor.currentFile);

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
    }
}
