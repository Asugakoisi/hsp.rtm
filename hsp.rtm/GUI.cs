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
                       "CurrentPosY += FontSize";
            }
            var str = strings.Split('+').Select(i => i.Trim()).ToList();
            for (var i = 0; i < str.Count; i++)
            {
                if (str[i].Contains("\"") || str[i].Contains("'"))
                {
                    //これは文字か文字列
                }
                else
                {
                    //それ以外は数値か変数
                    double d;
                    if (double.TryParse(str[i], out d))
                    {
                        //TryParse出来た場合は数値
                    }
                    else
                    {
                        //それ以外は変数
                        //変数はManager.Variablesを参照するように変更
                        str[i] = "Variables[\"" + str[i] + "\"]";
                    }
                }
            }

            return "g.DrawString(" + string.Join(" ", str) + ".ToString(), font, brush, CurrentPosX, CurrentPosY);\n" +
                   "CurrentPosY += FontSize";
        }

        public static string Pos(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

            if (strings.Equals(string.Empty))
            {
                return "CurrentPosX = CurrentPosX" +
                       "CurrentPosY = CurrentPosY";
            }
            if (p.Count() == 2)
            {
                return "CurrentPosX = " + p[0] + ";\n" +
                       "CurrentPosY = " + p[1];
            }
            return "Console.WriteLine(\"error\")";
        }

        public static string Wait(string strings)
        {
            Analyzer.UsingCheck("using System.Threading");

            return "Thread.Sleep(" + strings + " * 10);\n" +
                   "Application.DoEvents()";
        }


        public static string Screen(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

            //Program.Window.Add();

            if (!Analyzer.AddFunction[0].Contains("public void screen"))
            {
                Analyzer.AddFunction[0] += "public void screen(Form form, int width, int height)\n{\n" +
                                          "form.ClientSize = new Size(width, height);\n}\n\n";
            }

            return "screen(form" + p[0] + ", " + p[1] + ", " + p[2] + ")";
        }

        public static string Bgscr(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

            //Program.Window.Add();

            if (!Analyzer.AddFunction[0].Contains("public void screen"))
            {
                Analyzer.AddFunction[0] += "public void screen(Form form, int width, int height)\n{\n" +
                                          "form.ClientSize = new Size(width, height);\n}\n\n";
            }

            return "screen(form" + p[0] + ", " + p[1] + ", " + p[2] + ");\n" +
                   "CurrentScreenID.FormBorderStyle = FormBorderStyle.None;";
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
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }
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
                        return "";
                    case "3":
                        return "";
                    default:
                        return "Console.WriteLine(\"error\")";
                }
            }
            return "Console.WriteLine(\"error\")";
        }

        public static string Mouse(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

            if (!p.Any())
            {
                return "Cursor.Position = new Point(CurrentPosX, CurrentPosY)";
            }
            if (p.Count() == 1)
            {
                return "Cursor.Position = new Point(" + p[0] + ", CurrentPosY)";
            }
            if (p.Count() == 2)
            {
                return "Cursor.Position = new Point(" + p[0] + ", " + p[1] + ")";
            }
            return "Console.WriteLine(\"error\")";
        }

        public new static string Font(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }
            if (p.Count() == 1)
            {
                return "FontSize = 12;\n" +
                       "font = new Font(\"" + p[0] + "\", FontSize)";
            }
            if (p.Count() == 2)
            {
                return "FontSize = " + p[1] + ";\n" +
                       "font = new Font(\"" + p[0] + "\", FontSize)";
            }
            if (p.Count() == 3)
            {
                return "FontSize = " + p[1] + ";\n" +
                       "font = new Font(\"" + p[0] + "\", FontSize, " + p[2] + ")";
            }
            return "Console.WriteLine(\"error\")";
        }

        public static string Circle(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

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
            var str = "";
            if (BufferFlag)
            {
                str = "bgr.Graphics.";
            }
            else
            {
                str = "g.";
            }

            if (p.Count() == 4)
            {
                return str + "FillEllipse(brush, " + p[0] + ", " + p[1] + ", " +
                       p[2] + " - " + p[0] + ", " + p[3] + " - " + p[1] + ")";
            }
            if (p.Count() == 5)
            {
                if (p[4].Equals("0"))
                {
                    return str + "DrawEllipse(pen, " + p[0] + ", " + p[1] + ", " +
                           p[2] + " - " + p[0] + ", " + p[3] + " - " + p[1] + ")";
                }
                return str + "FillEllipse(brush, " + p[0] + ", " + p[1] + ", " +
                       p[2] + " - " + p[0] + ", " + p[3] + " - " + p[1] + ")";
            }
            return "Console.WriteLine(\"error\")";
        }

        public static string Boxf(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

            var str = "";
            if (BufferFlag)
            {
                str = "bgr.Graphics.";
            }
            else
            {
                str = "g.";
            }

            if (p.Count() == 1)
            {
                return str + "FillRectangle(brush, 0, 0, " +
                       "CurrentScreenID.Width, " + "CurrentScreenID.Height)";
            }
            if (p.Count() == 2)
            {
                return str + "FillRectangle(brush, " + p[0] + ", " + p[1] + ", " +
                       "CurrentScreenID.Width, " + "CurrentScreenID.Height)";
            }
            if (p.Count() == 4)
            {
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

                if (p[0].Equals(string.Empty))
                {
                    p[0] = "0";
                }
                if (p[1].Equals(string.Empty))
                {
                    p[1] = "0";
                }
                if (p[2].Equals(string.Empty))
                {
                    p[2] = "CurrentScreenID.Width";
                }
                if (p[3].Equals(string.Empty))
                {
                    p[3] = "CurrentScreenID.Height";
                }

                return str + "FillRectangle(brush, " + p[0] + ", " + p[1] + ", " +
                       p[2] + " - " + p[0] + ", " + p[3] + " - " + p[1] + ")";
            }
            return "Console.WriteLine(\"error\")";
        }

        public static string Line(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

            var str = "";
            if (BufferFlag)
            {
                str = "bgr.Graphics.";
            }
            else
            {
                str = "g.";
            }

            if (p.Count() == 2)
            {
                return str + "DrawLine(pen, CurrentPosX, CurrentPosY, " + p[0] + ", " + p[1] + ");\n" +
                       "CurrentPosX = " + p[0] + ";\nCurrentPosY = " + p[1];
            }
            if (p.Count() == 4)
            {
                return str + "DrawLine(pen, " + p[2] + ", " + p[3] + ", " + p[0] + ", " + p[1] + ");\n" +
                       "CurrentPosX = " + p[0] + ";\nCurrentPosY = " + p[1];
            }
            return "Console.WriteLine(\"error\")";
        }

        public static string Color(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

            if (p.Count() == 3)
            {
                if (p[0].Equals(string.Empty))
                {
                    p[0] = "0";
                }
                if (p[1].Equals(string.Empty))
                {
                    p[1] = "0";
                }
                if (p[2].Equals(string.Empty))
                {
                    p[2] = "0";
                }

                return "brush = new SolidBrush(Color.FromArgb(" + p[0] + ", " + p[1] + ", " + p[2] + "));\n" +
                       "pen = new Pen(Color.FromArgb(" + p[0] + ", " + p[1] + ", " + p[2] + "))";
            }
            return "Console.WriteLine(\"error\")";
        }

        public static string Picload(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

            var str = "";
            if (BufferFlag)
            {
                str = "bgr.Graphics.";
            }
            else
            {
                str = "g.";
            }

            if (!Analyzer.AddFunction[0].Contains("public void screen"))
            {
                Analyzer.AddFunction[0] += "public void screen(Form form, int width, int height)\n{\n" +
                                          "form.ClientSize = new Size(width, height);\n}\n\n";
            }

            if (p.Count() == 1)
            {
                return "Image img = Image.FromFile(" + p[0] + ");\n" +
                       "screen(CurrentScreenID, img.Width, img.Height);\n" +
                       str + "DrawImage(img, 0, 0, img.Width, img.Height)";
            }
            if (p.Count() == 2)
            {
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
                    return ""; ////
                }
                return "Console.WriteLine(\"error\")";
            }
            return "Console.WriteLine(\"error\")";
        }

        public static string Getkey(string strings)
        {
            Analyzer.UsingCheck("using System.Runtime.InteropServices");

            if (!Analyzer.ProgramField.Contains("[DllImport(\"user32.dll\")]\n"))
            {
                Analyzer.ProgramField += "[DllImport(\"user32.dll\")]\n" +
                                        "private static extern ushort GetAsyncKeyState(int vKey);\n";
            }

            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }
            var str = "";
            //変数名として正しいか
            if (Analyzer.VariableNameRule.Contains(p[0][0]))
            {
                //変数名ではない
            }
            else
            {
                //変数リストに含まれていない場合
                if (!Analyzer.VariableList.Contains(p[0]))
                {
                    //変数リストに追加
                    Analyzer.VariableList.Add(p[0]);
                    str = "dynamic ";
                }
            }

            if (p.Count() == 1)
            {
                return str + p[0] + " = GetAsyncKeyState(1) >> 15";
            }
            if (p.Count() == 2)
            {
                return str + p[0] + " = GetAsyncKeyState(" + p[1] + ") >> 15";
            }
            return "Console.WriteLine(\"error\")";
        }

        public static string Objsize(string strings)
        {
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }

            Analyzer.ProgramField += "objsizeX = " + p[0] + ";\n" +
                                    "objsizeY = " + p[1] + ";\n" +
                                    "objSpace = " + p[2];
            return "//boxSize(" + p[0] + ", " + p[1] + ")";
        }

        public static string Dialog(string strings)
        {
            strings = Analyzer.StringUnEscape(strings);
            var p = strings.Split(',');

            for (var i = 0; i < p.Count(); i++)
            {
                p[i] = p[i].Trim();
            }
            if (p.Count() == 1)
            {
                return "MessageBox.Show(" + p[0] + ", \"\", " +
                       "MessageBoxButtons.OK, MessageBoxIcon.Information)";
            }
            switch (p[1])
            {
                case "0":
                    return "MessageBox.Show(" + p[0] + ", " + p[2] + ", " +
                           "MessageBoxButtons.OK, MessageBoxIcon.Information)";
                case "1":
                    return "MessageBox.Show(" + p[0] + ", " + p[2] + ", " +
                           "MessageBoxButtons.OK, MessageBoxIcon.Warning);\n";
                case "2":
                    return "MessageBox.Show(" + p[0] + ", " + p[2] + ", " +
                           "MessageBoxButtons.YesNo, MessageBoxIcon.Information)";
                case "3":
                    return "MessageBox.Show(" + p[0] + ", " + p[2] + ", " +
                           "MessageBoxButtons.YesNo, MessageBoxIcon.Warning)";
                case "16":
                    return "";
                case "17":
                    return "";
                case "32":
                    return "";
                case "33":
                    return "";
                default:
                    return "Console.WriteLine(\"error\")";
            }
        }

        public static void Mousex(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.PointToClient(Cursor.Position).X";
        }

        public static void Mousey(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.PointToClient(Cursor.Position).Y";
        }

        public static void Ginfo_sizeX(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.Width";
        }

        public static void Ginfo_sizeY(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.Height";
        }

        public static void Ginfo_r(List<string> sentence, int i)
        {
            sentence[i] = "pen.Color.R";
        }

        public static void Ginfo_g(List<string> sentence, int i)
        {
            sentence[i] = "pen.Color.G";
        }

        public static void Ginfo_b(List<string> sentence, int i)
        {
            sentence[i] = "pen.Color.B";
        }

        public static void Ginfo_cx(List<string> sentence, int i)
        {
            sentence[i] = "CurrentPosX";
        }

        public static void Ginfo_cy(List<string> sentence, int i)
        {
            sentence[i] = "CurrentPosY";
        }

        public static void Ginfo_dispx(List<string> sentence, int i)
        {
            sentence[i] = "Screen.PrimaryScreen.Bounds.Width";
        }

        public static void Ginfo_dispy(List<string> sentence, int i)
        {
            sentence[i] = "Screen.PrimaryScreen.Bounds.Height";
        }

        public static void Ginfo_wx1(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.Left";
        }

        public static void Ginfo_wx2(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.Right";
        }

        public static void Ginfo_wy1(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.Top";
        }

        public static void Ginfo_wy2(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.Bottom";
        }

        public static void Ginfo_sel(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID";
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

        public static void Msgothic(List<string> sentence, int i)
        {
            sentence[i] = "ＭＳ ゴシック";
        }

        public static void Msmincho(List<string> sentence, int i)
        {
            sentence[i] = "ＭＳ 明朝";
        }

        public static void Font_normal(List<string> sentence, int i)
        {
            sentence[i] = "FontStyle.Regular";
        }

        public static void Font_bold(List<string> sentence, int i)
        {
            sentence[i] = "FontStyle.Bold";
        }

        public static void Font_italic(List<string> sentence, int i)
        {
            sentence[i] = "FontStyle.Italic";
        }

        public static void Font_underline(List<string> sentence, int i)
        {
            sentence[i] = "FontStyle.Underline";
        }

        public static void Font_strikeout(List<string> sentence, int i)
        {
            sentence[i] = "FontStyle.Strikeout";
        }

        public static void Screen_normal(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.WindowState = FormWindowState.Normal";
        }

        public static void Screen_hide(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.WindowState = FormWindowState.Minimized;\n" +
                          "ShowInTaskbar = false";
        }

        public static void Screen_fixedsize(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.FormBorderStyle = FormBorderStyle.FixedSingle";
        }

        public static void Screen_tool(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.FormBorderStyle = FormBorderStyle.FixedToolWindow";
        }

        public static void Screen_frame(List<string> sentence, int i)
        {
            sentence[i] = "CurrentScreenID.FormBorderStyle = FormBorderStyle.Fixed3D";
        }
    }
}
