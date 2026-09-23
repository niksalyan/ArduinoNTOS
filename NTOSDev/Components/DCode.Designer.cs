namespace NTOSDev.Components
{
    partial class DCode
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
            textBox1 = new TextBox();
            debugOutput = new TextBox();
            listBox1 = new ListBox();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(0, 0);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Size = new Size(293, 275);
            textBox1.TabIndex = 4;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // debugOutput
            // 
            debugOutput.BorderStyle = BorderStyle.None;
            debugOutput.Dock = DockStyle.Bottom;
            debugOutput.Location = new Point(0, 275);
            debugOutput.Multiline = true;
            debugOutput.Name = "debugOutput";
            debugOutput.ReadOnly = true;
            debugOutput.ScrollBars = ScrollBars.Vertical;
            debugOutput.Size = new Size(646, 80);
            debugOutput.TabIndex = 6;
            // 
            // listBox1
            // 
            listBox1.BorderStyle = BorderStyle.None;
            listBox1.Dock = DockStyle.Right;
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(293, 0);
            listBox1.Name = "listBox1";
            listBox1.SelectionMode = SelectionMode.MultiExtended;
            listBox1.Size = new Size(353, 275);
            listBox1.TabIndex = 5;
            // 
            // DCode
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(646, 355);
            Controls.Add(textBox1);
            Controls.Add(listBox1);
            Controls.Add(debugOutput);
            Name = "DCode";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private TextBox debugOutput;
        private ListBox listBox1;
    }
}
