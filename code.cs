using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace NameSpace
{
public class Program
{
public Form form0;
public Form CurrentScreenID;
public Form DebugWindow;
public Dictionary<string, dynamic> Variables;

public Program(Form _form, Dictionary<string, dynamic> _variables, Form _debugWindow)
{
form0 = _form;
CurrentScreenID = form0;
Variables = _variables;
DebugWindow = _debugWindow;
DebugWindow.Paint += dPaint;
DebugWindow.Show();
}

private void dPaint(object sender, PaintEventArgs e)
{
var keys = Variables.Keys.ToList();
var values = Variables.Values.ToList();
for(var i=0; i<keys.Count; i++)
{
e.Graphics.DrawString(keys[i] + " => " + values[i], new Font("FixedSys", 14), new SolidBrush(Color.FromArgb(0, 0, 0)), 0, i*50);
}
}

public void initScreen(Form form)
{
form.ClientSize = new Size(640, 480);
form.Text = "hsp.cs";
form.BackColor = Color.FromArgb(255, 255, 255);
form.MaximizeBox = false;
form.FormBorderStyle = FormBorderStyle.FixedSingle;
form.Paint += Paint;
}

public void screen(Form form, int width, int height)
{
form.ClientSize = new Size(width, height);
}


public void Paint(object sender, PaintEventArgs e)
{
var FontSize = 14;
var CurrentPosX = 0;
var CurrentPosY = 0;
Graphics g = e.Graphics;
Brush brush = new SolidBrush(Color.FromArgb(0, 0, 0));
Pen pen = new Pen(Color.FromArgb(0, 0, 0));
Font font = new Font("FixedSys", FontSize);
try
{
screen(form0, 1000, 1000);
}
catch(Exception)
{
}
DebugWindow.Refresh();}
}
}

class Test
{
static void Main()
{
}
}
