using System;
#if platform_windows
using System.Windows.Forms;
using cnzhnet.music_decrypt.Views;
#else
using cnzhnet.music_decrypt.Services;
#endif

namespace cnzhnet.music_decrypt
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
#if platform_windows
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
#else
        /// <summary>
        /// 输出命令行帮助信息.
        /// </summary>
        static void PrintHelp()
        {
            Console.WriteLine("==============================");
            Console.WriteLine("  cnzhnet 会员大法音频解密器");
            string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            Console.WriteLine($"  版本：{ver}");
            Console.Write("----------------------------\r\n");
            Console.WriteLine("  source <源目录>\t| 用于指定加密音频所在目录(完整路径)");
            Console.WriteLine("  output <输出目录>\t| 用于指定解密结果的输出目录");
            Console.WriteLine("  -d\t\t\t| 执行解密任务");
            Console.WriteLine("  clear\t\t\t| 清屏");
            Console.WriteLine("  exit\t\t\t| 退出程序");
            Console.Write("\r\n");
        }
        /// <summary>
        /// 应用程序入口点主方法.
        /// </summary>
        static void Main()
        {
            PrintHelp();
            string command, tmp;
            bool exitApp = false;
            CommandLineApp app = new CommandLineApp();
            do
            {
                Console.Write("输入命令>：");
                command = Console.ReadLine();
                tmp = command.ToLower();
                switch (tmp) 
                {
                    case "clear":
                        Console.Clear();
                        PrintHelp();
                        break;
                    case "exit":
                        exitApp = true;
                        continue;
                    default:
                        app.DoCommand(command);
                        break;
                }
            } while (!exitApp);
        }
#endif
    }
}
