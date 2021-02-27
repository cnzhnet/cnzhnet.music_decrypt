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
        /// ��������а�����Ϣ.
        /// </summary>
        static void PrintHelp()
        {
            Console.WriteLine("==============================");
            Console.WriteLine("  cnzhnet ��Ա����Ƶ������");
            string ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            Console.WriteLine($"  �汾��{ver}");
            Console.Write("----------------------------\r\n");
            Console.WriteLine("  source <ԴĿ¼>\t| ����ָ��������Ƶ����Ŀ¼(����·��)");
            Console.WriteLine("  output <���Ŀ¼>\t| ����ָ�����ܽ�������Ŀ¼");
            Console.WriteLine("  -d\t\t\t| ִ�н�������");
            Console.WriteLine("  clear\t\t\t| ����");
            Console.WriteLine("  exit\t\t\t| �˳�����");
            Console.Write("\r\n");
        }
        /// <summary>
        /// Ӧ�ó�����ڵ�������.
        /// </summary>
        static void Main()
        {
            PrintHelp();
            string command, tmp;
            bool exitApp = false;
            CommandLineApp app = new CommandLineApp();
            do
            {
                Console.Write("��������>��");
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
