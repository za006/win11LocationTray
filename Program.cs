using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace win11LocationTray
{
    static class Proxy
    {
        private const string RegistrySubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location";
        private const string RegistryValueName = "Value";

        public static bool IsOn()
        {
            try
            {

                using (var key = Registry.LocalMachine.OpenSubKey(RegistrySubKey, false))
                {
                    if (key != null)
                    {
                        object val = key.GetValue(RegistryValueName);
                        if (val != null)
                        {
                            return val.ToString().Equals("Allow", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public static void Toggle()
        {
            try
            {
                bool currentState = IsOn();

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "SystemSettingsAdminFlows.exe";

                psi.Arguments = "SetCamSystemGlobal 1 " + (currentState ? "0" : "1");
                psi.Verb = "runas"; // 触发 Windows 正常的管理员提权提示
                psi.CreateNoWindow = true;
                psi.UseShellExecute = true;

                Process p = Process.Start(psi);
                if (p != null)
                {
                    p.WaitForExit(); // 等待切换完成
                }

                using (var key = Registry.LocalMachine.OpenSubKey(RegistrySubKey, true))
                {
                    if (key != null)
                    {
                        string targetValue = currentState ? "Deny" : "Allow";
                        key.SetValue(RegistryValueName, targetValue, RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改定位状态失败，请确保授予了管理员权限: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    static class Program
    {
        private static NotifyIcon _trayIcon;
        private static Icon _iconOn;
        private static Icon _iconOff;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                _iconOn = new Icon("ProxyOn.ico");
                _iconOff = new Icon("ProxyOff.ico");
            }
            catch
            {
                _iconOn = SystemIcons.Information;
                _iconOff = SystemIcons.Application;
            }

            _trayIcon = new NotifyIcon
            {
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("定位设置 (Settings)", OpenSettings),
                    new MenuItem("-"),
                    new MenuItem("退出 (Quit)", Quit)
                }),
                Visible = true
            };

            _trayIcon.Click += TrayIcon_Click;

            UpdateTrayState();

            Application.Run();
        }

        private static void TrayIcon_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouseArgs = e as MouseEventArgs;
            if (mouseArgs != null && mouseArgs.Button != MouseButtons.Left)
                return;

            Proxy.Toggle();

            UpdateTrayState();
        }

        private static void UpdateTrayState()
        {
            bool isOn = Proxy.IsOn();

            _trayIcon.Icon = isOn ? _iconOn : _iconOff;
            _trayIcon.Text = isOn ? "系统定位：已开启 (Allow)" : "系统定位：已关闭 (Deny)";
        }

        private static void OpenSettings(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c start ms-settings:privacy-location",
                    CreateNoWindow = true,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private static void Quit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }
    }
}
