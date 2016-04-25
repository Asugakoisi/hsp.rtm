using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Security.Principal;

namespace uninstall
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!IsAdministrator)
            {
                var psi = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = Application.ExecutablePath,
                    Verb = "runas",
                    Arguments = string.Join(" ", args)
                };

                try
                {
                    Process.Start(psi);
                }
                catch (Win32Exception)
                {
                    return;
                }
            }
            else
            {
                var agree = MessageBox.Show("hsp.rtmをアンインストールしますか？", "hsp.rtm", MessageBoxButtons.OKCancel);
                if (agree == DialogResult.OK)
                {
                    var rtm = Process.GetProcessesByName("hsp.rtm");
                    foreach (var p in rtm)
                    {
                        p.Kill();
                    }
                    var watcher = Process.GetProcessesByName("hsp.watcher");
                    foreach (var p in watcher)
                    {
                        p.Kill();
                    }

                    Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.vscode\\extensions\\hsp-rtm", true);
                    MessageBox.Show("アンインストールが完了しました!");
                }
            }
        }

        /// <summary>
        /// 管理者権限として起動しているか
        /// </summary>
        static bool IsAdministrator
        {
            get
            {
                var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}