namespace NTOSDev.Components
{
    partial class DImage
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
            imageBuilder1 = new NTOSImage.ImageBuilder();
            SuspendLayout();
            // 
            // imageBuilder1
            // 
            imageBuilder1.Dock = DockStyle.Fill;
            imageBuilder1.Location = new Point(0, 0);
            imageBuilder1.Name = "imageBuilder1";
            imageBuilder1.Size = new Size(936, 487);
            imageBuilder1.TabIndex = 0;
            // 
            // DImage
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(936, 487);
            Controls.Add(imageBuilder1);
            Name = "DImage";
            Text = "Image Converter";
            ResumeLayout(false);
        }

        #endregion

        private NTOSImage.ImageBuilder imageBuilder1;
    }
}
