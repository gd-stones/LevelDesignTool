using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
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
        private string hash = "";

        public LDT_v2()
        {
            InitializeComponent();
            ReadItemsFile();
            DisplayItems();
            InitializeMapToolPanel();
        }

        private void InitializeMapToolPanel()
        {
            Map_Tool.Paint += Map_Tool_DrawGrid;
            Map_Tool.AllowDrop = true;
            Map_Tool.MouseClick += Map_Tool_MouseClick;
            Map_Tool.AutoScrollMinSize = new Size(3200, 1248);
            Map_Tool.AutoScroll = true;
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
            item.startCollider = new Point (0,0);
            item.endCollider = new Point (item.size.Width * item.length + 1, item.size.Height + 1);

            Panel itemPanel = new Panel
            {
                Size = new Size(item.size.Width * item.length + 2, item.size.Height + 2),
                Location = location,
                Tag = item  // Store the item as a Tag in the Panel
            };

            int offsetX = 1;
            for (int i = 0; i < item.length; i++)
            {
                PictureBox pictureBox = new PictureBox
                {
                    Image = item.image,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = item.size,
                    Location = new Point(offsetX, 1)
                };
                pictureBox.MouseDown += Item_MouseDown;
                pictureBox.MouseMove += Item_MouseMove;
                pictureBox.MouseUp += Item_MouseUp;
                pictureBox.MouseDoubleClick += PictureBox_MouseDoubleClick;

                itemPanel.Controls.Add(pictureBox);
                offsetX += item.size.Width;
            }

            itemPanel.Paint += (sender, e) =>
            {
                Pen pen = new Pen(Color.Red, 1);
                e.Graphics.DrawRectangle(pen, item.startCollider.X, item.startCollider.Y, item.endCollider.X, item.endCollider.Y);
            };

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
                    // Used for saving changes
                    hash = item.hashKey;
                }
            }
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
            if (itemDetails.TryGetValue("StartCollider", out string startCollider))
                displayText.AppendLine("StartCollider: " + startCollider);
            if (itemDetails.TryGetValue("EndCollider", out string endCollider))
                displayText.AppendLine("EndCollider: " + endCollider);

            Item_Content.Text = displayText.ToString();
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

            _currentDragPanel.Location = newLocation;
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
            Panel panel = (Panel)sender;
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Black);
            int numRows = 1248 / gridSize;
            int numCols = 3200 / gridSize;

            for (int i = 0; i <= numRows; i++)
            {
                int y = i * gridSize;
                g.DrawLine(pen, 0, y, numCols * gridSize, y); // Draw line from left to right
            }
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
            StringBuilder exportDataL = new StringBuilder();

            foreach (Panel panel in itemPanels)
            {
                if (panel.Tag is Item item)
                {
                    var adjustedLocation = new Point(panel.Location.X + scrollPosition.X, panel.Location.Y + scrollPosition.Y);
                    var size = new Size(item.size.Width, item.size.Height);
                    int type = item.type;
                    int length = item.length;
                    string hashKey = item.hashKey;
                    Point startCollider  = item.startCollider;
                    Point endCollider = item.endCollider;

                    string itemPanelInfo = $"{{HashKey: {hashKey}, Type: {type}, Position: ({adjustedLocation.X}, {adjustedLocation.Y}), Size: ({size.Width}, {size.Height}), Length: {length}, " +
                        $"StartCollider: ({startCollider.X}, {startCollider.Y}), EndCollider: ({endCollider.X}, {endCollider.Y})}}";
                    exportData.AppendLine(itemPanelInfo);

                    string _itemPanelInfo = $"{{Type: {type}, Position: ({adjustedLocation.X}, {1246 - adjustedLocation.Y - size.Height}), Size: ({size.Width}, {size.Height}), Length: {length}, " +
                        $"StartCollider: ({startCollider.X}, {startCollider.Y}), EndCollider: ({endCollider.X}, {endCollider.Y})}}";
                    exportDataL.AppendLine(_itemPanelInfo);
                }
                else
                {
                    MessageBox.Show("Panel's Tag property is not of type Item!", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string exportFilePath = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Level.txt";
            string exportFilePathForL = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Level_Export.txt";

            try
            {
                File.WriteAllText(exportFilePath, exportData.ToString());
                File.WriteAllText(exportFilePathForL, exportDataL.ToString());
                //MessageBox.Show($"Exported item data to {exportFilePath}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting item data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Edit_Item_Click(object sender, EventArgs e)
        {

        }

        private void Save_Item_Click(object sender, EventArgs e)
        {
            string itemHashKey = hash;
            string newText = Item_Content.Text.Replace(Environment.NewLine, "");

            newText = newText.Replace("Type: ", ", Type: ");
            newText = newText.Replace("Position: ", ", Position: ");
            newText = newText.Replace("Size: ", ", Size: ");
            newText = newText.Replace("Length: ", ", Length: ");
            newText = newText.Replace("StartCollider: ", ", StartCollider: ");
            newText = newText.Replace("EndCollider: ", ", EndCollider: ");

            string newItemInfo = $"{{HashKey: {itemHashKey}{newText}}}";

            List<string> lines = new List<string>();
            try
            {
                lines = File.ReadAllLines(@"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Level.txt").ToList();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Failed to read file: " + ex.Message);
                return;
            }

            int indexToUpdate = lines.FindIndex(line => line.Contains(itemHashKey));
            if (indexToUpdate == -1)
            {
                MessageBox.Show($"Item with hash key '{itemHashKey}' not found.");
                return;
            }

            lines[indexToUpdate] = newItemInfo;

            try
            {
                File.WriteAllLines(@"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Level.txt", lines);
                UpdateItem(itemHashKey, newText);
                hash = "";
            }
            catch (IOException ex)
            {
                MessageBox.Show("Failed to save file: " + ex.Message);
            }
        }

        private void UpdateItem(string hashKey, string newText)
        {
            foreach (Panel panel in Map_Tool.Controls.OfType<Panel>())
            {
                Item item = panel.Tag as Item;
                if (item != null && item.hashKey == hashKey)
                {
                    panel.Controls.Clear();

                    string[] values = newText.Split(',');
                    string[] tmp;
                    if (values.Length >= 6)
                    {
                        tmp = values[2].Split('(');
                        int.TryParse(tmp[1].Trim(), out int posX);
                        tmp = values[3].Split(')');
                        int.TryParse(tmp[0].Trim(), out int posY);
                        tmp = values[4].Split("(");
                        int.TryParse(tmp[1].Trim(), out int width);
                        tmp = values[5].Split(")");
                        int.TryParse(tmp[0].Trim(), out int height);
                        tmp = values[6].Split(" ");
                        int.TryParse(tmp[2].Trim(), out int length);

                        //tmp = values[7].Split("(");
                        //int.TryParse(tmp[1].Trim(), out int sX);
                        //tmp = values[8].Split(")");
                        //int.TryParse(tmp[0].Trim(), out int sY);
                        //tmp = values[9].Split("(");
                        //int.TryParse(tmp[1].Trim(), out int eX);
                        //tmp = values[10].Split(")");
                        //int.TryParse(tmp[0].Trim(), out int eY);

                        int offsetX = 1;
                        for (int i = 0; i < length; i++)
                        {
                            PictureBox pictureBox = new PictureBox
                            {
                                Image = item.image,
                                SizeMode = PictureBoxSizeMode.Zoom,
                                Size = new Size(width, height),
                                Location = new Point(offsetX, 1)
                            };

                            pictureBox.MouseDown += Item_MouseDown;
                            pictureBox.MouseMove += Item_MouseMove;
                            pictureBox.MouseUp += Item_MouseUp;
                            pictureBox.MouseDoubleClick += PictureBox_MouseDoubleClick;

                            panel.Controls.Add(pictureBox);
                            offsetX += width;
                        }
                        // Update Tag
                        item.length = length;
                        item.size = new Size(width, height);
                        //item.startCollider = new Point(0, 0);
                        item.endCollider = new Point(width * length + 1, height + 1);

                        panel.Width = width * length + 2;
                        panel.Height = height + 2;
                        panel.Location = new Point(posX, posY);
                        panel.Refresh();
                        break;
                    }
                }
            }
        }

        private void Delete_Item_Click(object sender, EventArgs e)
        {
            string itemHashKey = hash;

            List<string> lines = new List<string>();
            try
            {
                lines = File.ReadAllLines(@"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Level.txt").ToList();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Failed to read file: " + ex.Message);
                return;
            }

            int indexToDelete = lines.FindIndex(line => line.Contains(itemHashKey));
            if (indexToDelete == -1)
            {
                MessageBox.Show($"Item with hash key '{itemHashKey}' not found.");
                return;
            }
            lines.RemoveAt(indexToDelete);

            try
            {
                File.WriteAllLines(@"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Level.txt", lines);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Failed to write to file: " + ex.Message);
                return;
            }

            foreach (Panel panel in Map_Tool.Controls.OfType<Panel>())
            {
                Item item = panel.Tag as Item;
                if (item != null && item.hashKey == itemHashKey)
                {
                    Map_Tool.Controls.Remove(panel);
                    break;
                }
            }
            Item_Content.Clear();

            // Clear the hash key
            hash = "";
        }

        private void Item_Editor_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Item_Content_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
