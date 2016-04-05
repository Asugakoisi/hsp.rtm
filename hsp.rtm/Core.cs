//Windowsアプリケーションとしてビルドすること
//System.Windows.FormsとSystem.Drawingを追加

using System.Drawing;
using System.Windows.Forms;

namespace hsp.rtm
{
    public static class Core
    {
        public static Window window = new Window();
        public static Form DebugWindow = new Form();

        static void Main()
        {
            window.Load += window._Load;
            window.Size = new Size(640, 480);
            window.Text = "HSP Real-Time Debug";

            DebugWindow.Text = "Debug Window";
            DebugWindow.Size = new Size(640, 480);

            //RTMが終了したときに, 一緒にwatcherも終了させる
            window.FormClosed += window.ExitWatcher;

            Application.Run(window);
        }
    }
}