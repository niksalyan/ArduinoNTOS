namespace NTOSEmulator
{
    partial class EmulatorControl
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
            numpadControl = new NTOSEmulator.Controls.NumpadControl();
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
            numpadControl.Location = new Point(0, 241);
            numpadControl.MinimumSize = new Size(180, 180);
            numpadControl.Name = "numpadControl";
            numpadControl.SecondaryTextColor = Color.FromArgb(145, 153, 165);
            numpadControl.Size = new Size(800, 209);
            numpadControl.TabIndex = 0;
            // 
            // EmulatorControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(numpadControl);
            Controls.Add(screenContainer);
            DoubleBuffered = true;
            Name = "EmulatorControl";
            Size = new Size(800, 450);
            Load += Emulator_Load;
            Resize += Emulator_Resize;
            ResumeLayout(false);
        }

        #endregion

        private Panel screenContainer;
        private Controls.NumpadControl numpadControl;
    }
}
