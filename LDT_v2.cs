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
using System.Text.RegularExpressions;
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
            ReadItemsFile();
            DisplayItems();
            InitializeMapToolPanel();

            this.Edit_Item.Click += new System.EventHandler(this.Edit_Item_Click);
            this.Item_Content.ReadOnly = true;
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
                    Item newItem = new Item(item.image, item.size, new Vector2(0, 0), item.type, item.length);
                    CreateItemAtLocation(newItem, e.Location);
                }
                selectedItemType = 0;
            }
        }

        private Panel _currentDragPanel;
        private Point _dragOffset;
        private bool _isDragging;

        private void CreateItemAtLocation(Item item, Point location)
        {
            string hashKey = Guid.NewGuid().ToString();
            item.hashKey = hashKey;

            Panel itemPanel = new Panel
            {
                Size = new Size(item.size.Width * item.length, item.size.Height),
                Location = location,
                Tag = item  // Store the item as a Tag in the Panel
            };

            int offsetX = 0;
            for (int i = 0; i < item.length; i++)
            {
                PictureBox pictureBox = new PictureBox
                {
                    Image = item.image,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = item.size,
                    Location = new Point(offsetX, 0)
                };
                pictureBox.MouseDown += Item_MouseDown;
                pictureBox.MouseMove += Item_MouseMove;
                pictureBox.MouseUp += Item_MouseUp;
                pictureBox.MouseDoubleClick += PictureBox_MouseDoubleClick;

                itemPanel.Controls.Add(pictureBox);
                offsetX += item.size.Width;
            }

            Map_Tool.Controls.Add(itemPanel);
        }

        private void PictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (sender is PictureBox pictureBox)
            {
                Item item = pictureBox.Parent.Tag as Item;
                if (item != null)
                {
                    DisplayItemInformation(item);
                }
            }
        }

        private Dictionary<string, string> ParseItemInfo(string info)
        {
            var itemInfo = new Dictionary<string, string>();
            var matches = Regex.Matches(info, @"(\w+):\s*((?:\([^)]+\))|[^,}]+)");

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 3)
                {
                    var key = match.Groups[1].Value.Trim();
                    var value = match.Groups[2].Value.Trim();
                    value = value.TrimEnd('}');

                    itemInfo[key] = value;
                }
            }

            return itemInfo;
        }


        private void DisplayItemInformation(Item item)
        {
            string info = GetItemInformationFromLevelFile(item.hashKey);
            var itemDetails = ParseItemInfo(info);
            StringBuilder displayText = new StringBuilder();

            if (itemDetails.TryGetValue("Type", out string type))
                displayText.AppendLine("Type: " + type);
            if (itemDetails.TryGetValue("Position", out string position))
                displayText.AppendLine("Position: " + position);
            if (itemDetails.TryGetValue("Size", out string size))
                displayText.AppendLine("Size: " + size);
            if (itemDetails.TryGetValue("Length", out string length))
                displayText.AppendLine("Length: " + length);

            foreach (Control control in Item_Editor.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.Text = displayText.ToString();
                    break;
                }
                else if (control is Label label)
                {
                    label.Text = displayText.ToString();
                    break;
                }
            }
        }

        private string GetItemInformationFromLevelFile(string hashKey)
        {
            string levelFilePath = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Level.txt";
            string[] lines = File.ReadAllLines(levelFilePath);
            string keyIdentifier = $"HashKey: {hashKey}";

            foreach (string line in lines)
            {
                if (line.Contains(keyIdentifier))
                {
                    return line;
                }
            }
            return "Information not found";
        }

        private void Item_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = sender as Control;
            _currentDragPanel = control as Panel ?? control.Parent as Panel;

            if (_currentDragPanel == null) return;

            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                Point clientPoint = _currentDragPanel.PointToClient(Cursor.Position); // Get cursor position relative to panel
                _dragOffset = clientPoint;
                _currentDragPanel.BringToFront();
            }
        }

        private void Item_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _currentDragPanel == null) return;

            Point screenPoint = Cursor.Position;
            Point newLocation = Map_Tool.PointToClient(screenPoint); // Convert to coordinates relative to Map_Tool

            newLocation.Offset(-_dragOffset.X, -_dragOffset.Y);
            newLocation.X = Math.Max(0, newLocation.X);
            newLocation.Y = Math.Max(0, newLocation.Y);
            newLocation.X = Math.Min(Map_Tool.ClientSize.Width - _currentDragPanel.Width, newLocation.X);
            newLocation.Y = Math.Min(Map_Tool.ClientSize.Height - _currentDragPanel.Height, newLocation.Y);

            _currentDragPanel.Location = newLocation; // Set the new location
        }

        private void Item_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                _currentDragPanel = null;
                Map_Tool.Invalidate(); // Optional: Redraw the Map_Tool control if needed
            }
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
            int x = 10;
            int y = 10;

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

        private void ReadItemsFile()
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

            foreach (Item item in items)
            {
                item.ItemClicked += Item_ItemClicked;
            }
        }

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            List<Panel> itemPanels = new List<Panel>();
            Point scrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);

            foreach (Control control in Map_Tool.Controls)
            {
                if (control is Panel panel && panel.Tag is Item)
                {
                    itemPanels.Add(panel);
                }
            }

            itemPanels.Sort((x, y) =>
            {
                int result = x.Top.CompareTo(y.Top);
                return (result == 0) ? x.Left.CompareTo(y.Left) : result;
            });

            StringBuilder exportData = new StringBuilder();

            foreach (Panel panel in itemPanels)
            {
                if (panel.Tag is Item item)
                {
                    var adjustedLocation = new Point(panel.Location.X + scrollPosition.X, panel.Location.Y + scrollPosition.Y);
                    var size = new Size(item.size.Width, item.size.Height);
                    int type = item.type;
                    int length = item.length;
                    string hashKey = item.hashKey;

                    string itemPanelInfo = $"{{HashKey: {hashKey}, Type: {type}, Position: ({adjustedLocation.X}, {adjustedLocation.Y}), Size: ({size.Width}, {size.Height}), Length: {length}}}";
                    exportData.AppendLine(itemPanelInfo);
                }
                else
                {
                    MessageBox.Show("Panel's Tag property is not of type Item!", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string exportFilePath = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Level.txt";

            try
            {
                File.WriteAllText(exportFilePath, exportData.ToString());
                MessageBox.Show($"Exported item data to {exportFilePath}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting item data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Edit_Item_Click(object sender, EventArgs e)
        {
            if (this.Item_Content.ReadOnly)
            {
                MessageBox.Show("eee");
                this.Item_Content.ReadOnly = false;
                this.Item_Content.Focus(); 
                //((Button)sender).Text = "Save Changes";
            }
            else
            {
                this.Item_Content.ReadOnly = true;
                //SaveEditedContent(this.Item_Content.Text);
                //((Button)sender).Text = "Edit Item";
            }
        }

        private void Save_Item_Click(object sender, EventArgs e)
        {

        }

        private void Delete_Item_Click(object sender, EventArgs e)
        {

        }

        private void Item_Editor_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Item_Content_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
