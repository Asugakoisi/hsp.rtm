using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace hsp.rtm
{
    public class GUI : Form
    {
        public static bool BufferFlag;

        public static string Print(string strings)
        {
            strings = Analyzer.StringUnEscape(strings);
            if (strings == string.Empty)
            {
                return "g.DrawString(\"\", font, brush, CurrentPosX, CurrentPosY);\n" +
                       "CurrentPosY += FontSize * 2";
            }
            var str = strings.Split('+').Select(i => i.Trim()).ToList();
            for (int i = 0; i < str.Count; i++)
            {
                if (str[i].Contains("\"") || str[i].Contains("'"))
                {
                    // これは文字か文字列
                }
                else
                {
                    // それ以外は数値か変数
                    double d;
                    if (double.TryParse(str[i], out d))
                    {
                        // TryParse出来た場合は数値
                    }
                    else
                    {
                        // それ以外は変数
                        // 変数はManager.Variablesを参照するように変更
                        if (Analyzer.ArrayVariableList.Contains(str[i]))
                        {
                            // 配列の場合
                            str[i] = str[i] + ".ToString()";
                        }
                    }
                }
            }

            return "g.DrawString((" + string.Join(" + ", str) + ").ToString(), font, brush, CurrentPosX, CurrentPosY);\n" +
                   "CurrentPosY += FontSize*2";
        }

        public static string Pos(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 2);
            string[] p = Analyzer.defaultArgs(strings, "CurrentPosX", "CurrentPosY");

            return "CurrentPosX = " + p[0] + ";\n" + "CurrentPosY = " + p[1];
        }

        public static string Wait(string strings)
        {
            Analyzer.UsingCheck("System.Threading");

            return "Thread.Sleep(" + strings + " * 10);\n" +
                   "Application.DoEvents()";
        }

        public static string Mci(string strings)
        {
            Analyzer.UsingCheck("System.Runtime.InteropServices");
            Analyzer.UsingCheck("System.Text");

            if (!Analyzer.ProgramField.Contains("private static extern int mciSendString(string command, " +
                                                    "StringBuilder buffer, int bufferSize, IntPtr hwndCallback);\n"))
            {
                Analyzer.ProgramField += "[DllImport(\"winmm.dll\")]\n" +
                                         "private static extern int mciSendString(string command, " +
                                         "StringBuilder buffer, int bufferSize, IntPtr hwndCallback);\n";
            }
            return "mciSendString(" + strings + ", null, 0, IntPtr.Zero);";
        }

        public static string Screen(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 3);
            string[] p = Analyzer.defaultArgs(strings, "0", "640", "480");

            if (!Analyzer.AddFunction[0].Contains("public void screen"))
            {
                Analyzer.AddFunction[0] += "public void screen(Form form, int width, int height)\n{\n" +
                                          "form.ClientSize = new Size(width, height);\n}\n\n";
            }

            return "screen(form" + p[0] + ", " + p[1] + ", " + p[2] + ")";
        }

        public static string Bgscr(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 3);
            string[] p = Analyzer.defaultArgs(strings, "0", "640", "480");

            if (!Analyzer.AddFunction[0].Contains("public void screen"))
            {
                Analyzer.AddFunction[0] += "public void screen(Form form, int width, int height)\n{\n" +
                                          "form.ClientSize = new Size(width, height);\n}\n\n";
            }

            return "screen(form" + p[0] + ", " + p[1] + ", " + p[2] + ");\n" +
                   "CurrentScreenID.FormBorderStyle = FormBorderStyle.None;";
        }

        public static new string Width(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 4);
            string[] p = Analyzer.defaultArgs(strings, "-1", "-1", "-1", "-1");

            p[0] = int.Parse(p[0]) >= 0 ? p[0] : "CurrentScreenID.Bounds.Width";
            p[1] = int.Parse(p[1]) >= 0 ? p[1] : "CurrentScreenID.Bounds.Height";
            p[2] = int.Parse(p[2]) >= 0 ? p[2] : "CurrentScreenID.Bounds.X";
            p[3] = int.Parse(p[3]) >= 0 ? p[3] : "CurrentScreenID.Bounds.Y";

            return "CurrentScreenID.SetDesktopBounds(" + p[2] + ", " + p[3] + ", " + p[0] + ", " + p[1] + ");";
        }

        public static string Title(string strings)
        {
            if (!Analyzer.AddFunction[0].Contains("public void Title"))
            {
                Analyzer.AddFunction[0] += "public void Title(Form form, string strings)\n{\n" +
                                          "form.Text = strings;\n}\n\n";
            }

            return "Title(CurrentScreenID, " + strings + ")";
        }

        public static string Redraw(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 1);
            string[] p = Analyzer.defaultArgs(strings, "1");

            if (p.Count() == 1)
            {
                switch (p[0])
                {
                    case "0":
                        BufferFlag = true;
                        return "BufferedGraphicsContext bgc = BufferedGraphicsManager.Current;\n" +
                               "BufferedGraphics bgr = bgc.Allocate(CurrentScreenID.CreateGraphics(), CurrentScreenID.DisplayRectangle)";
                    case "1":
                        BufferFlag = false;
                        return "bgr.Render()";
                    case "2":
                        return string.Empty;
                    case "3":
                        return string.Empty;
                    default:
                        Error.AlertError("エラーが発生しました");
                        return null;
                }
            }
            Error.AlertError("エラーが発生しました");
            return null;
        }

        public static string Mouse(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 2);
            string[] p = Analyzer.defaultArgs(strings, "CurrentPosX", "CurrentPosY");

            return "Cursor.Position = new Point(" + p[0] + ", " + p[1] + ")";
        }

        public new static string Font(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 3);
            string[] p = Analyzer.defaultArgs(strings, "12", "0");

            return "FontSize = " + p[1] + ";\n" +
                    "font = new Font(\"" + p[0] + "\", FontSize, " + p[2] + ")";
        }

        public static string Circle(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 5);
            string[] p = Analyzer.defaultArgs("0", "0", "", "", "1");

            /*if (int.Parse(p[0].ToString()) > int.Parse(p[2].ToString()))
            {
                var temp = p[0];
                p[0] = p[2];
                p[2] = temp;
            }
            if (int.Parse(p[1].ToString()) > int.Parse(p[3].ToString()))
            {
                var temp = p[1];
                p[1] = p[3];
                p[3] = temp;
            }*/

            string str = BufferFlag ? "bgr.Graphics." : "g.";

            if (p[4].Equals("0"))
            {
                return str + "DrawEllipse(pen, " + p[0] + ", " + p[1] + ", " +
                        p[2] + " - " + p[0] + ", " + p[3] + " - " + p[1] + ")";
            }
            return str + "FillEllipse(brush, " + p[0] + ", " + p[1] + ", " +
                    p[2] + " - " + p[0] + ", " + p[3] + " - " + p[1] + ")";
        }

        public static string Boxf(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 4);
            string[] p = Analyzer.defaultArgs("0", "0", "CurrentScreenID.Width", "CurrentScreenID.Height");

            string str = BufferFlag ? "bgr.Graphics." : "g.";

            /*if (int.Parse(p[0].ToString()) > int.Parse(p[2].ToString()))
            {
                var temp = p[0];
                p[0] = p[2];
                p[2] = temp;
            }
            if (int.Parse(p[1].ToString()) > int.Parse(p[3].ToString()))
            {
                var temp = p[1];
                p[1] = p[3];
                p[3] = temp;
            }左上と右下じゃなくても動かすための*/


            return str + "FillRectangle(brush, " + p[0] + ", " + p[1] + ", " +
                    p[2] + " - " + p[0] + ", " + p[3] + " - " + p[1] + ")";
        }

        public static string Line(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 4);
            string[] p = Analyzer.defaultArgs("", "", "CurrentPosX", "CurrentPosY");

            string str = BufferFlag ? "bgr.Graphics." : "g.";

            return str + "DrawLine(pen, " + p[2] + ", " + p[3] + ", " + p[0] + ", " + p[1] + ");\n" +
                    "CurrentPosX = " + p[0] + ";\nCurrentPosY = " + p[1];
        }
        
        public static string Cls(string strings)
        {
            string str = BufferFlag ? "bgr.Graphics." : "g.";
            switch (strings)
            {
                case "0":
                    return str + "FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255)), 0, 0, CurrentScreenID.Width, CurrentScreenID.Height)";
                case "1":
                    return str + "FillRectangle(new SolidBrush(Color.FromArgb(192, 192, 192)), 0, 0, CurrentScreenID.Width, CurrentScreenID.Height)";
                case "2":
                    return str + "FillRectangle(new SolidBrush(Color.FromArgb(128, 128, 128)), 0, 0, CurrentScreenID.Width, CurrentScreenID.Height)";
                case "3":
                    return str + "FillRectangle(new SolidBrush(Color.FromArgb(64, 64, 64)), 0, 0, CurrentScreenID.Width, CurrentScreenID.Height)";
                case "4":
                    return str + "FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 0)), 0, 0, CurrentScreenID.Width, CurrentScreenID.Height)";
            }
            Error.AlertError("エラーが発生しました");
            return null;
        }

        public static string Color(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 3);
            string[] p = Analyzer.defaultArgs("0", "0", "0");

            return "brush = new SolidBrush(Color.FromArgb(" + p[0] + ", " + p[1] + ", " + p[2] + "));\n" +
                    "pen = new Pen(Color.FromArgb(" + p[0] + ", " + p[1] + ", " + p[2] + "))";
        }

        public static string Hsvcolor(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 3);
            string[] p = Analyzer.defaultArgs("0", "0", "0");

            float h = float.Parse(p[0]) % 32 / 32;
            int max = int.Parse(p[2]);
            int min = (int)System.Math.Round((1.0 - (float.Parse(p[1]) / 255.0)) * max);

            switch (int.Parse(p[0]) / 32)
            {
                case 0:
                    p[0] = max.ToString();
                    p[1] = System.Math.Round((h * (max - min) + min)).ToString();
                    p[2] = min.ToString();
                    break;
                case 1:
                    p[0] = System.Math.Round((h * (max - min) + min)).ToString();
                    p[1] = max.ToString();
                    p[2] = min.ToString();
                    break;
                case 2:
                    p[0] = min.ToString();
                    p[1] = max.ToString();
                    p[2] = System.Math.Round((h * (max - min) + min)).ToString();
                    break;
                case 3:
                    p[0] = min.ToString();
                    p[1] = System.Math.Round((h * (max - min) + min)).ToString();
                    p[2] = max.ToString();
                    break;
                case 4:
                    p[0] = System.Math.Round((h * (max - min) + min)).ToString();
                    p[1] = min.ToString();
                    p[2] = max.ToString();
                    break;
                case 5:
                    p[0] = max.ToString();
                    p[1] = min.ToString();
                    p[0] = System.Math.Round((h * (max - min) + min)).ToString();
                    break;
            }

            return "brush = new SolidBrush(Color.FromArgb(" + p[0] + ", " + p[1] + ", " + p[2] + "));\n" +
                    "pen = new Pen(Color.FromArgb(" + p[0] + ", " + p[1] + ", " + p[2] + "))";
        }

        public static string Picload(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 2);
            string[] p = Analyzer.defaultArgs("", "0");

            string str = BufferFlag ? "bgr.Graphics." : "g.";

            if (!Analyzer.AddFunction[0].Contains("public void screen"))
            {
                Analyzer.AddFunction[0] += "public void screen(Form form, int width, int height)\n{\n" +
                                          "form.ClientSize = new Size(width, height);\n}\n\n";
            }

            if (p[1] == "0")
            {
                return "Image img = Image.FromFile(" + p[0] + ");\n" +
                        "screen(CurrentScreenID, img.Width, img.Height);\n" +
                        str + "DrawImage(img, 0, 0, img.Width, img.Height)";
            }
            if (p[1] == "1")
            {
                return "Image img = Image.FromFile(" + p[0] + ");\n" +
                        str + "DrawImage(img, 0, 0, img.Width, img.Height)";
            }
            if (p[1] == "2")
            {
                return string.Empty; ////
            }

            Error.AlertError("エラーが発生しました");
            return null;
        }

        public static string Getkey(string strings)
        {
            Analyzer.UsingCheck("System.Runtime.InteropServices");

            if (!Analyzer.ProgramField.Contains("private static extern ushort GetAsyncKeyState(int vKey);\n"))
            {
                Analyzer.ProgramField += "[DllImport(\"user32.dll\")]\n" +
                                         "private static extern ushort GetAsyncKeyState(int vKey);\n";
            }

            strings = Analyzer.completeArgsNum(strings, 2);
            string[] p = Analyzer.defaultArgs("", "1");

            bool notExistVarialbe = false;

            // 変数名として正しいか
            if (Analyzer.VariableNameRule.Contains(p[0][0]))
            {
                // 変数名ではない
            }
            else
            {
                // 変数リストに含まれていない場合
                if (!Analyzer.VariableList.Contains(p[0]))
                {
                    // 変数リストに追加
                    Analyzer.VariableList.Add(p[0]);
                    notExistVarialbe = true;
                }
            }

            return notExistVarialbe
                ? "Variables[\"" + p[0] + "\"] = GetAsyncKeyState(" + p[1] + ") >> 15"
                : p[0] + " = GetAsyncKeyState(" + p[1] + ") >> 15";
        }

        public static string Stick(string strings)
        {
            Analyzer.UsingCheck("System.Runtime.InteropServices");

            if (!Analyzer.ProgramField.Contains("public int lastKey = 0;\n"))
            {
                Analyzer.ProgramField += "public int lastKey = 0;\n";
            }                

            if (!Analyzer.ProgramField.Contains("private static extern ushort GetAsyncKeyState(int vKey);\n"))
            {
                Analyzer.ProgramField += "[DllImport(\"user32.dll\")]\n" +
                                         "private static extern ushort GetAsyncKeyState(int vKey);\n";
            }
            if (!Analyzer.ProgramField.Contains("private static extern IntPtr GetActiveWindow();\n"))
            {
                Analyzer.ProgramField += "[DllImport(\"user32.dll\")]\n" +
                                         "private static extern IntPtr GetActiveWindow();\n";
            }

            strings = Analyzer.completeArgsNum(strings, 3);
            string[] p = Analyzer.defaultArgs("", "0", "1");

            // 変数名として正しいか
            if (Analyzer.VariableNameRule.Contains(p[0][0]))
            {
                // 変数名ではない
            }
            else
            {
                // 変数リストに含まれていない場合
                if (!Analyzer.VariableList.Contains(p[0]))
                {
                    // 変数リストに追加
                    Analyzer.VariableList.Add(p[0]);
                }
            }

            if (!Analyzer.AddFunction[0].Contains("public int stick"))
            {
                Analyzer.AddFunction[0] += "public int stick(int nonTriggerKey, bool isActiveWindow)\n{\n" +
                                           "if (isActiveWindow == true)\n{\n" +
                                           "if (GetActiveWindow() != CurrentScreenID.Handle) return 0;\n}\n" +
                                           "int key = 0;\n" +
                                           "int justKey = 0;\n" +
                                           "if (GetAsyncKeyState(37) >> 15 == 1) key |= 1;\n" +
                                           "if (GetAsyncKeyState(38) >> 15 == 1) key |= 2;\n" +
                                           "if (GetAsyncKeyState(39) >> 15 == 1) key |= 4;\n" +
                                           "if (GetAsyncKeyState(40) >> 15 == 1) key |= 8;\n" +
                                           "if (GetAsyncKeyState(32) >> 15 == 1) key |= 16;\n" +
                                           "if (GetAsyncKeyState(13) >> 15 == 1) key |= 32;\n" +
                                           "if (GetAsyncKeyState(17) >> 15 == 1) key |= 64;\n" +
                                           "if (GetAsyncKeyState(27) >> 15 == 1) key |= 128;\n" +
                                           "if (GetAsyncKeyState(1) >> 15 == 1) key |= 256;\n" +
                                           "if (GetAsyncKeyState(2) >> 15 == 1) key |= 512;\n" +
                                           "if (GetAsyncKeyState(9) >> 15 == 1) key |= 1024;\n" +
                                           "justKey = (key ^ lastKey | nonTriggerKey) & key;" +
                                           "lastKey = key;\n" +
                                           "return justKey;\n}\n";
            }

            if (p[2] == "0")
            {
                p[2] = "false";
            }
            if (p[2] == "1")
            {
                p[2] = "true";
            }
            return "Variables[\"" + p[0] + "\"] = stick(" + p[1] + ", " + p[2] + ");";
        }

        public static string Objsize(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 2);
            string[] p = Analyzer.defaultArgs("64", "24", "0");

            Analyzer.ProgramField += "objsizeX = " + p[0] + ";\n" +
                                    "objsizeY = " + p[1] + ";\n" +
                                    "objSpace = " + p[2];
            return "//boxSize(" + p[0] + ", " + p[1] + ")";
        }

        public static string Dialog(string strings)
        {
            strings = Analyzer.completeArgsNum(strings, 3);
            string[] p = Analyzer.defaultArgs("", "0", "");

            switch (p[1])
            {
                case "0":
                    return "MessageBox.Show(" + p[0] + ".ToString(), " + p[2] + ".ToString(), " +
                           "MessageBoxButtons.OK, MessageBoxIcon.Information)";
                case "1":
                    return "MessageBox.Show(" + p[0] + ".ToString(), " + p[2] + ".ToString(), " +
                           "MessageBoxButtons.OK, MessageBoxIcon.Warning);\n";
                case "2":
                    return "MessageBox.Show(" + p[0] + ".ToString(), " + p[2] + ".ToString(), " +
                           "MessageBoxButtons.YesNo, MessageBoxIcon.Information)";
                case "3":
                    return "MessageBox.Show(" + p[0] + ".ToString(), " + p[2] + ".ToString(), " +
                           "MessageBoxButtons.YesNo, MessageBoxIcon.Warning)";
                case "16":
                case "17":
                case "32":
                case "33":
                    return string.Empty;
                default:
                    Error.AlertError("エラーが発生しました");
                    return null;
            }
        }

        public static void Mouse(List<string> sentence, int i, string str)
        {
            switch (str)
            {
                case "x":
                    sentence[i] = "CurrentScreenID.PointToClient(Cursor.Position).X";
                    break;
                case "y":
                    sentence[i] = "CurrentScreenID.PointToClient(Cursor.Position).Y";
                    break;
            }
        }

        public static void Ginfo(List<string> sentence, int i, string str)
        {
            switch (str)
            {
                case "mx":
                    sentence[i] = "Cursor.Position.X";
                    break;
                case "my":
                    sentence[i] = "Cursor.Position.Y";
                    break;
                case "sizex":
                    sentence[i] = "CurrentScreenID.Width";
                    break;
                case "sizey":
                    sentence[i] = "CurrentScreenID.Height";
                    break;
                case "r":
                    sentence[i] = "pen.Color.R";
                    break;
                case "g":
                    sentence[i] = "pen.Color.G";
                    break;
                case "b":
                    sentence[i] = "pen.Color.B";
                    break;
                case "cx":
                    sentence[i] = "CurrentPosX";
                    break;
                case "cy":
                    sentence[i] = "CurrentPosY";
                    break;
                case "dispx":
                    sentence[i] = "Screen.PrimaryScreen.Bounds.Width";
                    break;
                case "dispy":
                    sentence[i] = "Screen.PrimaryScreen.Bounds.Height";
                    break;
                case "wx1":
                    sentence[i] = "CurrentScreenID.Left";
                    break;
                case "wx2":
                    sentence[i] = "CurrentScreenID.Right";
                    break;
                case "wy1":
                    sentence[i] = "CurrentScreenID.Top";
                    break;
                case "wy2":
                    sentence[i] = "CurrentScreenID.Bottom";
                    break;
                case "sel":
                    sentence[i] = "CurrentScreenID";
                    break;
            }
        }

        public static void Hwnd(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.Handle";
        }

        public static void __date__(List<string> sentence, int i)
        {
            sentence[i] = "DateTime.Now.ToString(\"d\")";
        }

        public static void __time__(List<string> sentence, int i)
        {
            sentence[i] = "DateTime.Now.ToString(\"T\")";
        }

        public static void Ms(List<string> sentence, int i, string str)
        {
            switch (str)
            {
                case "gothic":
                    sentence[i] = "ＭＳ ゴシック";
                    break;
                case "mincho":
                    sentence[i] = "ＭＳ 明朝";
                    break;
            }
        }

        public new static void Font(List<string> sentence, int i, string str)
        {
            switch (str)
            {
                case "normal":
                    sentence[i] = "FontStyle.Regular";
                    break;
                case "bold":
                    sentence[i] = "FontStyle.Bold";
                    break;
                case "italic":
                    sentence[i] = "FontStyle.Italic";
                    break;
                case "underline":
                    sentence[i] = "FontStyle.Underline";
                    break;
                case "strikeout":
                    sentence[i] = "FontStyle.Strikeout";
                    break;
            }
        }

        public static void Screen(List<string> sentence, int i, string str)
        {
            switch (str)
            {
                case "normal":
                    sentence[i] = "CurrentScreenID.WindowState = FormWindowState.Normal";
                    break;
                case "hide":
                    sentence[i] = "CurrentScreenID.WindowState = FormWindowState.Minimized;\n" +
                                  "ShowInTaskbar = false";
                    break;
                case "fixedsize":
                    sentence[i] = "CurrentScreenID.FormBorderStyle = FormBorderStyle.FixedSingle";
                    break;
                case "tool":
                    sentence[i] = "CurrentScreenID.FormBorderStyle = FormBorderStyle.FixedToolWindow";
                    break;
                case "frame":
                    sentence[i] = "CurrentScreenID.FormBorderStyle = FormBorderStyle.Fixed3D";
                    break;
            }
        }
    }
}
