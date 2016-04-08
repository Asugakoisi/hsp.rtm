//Windowsアプリケーションとしてビルドすること
//System.Windows.FormsとSystem.Drawingを追加

using System.Drawing;
using System.Windows.Forms;

namespace hsp.rtm
{
    public static class Core
    {
        public static Window MainWindow = new Window();
        public static Form DebugWindow = new Form();

        static void Main()
        {
            MainWindow.Load += MainWindow._Load;
            MainWindow.Size = new Size(640, 480);
            MainWindow.Text = "HSP Real-Time Debug";

            DebugWindow.Text = "Debug Window";
            DebugWindow.Size = new Size(640, 480);

            //RTMが終了したときに, 一緒にwatcherも終了させる
            MainWindow.FormClosed += MainWindow.ExitWatcher;

            Application.Run(MainWindow);
        }
    }
}