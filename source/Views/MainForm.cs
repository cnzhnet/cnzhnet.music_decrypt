#if platform_windows
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// 创建主窗口实例.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

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
            StreamDecrypter.RegisterDecrypter(".kwm", typeof(KwmStreamDecrypter));
            // ... 其它类型的音频支持扩展后在此注册.
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
                bottomPanel.Enabled = true;
                return;
            }
            DecryptAudioItem item;
            IStreamDecrypter decrypter;
            do  // 若所添加的加密音频未被支持则应忽略之并递进处理下一个.
            {
                item = audioItems[processed];
                decrypter = StreamDecrypter.GetDecrypter(Path.GetExtension(item.File));
                if (decrypter == null)
                {
                    item.Status = "不支持.";
                    dataGridView1.InvalidateRow(processed);
                    Interlocked.Increment(ref processed);
                }
            }
            while (decrypter == null);
            // 准备解密支持的加密音频.
            decrypter.Completed += Decrypter_Completed;
            decrypter.Source = File.Open(item.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            item.Output = $"{Path.Combine(outputDir.Text, Path.GetFileNameWithoutExtension(item.File))}.tmp";
            item.Status = "解密中...";
            if (File.Exists(item.Output))
                File.Delete(item.Output);
            decrypter.Output = File.Open(item.Output, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            decrypter.Output.Position = 0;
            decrypter.Decrypt(item.Id); // 解密音频.
        }
        /// <summary>
        /// 解密码完成的事件处理程序.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Decrypter_Completed(IStreamDecrypter sender, CompletedEventArgs e)
        {   // 释放为目标解密器打开的源及输出目标流.
            sender.Source?.Close();
            sender.Source?.Dispose();
            sender.Source = null;
            sender.Output?.Flush();
            sender.Output?.Close();
            sender.Output?.Dispose();
            sender.Output = null;
            // 更新当前解密项的列表中的显示状态.
            DecryptAudioItem item = audioItems.Where(p => p.Id == e.Id).First();
            int index = audioItems.IndexOf(item);
            if (item != null)
            {
                if (e.Success)
                {
                    item.Status = "已解密";
                    File.Move(item.Output, $"{Path.Combine(Path.GetDirectoryName(item.Output), Path.GetFileNameWithoutExtension(item.File))}.flac", true);
                }
                else
                {
                    item.Status = $"解密错误：{e.Error.Message}";
                    File.Delete(item.Output);
                }
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
            bottomPanel.Enabled = false;
            DecryptItem();
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
            }
        }
    }
}
#endif
