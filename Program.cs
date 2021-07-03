using ForceWinSleep.Properties;
using System;
using System.Windows.Forms;

namespace ForceWinSleep
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            Boolean createdNew; //返回是否赋予了使用线程的互斥体初始所属权
            System.Threading.Mutex instance = new System.Threading.Mutex(true, "ForceWinSleep", out createdNew); //同步基元变量
            if (createdNew) //赋予了线程初始所属权，也就是首次使用互斥体
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                    var mainForm = new MainForm();
                    Application.Run(mainForm);

                instance.ReleaseMutex();
            } else {
                MessageBox.Show(Resources.SingleRunMessage, Resources.SingleRunTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            return 0;
        }
    }
}
