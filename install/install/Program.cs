using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Security.Principal;

namespace install
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!IsAdmin)
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
                if (args == null || args.Length < 1)
                {
                    var isVisualStudioCodeInstalled =
                            MessageBox.Show("Visual Studio Codeをインストールしていますか？", "Step 1", MessageBoxButtons.YesNo);
                    if (isVisualStudioCodeInstalled == DialogResult.No)
                    {
                        MessageBox.Show("Visual Studio Codeをインストールして下さい");
                        Process.Start("https://www.visualstudio.com/ja-jp/products/code-vs.aspx");
                        return;
                    }

                    var agree = MessageBox.Show("hsp.rtmをインストールしますか？", "Step 2", MessageBoxButtons.OKCancel);
                    if (agree == DialogResult.OK)
                    {
                        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                                                  "\\.vscode\\extensions\\hsp-rtm");
                        CopyDirectory(Directory.GetCurrentDirectory() + "\\data",
                            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.vscode\\extensions\\hsp-rtm");
                        MessageBox.Show("インストールが完了しました!");
                    }
                }
                else if (args[0].Equals("remove"))
                {
                    var agree = MessageBox.Show("hsp.rtmをアンインストールしますか？", "Step 2", MessageBoxButtons.OKCancel);
                    if (agree == DialogResult.OK)
                    {
                        Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.vscode\\extensions\\hsp-rtm", true);
                        MessageBox.Show("アンインストールが完了しました!");
                    }
                }
            }
        }

        static bool IsAdmin
        {
            get
            {
                var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// ディレクトリをコピーする
        /// </summary>
        /// <param name="sourceDirName">コピーするディレクトリ</param>
        /// <param name="destDirName">コピー先のディレクトリ</param>
        static void CopyDirectory(string sourceDirName, string destDirName)
        {
            //コピー先のディレクトリがないときは作る
            if (!System.IO.Directory.Exists(destDirName))
            {
                System.IO.Directory.CreateDirectory(destDirName);
                //属性もコピー
                System.IO.File.SetAttributes(destDirName,
                    System.IO.File.GetAttributes(sourceDirName));
            }

            //コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[destDirName.Length - 1] !=
                    System.IO.Path.DirectorySeparatorChar)
                destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;

            //コピー元のディレクトリにあるファイルをコピー
            string[] files = System.IO.Directory.GetFiles(sourceDirName);
            foreach (string file in files)
                System.IO.File.Copy(file,
                    destDirName + System.IO.Path.GetFileName(file), true);

            //コピー元のディレクトリにあるディレクトリについて、再帰的に呼び出す
            string[] dirs = System.IO.Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
                CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir));
        }
    }
}
