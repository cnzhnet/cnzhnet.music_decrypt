using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using cnzhnet.music_decrypt.Models;

namespace cnzhnet.music_decrypt.Services
{
    /// <summary>
    /// 表示命令行支持程序.
    /// </summary>
    public class CommandLineApp
    {
        private List<DecryptAudioItem> audioItems;
        private string sourcePath, outputPath;
        private int currentLineCursor;

        /// <summary>
        /// 创建一个 <see cref="CommandLineApp"/> 的对象实例.
        /// </summary>
        public CommandLineApp()
        {
            audioItems = new List<DecryptAudioItem>();
            // 注册命令行工具的解密音频支持.
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
        }

        /// <summary>
        /// 从命令行参数中获取路径信息.
        /// </summary>
        /// <param name="args">传入的命令行参数.</param>
        /// <returns></returns>
        private string GetArgsPath(string[] args)
        {
            if (args.Length < 2)
                return string.Empty;
            StringBuilder pb = new StringBuilder(args[1]);
            for (int i = 2; i < args.Length; ++i)
                pb.AppendFormat(" {0}", args[i]);
            char quote = pb[0];
            if (quote == '\'' || quote == '\"') // 去掉路径左端的引号字符
                pb.Remove(0, 1);
            quote = pb[pb.Length - 1];
            if (quote == '\'' || quote == '\"') // 去掉路径右端的引号字符
                pb.Remove(pb.Length - 1, 1);
            return pb.ToString();
        }
        /// <summary>
        /// 向列表中添加要解密的源音频文件.
        /// </summary>
        private void AddSourceFile()
        {
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine("指定的源目录不存在.");
                sourcePath = null;
                return;
            }
            audioItems.Clear();
            DirectoryInfo dir = new DirectoryInfo(sourcePath);
            AudioSupported[] supported = AudioDecrypter.GetSupportedAudios();
            FileInfo[] files = dir.GetFiles();
            int total = 0;
            foreach (FileInfo af in files)
            {
                if (supported.Count(p => p.Extension == Path.GetExtension(af.Name).ToLower()) < 1)
                    continue;
                audioItems.Add(new DecryptAudioItem {
                    File = af.Name,
                    FullPath = af.FullName
                });
                Console.WriteLine($"[添加音频]: {af.Name}");
                total++;
            }
            Console.Write("----------------------------\r\n");
            Console.WriteLine($"共添加 {total} 个支持的音频文件.");
        }
        /// <summary>
        /// 执行解密.
        /// </summary>
        private void DoDecrypt()
        {
            if (audioItems.Count < 1)
            {
                Console.WriteLine("没有需要解密的音频文件.");
                return;
            }
            if (string.IsNullOrEmpty(outputPath))
            {
                Console.WriteLine("未指定输出目录.");
                return;
            }
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            IAudioDecrypter decrypter = null;
            int processed = 0;
            string extension;
            for (int i = 0; i < audioItems.Count; ++i)
            {
                try
                {
                    extension = Path.GetExtension(audioItems[i].File).ToLower();
                    decrypter = AudioDecrypter.GetDecrypter(extension);
                    if (decrypter == null)
                        continue;
                    decrypter.UseMultithreaded = false;
                    decrypter.Progress += Decrypter_Progress;
                    audioItems[i].Output = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(audioItems[i].File)}.tmp");
                    decrypter.Source = File.Open(audioItems[i].FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (File.Exists(audioItems[i].Output))
                        File.Delete(audioItems[i].Output);
                    decrypter.Output = File.Open(audioItems[i].Output, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    decrypter.Output.Position = 0;
                    currentLineCursor = Console.CursorTop;
                    Console.Write($"[正在解密：{audioItems[i].File}] 0%");
                    decrypter.Decrypt(audioItems[i]);
                    decrypter.Output.Close();
                    if (!string.IsNullOrEmpty(audioItems[i].OutputExt)) 
                    {
                        extension = audioItems[i].Output;
                        audioItems[i].Output = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(audioItems[i].File)}{audioItems[i].OutputExt}");
                        File.Move(extension, audioItems[i].Output, true);
                    }
                    processed++;
                    UpdateCurrentLine(new string(' ', Console.WindowWidth));
                    Console.WriteLine($"[已解密音频]：{Path.GetFileName(audioItems[i].Output)}");
                }
                catch (Exception Ex)
                {
                    Console.WriteLine($"[{audioItems[i].File} 错误]：{Ex.Message}");
                    try
                    {
                        if (File.Exists(audioItems[i].Output))
                            File.Delete(audioItems[i].Output);
                    }
                    catch { }
                }
                finally
                {
                    if (decrypter != null)
                    {
                        decrypter.Source?.Dispose();
                        decrypter.Output?.Dispose();
                        decrypter.Source = null;
                        decrypter.Output = null;
                    }
                }
            }
            Console.WriteLine($"成功解密 {processed} 个音频文件.");
            GC.Collect();
        }

        /// <summary>
        /// 用于输出解密进度.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="item"></param>
        /// <param name="progress"></param>
        private void Decrypter_Progress(IAudioDecrypter sender, DecryptAudioItem item, float progress)
        {
            UpdateCurrentLine(new string(' ', Console.WindowWidth));
            UpdateCurrentLine($"[正在解密：{item.File}] {Math.Round(progress, 2)}% ");
        }

        /// <summary>
        /// 执行命令.
        /// </summary>
        /// <param name="command">命信.</param>
        public void DoCommand(string command)
        {
            string[] args = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (args.First().ToLower())
            {
                case "source":
                    sourcePath = GetArgsPath(args);
                    if (string.IsNullOrEmpty(sourcePath))
                        Console.WriteLine("指定的源目录无效.");
                    else
                        AddSourceFile();
                    break;
                case "output":
                    outputPath = GetArgsPath(args);
                    if (!string.IsNullOrEmpty(outputPath) && Directory.Exists(outputPath))
                    {
                        Console.WriteLine("已设置要解密音频所在的目录."); 
                    }
                    else
                    {
                        outputPath = null;
                        Console.WriteLine("指定的源目录无效或不存在.");
                    }
                    break;
                case "-d":
                    Console.Write("---------------------------------\r\n");
                    DoDecrypt();
                    break;
                case "clear":
                    Console.Clear();
                    PrintHelp();
                    break;
                default:
                    Console.WriteLine($"“{command}”不是支持的命令.");
                    break;
            }
        }
        /// <summary>
        /// 更新控制台中当前行的内容.
        /// </summary>
        /// <param name="text">要输出的内容.</param>
        private void UpdateCurrentLine(string text)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(text);
            Console.SetCursorPosition(0, currentLineCursor);
        }
        /// <summary>
        /// 打印帮助信息.
        /// </summary>
        public void PrintHelp()
        {
            Console.WriteLine("==================================");
            Console.WriteLine("  CMDT 音频解密");
            string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            Console.WriteLine($"  版本：{ver}   cnzhnet");
            Console.Write("----------------------------------\r\n");
            Console.WriteLine("  source <源目录>\t| 用于指定加密音频所在目录(完整路径)");
            Console.WriteLine("  output <输出目录>\t| 用于指定解密结果的输出目录");
            Console.WriteLine("  -d\t\t\t| 执行解密任务");
            Console.WriteLine("  clear\t\t\t| 清屏");
            Console.WriteLine("  exit\t\t\t| 退出程序");
            Console.Write("\r\n");
        }
    }
}
