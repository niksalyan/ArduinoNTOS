namespace NTOSEmulator
{
    partial class Emulator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Emulator));
            screenContainer = new Panel();
            tabControl = new TabControl();
            tabPage2 = new TabPage();
            numpadControl = new NTOSEmulator.Controls.NumpadControl();
            consoleTab = new TabPage();
            debugOutput = new TextBox();
            toolStrip1 = new ToolStrip();
            comPortsList = new ToolStripComboBox();
            connectButton = new ToolStripButton();
            showBytecode = new ToolStripButton();
            uploadButton = new ToolStripButton();
            toolStripButton1 = new ToolStripButton();
            variablesTab = new TabPage();
            variablesGrid = new DataGridView();
            tabPage1 = new TabPage();
            bytecodeGrid = new DataGridView();
            tabControl.SuspendLayout();
            tabPage2.SuspendLayout();
            consoleTab.SuspendLayout();
            toolStrip1.SuspendLayout();
            variablesTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)variablesGrid).BeginInit();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bytecodeGrid).BeginInit();
            SuspendLayout();
            // 
            // screenContainer
            // 
            screenContainer.Dock = DockStyle.Top;
            screenContainer.Location = new Point(0, 0);
            screenContainer.Name = "screenContainer";
            screenContainer.Size = new Size(800, 241);
            screenContainer.TabIndex = 0;
            screenContainer.Paint += screenContainer_Paint;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPage2);
            tabControl.Controls.Add(consoleTab);
            tabControl.Controls.Add(variablesTab);
            tabControl.Controls.Add(tabPage1);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 241);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(800, 209);
            tabControl.TabIndex = 1;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(numpadControl);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(792, 176);
            tabPage2.TabIndex = 3;
            tabPage2.Text = "Input";
            // 
            // numpadControl
            // 
            numpadControl.BackColor = Color.FromArgb(24, 27, 32);
            numpadControl.BackgroundColor = Color.FromArgb(24, 27, 32);
            numpadControl.Dock = DockStyle.Fill;
            numpadControl.ForeColor = Color.FromArgb(235, 238, 242);
            numpadControl.KeyColor = Color.FromArgb(45, 49, 58);
            numpadControl.KeyHoverColor = Color.FromArgb(58, 64, 75);
            numpadControl.KeyPressedColor = Color.FromArgb(70, 125, 175);
            numpadControl.KeySpacing = 8;
            numpadControl.KeyTextColor = Color.FromArgb(235, 238, 242);
            numpadControl.Location = new Point(3, 3);
            numpadControl.MinimumSize = new Size(180, 180);
            numpadControl.Name = "numpadControl";
            numpadControl.SecondaryTextColor = Color.FromArgb(145, 153, 165);
            numpadControl.Size = new Size(786, 180);
            numpadControl.TabIndex = 0;
            // 
            // consoleTab
            // 
            consoleTab.Controls.Add(debugOutput);
            consoleTab.Controls.Add(toolStrip1);
            consoleTab.Location = new Point(4, 29);
            consoleTab.Name = "consoleTab";
            consoleTab.Padding = new Padding(3);
            consoleTab.Size = new Size(792, 176);
            consoleTab.TabIndex = 0;
            consoleTab.Text = "Console";
            // 
            // debugOutput
            // 
            debugOutput.BorderStyle = BorderStyle.None;
            debugOutput.Dock = DockStyle.Fill;
            debugOutput.Location = new Point(3, 31);
            debugOutput.Multiline = true;
            debugOutput.Name = "debugOutput";
            debugOutput.ReadOnly = true;
            debugOutput.ScrollBars = ScrollBars.Vertical;
            debugOutput.Size = new Size(786, 142);
            debugOutput.TabIndex = 0;
            // 
            // toolStrip1
            // 
            toolStrip1.CanOverflow = false;
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { comPortsList, connectButton, showBytecode, uploadButton, toolStripButton1 });
            toolStrip1.Location = new Point(3, 3);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.ShowItemToolTips = false;
            toolStrip1.Size = new Size(786, 28);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // comPortsList
            // 
            comPortsList.DropDownStyle = ComboBoxStyle.DropDownList;
            comPortsList.Name = "comPortsList";
            comPortsList.Size = new Size(75, 28);
            // 
            // connectButton
            // 
            connectButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            connectButton.Image = (Image)resources.GetObject("connectButton.Image");
            connectButton.ImageTransparentColor = Color.Magenta;
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(67, 25);
            connectButton.Text = "Connect";
            connectButton.Click += connectButton_Click;
            // 
            // showBytecode
            // 
            showBytecode.DisplayStyle = ToolStripItemDisplayStyle.Text;
            showBytecode.Image = (Image)resources.GetObject("showBytecode.Image");
            showBytecode.ImageTransparentColor = Color.Magenta;
            showBytecode.Name = "showBytecode";
            showBytecode.Size = new Size(41, 25);
            showBytecode.Text = "HEX";
            showBytecode.Click += showBytecode_Click;
            // 
            // uploadButton
            // 
            uploadButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            uploadButton.Image = (Image)resources.GetObject("uploadButton.Image");
            uploadButton.ImageTransparentColor = Color.Magenta;
            uploadButton.Name = "uploadButton";
            uploadButton.Size = new Size(62, 25);
            uploadButton.Text = "Upload";
            uploadButton.Click += uploadButton_Click;
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(34, 25);
            toolStripButton1.Text = "❌";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // variablesTab
            // 
            variablesTab.Controls.Add(variablesGrid);
            variablesTab.Location = new Point(4, 29);
            variablesTab.Name = "variablesTab";
            variablesTab.Padding = new Padding(3);
            variablesTab.Size = new Size(792, 176);
            variablesTab.TabIndex = 1;
            variablesTab.Text = "Variables";
            // 
            // variablesGrid
            // 
            variablesGrid.AllowUserToAddRows = false;
            variablesGrid.AllowUserToDeleteRows = false;
            variablesGrid.BorderStyle = BorderStyle.None;
            variablesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            variablesGrid.Dock = DockStyle.Fill;
            variablesGrid.Location = new Point(3, 3);
            variablesGrid.Name = "variablesGrid";
            variablesGrid.ReadOnly = true;
            variablesGrid.RowHeadersVisible = false;
            variablesGrid.RowHeadersWidth = 51;
            variablesGrid.Size = new Size(786, 170);
            variablesGrid.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(bytecodeGrid);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(792, 176);
            tabPage1.TabIndex = 2;
            tabPage1.Text = "Instructions";
            // 
            // bytecodeGrid
            // 
            bytecodeGrid.AllowUserToAddRows = false;
            bytecodeGrid.AllowUserToDeleteRows = false;
            bytecodeGrid.BorderStyle = BorderStyle.None;
            bytecodeGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            bytecodeGrid.Dock = DockStyle.Fill;
            bytecodeGrid.Location = new Point(3, 3);
            bytecodeGrid.Name = "bytecodeGrid";
            bytecodeGrid.ReadOnly = true;
            bytecodeGrid.RowHeadersVisible = false;
            bytecodeGrid.RowHeadersWidth = 51;
            bytecodeGrid.Size = new Size(786, 170);
            bytecodeGrid.TabIndex = 1;
            // 
            // Emulator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabControl);
            Controls.Add(screenContainer);
            DoubleBuffered = true;
            Name = "Emulator";
            Size = new Size(800, 450);
            Load += Emulator_Load;
            Resize += Emulator_Resize;
            tabControl.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            consoleTab.ResumeLayout(false);
            consoleTab.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            variablesTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)variablesGrid).EndInit();
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bytecodeGrid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel screenContainer;
        private TabControl tabControl;
        private TabPage consoleTab;
        private TabPage variablesTab;
        private TabPage tabPage1;
        private TextBox debugOutput;
        private DataGridView variablesGrid;
        private DataGridView bytecodeGrid;
        private TabPage tabPage2;
        private Controls.NumpadControl numpadControl;
        private ToolStrip toolStrip1;
        private ToolStripComboBox comPortsList;
        private ToolStripButton connectButton;
        private ToolStripButton uploadButton;
        private ToolStripButton showBytecode;
        private ToolStripButton toolStripButton1;
    }
}
