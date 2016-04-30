// Windowsアプリケーションとしてビルドすること
// System.Windows.FormsとSystem.Drawingを追加

using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace hsp.rtm
{
    public static class Core
    {
        public static Window MainWindow;
        public static Form DebugWindow, ErrorWindow;

        static void Main()
        {
            MainWindow = new Window();
            MainWindow.Load += MainWindow._Load;
            MainWindow.Size = new Size(640, 480);
            MainWindow.Text = "HSP Real-Time Debug";

            DebugWindow = new Form
            {
                Text = "Debug Window",
                Size = new Size(600, 800)
            };

            ErrorWindow = new Form
            {
                Text = "Error Message",
                Size = new Size(600, 400)
            };
            ErrorWindow.Paint += ErrorPaint;
            ErrorWindow.Show();

            // RTMが終了したときに, 一緒にwatcherも終了させる
            MainWindow.FormClosed += MainWindow.ExitWatcher;

            Application.Run(MainWindow);
        }

        public static void ErrorPaint(object sender, PaintEventArgs e)
        {
            var FontSize = 10;
            int x, y;
            x = y = 0;
            var g = e.Graphics;
            var brush = new SolidBrush(Color.FromArgb(255, 0, 0));
            var font = new Font("FixedSys", FontSize);
            if (Error.ErrorMessages.Count > 0)
            {
                foreach (var error in Error.ErrorMessages)
                {
                    g.DrawString(error, font, brush, x, y);
                    var count = error.Count(i => i == '\n');
                    for (var i = 0; i < count; i++)
                    {
                        y += FontSize * 8;
                    }
                }
            }
        }
    }
}