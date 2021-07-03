using ForceWinSleep.Properties;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Windows.Forms;

namespace ForceWinSleep
{
    public partial class MainForm : Form
    {
        private Timer hideTimer;
        private System.Threading.Timer checkTimer;
        private UserActivityHook hook;
        private int lastInputTick = 0;

        public MainForm()
        {
            InitializeComponent();
            Text = Resources.AppName;
            mainLabel.Text = Resources.AppName;
            notifyIcon1.Text = Resources.AppName;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Normal;
            Shown += MainForm_Shown;

            lastInputTick = Environment.TickCount;
            hook = new UserActivityHook();
            hook.KeyDown += (s, e) => {
                lastInputTick = Environment.TickCount;
            };
            hook.OnMouseActivity += (s, e) => {
                lastInputTick = Environment.TickCount;
            };
            hook.Start();

            SystemEvents.PowerModeChanged += (s, e) => {
                if (e.Mode == PowerModes.Resume) {
                    lastInputTick = Environment.TickCount;
                }
                Console.WriteLine(string.Format("{0} PowerChanged: {1}", DateTime.Now, e.Mode));
            };

            checkTimer = new System.Threading.Timer(CheckTimerCallback, null, 0, 20000);
        }

        private void CheckTimerCallback(object state) {
            int elapsedMinutes = (Environment.TickCount - lastInputTick) / 60000;
            PowerLineStatus powerLineStatus = SystemInformation.PowerStatus.PowerLineStatus;

            if ((powerLineStatus == PowerLineStatus.Online && elapsedMinutes >= Settings.Default.SleepWhenPowerOnline)
                || (powerLineStatus == PowerLineStatus.Offline && elapsedMinutes >= Settings.Default.SleepWhenPowerOffline)) { 
                Console.WriteLine(string.Format("{0} Sleep", DateTime.Now));
                Application.SetSuspendState(PowerState.Suspend, true, true);
            } else if ((powerLineStatus == PowerLineStatus.Online && elapsedMinutes >= Settings.Default.TurnOffScreenWhenPowerOnline)
                || (powerLineStatus == PowerLineStatus.Offline && elapsedMinutes >= Settings.Default.TurnOffScreenWhenPowerOffline)) { 
                Console.WriteLine(string.Format("{0} Turn off screen", DateTime.Now));
                TurnOffScreen();
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (hideTimer == null) {
                hideTimer = new Timer();
                hideTimer.Interval = 2000;
                hideTimer.Tick += new EventHandler(hideTimer_Tick);
                hideTimer.Enabled = true;
            }
        }

        private void hideTimer_Tick(object sender, EventArgs e)
        {
            Visible = false;
            hideTimer.Enabled = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) {
                this.Visible = false;
                e.Cancel = true;
            } else if (e.CloseReason == CloseReason.ApplicationExitCall) {
                if (MessageBox.Show(this, Resources.ExitMessage, Resources.ExitTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                    e.Cancel = true;
                }
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;
        }


        private const int WM_SYSCOMMAND = 0x112; //系统消息
        private const int SC_MONITORPOWER = 0xF170; //关闭显示器的系统命令
        private const int POWER_OFF = 2; //2 为关闭, 1为省电状态，-1为开机
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff); //广播消息，所有顶级窗体都会接收
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private void TurnOffScreen() {
            //SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, POWER_OFF);
            //Invoke(new MethodInvoker(() => {
            //    SendMessage(this.Handle, WM_SYSCOMMAND, SC_MONITORPOWER, POWER_OFF);
            //}));
        }
    }
}
