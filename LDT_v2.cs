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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LevelDesignTool
{
    public partial class LDT_v2 : Form
    {
        private const int GridSize = 16;
        private List<Item> OriginalItems = new List<Item>();
        private Panel ActivateItemPanel = null;
        private int SelectedItemType = 0;
        private Panel _CurrentDragPanel;
        private Point _DragOffset;
        private bool _IsDragging;

        public LDT_v2()
        {
            InitializeComponent();
            ReadItemsTypeFile();
            DisplayItemsType();
            InitializeMapToolPanel();
            ReadItemsFile();
            DisplayItems();

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            this.Load += Form_Load;
        }

        #region Display item type in panel Item_List_View
        private void DisplayItemsType()
        {
            int x = 10;
            int y = 10;

            foreach (Item ItemType in OriginalItems)
            {
                PictureBox PictureBox_ = new PictureBox();
                PictureBox_ = ItemType.GenerateImage(new Point(x, y));
                PictureBox_.MouseDown += PictureBox_MouseDown;
                Item_List_View.Controls.Add(PictureBox_);

                x += PictureBox_.Width + 10;
                if (x + PictureBox_.Width > Item_List_View.Width)
                {
                    x = 10;
                    y += PictureBox_.Height + 10;
                }
            }

            Item_List_View.MouseDown += ItemListView_MouseDown;
        }

        private void ReadItemsTypeFile()
        {
            string FilePath = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Items.txt";
            string[] Lines = File.ReadAllLines(FilePath);

            string ImagePath = "";
            Size Size_ = new Size();
            int Type = 0;
            int Length = 0;
            bool IsInBlock = false;

            foreach (string Line in Lines)
            {
                if (Line.StartsWith("{"))
                {
                    ImagePath = "";
                    Size_ = Size.Empty;
                    Type = 0;
                    Length = 0;
                    IsInBlock = true;
                    continue;
                }

                if (Line.StartsWith("}"))
                {
                    IsInBlock = false;

                    if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
                    {
                        Image Image_ = Image.FromFile(ImagePath);
                        Item Item_ = new Item(Image_, Size_, Type, Length);
                        OriginalItems.Add(Item_);
                    }
                    continue;
                }

                if (!IsInBlock) continue;

                if (Line.StartsWith("type:"))
                {
                    Type = int.Parse(Line.Substring(6).Trim());
                }
                else if (Line.StartsWith("path:"))
                {
                    ImagePath = Line.Substring(6).Trim();
                }
                else if (Line.StartsWith("size:"))
                {
                    var SizeParts = Line.Substring(6).Trim().Split(',');
                    Size_ = new Size(int.Parse(SizeParts[0]), int.Parse(SizeParts[1]));
                }
                else if (Line.StartsWith("length:"))
                {
                    Length = int.Parse(Line.Substring(8).Trim());
                }
            }

            foreach (Item Item_ in OriginalItems)
            {
                Item_.ItemClicked += Item_ItemClicked;
            }
        }

        private PictureBox SelectedPictureBox = null;
        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PictureBox PictureBox_ = sender as PictureBox;

                if (SelectedPictureBox != null && SelectedPictureBox != PictureBox_)
                {
                    ControlPaint.DrawBorder(SelectedPictureBox.CreateGraphics(), SelectedPictureBox.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
                }

                if (PictureBox_ != null)
                {
                    ControlPaint.DrawBorder(PictureBox_.CreateGraphics(), PictureBox_.ClientRectangle, Color.Green, ButtonBorderStyle.Solid);
                    SelectedPictureBox = PictureBox_;
                }
            }

            foreach (Control control in Controls)
            {
                if (control is PictureBox pb && pb != SelectedPictureBox)
                {
                    ControlPaint.DrawBorder(pb.CreateGraphics(), pb.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
                }
            }
        }

        private void ItemListView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && SelectedPictureBox != null)
            {
                ControlPaint.DrawBorder(SelectedPictureBox.CreateGraphics(), SelectedPictureBox.ClientRectangle, Color.Red, ButtonBorderStyle.Solid);
                SelectedItemType = 0;
            }
        }
        #endregion

        #region Initialize panel Map_Tool
        private void InitializeMapToolPanel()
        {
            Map_Tool.Paint += Map_Tool_DrawGrid;
            Map_Tool.MouseClick += Map_Tool_MouseClick;
            Map_Tool.AutoScrollMinSize = new Size(3200, 1248);
            Map_Tool.AutoScroll = true;
            Map_Tool.AllowDrop = true;
        }

        private void Item_ItemClicked(object sender, int Type)
        {
            SelectedItemType = Type;
        }

        private void Map_Tool_DrawGrid(object sender, PaintEventArgs e)
        {
            Panel Panel_ = (Panel)sender;
            Graphics g = e.Graphics;
            Pen Pen_ = new Pen(Color.Black);
            int NumRows = 1248 / GridSize;
            int NumCols = 3200 / GridSize;

            for (int i = 0; i <= NumRows; i++)
            {
                int y = i * GridSize;
                g.DrawLine(Pen_, 0, y, NumCols * GridSize, y);
            }
            for (int j = 0; j <= NumCols; j++)
            {
                int x = j * GridSize;
                g.DrawLine(Pen_, x, 0, x, NumRows * GridSize);
            }
            Pen_.Dispose();
        }
        #endregion

        #region Display items in Map_Tool panel
        string FileOpen = "C:\\Users\\Admin\\Desktop\\Game\\Programming Language\\C Sharp\\LevelDesignTool\\Level_Export.txt";
        List<(string, string, string)> ItemInformation = new List<(string, string, string)>();

        private void ReadItemsFile()
        {
            string FilePath = string.Join("@", FileOpen);
            string[] Lines = File.ReadAllLines(FilePath);

            string CurrentName = "";
            string CurrentPosition = "";
            string CurrentWidth = "";

            foreach (string line in Lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (!string.IsNullOrEmpty(CurrentName) && !string.IsNullOrEmpty(CurrentPosition))
                    {
                        ItemInformation.Add((CurrentName, CurrentPosition, CurrentWidth));
                    }
                    CurrentName = line.Trim('[', ']');
                    CurrentPosition = "";
                    CurrentWidth = "";
                }
                else if (line.StartsWith("position"))
                {
                    CurrentPosition = line.Replace("position ", "");
                }
                else if (line.StartsWith("width"))
                {
                    CurrentWidth = line.Replace("width ", "");
                }
            }

            if (!string.IsNullOrEmpty(CurrentName) && !string.IsNullOrEmpty(CurrentPosition))
            {
                ItemInformation.Add((CurrentName, CurrentPosition, CurrentWidth));
            }
        }

        private void DisplayItems()
        {
            foreach (var Item_ in ItemInformation)
            {
                int Type = GetTypeFromName(Item_.Item1);
                Point Position = ConvertPositionToPoint(Item_.Item2);
                int Width_ = GetWidthFromName(Item_.Item3);

                Item OriginalItem = OriginalItems.FirstOrDefault(i => i.Type == Type);
                if (OriginalItem != null)
                {
                    Item NewItem = new Item(OriginalItem.Image_, OriginalItem.Size_, OriginalItem.Type, Width_ > 0 ? Width_ / OriginalItem.Size_.Width : OriginalItem.Length);
                    CreateItemAtLocation(NewItem, Position);
                }
            }
            SetActiveItemPanel(null);
            Item_Content.Clear();
        }

        private Point ConvertPositionToPoint(string Position)
        {
            string[] Parts = Position.Split(' ');
            int x = int.Parse(Parts[0]);
            int y = int.Parse(Parts[1]);
            return new Point(x, y);
        }

        private int GetWidthFromName(string Width_)
        {
            if (Width_ == "") return 0;
            else
            {
                return int.Parse(Width_);
            }
        }
        #endregion

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    DeleteItem();
                    break;
            }
        }

        #region Displays file name in title
        private void Form_Load(object sender, EventArgs e)
        {
            UpdateFormTitle();
        }

        private void UpdateFormTitle(int check = 0)
        {
            string FilePath = FileOpen;

            string[] Values = FilePath.Split('\\');
            if (check != 0)
            {
                this.Text = $"* {Values[Values.Length - 1]}";
                return;
            }
            this.Text = $"{Values[Values.Length - 1]}";
        }
        #endregion

        #region Generate item in Map_Top
        private void Map_Tool_MouseClick(object sender, MouseEventArgs e)
        {
            if (SelectedItemType != 0)
            {
                Item Item_ = OriginalItems.FirstOrDefault(i => i.Type == SelectedItemType);
                if (Item_ != null)
                {
                    Item NewItem = new Item(Item_.Image_, Item_.Size_, Item_.Type, Item_.Length);
                    CreateItemAtLocation(NewItem, e.Location);
                }
            }
            else if (SelectedItemType == 0)
            {
                Item_Content.Clear();
                SetActiveItemPanel(null);
                foreach (Control control in Map_Tool.Controls)
                {
                    if (control is Panel Panel_)
                    {
                        Panel_.Invalidate();
                    }
                }
            }
        }

        private Panel _ActiveItemPanel = null;
        private void CreateItemAtLocation(Item ItemType, Point Location_)
        {
            Point ScrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);
            var AdjustedLocation = new Point(Location_.X + ScrollPosition.X, Location_.Y + ScrollPosition.Y);

            ItemType.StartCollider = new Point(0, 0);
            ItemType.EndCollider = new Point(ItemType.Size_.Width * ItemType.Length + 1, ItemType.Size_.Height + 1);
            ItemType.Position = new Point(AdjustedLocation.X, 1246 - AdjustedLocation.Y - ItemType.Size_.Height);

            Panel ItemPanel = new Panel
            {
                Size = new Size(ItemType.Size_.Width * ItemType.Length + 2, ItemType.Size_.Height + 2),
                Location = Location_,
                Tag = ItemType  // Store the item as a Tag in the Panel
            };

            int OffsetX = 1;
            for (int i = 0; i < ItemType.Length; i++)
            {
                PictureBox PictureBox_ = new PictureBox
                {
                    Image = ItemType.Image_,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = ItemType.Size_,
                    Location = new Point(OffsetX, 1)
                };
                PictureBox_.MouseDown += Item_MouseDown;
                PictureBox_.MouseMove += Item_MouseMove;
                PictureBox_.MouseUp += Item_MouseUp;

                ItemPanel.Controls.Add(PictureBox_);
                OffsetX += ItemType.Size_.Width;
            }

            SetActiveItemPanel(ItemPanel);
            ItemPanel.Paint += (sender, e) =>
            {
                Panel Panel_ = sender as Panel;
                Pen Pen_ = new Pen(Panel_ == _ActiveItemPanel ? Color.Green : Color.Red, 1);
                e.Graphics.DrawRectangle(Pen_, 0, 0, Panel_.Width - 1, Panel_.Height - 1);
            };

            ActivateItemPanel = ItemPanel;
            Map_Tool.Controls.Add(ItemPanel);
            DisplayItemInformation();
        }

        private void SetActiveItemPanel(Panel Panel_)
        {
            _ActiveItemPanel = Panel_;
            if (Panel_ != null)
            {
                Panel_.Invalidate();
            }
            foreach (Control control in Map_Tool.Controls)
            {
                if (control is Panel OtherPanel && OtherPanel != Panel_)
                {
                    OtherPanel.Invalidate(); // Redraw the inactive panels
                }
            }
        }
        #endregion

        #region Display item in textbox (Item_Content)
        private void DisplayItemInformation()
        {
            StringBuilder DisplayText = new StringBuilder();
            Item ActivateItem = ActivateItemPanel.Tag as Item;

            if (ActivateItem != null)
            {
                int Type = ActivateItem.Type;
                if (Type >= 1 && Type <= 33)
                {
                    DisplayText.AppendLine(GetNameFromType(Type));
                }
                DisplayText.AppendLine($"position {ActivateItem.Position.X} {ActivateItem.Position.Y}");
                if (Type == 18 || Type == 33)
                {
                    DisplayText.AppendLine("width " + ActivateItem.Size_.Width * ActivateItem.Length);
                }
            }

            Item_Content.Text = DisplayText.ToString();
        }
        #endregion

        #region Movement item in panel Map_Tool
        private void Item_MouseDown(object sender, MouseEventArgs e)
        {
            Control control = sender as Control;
            _CurrentDragPanel = control as Panel ?? control.Parent as Panel;

            if (_CurrentDragPanel == null) return;

            if (e.Button == MouseButtons.Left)
            {
                _IsDragging = true;
                Point ClientPoint = _CurrentDragPanel.PointToClient(Cursor.Position);
                _DragOffset = ClientPoint;
                _CurrentDragPanel.BringToFront();

                // Display item in textbox (Item_Content)
                if (sender is PictureBox PictureBox_)
                {
                    Item Item_ = PictureBox_.Parent.Tag as Item;
                    if (Item_ != null)
                    {
                        ActivateItemPanel = PictureBox_.Parent as Panel;
                        DisplayItemInformation();
                    }

                    Panel Panel_ = PictureBox_.Parent as Panel;
                    if (Panel_ != null)
                    {
                        SetActiveItemPanel(Panel_);
                    }
                }
            }
        }

        private void Item_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_IsDragging || _CurrentDragPanel == null) return;

            Point ScreenPoint = Cursor.Position;
            Point NewLocation = Map_Tool.PointToClient(ScreenPoint); // Convert to coordinates relative to Map_Tool

            NewLocation.Offset(-_DragOffset.X, -_DragOffset.Y);
            NewLocation.X = Math.Max(0, NewLocation.X);
            NewLocation.Y = Math.Max(0, NewLocation.Y);
            NewLocation.Y = Math.Min(Map_Tool.ClientSize.Height - _CurrentDragPanel.Height, NewLocation.Y);

            _CurrentDragPanel.Location = NewLocation;

            Item Item_ = ActivateItemPanel.Tag as Item;
            Point ScrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);
            var AdjustedLocation = new Point(NewLocation.X + ScrollPosition.X, NewLocation.Y + ScrollPosition.Y);
            Item_.Position = new Point(AdjustedLocation.X, 1246 - AdjustedLocation.Y - Item_.Size_.Height);

            UpdateFormTitle(1);
        }

        private void Item_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _IsDragging = false;
                _CurrentDragPanel = null;
                Map_Tool.Invalidate(); // Redraw the Map_Tool
                DisplayItemInformation();
            }
        }
        #endregion

        #region Functions convert type -> name & name -> type
        private string GetNameFromType(int Type)
        {
            switch (Type)
            {
                case 1:
                    return "[item_bee]";
                case 2:
                    return "[item_bird]";
                case 3:
                    return "[item_boom]";
                case 4:
                    return "[item_box]";
                case 5:
                    return "[item_brick_gold]";
                case 6:
                    return "[item_brick_turquoise]";
                case 7:
                    return "[item_brick_normal]";
                case 8:
                    return "[item_bridge]";
                case 9:
                    return "[item_carnivorous_flower]";
                case 10:
                    return "[item_castle]";
                case 11:
                    return "[item_coin]";
                case 12:
                    return "[item_crab]";
                case 13:
                    return "[item_diamond]";
                case 14:
                    return "[item_fish]";
                case 15:
                    return "[item_flag]";
                case 16:
                    return "[item_flat]";
                case 17:
                    return "[item_frog]";
                case 18:
                    return "[item_ground]";
                case 19:
                    return "[item_island]";
                case 20:
                    return "[item_islet]";
                case 21:
                    return "[item_islet_tall]";
                case 22:
                    return "[item_mario]";
                case 23:
                    return "[item_mario_big]";
                case 24:
                    return "[item_mushroom]";
                case 25:
                    return "[item_octopus]";
                case 26:
                    return "[item_root]";
                case 27:
                    return "[item_shield]";
                case 28:
                    return "[item_snail]";
                case 29:
                    return "[item_snail_shell]";
                case 30:
                    return "[item_spike]";
                case 31:
                    return "[item_spring]";
                case 32:
                    return "[item_vial]";
                case 33:
                    return "[item_water]";
                default:
                    return "";
            }
        }

        private int GetTypeFromName(string Name)
        {
            switch (Name)
            {
                case "item_bee":
                    return 1;
                case "item_bird":
                    return 2;
                case "item_boom":
                    return 3;
                case "item_box":
                    return 4;
                case "item_brick_gold":
                    return 5;
                case "item_brick_turquoise":
                    return 6;
                case "item_brick_normal":
                    return 7;
                case "item_bridge":
                    return 8;
                case "item_carnivorous_flower":
                    return 9;
                case "item_castle":
                    return 10;
                case "item_coin":
                    return 11;
                case "item_crab":
                    return 12;
                case "item_diamond":
                    return 13;
                case "item_fish":
                    return 14;
                case "item_flag":
                    return 15;
                case "item_flat":
                    return 16;
                case "item_frog":
                    return 17;
                case "item_ground":
                    return 18;
                case "item_island":
                    return 19;
                case "item_islet":
                    return 20;
                case "item_islet_tall":
                    return 21;
                case "item_mario":
                    return 22;
                case "item_mario_big":
                    return 23;
                case "item_mushroom":
                    return 24;
                case "item_octopus":
                    return 25;
                case "item_root":
                    return 26;
                case "item_shield":
                    return 27;
                case "item_snail":
                    return 28;
                case "item_snail_shell":
                    return 29;
                case "item_spike":
                    return 30;
                case "item_spring":
                    return 31;
                case "item_vial":
                    return 32;
                case "item_water":
                    return 33;
                default:
                    return 0;
            }
        }
        #endregion

        #region Save file function (button Save file)
        private void ButtonExport_Click(object sender, EventArgs e)
        {
            SaveFile();
            UpdateFormTitle();
        }

        private void SaveFile()
        {
            List<Panel> ItemPanels = new List<Panel>();
            Point ScrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);

            foreach (Control control in Map_Tool.Controls)
            {
                if (control is Panel Panel_ && Panel_.Tag is Item)
                {
                    ItemPanels.Add(Panel_);
                }
            }

            ItemPanels.Sort((x, y) =>
            {
                int Result = x.Top.CompareTo(y.Top);
                return (Result == 0) ? x.Left.CompareTo(y.Left) : Result;
            });

            StringBuilder ExportData = new StringBuilder();

            foreach (Panel Panel_ in ItemPanels)
            {
                if (Panel_.Tag is Item Item_)
                {
                    var AdjustedLocation = new Point(Panel_.Location.X + ScrollPosition.X, Panel_.Location.Y + ScrollPosition.Y);
                    var Size_ = new Size(Item_.Size_.Width, Item_.Size_.Height);
                    int Type = Item_.Type;
                    int Length = Item_.Length;

                    string _ItemPanelInfo = "";
                    if (Type >= 1 && Type <= 33)
                    {
                        ExportData.AppendLine(GetNameFromType(Type));
                        _ItemPanelInfo = $"position {AdjustedLocation.X} {1246 - AdjustedLocation.Y - Size_.Height}";
                        ExportData.AppendLine(_ItemPanelInfo);
                        if (Type == 18 || Type == 33)
                        {
                            int TmpWidth = Item_.Size_.Width * Item_.Length;
                            _ItemPanelInfo = $"width {TmpWidth}";
                            ExportData.AppendLine(_ItemPanelInfo);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Panel's Tag property is not of type Item!", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string ExportFilePathForL = string.Join("@", FileOpen);
            try
            {
                File.WriteAllText(ExportFilePathForL, ExportData.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting item data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        // need modify
        #region Save changes function (button Apply)
        private void Save_Item_Click(object sender, EventArgs e)
        {
            string NewText = Item_Content.Text.Replace(Environment.NewLine, "");

            NewText = NewText.Replace("position ", ", position ");
            NewText = NewText.Replace("width ", ", width ");

            UpdateItem(NewText);
        }

        private void UpdateItem(string NewText)
        {
            Item Item_ = ActivateItemPanel.Tag as Item;
            if (Item_ != null)
            {
                ActivateItemPanel.Controls.Clear();

                string[] Values = NewText.Split(',');
                string[] Tmp;
                if (Values.Length >= 3)
                {
                    MessageBox.Show(Values.Length.ToString());

                    int Length = 1;
                    Tmp = Values[1].Split('(');
                    int.TryParse(Tmp[1].Trim(), out int PosX);
                    Tmp = Values[2].Split(')');
                    int.TryParse(Tmp[0].Trim(), out int PosY);

                    if (Values.Length >= 4)
                    {
                        Tmp = Values[3].Split(" ");
                        int.TryParse(Tmp[2].Trim(), out int _Width);
                        Length = _Width / Item_.Size_.Width;
                    }

                    int OffsetX = 1;
                    for (int i = 0; i < Length; i++)
                    {
                        PictureBox PictureBox_ = new PictureBox
                        {
                            Image = Item_.Image_,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Size = new Size(Item_.Size_.Width, Item_.Size_.Height),
                            Location = new Point(OffsetX, 1)
                        };
                        PictureBox_.MouseDown += Item_MouseDown;
                        PictureBox_.MouseMove += Item_MouseMove;
                        PictureBox_.MouseUp += Item_MouseUp;

                        ActivateItemPanel.Controls.Add(PictureBox_);
                        OffsetX += Item_.Size_.Width;
                    }
                    // Update Tag
                    Item_.Length = Length;
                    Item_.EndCollider = new Point(Item_.Size_.Width * Length + 1, Height + 1);
                    Item_.Position = new Point(PosX, PosY);

                    ActivateItemPanel.Width = Item_.Size_.Width * Length + 2;
                    Point scrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);
                    ActivateItemPanel.Location = new Point(PosX - scrollPosition.X, (1246 - Item_.Size_.Height - PosY - scrollPosition.Y));
                    ActivateItemPanel.Refresh();
                }
            }
        }
        #endregion

        #region Delete function (button Delete)
        private void Delete_Item_Click(object sender, EventArgs e)
        {
            DeleteItem();
        }

        private void DeleteItem()
        {
            Map_Tool.Controls.Remove(ActivateItemPanel);
            Map_Tool.Refresh();
            Item_Content.Clear();
        }
        #endregion

        #region Load file function (button Load file)
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = @"C:\Users\Admin\Desktop\Game\Programming Language\C Sharp\LevelDesignTool\Level_Export\";
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFolderPath = Path.GetDirectoryName(openFileDialog.FileName);
                FileOpen = string.Join("\n", Directory.GetFiles(selectedFolderPath));

                ItemInformation.Clear();
                Map_Tool.Controls.Clear();
                UpdateFormTitle();
                ReadItemsFile();
                DisplayItems();
            }
        }
        #endregion

        #region Not used
        private void Edit_Item_Click(object sender, EventArgs e)
        {

        }

        private void Item_Editor_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Item_Content_TextChanged(object sender, EventArgs e)
        {

        }

        private void Item_List_View_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Map_Tool_Paint(object sender, PaintEventArgs e)
        {

        }
        #endregion
    }
}