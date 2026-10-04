namespace NTOSDev.Components
{
    partial class ComUploader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComUploader));
            toolStrip1 = new ToolStrip();
            comPortsList = new ToolStripComboBox();
            connectButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            uploadButton = new ToolStripButton();
            dataGridView1 = new DataGridView();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { comPortsList, connectButton, toolStripSeparator1, uploadButton });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.ShowItemToolTips = false;
            toolStrip1.Size = new Size(634, 28);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // comPortsList
            // 
            comPortsList.Name = "comPortsList";
            comPortsList.Size = new Size(121, 28);
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
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 28);
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
            // dataGridView1
            // 
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.Location = new Point(0, 28);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(634, 252);
            dataGridView1.TabIndex = 2;
            // 
            // ComUploader
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(634, 280);
            Controls.Add(dataGridView1);
            Controls.Add(toolStrip1);
            Name = "ComUploader";
            Text = "Uploader";
            Load += ComUploader_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripComboBox comPortsList;
        private ToolStripButton connectButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton uploadButton;
        private DataGridView dataGridView1;
    }
}
