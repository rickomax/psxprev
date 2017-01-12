using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PSXPrev.Classes;

namespace PSXPrev.Forms
{
    public partial class StubForm : Form
    {
        public enum CommandType
        {
            Matrix,
            Vertex
        }

        public class StackCommand
        {
            public object Command;
            public CommandType Type;
        }

        public StubForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           // var commandStack = new List<StackCommand>();
           // var lines = textBox1.Text.Split('\n');
           // foreach (var line in lines)
           // {
           //     var commands = line.Split(' ');
           //     switch (commands[0])
           //     {
           //         case "m":
           //             var tx = float.Parse(commands[1]);
           //             var ty = float.Parse(commands[2]);
           //             var tz = float.Parse(commands[3]);
           //             var rx = float.Parse(commands[4]);
           //             var ry = float.Parse(commands[5]);
           //             var rz = float.Parse(commands[6]);
           //             var s = float.Parse(commands[7]);
           //             commandStack.Add(
           //                 new StackCommand
           //                 {
           //                     Command =
           //                         glm.translate(new mat4(s), new vec3(tx, ty, tz)) *
           //                         GeomUtils.CreateR(new vec3(rx * DialogUtils.Deg2Rad, ry * DialogUtils.Deg2Rad, rz * DialogUtils.Deg2Rad)),
           //                     Type = CommandType.Matrix
           //                 });
           //             break;
           //         case "v":
           //             var v1x = float.Parse(commands[1]);
           //             var v1y = float.Parse(commands[2]);
           //             var v1z = float.Parse(commands[3]);
           //             var v2x = float.Parse(commands[4]);
           //             var v2y = float.Parse(commands[5]);
           //             var v2z = float.Parse(commands[6]);
           //             commandStack.Add(
           //                 new StackCommand
           //                 {
           //                     Command = new []{
           //                         new vec4(v1x, v1y, v1z, 1f),
           //                         new vec4(v2x, v2y, v2z, 1f)}
           //                        ,
           //                     Type = CommandType.Vertex
           //                 });
           //             break;
           //     }
           // }
           //
           // var bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
           // var graphics = Graphics.FromImage(bitmap);
           // var middle = new vec4(bitmap.Width*0.5f, bitmap.Height*0.5f, 0f, 1f);
           // var matrixStack = mat4.identity();
           // foreach (var command in commandStack)
           // {
           //     switch (command.Type)
           //     {
           //         case CommandType.Matrix:
           //             matrixStack = (mat4)command.Command * matrixStack;
           //             break;
           //         case CommandType.Vertex:
           //             var random = new Random();
           //             var randomColor = System.Drawing.Color.FromArgb(255, random.Next(0, 255), random.Next(0, 255),
           //                 random.Next(0, 255));
           //             var pen = new Pen(randomColor, 1f);
           //             vec4[] vertices = (vec4[]) command.Command;
           //             vec4 transformed1 = middle + matrixStack* vertices[0];
           //             vec4 transformed2 = middle + matrixStack* vertices[1];
           //             graphics.DrawLine(pen, transformed1.x, transformed1.y , transformed2.x, transformed2.y);
           //             break;
           //     }
           // }
           // pictureBox1.Image = bitmap;
        }
    }
}
