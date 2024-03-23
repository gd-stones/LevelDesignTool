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
        private const int gridSize = 16;
        private List<Item> items = new List<Item>();
        private int selectedItemType = 0;

        public LDT_v2()
        {
            InitializeComponent();
            //Map_Tool.Paint += Map_Tool_DrawGrid;
            Map_Tool.AllowDrop = true;
            Map_Tool.MouseClick += Map_Tool_MouseClick;
            Map_Tool.AutoScrollMinSize = new Size(3200, 1248);
            InitializeMapToolPanel();
            //Map_Tool.MaximumSize = new Size(3200, 1248);

            ReadItemFromFile();
            DisplayItems();
            foreach (Item item in items)
            {
                item.ItemClicked += Item_ItemClicked;
            }
        }

        private void InitializeMapToolPanel()
        {
            Map_Tool.Width = 3200;
            Map_Tool.Height = 1248;
            Map_Tool.AutoScroll = true; // Allow scrolling if the panel is larger than the container

            // Reduce flickering by enabling double buffering
            Map_Tool.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(Map_Tool, true);

            // Subscribe to the Paint event of Map_Tool
            Map_Tool.Paint += new PaintEventHandler(Map_Tool_Paint);
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

            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;

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
            // The size of the grid
            int gridWidth = 3200;
            int gridHeight = 1248;
            int cellSize = 32;

            Graphics g = e.Graphics;
            Pen gridPen = new Pen(Color.Black);

            // Create a red pen for the first lines
            Pen firstLinePen = new Pen(Color.Red);

            // Adjust for the current scroll position
            Point scrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);

            // Draw the first horizontal line in red
            g.DrawLine(firstLinePen, scrollPosition.X, scrollPosition.Y + gridHeight, scrollPosition.X + gridWidth, scrollPosition.Y + gridHeight);

            // Draw the first vertical line in red
            g.DrawLine(firstLinePen, scrollPosition.X, scrollPosition.Y, scrollPosition.X, scrollPosition.Y + gridHeight);

            // Draw the rest of the horizontal grid lines
            for (int y = gridHeight - cellSize; y >= 0; y -= cellSize)
            {
                g.DrawLine(gridPen, scrollPosition.X, scrollPosition.Y + y, scrollPosition.X + gridWidth, scrollPosition.Y + y);
            }

            // Draw the rest of the vertical grid lines
            for (int x = cellSize; x <= gridWidth; x += cellSize)
            {
                g.DrawLine(gridPen, scrollPosition.X + x, scrollPosition.Y, scrollPosition.X + x, scrollPosition.Y + gridHeight);
            }

            // Dispose of the pens after using
            gridPen.Dispose();
            firstLinePen.Dispose();
        }

        private void Map_Tool_DrawGrid(object sender, PaintEventArgs e)
        {
            // sludion 1
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

            // slodion 2
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


            // slodion 3
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
                        type++;
                    }
                }
            }
        }

        //private void ButtonExport_Click(object sender, EventArgs e)
        //{
        //    // Create a new list to store the sorted PictureBox controls
        //    List<PictureBox> pictureBoxes = new List<PictureBox>();

        //    // Gather all PictureBox controls from the Map_Tool panel
        //    foreach (Control control in Map_Tool.Controls)
        //    {
        //        if (control is PictureBox)
        //        {
        //            pictureBoxes.Add((PictureBox)control);
        //        }
        //    }

        //    // Sort the PictureBoxes by Top first and then Left positions
        //    pictureBoxes.Sort((x, y) =>
        //    {
        //        int result = x.Top.CompareTo(y.Top);
        //        return (result == 0) ? x.Left.CompareTo(y.Left) : result;
        //    });

        //    // Now pictureBoxes are sorted, you can export their locations
        //    StringBuilder exportData = new StringBuilder();

        //    foreach (PictureBox pb in pictureBoxes)
        //    {
        //        exportData.AppendLine($"PictureBox: {pb.Location.X}, {pb.Location.Y}");
        //    }

        //    string exportFilePath = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\ExportedPositions.txt";

        //    try
        //    {
        //        File.WriteAllText(exportFilePath, exportData.ToString());
        //        MessageBox.Show($"Exported PictureBox positions to {exportFilePath}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error exporting positions: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            // Create a new list to store the sorted PictureBox controls
            List<PictureBox> pictureBoxes = new List<PictureBox>();

            // The AutoScrollPosition refers to the current scroll location of the Map_Tool Panel
            Point scrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);

            // Gather all PictureBox controls from the Map_Tool panel
            foreach (Control control in Map_Tool.Controls)
            {
                if (control is PictureBox)
                {
                    pictureBoxes.Add((PictureBox)control);
                }
            }

            // Sort the PictureBoxes by Top first and then Left positions
            pictureBoxes.Sort((x, y) =>
            {
                int result = x.Top.CompareTo(y.Top);
                return (result == 0) ? x.Left.CompareTo(y.Left) : result;
            });

            // Now pictureBoxes are sorted, you can export their locations
            StringBuilder exportData = new StringBuilder();

            foreach (PictureBox pb in pictureBoxes)
            {
                // Adjust the location based on the scroll position
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
