namespace CHOVY
{
    partial class CHOVYCmaSelector
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
            this.BackupList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CMADir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AIDSelector = new System.Windows.Forms.ComboBox();
            this.Browse = new System.Windows.Forms.Button();
            this.GitRifAndVerKey = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BackupList
            // 
            this.BackupList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.BackupList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BackupList.ForeColor = System.Drawing.Color.Lime;
            this.BackupList.FormattingEnabled = true;
            this.BackupList.Location = new System.Drawing.Point(12, 40);
            this.BackupList.Name = "BackupList";
            this.BackupList.Size = new System.Drawing.Size(617, 262);
            this.BackupList.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Chartreuse;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "CMA Dir:";
            // 
            // CMADir
            // 
            this.CMADir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CMADir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CMADir.ForeColor = System.Drawing.Color.Lime;
            this.CMADir.Location = new System.Drawing.Point(67, 6);
            this.CMADir.Name = "CMADir";
            this.CMADir.Size = new System.Drawing.Size(240, 20);
            this.CMADir.TabIndex = 2;
            this.CMADir.TextChanged += new System.EventHandler(this.CMADir_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Chartreuse;
            this.label2.Location = new System.Drawing.Point(373, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "AID:";
            // 
            // AIDSelector
            // 
            this.AIDSelector.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AIDSelector.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AIDSelector.ForeColor = System.Drawing.Color.Lime;
            this.AIDSelector.FormattingEnabled = true;
            this.AIDSelector.Location = new System.Drawing.Point(407, 6);
            this.AIDSelector.Name = "AIDSelector";
            this.AIDSelector.Size = new System.Drawing.Size(222, 21);
            this.AIDSelector.TabIndex = 4;
            this.AIDSelector.Text = "0000000000000000";
            this.AIDSelector.TextChanged += new System.EventHandler(this.AIDSelector_TextChanged);
            // 
            // Browse
            // 
            this.Browse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Browse.ForeColor = System.Drawing.Color.Chartreuse;
            this.Browse.Location = new System.Drawing.Point(313, 6);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(54, 21);
            this.Browse.TabIndex = 5;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // GitRifAndVerKey
            // 
            this.GitRifAndVerKey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GitRifAndVerKey.ForeColor = System.Drawing.Color.Red;
            this.GitRifAndVerKey.Location = new System.Drawing.Point(12, 308);
            this.GitRifAndVerKey.Name = "GitRifAndVerKey";
            this.GitRifAndVerKey.Size = new System.Drawing.Size(617, 25);
            this.GitRifAndVerKey.TabIndex = 6;
            this.GitRifAndVerKey.Text = "GO GIT THE RIF AND VERSION KEY!!!!";
            this.GitRifAndVerKey.UseVisualStyleBackColor = true;
            this.GitRifAndVerKey.Click += new System.EventHandler(this.GitRifAndVerKey_Click);
            // 
            // CHOVYCmaSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(641, 341);
            this.Controls.Add(this.GitRifAndVerKey);
            this.Controls.Add(this.Browse);
            this.Controls.Add(this.AIDSelector);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CMADir);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BackupList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CHOVYCmaSelector";
            this.Text = "CHOVYCmaSelectorForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox BackupList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox CMADir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox AIDSelector;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.Button GitRifAndVerKey;
    }
}