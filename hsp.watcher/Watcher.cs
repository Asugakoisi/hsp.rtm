//Windowsアプリケーションとしてビルドすること

using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace hsp.watcher
{
    /// <summary>
    /// メモ帳からコードを取得するクラス
    /// </summary>
    class NotepadText
    {
        private const int WM_GETTEXT = 0xd;
        private const int WM_GETTEXTLENGTH = 0xe;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, StringBuilder lParam);

        /// <summary>
        /// メモ帳からコードを取得
        /// </summary>
        /// <param name="hWnd">メモ帳のテキストエリアのウィンドウハンドル</param>
        /// <returns></returns>
        public static string GetText(IntPtr hWnd)
        {
            var textLength = SendMessage(hWnd, WM_GETTEXTLENGTH, 0, 0) + 1;
            var sb = new StringBuilder(textLength);
            if (textLength > 0)
            {
                SendMessage(hWnd, WM_GETTEXT, textLength, sb);
            }
            return sb.ToString();
        }
    }

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

    public class Sender
    {
        private static void Main()
        {
            //メモ帳を起動
            var notepad = Process.Start("notepad");

            //RTM本体を起動
            var rtm = Process.Start("hsp.rtm.exe");
            rtm.WaitForInputIdle();

            //コード差分用のバックアップ
            var old = "";
            while (true)
            {
                //メモ帳のテキストエリアのウィンドウハンドルを取得
                var hWnd = NotepadText.FindWindowEx(notepad.MainWindowHandle, IntPtr.Zero, "Edit", "");
                //メモ帳のテキストエリアの文字列を取得
                var str = NotepadText.GetText(hWnd);
                //コードが変更されていた場合
                if (!str.Equals(old))
                {
                    //メッセージとして変更されたコードを送信
                    var message = new WindowMessage();
                    message.SendData(rtm.MainWindowHandle, str);
                    old = str;
                }
                Thread.Sleep(100);
            }
        }
    }
}