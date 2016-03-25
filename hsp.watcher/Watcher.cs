using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace hsp.watcher
{
    /// <summary>
    /// RTMにメッセージを送信するクラス
    /// </summary>
    public class WindowMessage
    {
        [DllImport("User32.dll")]
        private static extern Int32 SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public Int32 cbData;
            public string lpData;
        }

        private const int WM_COPYDATA = 0x4A;

        /// <summary>
        /// RTMにメッセージを送信する
        /// </summary>
        /// <param name="hwnd">RTMのメインウィンドウハンドル</param>
        /// <param name="data">送信する文字列</param>
        public void SendData(IntPtr hwnd, string data)
        {
            var buf = Encoding.Default.GetBytes(data);
            var cds = new COPYDATASTRUCT
            {
                dwData = IntPtr.Zero,
                cbData = buf.Length + 1,
                lpData = data
            };
            SendMessage(hwnd, WM_COPYDATA, IntPtr.Zero, ref cds);
        }
    }

    public class Program
    {
        private static void Main()
        {
            //RTM本体を起動
            var rtm = Process.Start(Directory.GetCurrentDirectory() + "\\vscode-extension\\bin\\hsp.rtm.exe");
            rtm.WaitForInputIdle();

            while (true)
            {
                var code = Console.ReadLine();

                var message = new WindowMessage();
                message.SendData(rtm.MainWindowHandle, code.Replace("@@@@@", "\n"));

                Thread.Sleep(100);
            }
        }
    }
}