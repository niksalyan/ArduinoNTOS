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
            screenContainer = new Panel();
            tabControl = new TabControl();
            tabPage2 = new TabPage();
            numpadControl1 = new NTOSEmulator.Controls.NumpadControl();
            consoleTab = new TabPage();
            debugOutput = new TextBox();
            variablesTab = new TabPage();
            variablesGrid = new DataGridView();
            tabPage1 = new TabPage();
            bytecodeGrid = new DataGridView();
            tabPage3 = new TabPage();
            bytecodeOutput = new TextBox();
            tabControl.SuspendLayout();
            tabPage2.SuspendLayout();
            consoleTab.SuspendLayout();
            variablesTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)variablesGrid).BeginInit();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)bytecodeGrid).BeginInit();
            tabPage3.SuspendLayout();
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
            tabControl.Controls.Add(tabPage3);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 241);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(800, 209);
            tabControl.TabIndex = 1;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(numpadControl1);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(792, 176);
            tabPage2.TabIndex = 3;
            tabPage2.Text = "Input";
            // 
            // numpadControl1
            // 
            numpadControl1.BackColor = Color.FromArgb(24, 27, 32);
            numpadControl1.BackgroundColor = Color.FromArgb(24, 27, 32);
            numpadControl1.Dock = DockStyle.Fill;
            numpadControl1.ForeColor = Color.FromArgb(235, 238, 242);
            numpadControl1.KeyColor = Color.FromArgb(45, 49, 58);
            numpadControl1.KeyHoverColor = Color.FromArgb(58, 64, 75);
            numpadControl1.KeyPressedColor = Color.FromArgb(70, 125, 175);
            numpadControl1.KeySpacing = 8;
            numpadControl1.KeyTextColor = Color.FromArgb(235, 238, 242);
            numpadControl1.Location = new Point(3, 3);
            numpadControl1.MinimumSize = new Size(180, 180);
            numpadControl1.Name = "numpadControl1";
            numpadControl1.SecondaryTextColor = Color.FromArgb(145, 153, 165);
            numpadControl1.Size = new Size(786, 180);
            numpadControl1.TabIndex = 0;
            // 
            // consoleTab
            // 
            consoleTab.Controls.Add(debugOutput);
            consoleTab.Location = new Point(4, 29);
            consoleTab.Name = "consoleTab";
            consoleTab.Padding = new Padding(3);
            consoleTab.Size = new Size(792, 176);
            consoleTab.TabIndex = 0;
            consoleTab.Text = "Output";
            // 
            // debugOutput
            // 
            debugOutput.BorderStyle = BorderStyle.None;
            debugOutput.Dock = DockStyle.Fill;
            debugOutput.Location = new Point(3, 3);
            debugOutput.Multiline = true;
            debugOutput.Name = "debugOutput";
            debugOutput.ReadOnly = true;
            debugOutput.ScrollBars = ScrollBars.Vertical;
            debugOutput.Size = new Size(786, 170);
            debugOutput.TabIndex = 0;
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
            bytecodeGrid.RowHeadersWidth = 51;
            bytecodeGrid.Size = new Size(786, 170);
            bytecodeGrid.TabIndex = 1;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(bytecodeOutput);
            tabPage3.Location = new Point(4, 29);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(792, 176);
            tabPage3.TabIndex = 4;
            tabPage3.Text = "Bytecode";
            // 
            // bytecodeOutput
            // 
            bytecodeOutput.BorderStyle = BorderStyle.None;
            bytecodeOutput.Dock = DockStyle.Fill;
            bytecodeOutput.Location = new Point(3, 3);
            bytecodeOutput.Multiline = true;
            bytecodeOutput.Name = "bytecodeOutput";
            bytecodeOutput.ReadOnly = true;
            bytecodeOutput.ScrollBars = ScrollBars.Vertical;
            bytecodeOutput.Size = new Size(786, 170);
            bytecodeOutput.TabIndex = 1;
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
            Resize += Emulator_Resize;
            tabControl.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            consoleTab.ResumeLayout(false);
            consoleTab.PerformLayout();
            variablesTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)variablesGrid).EndInit();
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)bytecodeGrid).EndInit();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
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
        private Controls.NumpadControl numpadControl1;
        private TabPage tabPage3;
        private TextBox bytecodeOutput;
    }
}
