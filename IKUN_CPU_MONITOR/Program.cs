using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IKUN_CPU_MONITOR
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Mutex mutex = new Mutex(true, "IKUN_CPU_MONITOR", out bool isFirstInstance);
            if (!isFirstInstance)
            {
                MessageBox.Show(" 程序已经运行，不要重复启动...");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WorkApplicationContext());
            mutex.ReleaseMutex();
        }
    }
}
