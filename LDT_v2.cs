using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelDesignTool
{
    public partial class LDT_v2 : Form
    {
        private const int gridSize = 16;
        private List<Item> items = new List<Item>();
        private int selectedItemType = 0;

        public LDT_v2()
        {
            InitializeComponent();
            ReadItemFromFile();
            DisplayItems();
            foreach (Item item in items)
            {
                item.ItemClicked += Item_ItemClicked;
            }
            InitializeMapToolPanel();
        }

        private void InitializeMapToolPanel()
        {
            Map_Tool.Paint += Map_Tool_DrawGrid;
            Map_Tool.AllowDrop = true;
            Map_Tool.MouseClick += Map_Tool_MouseClick;
            Map_Tool.AutoScrollMinSize = new Size(3200, 1248);
            Map_Tool.AutoScroll = true;
            //Map_Tool.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(Map_Tool, true);
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
            Debug.WriteLine($"Debug: Item size before creating PictureBox: {item.size}");
            pictureBox.Location = location;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.Size = item.size;
            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;

            Debug.WriteLine($"Debug: PictureBox size after adding to Map_Tool: {pictureBox.Size}");
            Map_Tool.Controls.Add(pictureBox);
        }


        private Point _dragOffset;
        private bool _isDragging;

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var pb = (PictureBox)sender;
                _isDragging = true;
                _dragOffset = e.Location;
                pb.BringToFront(); // Ensure the PictureBox stays on top while dragging
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var pb = (PictureBox)sender;
                // Calculate new location of PictureBox relative to the parent panel
                pb.Location = new Point(pb.Location.X + e.X - _dragOffset.X, pb.Location.Y + e.Y - _dragOffset.Y);
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
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
            // solution 1
            //Panel panel = (Panel)sender;
            //Graphics g = e.Graphics;
            //Pen pen = new Pen(Color.Black);

            //int numRows = 1248 / gridSize;
            //int numCols = 3200 / gridSize;

            //for (int i = 0; i < numRows; i++)
            //{
            //    for (int j = 0; j < numCols; j++)
            //    {
            //        int x = j * gridSize;
            //        int y = i * gridSize;
            //        g.DrawRectangle(pen, x, y, gridSize, gridSize);
            //    }
            //}
            //pen.Dispose();

            // solution 2
            Panel panel = (Panel)sender;
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Black);

            // Assuming gridSize is already set somewhere in the class
            int numRows = 1248 / gridSize;
            int numCols = 3200 / gridSize;

            // Draw the horizontal grid lines
            for (int i = 0; i <= numRows; i++)
            {
                int y = i * gridSize;
                g.DrawLine(pen, 0, y, numCols * gridSize, y); // Draw line from left to right
            }

            // Draw the vertical grid lines
            for (int j = 0; j <= numCols; j++)
            {
                int x = j * gridSize;
                g.DrawLine(pen, x, 0, x, numRows * gridSize); // Draw line from top to bottom
            }
            pen.Dispose();


            // solution 3
            //Panel panel = (Panel)sender;
            //Graphics g = e.Graphics;

            //// Calculate number of rows and columns based on your panel size
            //int numRows = 1248 / gridSize;
            //int numCols = 3200 / gridSize;

            //g.Clear(panel.BackColor); // Clear the panel if necessary

            //// Set up the transform for the graphics object to flip and translate
            //g.ScaleTransform(1, -1); // Flip the y-axis
            //g.TranslateTransform(0, -1248); // Translate to bottom-left

            //Pen pen = new Pen(Color.Black);

            //// Draw the vertical grid lines
            //for (int i = 0; i <= numCols; i++)
            //{
            //    int x = i * gridSize;
            //    g.DrawLine(pen, x, 0, x, numRows * gridSize); // Draw line from bottom to top
            //}

            //// Draw the horizontal grid lines
            //for (int j = 0; j <= numRows; j++)
            //{
            //    int y = j * gridSize;
            //    g.DrawLine(pen, 0, y, numCols * gridSize, y); // Draw line from left to right
            //}

            //// Dispose of the pen to free up resources
            //pen.Dispose();
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

                x += pictureBox.Width + 10;
                if (x + pictureBox.Width > Item_List_View.Width)
                {
                    x = 10;
                    y += pictureBox.Height + 10;
                }
            }
        }

        private void ReadItemFromFile()
        {
            string filePath = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Items.txt";
            string[] lines = File.ReadAllLines(filePath);

            string imagePath = "";
            Size size = new Size();
            int type = 0;
            int length = 0;
            bool isInBlock = false;

            foreach (string line in lines)
            {
                if (line.StartsWith("{"))
                {
                    imagePath = "";
                    size = Size.Empty;
                    type = 0;
                    length = 0;
                    isInBlock = true;
                    continue;
                }

                if (line.StartsWith("}"))
                {
                    isInBlock = false; 
                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                    {
                        Image image = Image.FromFile(imagePath);
                        Item item = new Item(image, size, new Vector2(0, 0), type, length);
                        items.Add(item);
                    }
                    continue; 
                }

                if (!isInBlock) continue;

                if (line.StartsWith("type:"))
                {
                    type = int.Parse(line.Substring(6).Trim());
                }
                else if (line.StartsWith("path:"))
                {
                    imagePath = line.Substring(6).Trim();
                }
                else if (line.StartsWith("size:"))
                {
                    var sizeParts = line.Substring(6).Trim().Split(',');
                    size = new Size(int.Parse(sizeParts[0]), int.Parse(sizeParts[1]));
                }
                else if (line.StartsWith("length:"))
                {
                    length = int.Parse(line.Substring(8).Trim());
                }
            }
        }

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            List<PictureBox> pictureBoxes = new List<PictureBox>();
            Point scrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);

            foreach (Control control in Map_Tool.Controls)
            {
                if (control is PictureBox)
                {
                    pictureBoxes.Add((PictureBox)control);
                }
            }

            pictureBoxes.Sort((x, y) =>
            {
                int result = x.Top.CompareTo(y.Top);
                return (result == 0) ? x.Left.CompareTo(y.Left) : result;
            });

            StringBuilder exportData = new StringBuilder();

            foreach (PictureBox pb in pictureBoxes)
            {
                var adjustedLocation = new Point(pb.Location.X + scrollPosition.X, pb.Location.Y + scrollPosition.Y);
                exportData.AppendLine($"PictureBox: {adjustedLocation.X}, {adjustedLocation.Y}");
            }

            string exportFilePath = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\ExportedPositions.txt";

            try
            {
                File.WriteAllText(exportFilePath, exportData.ToString());
                MessageBox.Show($"Exported PictureBox positions to {exportFilePath}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting positions: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
