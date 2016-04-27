// Windowsアプリケーションとしてビルドすること
// System.Windows.FormsとSystem.Drawingを追加

using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace hsp.rtm
{
    public static class Core
    {
        public static Window MainWindow = new Window();
        public static Form DebugWindow = new Form();
        public static Form ErrorWindow = new Form();

        static void Main()
        {
            MainWindow.Load += MainWindow._Load;
            MainWindow.Size = new Size(640, 480);
            MainWindow.Text = "HSP Real-Time Debug";

            DebugWindow.Text = "Debug Window";
            DebugWindow.Size = new Size(600, 800);

            ErrorWindow.Text = "Error Message";
            ErrorWindow.Size = new Size(600, 400);
            ErrorWindow.Paint += ErrorPaint;
            ErrorWindow.Show();

            // RTMが終了したときに, 一緒にwatcherも終了させる
            MainWindow.FormClosed += MainWindow.ExitWatcher;

            Application.Run(MainWindow);
        }

        public static void ErrorPaint(object sender, PaintEventArgs e)
        {
            var FontSize = 10;
            var CurrentPosX = 0;
            var CurrentPosY = 0;
            var g = e.Graphics;
            var brush = new SolidBrush(Color.FromArgb(255, 0, 0));
            var font = new Font("FixedSys", FontSize);
            if (Error.ErrorMessages.Count > 0)
            {
                foreach (var error in Error.ErrorMessages)
                {
                    g.DrawString(error, font, brush, CurrentPosX, CurrentPosY);
                    var count = error.Count(i => i == '\n');
                    for (var i = 0; i < count; i++)
                    {
                        CurrentPosY += FontSize * 8;
                    }
                }
            }
        }
    }
}