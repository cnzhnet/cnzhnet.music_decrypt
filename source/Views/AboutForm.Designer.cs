
#if platform_windows
namespace cnzhnet.music_decrypt.Views
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.headPanel = new System.Windows.Forms.Panel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.cnzhnetIcon = new System.Windows.Forms.PictureBox();
            this.labelAppName = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.labelSupported = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.labelWaring = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.headPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cnzhnetIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // headPanel
            // 
            this.headPanel.BackColor = System.Drawing.Color.White;
            this.headPanel.Controls.Add(this.linkLabel1);
            this.headPanel.Controls.Add(this.cnzhnetIcon);
            this.headPanel.Controls.Add(this.labelAppName);
            this.headPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headPanel.Location = new System.Drawing.Point(0, 0);
            this.headPanel.Name = "headPanel";
            this.headPanel.Size = new System.Drawing.Size(381, 87);
            this.headPanel.TabIndex = 0;
            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabel1.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkLabel1.Location = new System.Drawing.Point(289, 11);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(80, 17);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "项目开源地址";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // cnzhnetIcon
            // 
            this.cnzhnetIcon.Image = ((System.Drawing.Image)(resources.GetObject("cnzhnetIcon.Image")));
            this.cnzhnetIcon.Location = new System.Drawing.Point(13, 11);
            this.cnzhnetIcon.Name = "cnzhnetIcon";
            this.cnzhnetIcon.Size = new System.Drawing.Size(64, 64);
            this.cnzhnetIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.cnzhnetIcon.TabIndex = 0;
            this.cnzhnetIcon.TabStop = false;
            // 
            // labelAppName
            // 
            this.labelAppName.AutoSize = true;
            this.labelAppName.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelAppName.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelAppName.Location = new System.Drawing.Point(83, 19);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new System.Drawing.Size(188, 44);
            this.labelAppName.TabIndex = 1;
            this.labelAppName.Text = "cnzhnet \r\n    Music Decrypt Tool";
            // 
            // labelCopyright
            // 
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.Location = new System.Drawing.Point(13, 108);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(60, 17);
            this.labelCopyright.TabIndex = 2;
            this.labelCopyright.Text = "(&C)巽翎君";
            // 
            // labelSupported
            // 
            this.labelSupported.AutoSize = true;
            this.labelSupported.Location = new System.Drawing.Point(13, 132);
            this.labelSupported.Name = "labelSupported";
            this.labelSupported.Size = new System.Drawing.Size(128, 17);
            this.labelSupported.TabIndex = 3;
            this.labelSupported.Text = "解密支持的文件格式：";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BackColor = System.Drawing.Color.White;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTextBox1.Location = new System.Drawing.Point(13, 152);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox1.Size = new System.Drawing.Size(356, 132);
            this.richTextBox1.TabIndex = 4;
            this.richTextBox1.Text = "";
            // 
            // labelWaring
            // 
            this.labelWaring.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelWaring.Location = new System.Drawing.Point(0, 299);
            this.labelWaring.Name = "labelWaring";
            this.labelWaring.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.labelWaring.Size = new System.Drawing.Size(381, 69);
            this.labelWaring.TabIndex = 5;
            this.labelWaring.Text = "警告：此项目仅用于学习交流。若使用此项目源代码所编译的任何程序从事任何商业行为引起的一切法律纠纷，概与作者无关，使用者将自行承担相应责任！";
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVersion.Location = new System.Drawing.Point(83, 108);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(286, 17);
            this.labelVersion.TabIndex = 2;
            this.labelVersion.Text = "版本  1.0.2.0";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(381, 368);
            this.Controls.Add(this.labelWaring);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.labelSupported);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelCopyright);
            this.Controls.Add(this.headPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "关于 - CMDT音频解密";
            this.headPanel.ResumeLayout(false);
            this.headPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cnzhnetIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

#endregion

        private System.Windows.Forms.Panel headPanel;
        private System.Windows.Forms.PictureBox cnzhnetIcon;
        private System.Windows.Forms.Label labelAppName;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Label labelSupported;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label labelWaring;
        private System.Windows.Forms.Label labelVersion;
    }
}
#endif