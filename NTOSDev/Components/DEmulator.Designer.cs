namespace NTOSDev.Components
{
    partial class DEmulator
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            emulator = new NTOSEmulator.Emulator();
            SuspendLayout();
            // 
            // emulator
            // 
            emulator.Dock = DockStyle.Fill;
            emulator.Location = new Point(0, 0);
            emulator.Name = "emulator";
            emulator.Size = new Size(675, 335);
            emulator.TabIndex = 0;
            // 
            // DEmulator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(675, 335);
            Controls.Add(emulator);
            Name = "DEmulator";
            ResumeLayout(false);
        }

        #endregion

        private NTOSEmulator.Emulator emulator;
    }
}
