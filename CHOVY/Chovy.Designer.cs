namespace CHOVY
{
    partial class CHOVY
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
            this.label1 = new System.Windows.Forms.Label();
            this.RifPath = new System.Windows.Forms.TextBox();
            this.Versionkey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.FindFromCMA = new System.Windows.Forms.Button();
            this.ISOPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.PsmChan = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.TotalProgress = new System.Windows.Forms.ProgressBar();
            this.Status = new System.Windows.Forms.Label();
            this.FREEDOM = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.CompressPBP = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PsmChan)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Chartreuse;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "RIF:";
            // 
            // RifPath
            // 
            this.RifPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.RifPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RifPath.ForeColor = System.Drawing.Color.Lime;
            this.RifPath.Location = new System.Drawing.Point(37, 16);
            this.RifPath.Name = "RifPath";
            this.RifPath.Size = new System.Drawing.Size(225, 20);
            this.RifPath.TabIndex = 1;
            // 
            // Versionkey
            // 
            this.Versionkey.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Versionkey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Versionkey.ForeColor = System.Drawing.Color.Lime;
            this.Versionkey.Location = new System.Drawing.Point(337, 16);
            this.Versionkey.MaxLength = 32;
            this.Versionkey.Name = "Versionkey";
            this.Versionkey.Size = new System.Drawing.Size(225, 20);
            this.Versionkey.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Chartreuse;
            this.label2.Location = new System.Drawing.Point(268, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "VersionKey:";
            // 
            // FindFromCMA
            // 
            this.FindFromCMA.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FindFromCMA.ForeColor = System.Drawing.Color.Lime;
            this.FindFromCMA.Location = new System.Drawing.Point(568, 16);
            this.FindFromCMA.Name = "FindFromCMA";
            this.FindFromCMA.Size = new System.Drawing.Size(109, 23);
            this.FindFromCMA.TabIndex = 4;
            this.FindFromCMA.Text = "Find from CMA";
            this.FindFromCMA.UseVisualStyleBackColor = true;
            this.FindFromCMA.Click += new System.EventHandler(this.FindFromCMA_Click);
            // 
            // ISOPath
            // 
            this.ISOPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ISOPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ISOPath.ForeColor = System.Drawing.Color.Lime;
            this.ISOPath.Location = new System.Drawing.Point(77, 54);
            this.ISOPath.Name = "ISOPath";
            this.ISOPath.Size = new System.Drawing.Size(345, 20);
            this.ISOPath.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Chartreuse;
            this.label3.Location = new System.Drawing.Point(74, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "ISO Image:";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::CHOVY.Properties.Resources.UMD;
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox2.Location = new System.Drawing.Point(15, 31);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(53, 43);
            this.pictureBox2.TabIndex = 9;
            this.pictureBox2.TabStop = false;
            // 
            // PsmChan
            // 
            this.PsmChan.BackgroundImage = global::CHOVY.Properties.Resources.idkbackground;
            this.PsmChan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.PsmChan.Location = new System.Drawing.Point(3, 3);
            this.PsmChan.Name = "PsmChan";
            this.PsmChan.Size = new System.Drawing.Size(123, 297);
            this.PsmChan.TabIndex = 5;
            this.PsmChan.TabStop = false;
            this.PsmChan.Click += new System.EventHandler(this.PsmChan_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Chartreuse;
            this.label4.Location = new System.Drawing.Point(132, 280);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(145, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Made Possible by the CBPS! ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Chartreuse;
            this.label5.Location = new System.Drawing.Point(664, 280);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(167, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "SilicaAndPina, dots_tb, Motoharu.";
            // 
            // BrowseButton
            // 
            this.BrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BrowseButton.ForeColor = System.Drawing.Color.Lime;
            this.BrowseButton.Location = new System.Drawing.Point(428, 52);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(60, 22);
            this.BrowseButton.TabIndex = 12;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // TotalProgress
            // 
            this.TotalProgress.BackColor = System.Drawing.Color.Black;
            this.TotalProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.TotalProgress.Location = new System.Drawing.Point(209, 227);
            this.TotalProgress.Name = "TotalProgress";
            this.TotalProgress.Size = new System.Drawing.Size(443, 23);
            this.TotalProgress.TabIndex = 13;
            // 
            // Status
            // 
            this.Status.AutoSize = true;
            this.Status.ForeColor = System.Drawing.Color.LawnGreen;
            this.Status.Location = new System.Drawing.Point(206, 211);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(62, 13);
            this.Status.TabIndex = 14;
            this.Status.Text = "Progress % ";
            // 
            // FREEDOM
            // 
            this.FREEDOM.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FREEDOM.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.FREEDOM.Location = new System.Drawing.Point(658, 227);
            this.FREEDOM.Name = "FREEDOM";
            this.FREEDOM.Size = new System.Drawing.Size(75, 23);
            this.FREEDOM.TabIndex = 15;
            this.FREEDOM.Text = "FREEDOM";
            this.FREEDOM.UseVisualStyleBackColor = true;
            this.FREEDOM.Click += new System.EventHandler(this.FREEDOM_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Black;
            this.groupBox1.Controls.Add(this.RifPath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Versionkey);
            this.groupBox1.Controls.Add(this.FindFromCMA);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.ForeColor = System.Drawing.Color.Lime;
            this.groupBox1.Location = new System.Drawing.Point(135, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(696, 47);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Keys To The Kingdom";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CompressPBP);
            this.groupBox2.Controls.Add(this.pictureBox2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.ISOPath);
            this.groupBox2.Controls.Add(this.BrowseButton);
            this.groupBox2.ForeColor = System.Drawing.Color.Lime;
            this.groupBox2.Location = new System.Drawing.Point(209, 74);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(524, 105);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "The PSP Game";
            // 
            // CompressPBP
            // 
            this.CompressPBP.AutoSize = true;
            this.CompressPBP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CompressPBP.Location = new System.Drawing.Point(15, 80);
            this.CompressPBP.Name = "CompressPBP";
            this.CompressPBP.Size = new System.Drawing.Size(93, 17);
            this.CompressPBP.TabIndex = 18;
            this.CompressPBP.Text = "Compress PBP";
            this.CompressPBP.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Lime;
            this.label6.Location = new System.Drawing.Point(735, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(96, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "100% percent free!\r\n";
            // 
            // CHOVY
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(843, 302);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.FREEDOM);
            this.Controls.Add(this.Status);
            this.Controls.Add(this.TotalProgress);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.PsmChan);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CHOVY";
            this.Text = "CHOVY";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CHOVY_FormClosing);
            this.Load += new System.EventHandler(this.CHOVY_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PsmChan)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox RifPath;
        private System.Windows.Forms.TextBox Versionkey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button FindFromCMA;
        private System.Windows.Forms.PictureBox PsmChan;
        private System.Windows.Forms.TextBox ISOPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.ProgressBar TotalProgress;
        private System.Windows.Forms.Label Status;
        private System.Windows.Forms.Button FREEDOM;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox CompressPBP;
        private System.Windows.Forms.Label label6;
    }
}

