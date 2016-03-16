//Windowsアプリケーションとしてビルドすること
//System.Windows.FormsとSystem.Drawingを追加

using System.Drawing;
using System.Windows.Forms;

namespace hsp.rtm
{
    class Core
    {
        public static Window window = new Window();

        static void Main()
        {
            window.Load += window._Load;
            window.ClientSize = new Size(640, 480);
            Application.Run(window);
        }
    }
}