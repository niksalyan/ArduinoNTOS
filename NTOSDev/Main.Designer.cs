namespace NTOSDev
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox1 = new TextBox();
            listBox1 = new ListBox();
            debugOutput = new TextBox();
            mainMenu = new MenuStrip();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            emulatorToolStripMenuItem = new ToolStripMenuItem();
            mainMenu.SuspendLayout();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(0, 28);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Size = new Size(447, 342);
            textBox1.TabIndex = 0;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // listBox1
            // 
            listBox1.BorderStyle = BorderStyle.None;
            listBox1.Dock = DockStyle.Right;
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(447, 28);
            listBox1.Name = "listBox1";
            listBox1.SelectionMode = SelectionMode.MultiExtended;
            listBox1.Size = new Size(353, 342);
            listBox1.TabIndex = 1;
            // 
            // debugOutput
            // 
            debugOutput.BorderStyle = BorderStyle.None;
            debugOutput.Dock = DockStyle.Bottom;
            debugOutput.Location = new Point(0, 370);
            debugOutput.Multiline = true;
            debugOutput.Name = "debugOutput";
            debugOutput.ReadOnly = true;
            debugOutput.ScrollBars = ScrollBars.Vertical;
            debugOutput.Size = new Size(800, 80);
            debugOutput.TabIndex = 2;
            // 
            // mainMenu
            // 
            mainMenu.ImageScalingSize = new Size(20, 20);
            mainMenu.Items.AddRange(new ToolStripItem[] { toolsToolStripMenuItem });
            mainMenu.Location = new Point(0, 0);
            mainMenu.Name = "mainMenu";
            mainMenu.Size = new Size(800, 28);
            mainMenu.TabIndex = 3;
            mainMenu.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { emulatorToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(58, 24);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // emulatorToolStripMenuItem
            // 
            emulatorToolStripMenuItem.Name = "emulatorToolStripMenuItem";
            emulatorToolStripMenuItem.Size = new Size(152, 26);
            emulatorToolStripMenuItem.Text = "Emulator";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox1);
            Controls.Add(listBox1);
            Controls.Add(debugOutput);
            Controls.Add(mainMenu);
            MainMenuStrip = mainMenu;
            Name = "Main";
            Text = "NTOS Dev";
            mainMenu.ResumeLayout(false);
            mainMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private ListBox listBox1;
        private TextBox debugOutput;
        private MenuStrip mainMenu;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem emulatorToolStripMenuItem;
    }
}
