using NTOSEmulator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace NTOSDev.Components
{
    public partial class Terminal : DockContent
    {
        public Terminal()
        {
            InitializeComponent();
            dataGridView1.DataSource = Emulator.DebugLines;
        }
    }
}
