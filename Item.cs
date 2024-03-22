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

        public List<Image> itemImage = new List<Image>();
        private Size size;
        private Vector2 position;
        public int type;

        public Item(Image image, Size size, Vector2 position, int type)
        {
            this.itemImage.Add(image);
            this.size = size;
            this.position = position;
            this.type = type;
        }

        public PictureBox GenerateImage(Point point)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = itemImage[0];
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Location = point;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.BackColor = Color.Black;

            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;

            return pictureBox;
        }

        public void DragDropItem()
        {
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {   
            if (e.Button == MouseButtons.Left)
            {
                //MessageBox.Show("Mouse Down: " + type);
                ItemClicked?.Invoke(this, type);

                PictureBox pictureBox = (PictureBox)sender;
                pictureBox.DoDragDrop(pictureBox.Image, DragDropEffects.Copy);
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PictureBox pictureBox = (PictureBox)sender;
                pictureBox.DoDragDrop(pictureBox.Image, DragDropEffects.Copy);
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            // Additional handling if needed
        }
    }
}
