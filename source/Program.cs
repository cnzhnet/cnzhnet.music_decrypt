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
        /// 应用程序入口点主方法.
        /// </summary>
        static void Main()
        {
            CommandLineApp app = new CommandLineApp();
            app.PrintHelp();
            string command, tmp;
            bool exitApp = false;
            do
            {
                Console.Write("输入命令>：");
                command = Console.ReadLine();
                tmp = command.ToLower();
                switch (tmp) 
                {
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
