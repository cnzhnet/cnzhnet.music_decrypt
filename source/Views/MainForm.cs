#if platform_windows
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using cnzhnet.music_decrypt.Models;
using cnzhnet.music_decrypt.Services;

namespace cnzhnet.music_decrypt.Views
{
    /// <summary>
    /// 主窗口.
    /// </summary>
    public partial class MainForm : Form
    {
        private BindingList<DecryptAudioItem> audioItems;
        private SynchronizationContext syncContext;
        private int processed;
        private bool allowedQuit;

        /// <summary>
        /// 创建主窗口实例.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            allowedQuit = true;
            audioItems = new BindingList<DecryptAudioItem>();
            dataGridView1.DataSource = audioItems;
        }
        /// <summary>
        /// 主窗口加载时执行此方法.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)
                return;
            syncContext = SynchronizationContext.Current;
            // 在此注册支持的音频流解密器.
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".kwm", "酷我音乐", typeof(KwmAudioDecrypter)));
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".ncm", "网易云音乐", typeof(NcmAudioDecrypter)));
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".qmc0", "QQ音乐", typeof(QmcAudioDecrypter)));
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".qmc3", "QQ音乐", typeof(QmcAudioDecrypter)));
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".qmcogg", "QQ音乐", typeof(QmcAudioDecrypter)));
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".mflac", "QQ音乐", typeof(QmcAudioDecrypter)));
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".qmcflac", "QQ音乐", typeof(QmcAudioDecrypter)));
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".kgm", "酷狗音乐", typeof(KgmAudioDecrypter)));
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".kgma", "酷狗音乐", typeof(KgmAudioDecrypter)));
            AudioDecrypter.RegisterDecrypter(AudioSupported.Create(".vpr", "酷狗音乐", typeof(KgmAudioDecrypter)));

            AudioSupported[] supporteds = AudioDecrypter.GetSupportedAudios();
            StringBuilder filterLeft = new StringBuilder($"{supporteds[0].Manufacturer}(*{supporteds[0].Extension})");
            StringBuilder filterRight = new StringBuilder($"*{supporteds[0].Extension}");
            for (int i = 1; i < supporteds.Length; ++i)
            {
                filterLeft.Append($";{supporteds[i].Manufacturer}(*{supporteds[i].Extension})");
                filterRight.Append($";*{supporteds[i].Extension}");
            }
            openFileDialog1.Filter = $"{filterLeft}|{filterRight}";
        }
        /// <summary>
        /// 在解密任务完成之前禁止退出.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            e.Cancel = !allowedQuit;
        }
        /// <summary>
        /// 添加文件.
        /// </summary>
        private void AppendFiles()
        {
            switch (openFileDialog1.ShowDialog(this))
            {
                case DialogResult.OK:
                case DialogResult.Yes:
                    foreach (string f in openFileDialog1.FileNames)
                    {
                        audioItems.Add(new DecryptAudioItem { 
                            File = Path.GetFileName(f),
                            FullPath = f
                        });
                    }
                    dataGridView1.Refresh();
                    break;
            }
        }
        /// <summary>
        /// 移除文件.
        /// </summary>
        private void RemoveItem()
        {
            if (dataGridView1.SelectedRows.Count < 1)
                return;
            List<DecryptAudioItem> items = new List<DecryptAudioItem>();
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                items.Add((DecryptAudioItem)(row.DataBoundItem));
            for (int i = 0; i < items.Count; ++i)
                audioItems.Remove(items[i]);
            dataGridView1.Refresh();
        }
        /// <summary>
        /// 选定输出目录.
        /// </summary>
        private void SelectOutputDir()
        {
            switch (folderBrowserDialog1.ShowDialog(this))
            {
                case DialogResult.OK:
                case DialogResult.Yes:
                    outputDir.Text = folderBrowserDialog1.SelectedPath;
                    break;
            }
        }
        /// <summary>
        /// 解密列表中的音频项（逐渐条递进式）.
        /// </summary>
        private void DecryptItem()
        {
            if (processed >= audioItems.Count) // 列表中的所有音频皆已处理完成时.
            {
                MessageBox.Show($"已处理完成 {processed} 加密个音频，处理情况见列表.", "任务完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UiEnable();
                allowedQuit = true;
                return;
            }
            DecryptAudioItem item;
            IAudioDecrypter decrypter;
            do  // 若所添加的加密音频未被支持则应忽略之并递进处理下一个.
            {
                item = audioItems[processed];
                decrypter = AudioDecrypter.GetDecrypter(Path.GetExtension(item.File));
                if (decrypter == null)
                {
                    item.Status = "不支持.";
                    dataGridView1.InvalidateRow(processed);
                    Interlocked.Increment(ref processed);
                }
            }
            while (decrypter == null);
            dataGridView1.Rows[processed].Selected = true;
            progressBar1.Value = 0;
            // 准备解密支持的加密音频.
            decrypter.Progress -= Decrypter_Progress;
            decrypter.Progress += Decrypter_Progress;
            decrypter.Completed -= Decrypter_Completed;
            decrypter.Completed += Decrypter_Completed;
            decrypter.Source = File.Open(item.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            item.Output = $"{Path.Combine(outputDir.Text, Path.GetFileNameWithoutExtension(item.File))}.tmp";
            item.Status = "解密中...";
            if (File.Exists(item.Output))
                File.Delete(item.Output);
            dataGridView1.InvalidateRow(processed);
            decrypter.Output = File.Open(item.Output, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            decrypter.Output.Position = 0;
            decrypter.Decrypt(item); // 解密音频.
        }

        /// <summary>
        /// 更新界面上的解密进度提示.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="item"></param>
        /// <param name="progress"></param>
        private void Decrypter_Progress(IAudioDecrypter sender, DecryptAudioItem item, float progress)
        {
            syncContext.Send((state) => progressBar1.Value = (int)progress, null);
        }

        /// <summary>
        /// 解密码完成的事件处理程序.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Decrypter_Completed(IAudioDecrypter sender, CompletedEventArgs e)
        {   // 释放为目标解密器打开的源及输出目标流.
            sender.Source?.Close();
            sender.Source?.Dispose();
            sender.Source = null;
            sender.Output?.Close();
            sender.Output?.Dispose();
            sender.Output = null;
            // 更新当前解密项的列表中的显示状态.
            DecryptAudioItem item = e.Item;
            int index = audioItems.IndexOf(item);
            if (e.Success)
            {
                item.Status = "已解密";
                if (!string.IsNullOrEmpty(item.OutputExt))
                {
                    string tmpPath = item.Output;
                    item.Output = $"{Path.Combine(Path.GetDirectoryName(tmpPath), Path.GetFileNameWithoutExtension(item.File))}{item.OutputExt}";
                    File.Move(tmpPath, item.Output, true);
                }
            }
            else
            {
                item.Status = $"解密错误：{e.Error.Message}";
                File.Delete(item.Output);
                item.Output = null;
            }
            syncContext.Send((state) => {
                dataGridView1.InvalidateRow((int)state);
                Interlocked.Increment(ref processed); // 递进到列表中的下一个音频.           
                DecryptItem(); // 下一个音频的解密.
            }, index);
        }

        /// <summary>
        /// 启动解密任务.
        /// </summary>
        private void StartDectypt()
        {
            if (string.IsNullOrEmpty(outputDir.Text)) 
            {
                MessageBox.Show("请先选择输出目录！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (audioItems.Count < 1) 
            {
                MessageBox.Show("请先添加要解密的音频文件！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Interlocked.Exchange(ref processed, 0);
            UiEnable(false);
            allowedQuit = false;
            DecryptItem();
        }
        /// <summary>
        /// 启用或禁用用户按钮.
        /// </summary>
        /// <param name="enabled"></param>
        private void UiEnable(bool enabled = true)
        {
            outputDir.Enabled = enabled;
            btnBrowser.Enabled = enabled;
            btnAddFile.Enabled = enabled;
            btnRemove.Enabled = enabled;
            btnDecrypt.Enabled = enabled;
        }

        /// <summary>
        /// 按钮事件响应处理程序.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;

            switch (btn.Name)
            {
                case "btnBrowser":
                    SelectOutputDir();
                    break;
                case "btnAddFile":
                    AppendFiles();
                    break;
                case "btnRemove":
                    RemoveItem();
                    break;
                case "btnDecrypt":
                    StartDectypt();
                    break;
                case "btnAbout":
                    using (AboutForm about = new AboutForm()) 
                    {
                        about.ShowDialog(this);
                    }
                    break;
            }
        }
    }
}
#endif
