using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelDesignTool
{
    public partial class LDT_v2 : Form
    {
        private const int gridSize = 20; // Size of each square in the grid
        private List<Item> items = new List<Item>();
        private int selectedItemType = 0;

        public LDT_v2()
        {
            InitializeComponent();
            Map_Tool.Paint += Map_Tool_DrawGrid;
            Map_Tool.AllowDrop = true;
            Map_Tool.MouseClick += Map_Tool_MouseClick;

            ReadItemFromFile();
            DisplayItems();
            foreach (Item item in items)
            {
                item.ItemClicked += Item_ItemClicked;
            }
        }

        private void Map_Tool_MouseClick(object sender, MouseEventArgs e)
        {
            if (selectedItemType != 0)
            {
                Item item = items.FirstOrDefault(i => i.type == selectedItemType);
                if (item != null)
                {
                    CreatePictureBoxAtLocation(item, e.Location);
                }
                selectedItemType = 0;
            }
        }

        private void CreatePictureBoxAtLocation(Item item, Point location)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = item.itemImage[0];
            pictureBox.Location = location;
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            Map_Tool.Controls.Add(pictureBox);
        }

        private void Item_ItemClicked(object sender, int type)
        {
            selectedItemType = type;
        }

        private void Map_Tool_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Map_Tool_DrawGrid(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Black);

            int numRows = panel.Height / gridSize;
            int numCols = panel.Width / gridSize;

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    int x = j * gridSize;
                    int y = i * gridSize;
                    g.DrawRectangle(pen, x, y, gridSize, gridSize);
                }
            }
        }

        private void Item_List_View_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DisplayItems()
        {
            int x = 10; // Initial x position
            int y = 10; // Initial y position

            foreach (Item item in items)
            {
                PictureBox pictureBox = new PictureBox();
                pictureBox = item.GenerateImage(new Point(x, y));
                Item_List_View.Controls.Add(pictureBox);

                x += pictureBox.Width + 10; // Add some spacing between items
                if (x + pictureBox.Width > Item_List_View.Width)
                {
                    x = 10; // Reset x position
                    y += pictureBox.Height + 10; // Move to the next row
                }
            }
        }

        private void ReadItemFromFile()
        {
            string filePath = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Items.txt";
            string[] lines = File.ReadAllLines(filePath);

            int type = 1;
            foreach (string line in lines)
            {
                if (line.StartsWith("path:"))
                {
                    string imagePath = line.Substring(6).Trim();
                    if (File.Exists(imagePath))
                    {
                        Image image = Image.FromFile(imagePath);
                        Item item = new Item(image, new Size(100, 100), new Vector2(0, 0), type); // Adjust size and position as needed
                        items.Add(item);
                        type ++;
                    }
                }
            }
        }
    }
}
