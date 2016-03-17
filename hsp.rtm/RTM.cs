using System;
using System.Linq;
using Microsoft.CSharp;
using System.Windows.Forms;
using System.CodeDom.Compiler;
using System.Collections.Generic;
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
                var cds = (COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT));
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
                //変数の退避と復元について
                //変数は全てこちらで管理して, 実行プログラムでは参照するだけにしたほうが良さそう
                //初めに, VariablesというDictionary(キーが変数名, バリューが実値)を宣言
                //HSPをC#に変換後, Analyzer.VariableListを元にDictionaryを作成
                //キーがもともと存在する場合はそのままで, 存在しない場合は追加
                //必要なくなった要素はDictionaryから削除するのを忘れないように
                //バリューは一切操作しないので, 値は変化しないで継続出来る
                //変更点は生成コードでは一切変数定義しないことと, しっかりこちら側を参照するようにすること
                //(例) Parent.Variables["x"] = 10;

                //多分他にも初期化しないといけないものある
                //変数リストを初期化
                Analyzer.VariableList = new List<string>()
                {
                    "strsize",
                    "stat",
                    "cnt"
                };

                //全角スペースとタブを半角スペースに変換し, 改行でスプリット
                var hspArrayData = str.Split('\n').Where(i => i.Length != 0).ToList();

                //HSPのコードをC#のコードに変換
                var code = Analyzer.GenerateCode(hspArrayData);

                //デバッグ用のコード出力
                var sw = new StreamWriter("code.cs", false, Encoding.UTF8);
                sw.WriteLine(code);
                sw.Close();

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
                instance = Activator.CreateInstance(dataType, Core.window);

                //既に追加されているイベントを破棄
                if (oldInstance != null)
                {
                    DeleteEvent("Paint", typeof(PaintEventHandler), "Paint");
                }
                //新しくコンパイルしたイベントを追加
                AddEvent("Paint", typeof(PaintEventHandler), "Paint");
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