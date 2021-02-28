using System;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using cnzhnet.music_decrypt.Models;
using cnzhnet.music_decrypt.Services;

namespace cnzhnet.music_decrypt.Views
{
    /// <summary>
    /// 关于窗口.
    /// </summary>
    public partial class AboutForm : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public AboutForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)
                return;
            labelVersion.Text = $"版本  {this.GetType().Assembly.GetName().Version}";

            AudioSupported[] supporteds = AudioDecrypter.GetSupportedAudios();
            StringBuilder builder = new StringBuilder();
            foreach (AudioSupported support in supporteds)
                builder.AppendLine($"{support.Manufacturer}：*{support.Extension}");
            richTextBox1.Text = builder.ToString();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { 
                FileName = "https://github.com/cnzhnet/cnzhnet.music_decrypt",
                UseShellExecute = true
            });
        }
    }
}
