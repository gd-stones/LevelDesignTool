using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LevelDesignTool
{
    internal class Item
    {
        public event EventHandler<int> ItemClicked; // Custom event to notify the type
        public Image Image_;
        public Size Size_;
        public Point Position;
        public int Type;
        public int Length;
        public Point StartCollider;
        public Point EndCollider;

        public Item()
        {

        }

        public Item(Image Image_, Size Size_, int Type, int Length)
        {
            this.Image_ = Image_;
            this.Size_ = Size_;
            this.Type = Type;
            this.Length = Length;
        }

        public PictureBox GenerateImage(Point Point_)
        {
            PictureBox PictureBox_ = new PictureBox();
            PictureBox_.Image = Image_;
            PictureBox_.SizeMode = PictureBoxSizeMode.Zoom;
            PictureBox_.Location = Point_;
            PictureBox_.BorderStyle = BorderStyle.FixedSingle;
            PictureBox_.BackColor = Color.Black;

            PictureBox_.Paint += (sender, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, PictureBox_.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
            };
            PictureBox_.MouseDown += PictureBox_MouseDown;
            return PictureBox_;
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {   
            if (e.Button == MouseButtons.Left)
            {
                ItemClicked?.Invoke(this, Type);
            }
        }
    }
}
