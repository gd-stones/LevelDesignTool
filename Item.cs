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
        public Image image ;
        public Size size;
        public Vector2 position;
        public int type;
        public int length;
        public string hashKey;

        public Item(Image image, Size size, Vector2 position, int type, int length)
        {
            this.image = image;
            this.size = size;
            this.position = position;
            this.type = type;
            this.length = length;
        }

        public PictureBox GenerateImage(Point point)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = image;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Location = point;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.BackColor = Color.Black;

            pictureBox.MouseDown += PictureBox_MouseDown;

            return pictureBox;
        }

        public void DragDropItem()
        {

        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {   
            if (e.Button == MouseButtons.Left)
            {
                ItemClicked?.Invoke(this, type);
            }
        }
    }
}
