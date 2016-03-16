using System;
using System.Linq;
using Microsoft.CSharp;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace hsp.rtm
{
    /// <summary>
    /// Watcherから送信されてくるコードを受け取るクラス
    /// </summary>
    public class WindowMessage
    {
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public Int32 cbData;
            public string lpData;
        }

        public event EventHandler<ReceiveDataEventArgs> OnReceiveData;

        /// <summary>
        /// Watcherから送信されてくるデータを受け取る
        /// </summary>
        /// <param name="m">処理するためのMessageインスタンス</param>
        public void ReceiveData(Message m)
        {
            if (OnReceiveData != null)
            {
                COPYDATASTRUCT cds = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT));
                OnReceiveData(this, new ReceiveDataEventArgs(cds.lpData));
            }
        }
    }

    public class ReceiveDataEventArgs : EventArgs
    {
        public ReceiveDataEventArgs(string data)
        {
            ReceiveData = data;
        }

        public string ReceiveData { get; }
    }

    public class Window : Form
    {
        private const int WM_COPYDATA = 0x4A;

        private WindowMessage message { get; set; }

        public void _Load(object sender, EventArgs e)
        {
            message = new WindowMessage();
            message.OnReceiveData += (_o, _e) => RTM.Execute(_e.ReceiveData);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_COPYDATA)
            {
                message.ReceiveData(m);
            }
            base.WndProc(ref m);
        }
    }

    public class RTM
    {
        private static dynamic instance;
        private static dynamic oldInstance;

        /// <summary>
        /// 動的にイベントを追加
        /// </summary>
        /// <param name="eventName">追加するイベント名</param>
        /// <param name="eventType">追加するイベントハンドラの型情報</param>
        /// <param name="instanceMethodName">追加するイベントのインスタンス内での名前</param>
        public static void AddEvent(string eventName, Type eventType, string instanceMethodName)
        {
            Core.window.GetType()
                .GetEvent(eventName)
                .AddEventHandler(Core.window,
                    Delegate.CreateDelegate(eventType, instance, instance.GetType().GetMethod(instanceMethodName)));
        }

        /// <summary>
        /// 動的にイベントを削除
        /// </summary>
        /// <param name="eventName">削除するイベント名</param>
        /// <param name="eventType">削除するイベントハンドラの型情報</param>
        /// <param name="instanceMethodName">削除するイベントのインスタンス内での名前</param>
        public static void DeleteEvent(string eventName, Type eventType, string instanceMethodName)
        {
            Core.window.GetType()
                .GetEvent(eventName)
                .RemoveEventHandler(Core.window,
                    Delegate.CreateDelegate(eventType, oldInstance, oldInstance.GetType().GetMethod(instanceMethodName)));
        }

        public static void Execute(string str)
        {
            try
            {
                //全角スペースとタブを半角スペースに変換し, 改行でスプリット
                var hspArrayData = str.Split('\n').Where(i => i.Length != 0).ToList();

                //HSPのコードをC#のコードに変換
                var code = Analyzer.GenerateCode(hspArrayData);

                //生成したコードを実行
                var param = new CompilerParameters();

                param.ReferencedAssemblies.AddRange(new[]
                {
                    "mscorlib.dll", "System.dll", "System.Core.dll", "Microsoft.CSharp.dll", "System.IO.dll",
                    "System.Windows.Forms.dll", "System.Drawing.dll"
                });

                //GUIアプリケーションとしてコンパイルするためのオプション
                param.CompilerOptions = "/t:winexe";

                //生成したコードをコンパイルしてAssemblyを得る
                var assembly = new CSharpCodeProvider()
                    .CompileAssemblyFromSource(param, code)
                    .CompiledAssembly;

                //Programの型情報を取得
                var dataType = assembly.GetType("NameSpace.Program");

                //前のインスタンスをバックアップ
                oldInstance = instance;

                //Programのインスタンスを作成
                instance = Activator.CreateInstance(dataType);

                //既に追加されているイベントを破棄
                if (oldInstance != null)
                {
                    DeleteEvent("Paint", typeof (PaintEventHandler), "Paint");
                }
                //新しくコンパイルしたイベントを追加
                AddEvent("Paint", typeof (PaintEventHandler), "Paint");
                //リフレッシュ
                Core.window.Refresh();
            }
            catch (Exception)
            {
                //何かしらのエラー
                //構文エラーとかは別途で警告出したい
            }
        }
    }
}