namespace BMFM
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLoadROM = new System.Windows.Forms.Button();
            this.nudLevel = new System.Windows.Forms.NumericUpDown();
            this.btnLoadLevel = new System.Windows.Forms.Button();
            this.levelViewer1 = new BMFM.LevelViewer();
            this.exportImage = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // btnLoadROM
            // 
            this.btnLoadROM.Location = new System.Drawing.Point(12, 12);
            this.btnLoadROM.Name = "btnLoadROM";
            this.btnLoadROM.Size = new System.Drawing.Size(86, 23);
            this.btnLoadROM.TabIndex = 0;
            this.btnLoadROM.Text = "Load ROM";
            this.btnLoadROM.UseVisualStyleBackColor = true;
            this.btnLoadROM.Click += new System.EventHandler(this.btnLoadROM_Click);
            // 
            // nudLevel
            // 
            this.nudLevel.Location = new System.Drawing.Point(128, 15);
            this.nudLevel.Maximum = new decimal(new int[] {
            29,
            0,
            0,
            0});
            this.nudLevel.Name = "nudLevel";
            this.nudLevel.Size = new System.Drawing.Size(46, 20);
            this.nudLevel.TabIndex = 2;
            // 
            // btnLoadLevel
            // 
            this.btnLoadLevel.Location = new System.Drawing.Point(180, 12);
            this.btnLoadLevel.Name = "btnLoadLevel";
            this.btnLoadLevel.Size = new System.Drawing.Size(92, 23);
            this.btnLoadLevel.TabIndex = 3;
            this.btnLoadLevel.Text = "Load Level";
            this.btnLoadLevel.UseVisualStyleBackColor = true;
            this.btnLoadLevel.Click += new System.EventHandler(this.btnLoadLevel_Click);
            // 
            // levelViewer1
            // 
            this.levelViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.levelViewer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.levelViewer1.Location = new System.Drawing.Point(-1, 41);
            this.levelViewer1.Name = "levelViewer1";
            this.levelViewer1.Size = new System.Drawing.Size(569, 297);
            this.levelViewer1.TabIndex = 4;
            // 
            // exportImage
            // 
            this.exportImage.Location = new System.Drawing.Point(318, 12);
            this.exportImage.Name = "exportImage";
            this.exportImage.Size = new System.Drawing.Size(99, 23);
            this.exportImage.TabIndex = 5;
            this.exportImage.Text = "Export Image";
            this.exportImage.UseVisualStyleBackColor = true;
            this.exportImage.Click += new System.EventHandler(this.exportImage_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 338);
            this.Controls.Add(this.exportImage);
            this.Controls.Add(this.levelViewer1);
            this.Controls.Add(this.btnLoadLevel);
            this.Controls.Add(this.nudLevel);
            this.Controls.Add(this.btnLoadROM);
            this.Name = "Form1";
            this.Text = "BMFM Level Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.nudLevel)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLoadROM;
        private System.Windows.Forms.NumericUpDown nudLevel;
        private System.Windows.Forms.Button btnLoadLevel;
        private LevelViewer levelViewer1;
        private System.Windows.Forms.Button exportImage;
    }
}

