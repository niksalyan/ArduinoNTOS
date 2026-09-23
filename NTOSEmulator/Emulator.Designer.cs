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
            consoleTab = new TabPage();
            debugOutput = new TextBox();
            variablesTab = new TabPage();
            variablesGrid = new DataGridView();
            tabPage1 = new TabPage();
            bytecodeGrid = new DataGridView();
            tabControl.SuspendLayout();
            consoleTab.SuspendLayout();
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
            tabPage1.Text = "Bytecode";
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
            // Emulator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabControl);
            Controls.Add(screenContainer);
            Name = "Emulator";
            Size = new Size(800, 450);
            Resize += Emulator_Resize;
            tabControl.ResumeLayout(false);
            consoleTab.ResumeLayout(false);
            consoleTab.PerformLayout();
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
    }
}
