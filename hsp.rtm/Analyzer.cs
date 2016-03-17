using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace hsp.rtm
{
    class Analyzer
    {
        public static string GenerateCode(List<string> hspArrayData)
        {
            for (var i = 0; i < hspArrayData.Count; i++)
            {
                if (hspArrayData[i].Equals("{") || hspArrayData[i].Equals("}")) continue;

                //データの整形
                //前後の空白文字を削除
                hspArrayData[i] = hspArrayData[i].Trim();

                if (hspArrayData[i].Equals(""))
                {
                    continue;
                }

                //直前にエスケープのないダブルクオーテーションが存在した場合
                //文字列部分をStringListに格納し
                //その部分を＠＋＠StringListのindex＠ー＠で置換する
                //Example: hoge = "fu" + "ga"
                //         hoge = ＠＋＠0＠ー＠ + ＠＋＠1＠ー＠
                //StringListには"fu"と"ga"が格納される
                //このときダブルクオーテーションも含まれているので注意
                hspArrayData[i] = StringEscape(hspArrayData[i]);

                //コメントを取り除く
                var commentOutIndex = hspArrayData[i].IndexOf("//", StringComparison.Ordinal);
                if (commentOutIndex > -1)
                {
                    hspArrayData[i] = hspArrayData[i].Substring(0, commentOutIndex).Trim();
                }

                //スラッシュとアスタリスクによるコメントアウトをエスケープする
                commentOutIndex = hspArrayData[i].IndexOf("/*", StringComparison.Ordinal);
                if (commentOutIndex > -1)
                {
                    hspArrayData[i] = hspArrayData[i].Substring(0, commentOutIndex).Trim();
                    CommentFlag = true;
                }
                if (CommentFlag)
                {
                    commentOutIndex = hspArrayData[i].IndexOf("*/", StringComparison.Ordinal);
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

                if (hspArrayData[i].Equals(""))
                {
                    continue;
                }

                hspArrayData[i] = hspArrayData[i]
                    //データ中の空白文字を全て半角スペースに変換
                    .Replace('　', ' ')
                    .Replace('\t', ' ')
                    //関数部分で正確にスプリットするために()の直前後に半角スペースを追加
                    .Replace("(", " ( ")
                    .Replace(")", " ) ")
                    .Replace("=", " = ")
                    .Replace("+", " + ")
                    .Replace("-", " - ")
                    .Replace("*", " * ")
                    .Replace("/", " / ")
                    //連続する演算子を修正
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

                //ラベルの定義
                if (hspArrayData[i][0] == '*')
                {
                    hspArrayData[i] = hspArrayData[i].Substring(1).Trim() + ":";
                }

                //１番最初のsentenceを抜き出す
                var spaceIndex = hspArrayData[i].IndexOf(" ", StringComparison.OrdinalIgnoreCase);
                var firstSentence = spaceIndex < 0
                    ? hspArrayData[i].Trim()
                    : hspArrayData[i].Substring(0, spaceIndex).Trim();

                //配列処理
                hspArrayData[i] = ArrayVariable(hspArrayData[i]);

                //マクロ処理
                hspArrayData[i] = Macro(hspArrayData[i]);

                //関数処理
                hspArrayData[i] = Function(hspArrayData[i]);

                //基本文法の処理
                if (BasicList.Contains(firstSentence))
                {
                    switch (firstSentence)
                    {
                        //if文の処理
                        case "if":
                            //"{}"を使って複数行で書かれている場合
                            //必ず文中に"{"が入っている
                            var bracketIndex = hspArrayData[i].IndexOf("{", StringComparison.Ordinal);
                            if (bracketIndex < 0)
                            {
                                //処理が1行で書かれている場合
                                //ifと条件文の間に"()"を入れる
                                var coronIndex = hspArrayData[i].IndexOf(":", StringComparison.Ordinal);
                                if (coronIndex < 0)
                                {
                                    //条件文の後に処理が書かれていないため無効な文
                                    //エラーとして吐き出すよりも警告として表示したほうが良い？
                                    Console.WriteLine("条件文の後に実行すべき処理が書かれていません");
                                }
                                else
                                {
                                    var tmpString = hspArrayData[i].Substring(coronIndex + 1);
                                    hspArrayData[i] = "if (" +
                                                      hspArrayData[i].Substring("if ".Length,
                                                          coronIndex - "if ".Length - 1) +
                                                      ")\n{";

                                    //":"以降をhspArrayDataにInsertする
                                    var tmpArray = tmpString.Split(':');
                                    var index = i + 1;
                                    if (tmpArray.Length > 0)
                                    {
                                        for (var j = 0; j < tmpArray.Length; j++)
                                        {
                                            hspArrayData.Insert(i + j + 1, tmpArray[j].Trim());
                                            index = i + j + 1;
                                        }
                                    }

                                    //末尾に"}"を付けるためのフラグ
                                    IfFlag.Add(index);
                                }
                            }
                            else
                            {
                                //複数行の処理
                                hspArrayData[i] = "if (" +
                                                  hspArrayData[i].Substring("if ".Length,
                                                      bracketIndex - "if ".Length - 1) + ")\n" +
                                                  hspArrayData[i].Substring(bracketIndex);
                            }

                            //if文の条件における"="の好意的解釈
                            //"="が1つでも"=="として扱う
                            hspArrayData[i] = hspArrayData[i]
                                .Replace(" ", "")
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

                        //elseの処理
                        case "else":
                            hspArrayData[i] = "}\n else \n{";
                            break;

                        //forの処理
                        case "for":
                            var forConditionalSentence =
                                hspArrayData[i].Substring(spaceIndex).Split(',').Select(_ => _.Trim()).ToList();
                            if (forConditionalSentence.Count() != 4)
                            {
                                //要素数がオカシイのでエラー
                                Console.WriteLine("for文の要素数がオカシイです");
                                Console.WriteLine("現在の要素数 = " + forConditionalSentence.Count());
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

                        //breakの処理
                        case "_break":
                            hspArrayData[i] = "break";
                            break;

                        //continueの処理
                        case "_continue":
                            hspArrayData[i] = "continue";
                            break;

                        //whileの処理
                        case "while":
                            var whileConditionalSentence = hspArrayData[i].Substring(spaceIndex).Trim();
                            hspArrayData[i] = "while (" + whileConditionalSentence + ")\n{";
                            break;

                        //repeatの処理
                        case "repeat":
                            var repeatConditionalSentence = hspArrayData[i].Substring(spaceIndex).Trim();
                            int counter;
                            if (int.TryParse(repeatConditionalSentence, out counter))
                            {
                                hspArrayData[i] = "for (cnt=0; cnt<" + counter + "; cnt++)\n{";

                                //システム変数cntが定義されていない場合は定義
                                if (!VariableDefinition.Contains("int cnt = 0;"))
                                {
                                    VariableDefinition += "int cnt = 0;\n";
                                }
                            }
                            else
                            {
                                //repeatに渡されている値が数字ではないのでエラー
                                Console.WriteLine("repeatに渡されている値(" + repeatConditionalSentence + ")は数字ではありません");
                            }
                            break;

                        //switchの処理
                        case "switch":
                            var switchSpaceIndex = hspArrayData[i].IndexOf(" ", StringComparison.Ordinal);
                            if (switchSpaceIndex < 0)
                            {
                                //switchの条件文としてオカシイのでエラー
                                Console.WriteLine("switch文の条件文が書かれていません");
                            }
                            else
                            {
                                var switchConditionalSentence = hspArrayData[i].Substring(switchSpaceIndex).Trim();
                                var switchTmpString = __LocalName("switchTmpString");
                                hspArrayData[i] = "string " + switchTmpString + " = " + switchConditionalSentence +
                                                  ".ToString();\n" + "switch (" + switchTmpString + ") \n{";
                            }
                            _switchFlag = true;
                            break;
                        case "swend":
                            //1つ目の要素はswitch文なので取り除く
                            SwitchList.RemoveAt(0);

                            for (var j = 0; j < SwitchList.Count; j++)
                            {
                                if (hspArrayData[SwitchList[j]].Equals("default:"))
                                {
                                    if (!hspArrayData[SwitchList[j] - 1].Contains("break;"))
                                    {
                                        var defaultString = "";
                                        for (var k = SwitchList[j] + 1; k < SwitchList[SwitchList.Count - 1]; k++)
                                        {
                                            defaultString += hspArrayData[k] + "\nbreak;";
                                        }
                                        hspArrayData[SwitchList[j] - 1] += defaultString;
                                    }
                                }

                                var endIndex = hspArrayData[SwitchList[j]].IndexOf(" ", StringComparison.Ordinal);
                                if (endIndex < 0)
                                {
                                    //
                                }
                                else
                                {
                                    var caseName = hspArrayData[SwitchList[j]].Substring(endIndex).Trim();
                                    var first = hspArrayData[SwitchList[j]].Substring(0, endIndex).Trim();
                                    if (_firstCase)
                                    {
                                        _firstCase = false;
                                    }
                                    else if (first.Equals("case"))
                                    {
                                        if (!hspArrayData[SwitchList[j] - 1].Contains("break;"))
                                        {
                                            hspArrayData[SwitchList[j] - 1] += "\ngoto case " +
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
                            var caseSpaceIndex = hspArrayData[i].IndexOf(" ", StringComparison.Ordinal);
                            if (caseSpaceIndex < 0)
                            {
                                //case文の値が不正なのでエラー
                                Console.WriteLine("case文の値が不正です");
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

                        //色々な後処理
                        case "next":
                        case "wend":
                        case "loop":
                            hspArrayData[i] = "}";
                            break;

                        //gotoの処理
                        case "goto":
                            hspArrayData[i] = hspArrayData[i].Replace("*", "");
                            break;

                        case "gosub":
                            hspArrayData[i] = "goto " + hspArrayData[i].Substring("gosub".Length).Replace("*", "");
                            var label = __LocalName("label");
                            hspArrayData.Insert(i + 1, label + ":");
                            ReturnLabelList.Add(label);
                            break;
                    }
                }

                //コマンド処理
                //sentenceがコマンドかどうか
                else if (CommandList.Contains(firstSentence))
                {
                    //コマンドの引数部分を取得
                    var commandArguments = "";
                    if (spaceIndex > -1)
                    {
                        commandArguments = hspArrayData[i].Substring(spaceIndex + 1);
                    }

                    switch (firstSentence)
                    {
                        case "print":
                        case "mes":
                            //hspArrayData[i] = HSP.Print(commandArguments);
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
                        case "color":
                            hspArrayData[i] = GUI.Color(commandArguments);
                            break;
                        case "picload":
                            hspArrayData[i] = GUI.Picload(commandArguments);
                            break;
                        case "getkey":
                            hspArrayData[i] = GUI.Getkey(commandArguments);
                            break;
                        case "objsize":
                            hspArrayData[i] = GUI.Objsize(commandArguments);
                            break;
                        case "dialog":
                            hspArrayData[i] = GUI.Dialog(commandArguments);
                            break;
                    }

                    //if文の後処理
                    if (IfFlag.Count > 0)
                    {
                        foreach (var t in IfFlag.Where(t => t == i))
                        {
                            hspArrayData[i] += "\n}";
                        }
                    }
                }

                //基本文法でもコマンドでもないものは変数
                else if (!BasicList.Contains(firstSentence) && !FunctionList.Contains(firstSentence) &&
                         hspArrayData[i][hspArrayData[i].Length - 1] != ':')
                {
                    //変数名として正しいか
                    if (VariableNameRule.Contains(firstSentence[0]))
                    {
                        //変数名ではない
                    }
                    else
                    {
                        //変数リストに含まれていない場合
                        if (!VariableList.Contains(firstSentence) && !ArrayVariableList.Contains(firstSentence))
                        {
                            //変数宣言
                            hspArrayData[i] = "dynamic " + hspArrayData[i];
                            //変数リストに追加
                            VariableList.Add(firstSentence);
                        }
                    }
                }

                //HSPではmodを￥で表記するので%に置換
                hspArrayData[i] = hspArrayData[i].Replace("\\", "%");

                if (_switchFlag)
                {
                    SwitchList.Add(i);
                }
            }

            //returnの処理
            for (var i = 0; i < hspArrayData.Count; i++)
            {
                if (hspArrayData[i].Equals("return"))
                {
                    var returnLabel = ReturnLabelList[ReturnLabelList.Count - 1];
                    ReturnLabelList.RemoveAt(ReturnLabelList.Count - 1);
                    hspArrayData[i] = "goto " + returnLabel;
                }
            }

            //文字列をアンエスケープ
            for (var i = 0; i < hspArrayData.Count; i++)
            {
                hspArrayData[i] = StringUnEscape(hspArrayData[i]);
            }

            //各行の末尾にセミコロンを追加
            for (var i = 0; i < hspArrayData.Count; i++)
            {
                if (hspArrayData[i].Equals("") || hspArrayData[i].Equals("{") || hspArrayData[i].Equals("}") ||
                    hspArrayData[i][hspArrayData[i].Length - 1].Equals(':')) continue;

                if (hspArrayData[i][hspArrayData[i].Length - 1] != '{' &&
                    hspArrayData[i][hspArrayData[i].Length - 1] != ';')
                {
                    hspArrayData[i] += ';';
                }
            }

            //C#のコードを完成
            var code = Using + ProgramHeader + ProgramField + "\n" + SubFunction + "\n" + MainFunction + VariableDefinition +
                       AddMainFunction + AddFunction[0] + AddFunction[1] + string.Join("\n", hspArrayData) +
                       "\n}\ncatch(Exception)\n{\n}\n}\n}\n" + ProgramFooter;

            return code;
        }

        /// <summary>
        ///     コード中の""で括られた文字列をエスケープ
        /// </summary>
        /// <param name="hspArrayString"></param>
        /// <returns></returns>
        public static string StringEscape(string hspArrayString)
        {
            var hspStringData = hspArrayString;
            while (true)
            {
                var preIndex = hspArrayString.IndexOf("\"", StringComparison.OrdinalIgnoreCase);
                if (preIndex == -1 || hspArrayString[preIndex - 1] == '\\') break;
                var x = hspArrayString.Substring(preIndex + 1);
                var postIndex = x.IndexOf("\"", StringComparison.OrdinalIgnoreCase);
                if (postIndex == -1 || hspArrayString[preIndex + postIndex] == '\\') break;
                var midString = hspArrayString.Substring(preIndex, postIndex + 2);
                StringList.Add(midString);
                hspArrayString = hspArrayString.Replace(midString, "");
                hspStringData = hspStringData.Replace(midString, "＠＋＠" + (StringList.Count - 1) + "＠ー＠");
            }
            return hspStringData;
        }

        /// <summary>
        ///     エスケープした文字列を元に戻す
        /// </summary>
        /// <param name="hspArrayString"></param>
        /// <returns></returns>
        public static string StringUnEscape(string hspArrayString)
        {
            var hspStringData = hspArrayString;
            while (true)
            {
                var preStringIndex = hspArrayString.IndexOf("＠＋＠", StringComparison.OrdinalIgnoreCase);
                if (preStringIndex != -1)
                {
                    var postStringIndex = hspArrayString.IndexOf("＠ー＠", StringComparison.OrdinalIgnoreCase);
                    if (postStringIndex != -1)
                    {
                        var o = hspArrayString.Substring(preStringIndex, postStringIndex - preStringIndex + 3);
                        var index = int.Parse(o.Replace("＠＋＠", "").Replace("＠ー＠", ""));
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
        ///     関数呼び出し
        /// </summary>
        /// <param name="hspArrayString"></param>
        /// <returns></returns>
        public static string Function(string hspArrayString)
        {
            //要素単位で分解するために半角スペースでスプリット
            var sentence = hspArrayString.Replace("  ", " ").Split(' ').ToList();
            for (var j = 0; j < sentence.Count; j++)
            {
                //余計なものは省く
                //関数は必ず関数名の後に"("が来るはず
                sentence[j] = sentence[j].Trim();
                if (sentence[j] == null ||
                    sentence[j].Equals("\n") ||
                    sentence[j].Equals("") ||
                    !FunctionList.Contains(sentence[j]) ||
                    sentence[j + 1][0] != '(')
                    continue;

                //初めに")"が来る行と, それまでに"("が幾つ出てくるか数える
                var bracketStartCount = 0;
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

                //"("の数だけ該当する")"をズラす
                for (var l = 0; l < bracketStartCount - 1; l++)
                {
                    var flag = false;
                    for (var m = k + 1; m < sentence.Count; m++)
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
                        //カッコの数がオカシイのでエラー
                        =============================*/
                        Console.WriteLine("Error");
                    }
                }

                //sentence[j]が関数名
                //sentence[k]が関数の")"
                //sentence[j + 1]～sentence[k]で"("～")"まで
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
            //結果を反映
            return string.Join(" ", sentence);
        }

        public static string Macro(string hspArrayString)
        {
            //要素単位で分解するために半角スペースでスプリット
            var sentence = hspArrayString.Replace("  ", " ").Split(' ').ToList();
            for (var i = 0; i < sentence.Count; i++)
            {
                //余計なものは省く
                sentence[i] = sentence[i].Trim();
                if (sentence[i] == null ||
                    sentence[i].Equals("\n") ||
                    sentence[i].Equals(""))
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
                            GUI.Mousex(sentence, i);
                            break;
                        case "mousey":
                            GUI.Mousey(sentence, i);
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
                            Base.Ginfo_mx(sentence, i);
                            break;
                        case "ginfo_my":
                            Base.Ginfo_my(sentence, i);
                            break;
                        case "ginfo_sizex":
                            GUI.Ginfo_sizeX(sentence, i);
                            break;
                        case "ginfo_sizey":
                            GUI.Ginfo_sizeY(sentence, i);
                            break;
                        case "ginfo_r":
                            GUI.Ginfo_r(sentence, i);
                            break;
                        case "ginfo_g":
                            GUI.Ginfo_g(sentence, i);
                            break;
                        case "ginfo_b":
                            GUI.Ginfo_b(sentence, i);
                            break;
                        case "ginfo_cx":
                            GUI.Ginfo_cx(sentence, i);
                            break;
                        case "ginfo_cy":
                            GUI.Ginfo_cy(sentence, i);
                            break;
                        case "ginfo_dispx":
                            GUI.Ginfo_dispx(sentence, i);
                            break;
                        case "ginfo_dispy":
                            GUI.Ginfo_dispy(sentence, i);
                            break;
                        case "ginfo_wx1":
                            GUI.Ginfo_wx1(sentence, i);
                            break;
                        case "ginfo_wx2":
                            GUI.Ginfo_wx2(sentence, i);
                            break;
                        case "ginfo_wy1":
                            GUI.Ginfo_wy1(sentence, i);
                            break;
                        case "ginfo_wy2":
                            GUI.Ginfo_wy2(sentence, i);
                            break;
                        case "ginfo_sel":
                            GUI.Ginfo_sel(sentence, i);
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
                            GUI.Msgothic(sentence, i);
                            break;
                        case "msmincho":
                            GUI.Msmincho(sentence, i);
                            break;
                        case "font_normal":
                            GUI.Font_normal(sentence, i);
                            break;
                        case "font_bold":
                            GUI.Font_bold(sentence, i);
                            break;
                        case "font_italic":
                            GUI.Font_italic(sentence, i);
                            break;
                        case "font_underline":
                            GUI.Font_underline(sentence, i);
                            break;
                        case "font_strikeout":
                            GUI.Font_strikeout(sentence, i);
                            break;
                        case "screen_normal":
                            GUI.Screen_normal(sentence, i);
                            break;
                        case "screen_hide":
                            GUI.Screen_hide(sentence, i);
                            break;
                        case "screen_fixedsize":
                            GUI.Screen_fixedsize(sentence, i);
                            break;
                        case "screen_tool":
                            GUI.Screen_tool(sentence, i);
                            break;
                        case "screen_frame":
                            GUI.Screen_frame(sentence, i);
                            break;
                    }
                }
            }
            //結果を反映
            return string.Join(" ", sentence);
        }

        public static string ArrayVariable(string hspArrayString)
        {
            hspArrayString = hspArrayString.Replace("  ", " ");
            var sentence = hspArrayString.Split(' ').ToList();
            for (var j = 0; j < sentence.Count; j++)
            {
                sentence[j] = sentence[j].Trim();
                if (!ArrayVariableList.Contains(sentence[j]) ||
                    sentence[j + 1][0] != '(')
                    continue;

                sentence[j + 1] = "[";
                var bracketStartCount = 1;
                for (var k = j + 2; k < sentence.Count; k++)
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
            return string.Join(" ", sentence);
        }

        //基本文法
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

        //文字列を格納するリスト
        public static List<string> StringList = new List<string>();

        //関数リスト
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

        //コマンドリスト
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
            "color",
            "picload",
            "getkey",
            "objsize",
            "dialog"
        };

        //変数リスト
        public static List<string> VariableList = new List<string>()
        {
            "strsize",
            "stat",
            "cnt"
        };

        //配列変数リスト
        public static List<string> ArrayVariableList = new List<string>();

        //マクロリスト
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

        //using
        public static string Using = "using System;\nusing System.Drawing;\nusing System.Windows.Forms;\n";
        //header
        private const string ProgramHeader = "namespace NameSpace\n{\npublic class Program\n{\n";
        //field
        public static string ProgramField = "public Form form0;\n" +
                                            "public Form CurrentScreenID;\n" +
                                            "public Program(Form _form)\n" +
                                            "{\n" +
                                            "form0 = _form;\n" +
                                            "CurrentScreenID = form0;\n" +
                                            "}\n";
        //Main関数以外の関数の定義
        public static string SubFunction = "";
        //Main関数の定義
        private const string MainFunction = "";
        //システム変数宣言
        public static string VariableDefinition = "";
        //ウィンドウを動かすためのコードの追加
        private const string AddMainFunction = "";
        //Main関数とSub関数以外で必要な関数
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

        //footer
        public const string ProgramFooter = "\n}class Test\n{\nstatic void Main()\n{\n}\n}";

        //if文の末尾に"}"を付けるためのフラグ
        private static readonly List<int> IfFlag = new List<int>();

        //コメントをエスケープするためのフラグ
        public static bool CommentFlag = false;

        //switch文の中にいるかどうか
        private static bool _switchFlag = false;
        //switch文の行数を入れるためのリスト
        private static readonly List<int> SwitchList = new List<int>();
        //1つ目のcase文
        private static bool _firstCase = true;

        //変数名の先頭として存在してはいけない文字
        public static List<char> VariableNameRule =
            "0123456789!\"#$%&'()-^\\=~|@[`{;:]+*},./<>?".ToCharArray().ToList();

        private static readonly List<string> ReturnLabelList = new List<string>();

        public static List<Form> Window = new List<Form>();

        /// <summary>
        /// ローカル変数名を作成する関数
        /// GUIDを生成し, 変数名の末尾に追加する
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static string __LocalName(string variableName)
        {
            return variableName + "_" + Guid.NewGuid().ToString("N");
        }

        public static void UsingCheck(string usingName)
        {
            if (!Using.Contains(usingName))
            {
                Using += usingName + ";\n";
            }
        }
    }
}
