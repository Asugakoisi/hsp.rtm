using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace hsp.rtm
{
    public static class Analyzer
    {
        // ラベルからジャンプ台を生成するためのリスト
        public static List<string> LabelList = new List<string>();

        /// <summary>
        /// HSPのコードをC#に変換する
        /// </summary>
        /// <param name="hspArrayData">HSPのコード</param>
        /// <returns></returns>
        public static string GenerateCode(List<string> hspArrayData)
        {
            //C#のコードとして定義されているか
            bool isCSharp = false;

            for (int i = 0; i < hspArrayData.Count; i++)
            {
                // C#としてのusing
                /* 
                 * System
                 * System.Linq
                 * System.Drawing
                 * System.Diagnostics
                 * System.Windows.Forms
                 * System.Collections.Generic
                 */
                // 上記は標準でusingしているが, それ以外は@usingステートメントで定義
                // [例] @using System.Diagnostics
                if (hspArrayData[i].Trim().StartsWith("@using"))
                {
                    Using.Add(hspArrayData[i].Trim().Replace("@using", string.Empty).Replace(";", string.Empty).Trim());
                    hspArrayData[i] = string.Empty;
                    continue;
                }

                // C#としてのDLLの参照
                /*
                 * System.dll
                 * mscorlib.dll
                 * System.IO.dll
                 * System.Linq.dll
                 * System.Core.dll
                 * System.Drawing.dll
                 * Microsoft.CSharp.dll
                 * System.Windows.Forms.dll
                 */
                // 上記のDLLは標準で参照しているが, それ以外は@refステートメントで定義
                // DLLはパスが通るように指定すること
                // [例] @ref Microsoft.VisualBasic.dll
                if (hspArrayData[i].Trim().StartsWith("@ref"))
                {
                    ReferenceCheck(hspArrayData[i].Trim().Replace("@ref", string.Empty).Trim());
                    hspArrayData[i] = string.Empty;
                    continue;
                }

                /* 
                 * C#のコードを埋め込む場合は@csharpステートメントと@endステートメントで括る
                 */

                // C#のコードが始まる場合は変数の情報を更新
                if (hspArrayData[i].Trim().StartsWith("@csharp"))
                {
                    // 変数を更新
                    hspArrayData[i] =
                        VariableList.Aggregate(string.Empty,
                            (current, v) => current + (v + " = Variables[\"" + v + "\"];\n"));

                    isCSharp = true;
                    continue;
                }

                // C#のコードが終了したらフラグも戻す
                if (hspArrayData[i].Trim().StartsWith("@end"))
                {
                    // 変更を反映
                    hspArrayData[i] = VariableList.Aggregate(string.Empty,
                        (current, v) => current + ("Variables[\"" + v + "\"] = " + v + ";\n"));

                    isCSharp = false;
                    continue;
                }

                // C#のコードの場合は全てエスケープ
                if (isCSharp)
                {
                    continue;
                }

                // if文等の{}だけの行はスキップ
                if (hspArrayData[i].Equals("{") || hspArrayData[i].Equals("}")) continue;

                // データの整形
                // 前後の空白文字を削除
                hspArrayData[i] = hspArrayData[i].Trim();

                if (hspArrayData[i].Equals(string.Empty))
                {
                    continue;
                }

                // 直前にエスケープのないダブルクオーテーションが存在した場合
                // 文字列部分をStringListに格納し
                // その部分を＠＋＠StringListのindex＠ー＠で置換する
                // Example: hoge = "fu" + "ga"
                //          hoge = ＠＋＠0＠ー＠ + ＠＋＠1＠ー＠
                // StringListには"fu"と"ga"が格納される
                // このときダブルクオーテーションも含まれているので注意
                hspArrayData[i] = StringEscape(hspArrayData[i]);

                // //によるコメントはスキップ
                int commentOutIndex = hspArrayData[i].IndexOf("//");
                if (commentOutIndex > -1)
                {
                    continue;
                }

                // /* ... */によるコメントアウトをエスケープする
                commentOutIndex = hspArrayData[i].IndexOf("/*");
                if (commentOutIndex > -1)
                {
                    hspArrayData[i] = hspArrayData[i].Substring(0, commentOutIndex).Trim();
                    CommentFlag = true;
                }
                if (CommentFlag)
                {
                    commentOutIndex = hspArrayData[i].IndexOf("*/");
                    if (commentOutIndex > -1)
                    {
                        hspArrayData[i] = hspArrayData[i].Substring(commentOutIndex + "*/".Length).Trim();
                        CommentFlag = false;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (hspArrayData[i].Equals(string.Empty))
                {
                    continue;
                }

                hspArrayData[i] = hspArrayData[i]
                    // データ中の空白文字を全て半角スペースに変換
                    .Replace('　', ' ')
                    .Replace('\t', ' ')
                    // 関数部分で正確にスプリットするために()の直前後に半角スペースを追加
                    .Replace("(", " ( ")
                    .Replace(")", " ) ")
                    .Replace("=", " = ")
                    .Replace("+", " + ")
                    .Replace("-", " - ")
                    .Replace("*", " * ")
                    .Replace("/", " / ")
                    .Replace(",", " , ")
                    // 連続する演算子を修正
                    .Replace("=  =", "==")
                    .Replace("!  =", "!=")
                    .Replace("+  =", "+=")
                    .Replace("-  =", "-=")
                    .Replace("*  =", "*=")
                    .Replace("/  =", "/=")
                    .Replace("+  +", "++")
                    .Replace("-  -", "--")
                    .Replace("\\  =", "\\=")
                    .Trim();

                // ラベルの定義
                // HSPだと*で定義するが, C#は:なので置換
                if (hspArrayData[i].StartsWith("*"))
                {
                    hspArrayData[i] = hspArrayData[i].Substring(1).Trim() + ":";
                }

                // １番最初のsentenceを抜き出す
                int spaceIndex = hspArrayData[i].IndexOf(Space);
                string firstSentence = spaceIndex < 0
                    ? hspArrayData[i].Trim()
                    : hspArrayData[i].Substring(0, spaceIndex).Trim();

                // 変数の処理
                var str = hspArrayData[i].Split(' ').Select(j => j.Trim()).ToList();
                for (int j = 0; j < str.Count; j++)
                {
                    if (VariableList.Contains((str[j])))
                    {
                        str[j] = "Variables[\"" + str[j] + "\"]";
                    }
                }
                hspArrayData[i] = string.Join(Space, str);

                // プリプロセッサ処理
                hspArrayData[i] = Preprocessor(hspArrayData[i]);

                // 配列処理
                hspArrayData[i] = ArrayVariable(hspArrayData[i]);

                // マクロ処理
                hspArrayData[i] = Macro(hspArrayData[i]);

                // 関数処理
                hspArrayData[i] = Function(hspArrayData[i]);

                // 基本文法の処理
                if (BasicList.Contains(firstSentence))
                {
                    switch (firstSentence)
                    {
                        // if文の処理
                        case "if":
                            // "{}"を使って複数行で書かれている場合
                            // 必ず文中に"{"が入っている
                            int bracketIndex = hspArrayData[i].IndexOf("{");
                            if (bracketIndex < 0)
                            {
                                // 処理が1行で書かれている場合
                                // ifと条件文の間に"()"を入れる
                                int coronIndex = hspArrayData[i].IndexOf(":");
                                if (coronIndex < 0)
                                {
                                    // 条件文の後に処理が書かれていないため無効な文
                                    // エラーとして吐き出すよりも警告として表示したほうが良い？
                                    Error.AlertError("条件文の後に実行すべき処理が書かれていません");
                                }
                                else
                                {
                                    string tmpString = hspArrayData[i].Substring(coronIndex + 1);
                                    hspArrayData[i] = "if (" +
                                                      hspArrayData[i].Substring("if ".Length,
                                                          coronIndex - "if ".Length - 1) +
                                                      ")\n{";

                                    // ":"以降をhspArrayDataにInsertする
                                    string[] tmpArray = tmpString.Split(':');
                                    int index = i + 1;
                                    if (tmpArray.Length > 0)
                                    {
                                        for (int j = 0; j < tmpArray.Length; j++)
                                        {
                                            hspArrayData.Insert(i + j + 1, tmpArray[j].Trim());
                                            index = i + j + 1;
                                        }
                                    }

                                    // 末尾に"}"を付けるためのフラグ
                                    IfFlag.Add(index);
                                }
                            }
                            else
                            {
                                // 複数行の処理
                                hspArrayData[i] = "if (" +
                                                  hspArrayData[i].Substring("if ".Length,
                                                      bracketIndex - "if ".Length - 1) + ")\n" +
                                                  hspArrayData[i].Substring(bracketIndex);
                            }

                            // if文の条件における"="の好意的解釈
                            // "="が1つでも"=="として扱う
                            hspArrayData[i] = hspArrayData[i]
                                .Replace(Space, string.Empty)
                                .Replace("=", "==")
                                .Replace("====", " == ")
                                .Replace("!==", " != ")
                                .Replace("+==", " += ")
                                .Replace("-==", " -= ")
                                .Replace("*==", " *= ")
                                .Replace("/==", " /= ")
                                .Replace("<==", " <= ")
                                .Replace(">==", " >= ");

                            break;

                        // elseの処理
                        case "else":
                            hspArrayData[i] = "}\n else \n{";
                            break;

                        // forの処理
                        case "for":
                            var forConditionalSentence =
                                hspArrayData[i].Substring(spaceIndex).Split(',').Select(_ => _.Trim()).ToList();
                            if (forConditionalSentence.Count() != 4)
                            {
                                // 要素数がオカシイのでエラー
                                Error.AlertError("for文の要素数が不正です\n" +
                                                 "現在の要素数 = " + forConditionalSentence.Count);
                            }
                            else
                            {
                                hspArrayData[i] = "for (var " + forConditionalSentence[0] + " = " +
                                                  forConditionalSentence[1] +
                                                  "; " + forConditionalSentence[0] + " != " + forConditionalSentence[2] +
                                                  "; " +
                                                  forConditionalSentence[0] + " += " + forConditionalSentence[3] +
                                                  " )\n{";
                            }
                            break;

                        // breakの処理
                        case "_break":
                            hspArrayData[i] = "break";
                            break;

                        // continueの処理
                        case "_continue":
                            hspArrayData[i] = "continue";
                            break;

                        // whileの処理
                        case "while":
                            string whileConditionalSentence = hspArrayData[i].Substring(spaceIndex).Trim();
                            hspArrayData[i] = "while (" + whileConditionalSentence + ")\n{\n" +
                                              "ps = Process.GetProcessesByName(\"hsp.d\");\n" +
                                              "if (ps.Length > 0)\n" +
                                              "{\n" +
                                              "    var list = ps.Select(p => p.StartTime).ToList();\n" +
                                              "    list.Sort();\n" +
                                              "    foreach (var p in ps)\n" +
                                              "    {\n" +
                                              "        if (list.First() == p.StartTime)\n" +
                                              "        {\n" +
                                              "            try{p.Kill();}catch(Exception){}\n" +
                                              "            return;\n" +
                                              "        }\n" +
                                              "    }\n" +
                                              "}\n" +
                                              "now = DateTime.Now;\n" +
                                              "span = now - pre;\n" +
                                              "if((span.Minutes*60 + span.Seconds) * 1000 + span.Milliseconds > 500)\n" +
                                              "{\n" +
                                              "DebugWindow.Controls.Clear();\n" +
                                              "dPaint();\n" +
                                              "pre = now;\n" +
                                              "}\n";
                            break;

                        // repeatの処理
                        case "repeat":
                            string repeatConditionalSentence = hspArrayData[i].Substring(spaceIndex).Trim();
                            int counter;
                            if (int.TryParse(repeatConditionalSentence, out counter))
                            {
                                hspArrayData[i] = "for (Variables[\"cnt\"] = 0; Variables[\"cnt\"] < " + counter + "; Variables[\"cnt\"]++)\n{";
                            }
                            else
                            {
                                // repeatに渡されている値が数字ではないのでエラー
                                Error.AlertError("repeatに渡されている値(" + repeatConditionalSentence + ")は数字ではありません");
                            }
                            break;

                        // switchの処理
                        case "switch":
                            int switchSpaceIndex = hspArrayData[i].IndexOf(Space);
                            if (switchSpaceIndex < 0)
                            {
                                // switchの条件文としてオカシイのでエラー
                                Error.AlertError("switch文の条件文が書かれていません");
                            }
                            else
                            {
                                string switchConditionalSentence = hspArrayData[i].Substring(switchSpaceIndex).Trim(),
                                    switchTmpString = __LocalName("switchTmpString");
                                hspArrayData[i] = "string " + switchTmpString + " = " + switchConditionalSentence +
                                                  ".ToString();\n" + "switch (" + switchTmpString + ") \n{";
                            }
                            _switchFlag = true;
                            break;
                        case "swend":
                            // 1つ目の要素はswitch文なので取り除く
                            SwitchList.RemoveAt(0);

                            foreach (int s in SwitchList)
                            {
                                if (hspArrayData[s].Equals("default:"))
                                {
                                    if (!hspArrayData[s - 1].Contains("break;"))
                                    {
                                        string defaultString = string.Empty;
                                        for (int k = s + 1; k < SwitchList[SwitchList.Count - 1]; k++)
                                        {
                                            defaultString += hspArrayData[k] + "\nbreak;";
                                        }
                                        hspArrayData[s - 1] += defaultString;
                                    }
                                }

                                int endIndex = hspArrayData[s].IndexOf(Space);
                                if (endIndex < 0)
                                {
                                    //
                                }
                                else
                                {
                                    string caseName = hspArrayData[s].Substring(endIndex).Trim(),
                                        first = hspArrayData[s].Substring(0, endIndex).Trim();
                                    if (_firstCase)
                                    {
                                        _firstCase = false;
                                    }
                                    else if (first.Equals("case"))
                                    {
                                        if (!hspArrayData[s - 1].Contains("break;"))
                                        {
                                            hspArrayData[s - 1] += "\ngoto case " +
                                                                   caseName.Substring(0, caseName.Length - 1) +
                                                                   ";";
                                        }
                                    }
                                }
                            }

                            _switchFlag = false;
                            hspArrayData[i] = "}";
                            break;
                        case "swbreak":
                            hspArrayData[i] = "break;";
                            break;
                        case "case":
                            int caseSpaceIndex = hspArrayData[i].IndexOf(Space);
                            if (caseSpaceIndex < 0)
                            {
                                // case文の値が不正なのでエラー
                                Error.AlertError("case文の値が不正です");
                            }
                            else
                            {
                                hspArrayData[i] = "case \"" + hspArrayData[i].Substring(caseSpaceIndex).Trim() + "\"";
                            }
                            hspArrayData[i] += ":";
                            break;
                        case "default":
                            hspArrayData[i] += ":";
                            break;

                        // 色々な後処理
                        case "next":
                        case "wend":
                        case "loop":
                            hspArrayData[i] = "}";
                            break;

                        // gotoの処理
                        case "goto":
                            hspArrayData[i] = "ps = Process.GetProcessesByName(\"hsp.d\");\n" +
                                              "if (ps.Length > 0)\n" +
                                              "{\n" +
                                              "    var list = ps.Select(p => p.StartTime).ToList();\n" +
                                              "    list.Sort();\n" +
                                              "    foreach (var p in ps)\n" +
                                              "    {\n" +
                                              "        if (list.First() == p.StartTime)\n" +
                                              "        {\n" +
                                              "            try{p.Kill();}catch(Exception){}\n" +
                                              "            return;\n" +
                                              "        }\n" +
                                              "    }\n" +
                                              "}\n" + 
                                              "now = DateTime.Now;\n" +
                                              "span = now - pre;\n" +
                                              "if((span.Minutes*60 + span.Seconds) * 1000 + span.Milliseconds > 500)\n" +
                                              "{\n" +
                                              "DebugWindow.Controls.Clear();\n" +
                                              "dPaint();\n" +
                                              "pre = now;\n" +
                                              "}\n" +
                                              hspArrayData[i].Replace("*", string.Empty);
                            break;

                        case "gosub":
                            string label = __LocalName("label");
                            if (!LabelList.Contains(label))
                            {
                                LabelList.Add(label);
                            }

                            hspArrayData[i] = "ps = Process.GetProcessesByName(\"hsp.d\");\n" +
                                              "if (ps.Length > 0)\n" +
                                              "{\n" +
                                              "    var list = ps.Select(p => p.StartTime).ToList();\n" +
                                              "    list.Sort();\n" +
                                              "    foreach (var p in ps)\n" +
                                              "    {\n" +
                                              "        if (list.First() == p.StartTime)\n" +
                                              "        {\n" +
                                              "            try{p.Kill();}catch(Exception){}\n" +
                                              "            return;\n" +
                                              "        }\n" +
                                              "    }\n" +
                                              "}\n" +
                                              "LabelList.Add(\"" + label + "\");\n" +
                                              "now = DateTime.Now;\n" +
                                              "span = now - pre;\n" +
                                              "if((span.Minutes*60 + span.Seconds) * 1000 + span.Milliseconds > 500)\n" +
                                              "{\n" +
                                              "DebugWindow.Controls.Clear();\n" +
                                              "dPaint();\n" +
                                              "pre = now;\n" +
                                              "}\n" +
                                              "goto " + hspArrayData[i].Substring("gosub".Length).Replace("*", string.Empty) +
                                              ";\n" +
                                              label + ":\n";
                            break;
                    }
                }

                // コマンド処理
                // sentenceがコマンドかどうか
                else if (CommandList.Contains(firstSentence))
                {
                    // コマンドの引数部分を取得
                    string commandArguments = string.Empty;
                    if (spaceIndex > -1)
                    {
                        commandArguments = hspArrayData[i].Substring(spaceIndex + 1);
                    }

                    switch (firstSentence)
                    {
                        case "print":
                        case "mes":
                            hspArrayData[i] = GUI.Print(commandArguments);
                            break;
                        case "exist":
                            hspArrayData[i] = Base.Exist(commandArguments);
                            break;
                        case "delete":
                            hspArrayData[i] = Base.Delete(commandArguments);
                            break;
                        case "bcopy":
                            hspArrayData[i] = Base.Bcopy(commandArguments);
                            break;
                        case "mkdir":
                            hspArrayData[i] = Base.Mkdir(commandArguments);
                            break;
                        case "chdir":
                            hspArrayData[i] = Base.Chdir(commandArguments);
                            break;
                        case "split":
                            hspArrayData[i] = Base.Split(commandArguments);
                            break;
                        case "strrep":
                            hspArrayData[i] = Base.Strrep(commandArguments);
                            break;
                        case "dim":
                            hspArrayData[i] = Base.Dim(commandArguments);
                            break;
                        case "ddim":
                            hspArrayData[i] = Base.Ddim(commandArguments);
                            break;
                        case "end":
                            hspArrayData[i] = Base.End(commandArguments);
                            break;
                        case "stop":
                            hspArrayData[i] = Base.Stop(commandArguments);
                            break;
                        case "wait":
                            hspArrayData[i] = GUI.Wait(commandArguments);
                            break;
                        case "mci":
                            hspArrayData[i] = GUI.Mci(commandArguments);
                            break;
                        case "pos":
                            hspArrayData[i] = GUI.Pos(commandArguments);
                            break;
                        case "screen":
                            hspArrayData[i] = GUI.Screen(commandArguments);
                            break;
                        case "bgscr":
                            hspArrayData[i] = GUI.Bgscr(commandArguments);
                            break;
                        case "title":
                            hspArrayData[i] = GUI.Title(commandArguments);
                            break;
                        case "redraw":
                            hspArrayData[i] = GUI.Redraw(commandArguments);
                            break;
                        case "mouse":
                            hspArrayData[i] = GUI.Mouse(commandArguments);
                            break;
                        case "font":
                            hspArrayData[i] = GUI.Font(commandArguments);
                            break;
                        case "circle":
                            hspArrayData[i] = GUI.Circle(commandArguments);
                            break;
                        case "boxf":
                            hspArrayData[i] = GUI.Boxf(commandArguments);
                            break;
                        case "line":
                            hspArrayData[i] = GUI.Line(commandArguments);
                            break;
                        case "cls":
                            hspArrayData[i] = GUI.Cls(commandArguments);
                            break;
                        case "color":
                            hspArrayData[i] = GUI.Color(commandArguments);
                            break;
                        case "picload":
                            hspArrayData[i] = GUI.Picload(commandArguments);
                            break;
                        case "getkey":
                            hspArrayData[i] = GUI.Getkey(commandArguments);
                            break;
                        case "stick":
                            hspArrayData[i] = GUI.Stick(commandArguments);
                            break;
                        case "objsize":
                            hspArrayData[i] = GUI.Objsize(commandArguments);
                            break;
                        case "dialog":
                            hspArrayData[i] = GUI.Dialog(commandArguments);
                            break;
                    }
                }

                // 基本文法でもコマンドでもないものは変数
                else if (!BasicList.Contains(firstSentence) && !FunctionList.Contains(firstSentence) &&
                         hspArrayData[i][hspArrayData[i].Length - 1] != ':')
                {
                    // 変数名として正しいか
                    if (VariableNameRule.Contains(firstSentence[0]))
                    {
                        // 変数名ではない
                    }
                    else
                    {
                        // 変数リストに含まれていない場合
                        if (!VariableList.Contains(firstSentence) && !ArrayVariableList.Contains(firstSentence))
                        {
                            // 変数宣言
                            hspArrayData[i] = "Variables[\"" + firstSentence + "\"]" + hspArrayData[i].Substring(spaceIndex);
                            // 変数リストに追加
                            VariableList.Add(firstSentence);
                        }
                    }
                }

                // HSPではmodを￥で表記するので%に置換
                hspArrayData[i] = hspArrayData[i].Replace("\\", "%");

                if (_switchFlag)
                {
                    SwitchList.Add(i);
                }
            }

            // returnの処理
            for (int i = 0; i < hspArrayData.Count; i++)
            {
                if (hspArrayData[i].Equals("return"))
                {
                    hspArrayData[i] = "goto multi;";
                }
            }

            // 文字列をアンエスケープ
            for (int i = 0; i < hspArrayData.Count; i++)
            {
                hspArrayData[i] = StringUnEscape(hspArrayData[i]);
            }

            // 各行の末尾にセミコロンを追加
            for (int i = 0; i < hspArrayData.Count; i++)
            {
                hspArrayData[i] = hspArrayData[i].Trim();
                if (hspArrayData[i].Equals(string.Empty) ||
                    hspArrayData[i].Equals("{") ||
                    hspArrayData[i].Equals("}") ||
                    hspArrayData[i].EndsWith("{") ||
                    hspArrayData[i].EndsWith("}") ||
                    hspArrayData[i].EndsWith(":"))
                    continue;

                if (hspArrayData[i][hspArrayData[i].Length - 1] != '{' &&
                    hspArrayData[i][hspArrayData[i].Length - 1] != ';')
                    hspArrayData[i] += ';';
            }

            // if文の後処理
            foreach (int f in IfFlag)
            {
                hspArrayData[f] += "\n}\n";
            }

            // ラベルのジャンプ台を生成する
            string jump =
                LabelList.Aggregate("\nmulti:\n" + "switch(LabelList[0])\n" + "{",
                    (current, t) =>
                        current +
                        ("case \"" +
                         t +
                         "\":\n" +
                         "LabelList.RemoveAt(LabelList.Count - 1);\n" +
                         "goto " +
                         t +
                         ";\n")) +
                "}\n";

            // C#のコードを完成
            string code = string.Join("\n", Using.Select(i => "using " + i + ";"))
                       + ProgramHeader
                       + string.Join("\n", VariableList.Select(i => "public static dynamic " + i + ";").ToList())
                       + ProgramField
                       + ProgramConstructor
                       + DebugWindowPaint
                       + SubFunction
                       + MainFunction
                       + AddMainFunction
                       + string.Join("\n", AddFunction)
                       + string.Join("\n", hspArrayData)
                       + jump
                       + ProgramFooter;

            return code;
        }

        /// <summary>
        /// コード中の""で括られた文字列をエスケープ
        /// </summary>
        /// <param name="hspArrayString">エスケープ前の文字列</param>
        /// <returns>エスケープ後の文字列</returns>
        public static string StringEscape(string hspArrayString)
        {
            string hspStringData = hspArrayString;
            while (true)
            {
                int preIndex = hspArrayString.IndexOf("\"");
                if (preIndex == -1 || hspArrayString[preIndex - 1] == '\\') break;
                string x = hspArrayString.Substring(preIndex + 1);
                int postIndex = x.IndexOf("\"");
                if (postIndex == -1 || hspArrayString[preIndex + postIndex] == '\\') break;
                string midString = hspArrayString.Substring(preIndex, postIndex + 2);
                StringList.Add(midString);
                hspArrayString = hspArrayString.Replace(midString, string.Empty);
                hspStringData = hspStringData.Replace(midString, "＠＋＠" + (StringList.Count - 1) + "＠ー＠");
            }
            return hspStringData;
        }

        /// <summary>
        /// エスケープした文字列を元に戻す
        /// </summary>
        /// <param name="hspArrayString">エスケープ後の文字列</param>
        /// <returns>エスケープ前の文字列</returns>
        public static string StringUnEscape(string hspArrayString)
        {
            string hspStringData = hspArrayString;
            while (true)
            {
                int preStringIndex = hspArrayString.IndexOf("＠＋＠");
                if (preStringIndex != -1)
                {
                    int postStringIndex = hspArrayString.IndexOf("＠ー＠");
                    if (postStringIndex != -1)
                    {
                        string o = hspArrayString.Substring(preStringIndex, postStringIndex - preStringIndex + 3);
                        int index = int.Parse(o.Replace("＠＋＠", string.Empty).Replace("＠ー＠", string.Empty));
                        hspArrayString = hspArrayString.Replace(o, StringList[index]);
                        hspStringData = hspArrayString;
                    }
                }
                else
                {
                    break;
                }
            }
            return hspStringData;
        }

        /// <summary>
        /// 関数呼び出し
        /// </summary>
        /// <param name="hspArrayString">HSPのコード</param>
        /// <returns>変換後のC#のコード</returns>
        public static string Function(string hspArrayString)
        {
            // 要素単位で分解するために半角スペースでスプリット
            var sentence = hspArrayString.Replace("  ", Space).Split(' ').ToList();
            for (int j = 0; j < sentence.Count; j++)
            {
                // 余計なものは省く
                // 関数は必ず関数名の後に"("が来るはず
                sentence[j] = sentence[j].Trim();
                if (sentence[j] == null ||
                    sentence[j].Equals("\n") ||
                    sentence[j].Equals(Environment.NewLine) ||
                    sentence[j].Equals(string.Empty) ||
                    !FunctionList.Contains(sentence[j]) ||
                    sentence[j + 1][0] != '(')
                    continue;

                // 初めに")"が来る行と, それまでに"("が幾つ出てくるか数える
                int bracketStartCount = 0;
                int k;
                for (k = j + 1; k < sentence.Count; k++)
                {
                    if (sentence[k].Equals("("))
                    {
                        bracketStartCount++;
                    }
                    if (sentence[k].Equals(")"))
                    {
                        break;
                    }
                }

                // "("の数だけ該当する")"をズラす
                for (int l = 0; l < bracketStartCount - 1; l++)
                {
                    bool flag = false;
                    for (int m = k + 1; m < sentence.Count; m++)
                    {
                        if (sentence[m].Equals(")"))
                        {
                            k = m;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        /*============================
                        // カッコの数がオカシイのでエラー
                        =============================*/
                        Error.AlertError("関数呼び出しの括弧の数が足りません");
                    }
                }

                // sentence[j]が関数名
                // sentence[k]が関数の")"
                // sentence[j + 1]～sentence[k]で"("～")"まで
                switch (sentence[j])
                {
                    case "int":
                        Base.Int(sentence, j);
                        break;
                    case "double":
                        Base.Double(sentence, j);
                        break;
                    case "str":
                        Base.Str(sentence, j, k);
                        break;
                    case "abs":
                        Base.Abs(sentence, j, k);
                        break;
                    case "absf":
                        Base.Absf(sentence, j, k);
                        break;
                    case "sin":
                        Base.Sin(sentence, j);
                        break;
                    case "cos":
                        Base.Cos(sentence, j);
                        break;
                    case "tan":
                        Base.Tan(sentence, j);
                        break;
                    case "atan":
                        Base.Atan(sentence, j);
                        break;
                    case "deg2rad":
                        Base.Deg2Rad(sentence, j);
                        break;
                    case "rad2deg":
                        Base.Rad2Deg(sentence, j);
                        break;
                    case "expf":
                        Base.Expf(sentence, j);
                        break;
                    case "logf":
                        Base.Logf(sentence, j);
                        break;
                    case "powf":
                        Base.Powf(sentence, j);
                        break;
                    case "sqrt":
                        Base.Sqrt(sentence, j);
                        break;
                    case "instr":
                        Base.Instr(sentence, j, k);
                        break;
                    case "strlen":
                        Base.Strlen(sentence, j, k);
                        break;
                    case "strmid":
                        Base.Strmid(sentence, j, k);
                        break;
                    case "strtrim":
                        Base.Strtrim(sentence, j, k);
                        break;
                    case "limit":
                        Base.Limit(sentence, j, k);
                        break;
                    case "limitf":
                        Base.Limitf(sentence, j, k);
                        break;
                    case "length":
                        Base.Length(sentence, j, k, 1);
                        break;
                    case "length2":
                        Base.Length(sentence, j, k, 2);
                        break;
                    case "length3":
                        Base.Length(sentence, j, k, 3);
                        break;
                    case "length4":
                        Base.Length(sentence, j, k, 4);
                        break;
                    case "gettime":
                        Base.Gettime(sentence, j);
                        break;
                    case "rnd":
                        Base.Rnd(sentence, j, k);
                        break;
                }
            }
            // 結果を反映
            return string.Join(Space, sentence);
        }

        /// <summary>
        /// マクロ処理
        /// </summary>
        /// <param name="hspArrayString">HSPのコード</param>
        /// <returns>変換後のC#のコード</returns>
        public static string Macro(string hspArrayString)
        {
            // 要素単位で分解するために半角スペースでスプリット
            var sentence = hspArrayString.Replace("  ", Space).Split(' ').ToList();
            for (int i = 0; i < sentence.Count; i++)
            {
                // 余計なものは省く
                sentence[i] = sentence[i].Trim();
                if (sentence[i] == null ||
                    sentence[i].Equals("\n") ||
                    sentence[i].Equals(string.Empty))
                    continue;
                if (MacroList.Contains(sentence[i]))
                {
                    switch (sentence[i])
                    {
                        case "m_pi":
                            Base.M_pi(sentence, i);
                            break;
                        case "and":
                        case "not":
                        case "or":
                        case "xor":
                            Base.BitwiseOperation(sentence, i, sentence[i]);
                            break;
                        case "mousex":
                        case "mousey":
                            GUI.Mouse(sentence, i, sentence[i].Substring(5));
                            break;
                        case "dir_cmdline":
                        case "dir_cur":
                        case "dir_desktop":
                        case "dir_exe":
                        case "dir_mydoc":
                        case "dir_sys":
                        case "dir_win":
                            Base.Directory(sentence, i, sentence[i].Substring(4));
                            break;
                        case "ginfo_mx":
                        case "ginfo_my":
                        case "ginfo_sizex":
                        case "ginfo_sizey":
                        case "ginfo_r":
                        case "ginfo_g":
                        case "ginfo_b":
                        case "ginfo_cx":
                        case "ginfo_cy":
                        case "ginfo_dispx":
                        case "ginfo_dispy":
                        case "ginfo_wx1":
                        case "ginfo_wx2":
                        case "ginfo_wy1":
                        case "ginfo_wy2":
                        case "ginfo_sel":
                            GUI.Ginfo(sentence, i, sentence[i].Substring(6));
                            break;
                        case "hwnd":
                            GUI.Hwnd(sentence, i);
                            break;
                        case "__date__":
                            GUI.__date__(sentence, i);
                            break;
                        case "__time__":
                            GUI.__time__(sentence, i);
                            break;
                        case "msgothic":
                        case "msmincho":
                            GUI.Ms(sentence, i, sentence[i].Substring(2));
                            break;
                        case "font_normal":
                        case "font_bold":
                        case "font_italic":
                        case "font_underline":
                        case "font_strikeout":
                            GUI.Font(sentence, i, sentence[i].Substring(5));
                            break;
                        case "screen_normal":
                        case "screen_hide":
                        case "screen_fixedsize":
                        case "screen_tool":
                        case "screen_frame":
                            GUI.Screen(sentence, i, sentence[i].Substring(7));
                            break;
                    }
                }
            }
            // 結果を反映
            return string.Join(Space, sentence);
        }

        /// <summary>
        /// 配列の変数を定義する
        /// </summary>
        /// <param name="hspArrayString">HSPのコード</param>
        /// <returns>変換後のC#のコード</returns>
        public static string ArrayVariable(string hspArrayString)
        {
            var sentence = hspArrayString.Replace("  ", Space).Split(' ').ToList();
            for (int j = 0; j < sentence.Count; j++)
            {
                sentence[j] = sentence[j].Trim();
                if (!ArrayVariableList.Contains(sentence[j]) ||
                    sentence[j + 1][0] != '(')
                    continue;

                sentence[j + 1] = "[";
                int bracketStartCount = 1;
                for (int k = j + 2; k < sentence.Count; k++)
                {
                    if (sentence[k].Equals("("))
                    {
                        bracketStartCount++;
                    }
                    if (sentence[k].Equals(")"))
                    {
                        bracketStartCount--;
                    }
                    if (bracketStartCount == 0)
                    {
                        sentence[k] = "]";
                        break;
                    }
                }
            }
            return string.Join(Space, sentence);
        }

        /// <summary>
        /// プリプロセッサの処理
        /// </summary>
        /// <param name="hspArrayString">HSPのコード</param>
        /// <returns>変換後のC#のコード</returns>
        public static string Preprocessor(string hspArrayString)
        {
            // 要素単位で分解するために半角スペースでスプリット
            var sentence = hspArrayString.Replace("  ", Space).Split(' ').ToList();
            for (int i = 0; i < sentence.Count; i++)
            {
                // 余計なものは省く
                sentence[i] = sentence[i].Trim();
                if (sentence[i] == null ||
                    sentence[i].Equals("\n") ||
                    sentence[i].Equals(string.Empty))
                    continue;
                if (PreprocessorList.Contains(sentence[i]))
                {
                    switch (sentence[i])
                    {
                        case "#const":
                            Base.Const(sentence, i);
                            break;
                    }
                }
            }
            // 結果を反映
            return string.Join(Space, sentence);
        }

        // 基本文法
        public static List<string> BasicList = new List<string>()
        {
            "if",
            "else",
            "while",
            "wend",
            "for",
            "next",
            "_break",
            "_continue",
            "repeat",
            "loop",
            "switch",
            "swend",
            "swbreak",
            "case",
            "default",
            "goto",
            "gosub",
            "return"
        };

        // プリプロセッサリスト
        public static List<string> PreprocessorList = new List<string>()
        {
            "#const"
        };

        // 文字列を格納するリスト
        public static List<string> StringList = new List<string>();

        // 関数リスト
        public static readonly List<string> FunctionList = new List<string>()
        {
            "int",
            "double",
            "str",
            "abs",
            "absf",
            "sin",
            "cos",
            "tan",
            "atan",
            "deg2rad",
            "rad2deg",
            "expf",
            "logf",
            "powf",
            "sqrt",
            "instr",
            "strlen",
            "strmid",
            "strtrim",
            "limit",
            "limitf",
            "length",
            "length2",
            "length3",
            "length4",
            "gettime",
            "rnd"
        };

        // コマンドリスト
        public static readonly List<string> CommandList = new List<string>()
        {
            "print",
            "mes",
            "exist",
            "delete",
            "bcopy",
            "mkdir",
            "chdir",
            "split",
            "strrep",
            "dim",
            "ddim",
            "end",
            "stop",
            "wait",
            "mci",
            "pos",
            "screen",
            "bgscr",
            "title",
            "redraw",
            "mouse",
            "font",
            "circle",
            "boxf",
            "line",
            "cls",
            "color",
            "picload",
            "getkey",
            "stick",
            "objsize",
            "dialog"
        };

        // 変数リスト
        public static List<string> VariableList = new List<string>()
        {
            "strsize",
            "stat",
            "cnt"
        };

        // 配列変数リスト
        public static List<string> ArrayVariableList = new List<string>();

        // マクロリスト
        public static List<string> MacroList = new List<string>()
        {
            "m_pi",
            "and",
            "not",
            "or",
            "xor",
            "mousex",
            "mousey",
            "dir_cmdline",
            "dir_cur",
            "dir_desktop",
            "dir_exe",
            "dir_mydoc",
            "dir_sys",
            "dir_win",
            "ginfo_mx",
            "ginfo_my",
            "ginfo_sizex",
            "ginfo_sizey",
            "ginfo_r",
            "ginfo_g",
            "ginfo_b",
            "ginfo_cx",
            "ginfo_cy",
            "ginfo_dispx",
            "ginfo_dispy",
            "ginfo_wx1",
            "ginfo_wx2",
            "ginfo_wy1",
            "ginfo_wy2",
            "ginfo_sel",
            "hwnd",
            "__date__",
            "__time__",
            "msgothic",
            "msmincho",
            "font_normal",
            "font_bold",
            "font_italic",
            "font_underline",
            "font_strikeout",
            "screen_normal",
            "screen_hide",
            "screen_fixedsize",
            "screen_tool",
            "screen_frame"
        };

        // ref
        public static List<string> Reference = new List<string>()
        {
            "System.dll",
            "mscorlib.dll",
            "System.IO.dll",
            "System.Linq.dll",
            "System.Core.dll",
            "System.Drawing.dll",
            "Microsoft.CSharp.dll",
            "System.Windows.Forms.dll",
        };

        // using
        public static List<string> Using = new List<string>()
        {
            "System",
            "System.Linq",
            "System.Drawing",
            "System.Diagnostics",
            "System.Windows.Forms",
            "System.Collections.Generic"
        };

        // header
        private static string ProgramHeader = "namespace NameSpace\n{\npublic class Program\n{\n";

        // field
        public static string ProgramField = "public static DateTime pre = DateTime.Now;\n" +
                                            "public static DateTime now;\n" +
                                            "public static TimeSpan span;\n" +
                                            "public static Process[] ps;\n" +
                                            "public static List<string> LabelList = new List<string>();\n" +
                                            "public Form form0;\n" +
                                            "public Form CurrentScreenID;\n" +
                                            "public Form DebugWindow;\n" +
                                            "public Dictionary<string, dynamic> Variables;\n";

        // コンストラクタ
        private static string ProgramConstructor =
            "private static DataGridView view;\n" +
            "public Program(Form _form, Dictionary<string, dynamic> _variables, Form _debugWindow)\n" +
            "{\n" +
            "form0 = _form;\n" +
            "CurrentScreenID = form0;\n" +
            "Variables = _variables;\n" +
            "DebugWindow = _debugWindow;\n" +
            "view = new DataGridView" +
            "{" +
            "    Size = new Size(DebugWindow.Size.Width - 20, DebugWindow.Size.Height - 50)," +
            "    RowHeadersVisible = false," +
            "    AllowUserToAddRows = false," +
            "    ScrollBars = ScrollBars.Vertical," +
            "    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill" +
            "};" +
            "DebugWindow.Show();\n" +
            "}\n\n" +
            "private void CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)" +
            "{" +
            "    e.Cancel = true;" +
            "}\n\n";

        // ペイントイベント
        public static string DebugWindowPaint = "private void dPaint()\n" +
                                                "{\n" +
                                                "DebugWindow.Controls.Clear();\n" +
                                                "view.CellBeginEdit += CellBeginEdit;\n" +
                                                "view.Columns.Clear();\n" +
                                                "view.Rows.Clear();\n" +
                                                "view.Columns.Add(\"n\", \"変数名\");\n" +
                                                "view.Columns.Add(\"v\", \"値\");\n" +
                                                "var keys = Variables.Keys.ToList();\n" +
                                                "var values = Variables.Values.ToList();\n" +
                                                "for(var i=0; i<keys.Count; i++)\n" +
                                                "{\n" +
                                                "view.Rows.Add(keys[i], values[i]);\n" +
                                                "}\n" +
                                                "view.CellBeginEdit += CellBeginEdit;\n" +
                                                "DebugWindow.Controls.Add(view);\n" +
                                                "}\n";

        // Main関数以外の関数の定義
        public static string SubFunction = string.Empty;

        // Main関数の定義
        private static string MainFunction = "\n";

        // ウィンドウを動かすためのコードの追加
        private static readonly string AddMainFunction = string.Empty;

        // Main関数とSub関数以外で必要な関数
        public static List<string> AddFunction = new List<string>()
        {
            "public void initScreen(Form form)\n{\n" +
            "form.ClientSize = new Size(640, 480);\n" +
            "form.Text = \"hsp.cs\";\n" +
            "form.BackColor = Color.FromArgb(255, 255, 255);\n" +
            "form.MaximizeBox = false;\n" +
            "form.FormBorderStyle = FormBorderStyle.FixedSingle;\n" +
            "form.Paint += Paint;\n}\n\n",

            "public void Paint(object sender, PaintEventArgs e)\n{\n" +
            "var FontSize = 14;\n"+
            "var CurrentPosX = 0;\n" +
            "var CurrentPosY = 0;\n" +
            "Graphics g = e.Graphics;\n" +
            "Brush brush = new SolidBrush(Color.FromArgb(0, 0, 0));\n" +
            "Pen pen = new Pen(Color.FromArgb(0, 0, 0));\n" +
            "Font font = new Font(\"FixedSys\", FontSize);\n" +
            "try\n{\n"
        };

        // footer
        public static string ProgramFooter = "\n}\n" +
                                            "catch(Exception)\n" +
                                            "{\n" +
                                            "}\n" +
                                            "dPaint();\n" +
                                            "}\n" +
                                            "}\n" +
                                            "}\n\n" +
                                            "class Test\n" +
                                            "{\n" +
                                            "static void Main()\n" +
                                            "{\n" +
                                            "}\n" +
                                            "}";

        // if文の末尾に"}"を付けるためのフラグ
        public static List<int> IfFlag = new List<int>();

        // コメントをエスケープするためのフラグ
        public static bool CommentFlag = false;

        // switch文の中にいるかどうか
        private static bool _switchFlag = false;

        // switch文の行数を入れるためのリスト
        private static readonly List<int> SwitchList = new List<int>();

        // 1つ目のcase文
        private static bool _firstCase = true;

        // 変数名の先頭として存在してはいけない文字
        public static List<char> VariableNameRule =
            "0123456789!\"#$%&'()-^\\=~|@[`{;:]+*},./<>?".ToCharArray().ToList();

        public static List<Form> Window = new List<Form>();

        /// <summary>
        /// ローカル変数名を作成する関数
        /// GUIDを生成し, 変数名の末尾に追加する
        /// </summary>
        /// <param name="variableName">変数名</param>
        /// <returns>オリジナルの変数名</returns>
        public static string __LocalName(string variableName)
        {
            return variableName + "_" + Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// usingのリストに存在するか
        /// 存在しない場合は追加する
        /// </summary>
        /// <param name="usingName">usingする名前空間</param>
        public static void UsingCheck(string usingName)
        {
            if (!Using.Contains(usingName))
            {
                Using.Add(usingName);
            }
        }

        /// <summary>
        /// referenceのリストに存在するか
        /// 存在しない場合は追加する
        /// </summary>
        /// <param name="refName">参照するDLLのパス</param>
        public static void ReferenceCheck(string refName)
        {
            if (!Reference.Contains(refName))
            {
                Reference.Add(refName);
            }
        }

        public const string Space = " ";
    }
}
