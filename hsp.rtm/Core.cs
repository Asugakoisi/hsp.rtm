//Windowsアプリケーションとしてビルドすること
//System.Windows.FormsとSystem.Drawingを追加

using System;
using System.Management;
using System.Diagnostics;
using System.Windows.Forms;

namespace hsp.rtm
{
    class Core
    {
        public static Window window = new Window();

        static void Main()
        {
            window.Load += window._Load;

            //RTMが終了したときに, 一緒にwatcherも終了させる
            window.FormClosed += ExitWatcher;

            Application.Run(window);
        }

        /// <summary>
        /// watcherを終了させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ExitWatcher(object sender, EventArgs e)
        {
            var watcherID = GetParentProcess(Process.GetCurrentProcess().Id);
            var watcher = Process.GetProcessById(watcherID);
            watcher.Kill();
        }

        /// <summary>
        /// 親プロセスのIDを得る
        /// </summary>
        /// <param name="Id">自分のプロセスID</param>
        /// <returns></returns>
        public static int GetParentProcess(int Id)
        {
            var parentPid = 0;
            using (var mo = new ManagementObject("win32_process.handle='" + Id + "'"))
            {
                mo.Get();
                parentPid = Convert.ToInt32(mo["ParentProcessId"]);
            }
            return parentPid;
        }
    }
}