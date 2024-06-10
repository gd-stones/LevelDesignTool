using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace LevelDesignTool
{
    public partial class LDT_v2 : Form
    {
        int SwitchModeCommittingAndEditing = 2; // 2 = Editing && 1 = Committing; Can only be set to 1 or 2

        const int GridSize = 45;
        const int MapToolWidth = 31500;
        const int MapToolHeight = 990;

        List<Item> OriginalItems = new List<Item>();
        List<(string, string, string, string, string, string)> ItemInformation = new List<(string, string, string, string, string, string)>();
        Panel? ActivateItemPanel = null;
        Panel? CurrentDragPanel;
        PictureBox? SelectedPictureBox = null;
        Point DragOffset;

        string ProjectDirectory;
        string FileOpen;
        bool IsDragging;
        bool IsF1KeyPress = false;
        bool MoveItemUsingKeysFlag = true;
        int SelectedItemType = 0;
        int OlderSelectedItemType = 0;

        int Tier1PosYGround_BottomLeft = 90;
        int Tier2PosYGround_BottomLeft = 180;
        int Tier3PosYGround_BottomLeft = 270;
        int Tier4PosYGround_BottomLeft = 362;
        int Tier5PosYGround_BottomLeft = 452;
        int Tier6PosYGround_BottomLeft = 543;

        int Tier1PosYGround_TopLeft;
        int Tier2PosYGround_TopLeft;
        int Tier3PosYGround_TopLeft;
        int Tier4PosYGround_TopLeft;
        int Tier5PosYGround_TopLeft;
        int Tier6PosYGround_TopLeft;

        int Tier1PosYWater_BottomLeft = 64;
        int Tier2PosYWater_BottomLeft = 115;
        int Tier1PosYWater_TopLeft;
        int Tier2PosYWater_TopLeft;

        int Border = 3;
        int AdjHeight = 985; // MapToolHeight - 2 * Border + 1

        bool FlagMoveItemAsSoonAsItIsCreated = false;
        int HorizontalScrollPosition = 0;
        string OptionalData = "[map]\nskin 1\n";

        System.Windows.Forms.Timer timer;

#pragma warning disable CS8618
        public LDT_v2()
#pragma warning restore CS8618
        {
            InitializeComponent();
            InitializeForm();
            GetProjectDirectory();

            ReadItemsTypeFile();
            DisplayItemsType();

            InitializePanelMapTool();
            ReadItemsFile();
            DisplayItems();



            TextboxItemContent.MouseClick += TextboxItemContent_MouseClick;
        }

        #region Initialize Form
        void InitializeForm()
        {
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            KeyDown += MainForm_KeyDown;
            KeyUp += MainForm_KeyUp;
            Load += Form_Load;
        }

        void GetProjectDirectory()
        {
            if (SwitchModeCommittingAndEditing == 2)
#pragma warning disable CS8602
                ProjectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            else if (SwitchModeCommittingAndEditing == 1)
                ProjectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
        }
        #endregion

        #region Display item type in panel PanelItemListView
        void DisplayItemsType()
        {
            int x = 10;
            int y = 10;

            foreach (Item ItemType in OriginalItems)
            {
                PictureBox Pb = new PictureBox();
                Pb = ItemType.GenerateImage(new Point(x, y));
                Pb.MouseDown += PictureBox_MouseDown;
                PanelItemListView.Controls.Add(Pb);

                x += Pb.Width + 10;
                if (x + Pb.Width > PanelItemListView.Width)
                {
                    x = 10;
                    y += Pb.Height + 10;
                }
            }

            PanelItemListView.MouseDown += ItemListView_MouseDown;
        }

        void ReadItemsTypeFile()
        {
            string FilePath = "";

            if (SwitchModeCommittingAndEditing == 2)
                FilePath = Path.Combine(ProjectDirectory, "item\\item.txt");
            else if (SwitchModeCommittingAndEditing == 1)
                FilePath = Path.Combine(ProjectDirectory, "Map Editor\\item\\item.txt");

            string[] Lines = File.ReadAllLines(FilePath);
            int Type = 0;
            string ImagePath = "";
            Size Sz = new Size();
            int Length = 0;
            string AnchorPoint = "";
            string AdditionalProperties = "";
            bool IsInBlock = false;

            foreach (string Line in Lines)
            {
                if (Line.StartsWith("{"))
                {
                    ImagePath = "";
                    Sz = Size.Empty;
                    Type = 0;
                    Length = 0;
                    AnchorPoint = "";
                    AdditionalProperties = "";
                    IsInBlock = true;
                    continue;
                }

                if (Line.StartsWith("}"))
                {
                    IsInBlock = false;

                    if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
                    {
                        Image Img = Image.FromFile(ImagePath);
                        Item It = new Item(Img, Sz, Type, Length, AnchorPoint);
                        if (AdditionalProperties != "")
                            It.AdditionalProperties = AdditionalProperties;
                        OriginalItems.Add(It);
                    }
                    continue;
                }

                if (!IsInBlock) continue;

                if (Line.StartsWith("type:"))
                    Type = int.Parse(Line.Substring(6).Trim());
                else if (Line.StartsWith("path:"))
                {
                    ImagePath = Line.Substring(6).Trim();

                    if (SwitchModeCommittingAndEditing == 2)
                        ImagePath = Path.Combine(ProjectDirectory, ImagePath);

                    else if (SwitchModeCommittingAndEditing == 1)
                        ImagePath = Path.Combine(ProjectDirectory, "Map Editor", ImagePath);
                }
                else if (Line.StartsWith("size:"))
                {
                    var SizeParts = Line.Substring(6).Trim().Split(',');
                    Sz = new Size(int.Parse(SizeParts[0]), int.Parse(SizeParts[1]));
                }
                else if (Line.StartsWith("length:"))
                    Length = int.Parse(Line.Substring(8).Trim());
                else if (Line.StartsWith("anchor_point:"))
                    AnchorPoint = Line.Substring(14).Trim();
                else if (Line.StartsWith("additional_properties:"))
                    AdditionalProperties = Line.Substring(23).Trim();
            }

            foreach (Item It in OriginalItems)
            {
                It.ItemClicked += Item_ItemClicked;
            }
        }

        void DrawBorderForItemType(PictureBox Pb, string Tinge)
        {
            int BorderThickness = Border;

            ControlPaint.DrawBorder(Pb.CreateGraphics(), new Rectangle(0, 0, Pb.Width, Pb.Height),
                Tinge == "red" ? Color.Red : Color.Green, BorderThickness, ButtonBorderStyle.Solid,
                Tinge == "red" ? Color.Red : Color.Green, BorderThickness, ButtonBorderStyle.Solid,
                Tinge == "red" ? Color.Red : Color.Green, BorderThickness, ButtonBorderStyle.Solid,
                Tinge == "red" ? Color.Red : Color.Green, BorderThickness, ButtonBorderStyle.Solid);
        }

        void PictureBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PictureBox? Pb = sender as PictureBox;

                if (SelectedPictureBox != null && SelectedPictureBox != Pb)
                    DrawBorderForItemType(SelectedPictureBox, "red");

                if (Pb != null)
                {
                    DrawBorderForItemType(Pb, "green");
                    SelectedPictureBox = Pb;
                }
            }

            foreach (Control control in Controls)
            {
                if (control is PictureBox Pb && Pb != SelectedPictureBox)
                    DrawBorderForItemType(Pb, "red");
            }
        }

        void ItemListView_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && SelectedPictureBox != null)
            {
                DrawBorderForItemType(SelectedPictureBox, "red");
                SelectedItemType = 0;
            }
        }
        #endregion

        #region Initialize PanelMapTool
        void InitializePanelMapTool()
        {
            PanelMapTool.Paint += PanelMapTool_DrawGrid;

            Tier1PosYGround_TopLeft = MapToolHeight - Tier1PosYGround_BottomLeft - 2 * Border + 1;
            Tier2PosYGround_TopLeft = MapToolHeight - Tier2PosYGround_BottomLeft - 2 * Border + 1;
            Tier3PosYGround_TopLeft = MapToolHeight - Tier3PosYGround_BottomLeft - 2 * Border + 1;
            Tier4PosYGround_TopLeft = MapToolHeight - Tier4PosYGround_BottomLeft - 2 * Border + 1;
            Tier5PosYGround_TopLeft = MapToolHeight - Tier5PosYGround_BottomLeft - 2 * Border + 1;
            Tier6PosYGround_TopLeft = MapToolHeight - Tier6PosYGround_BottomLeft - 2 * Border + 1;

            Tier1PosYWater_TopLeft = MapToolHeight - Tier1PosYWater_BottomLeft - 2 * Border + 1;
            Tier2PosYWater_TopLeft = MapToolHeight - Tier2PosYWater_BottomLeft - 2 * Border + 1;

            PanelMapTool.Paint += DrawLandAndWaterBoundaries;

            PanelMapTool.MouseDown += PanelMapTool_MouseDown;
            PanelMapTool.MouseUp += PanelMapTool_MouseUp;
            PanelMapTool.MouseMove += PanelMapTool_MouseMove;

            PanelMapTool.AutoScrollMinSize = new Size(MapToolWidth, MapToolHeight);
            PanelMapTool.AutoScroll = true;
            PanelMapTool.AllowDrop = true;

            PanelMapTool.MouseWheel += PanelMapTool_MouseWheel;
            PanelMapTool.Scroll += PanelMapTool_Scroll;
        }

        void Item_ItemClicked(object? sender, int Type)
        {
            SelectedItemType = Type;
            OlderSelectedItemType = Type;
        }

        void DrawLandAndWaterBoundaries(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen p1 = new Pen(Color.Brown);
            Pen p2 = new Pen(Color.LightSkyBlue);

            g.DrawLine(p1, 0, Tier1PosYGround_TopLeft, MapToolWidth, Tier1PosYGround_TopLeft);
            g.DrawLine(p1, 0, Tier2PosYGround_TopLeft, MapToolWidth, Tier2PosYGround_TopLeft);
            g.DrawLine(p1, 0, Tier3PosYGround_TopLeft, MapToolWidth, Tier3PosYGround_TopLeft);
            g.DrawLine(p1, 0, Tier4PosYGround_TopLeft, MapToolWidth, Tier4PosYGround_TopLeft);
            g.DrawLine(p1, 0, Tier5PosYGround_TopLeft, MapToolWidth, Tier5PosYGround_TopLeft);
            g.DrawLine(p1, 0, Tier6PosYGround_TopLeft, MapToolWidth, Tier6PosYGround_TopLeft);

            g.DrawLine(p2, 0, Tier1PosYWater_TopLeft, MapToolWidth, Tier1PosYWater_TopLeft);
            g.DrawLine(p2, 0, Tier2PosYWater_TopLeft, MapToolWidth, Tier2PosYWater_TopLeft);

            p1.Dispose();
            p2.Dispose();
        }

        void PanelMapTool_DrawGrid(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen p = new Pen(Color.Black);
            int NumRows = MapToolHeight / GridSize;
            int NumCols = MapToolWidth / GridSize;

            for (int i = 0; i <= NumRows; i++)
            {
                int y = i * GridSize;
                g.DrawLine(p, 0, y, NumCols * GridSize, y);
            }
            for (int j = 0; j <= NumCols; j++)
            {
                int x = j * GridSize;
                g.DrawLine(p, x, 0, x, NumRows * GridSize);
            }
            p.Dispose();
        }

        void PanelMapTool_MouseWheel(object? sender, MouseEventArgs e)
        {
            int ScrollDirection = e.Delta > 0 ? -1 : 1;
            int NewScrollPosition = HorizontalScrollPosition + (GridSize * ScrollDirection);

            HorizontalScrollPosition = Math.Max(0, Math.Min(NewScrollPosition, MapToolWidth - PanelMapTool.Width));
            PanelMapTool.AutoScrollPosition = new Point(HorizontalScrollPosition, 0);
        }

        void PanelMapTool_Scroll(object? sender, ScrollEventArgs e)
        {
            int scrollChange = e.NewValue - e.OldValue;
            int scrollAmount = Math.Sign(scrollChange) * GridSize;

            HorizontalScrollPosition = Math.Max(0, Math.Min(HorizontalScrollPosition + scrollAmount, MapToolWidth - PanelMapTool.Width));
            PanelMapTool.AutoScrollPosition = new Point(HorizontalScrollPosition, 0);
        }
        #endregion

        #region Display items in PanelMapTool
        bool CheckAddProp(string Line)
        {
            string[] AddProp = { "wood", /*"gold",*/ "unbreakable", "island", "islet", "shell", "rotate", "red",
                "gray", "rock", "golem", "king_kong", "ice_monster", "island_rock", "islet_rock", "maybug",
                "monkey", "long", "rock", "scorpion", "spider"};

            foreach (string l in AddProp)
            {
                if (Line == l) return true;
            }

            return false;
        }

        void ExtractItemInformation(string[] Lines)
        {
            ItemInformation.Clear();

            string CurrentName = "";
            string CurrentPosition = "";
            string CurrentWidth = "";
            string CurrentItemAttached = "";
            string CurrentAddProp = "";
            string CurrentOtherNotes = "";

            foreach (string line in Lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (!string.IsNullOrEmpty(CurrentName) && !string.IsNullOrEmpty(CurrentPosition))
                        ItemInformation.Add((CurrentName, CurrentPosition, CurrentWidth, CurrentItemAttached, CurrentAddProp, CurrentOtherNotes));

                    CurrentName = line.Trim('[', ']');
                    CurrentPosition = "";
                    CurrentWidth = "";
                    CurrentItemAttached = "";
                    CurrentAddProp = "";
                    CurrentOtherNotes = "";
                }
                else if (line.StartsWith("position"))
                    CurrentPosition = line.Replace("position ", "");
                else if (line.StartsWith("width"))
                    CurrentWidth = line.Replace("width ", "");
                else if (line.StartsWith("bomb") || line.StartsWith("coin") || line.StartsWith("diamond") || line.StartsWith("vial") || line.StartsWith("shield"))
                    CurrentItemAttached = line;
                else if (line.StartsWith("gold") || CheckAddProp(line))
                    CurrentAddProp = line;
                else if (line != "" && line != "/n")
                    CurrentOtherNotes = CurrentOtherNotes + line + " - ";
            }

            if (!string.IsNullOrEmpty(CurrentName) && !string.IsNullOrEmpty(CurrentPosition))
                ItemInformation.Add((CurrentName, CurrentPosition, CurrentWidth, CurrentItemAttached, CurrentAddProp, CurrentOtherNotes));
        }

        void ReadItemsFile(int Flag = 0)
        {
            if (Flag == 0)
            {
                if (SwitchModeCommittingAndEditing == 2)
                    FileOpen = Path.Combine(ProjectDirectory, "level\\level_001_1.txt");
                else if (SwitchModeCommittingAndEditing == 1)
                    FileOpen = Path.Combine(ProjectDirectory, "Map Editor\\level\\level_001_1.txt");
            }

            string[] Lines = File.ReadAllLines(FileOpen);
            ExtractItemInformation(Lines);
        }

        void DisplayItems()
        {
            foreach (var It in ItemInformation)
            {
                int Type = GetTypeFromName(It.Item1, It.Item5);
                Point Position = ConvertPositionToPoint(It.Item2);
                int Width = GetWidthFromName(It.Item3);
                string Attached = It.Item4;
                string AddProp = It.Item5;
                string OtherNotes = It.Item6;

                Item? OriginalItem = OriginalItems.FirstOrDefault(i => i.Type == Type);
                if (OriginalItem != null)
                {
                    Item NewItem = new Item(OriginalItem.Img, OriginalItem.Sz, OriginalItem.Type,
                        Width > 0 ? Width / OriginalItem.Sz.Width : OriginalItem.Length, OriginalItem.AnchorPoint);

                    if (AddProp != "") NewItem.AdditionalProperties = AddProp;

                    if (Attached != "") NewItem.ItemAttached = Attached;

                    if (OtherNotes != "") NewItem.OtherNotes = OtherNotes;

                    CreateItemAtLocation(NewItem, Position, 0);
                }
            }

            SetActiveItemPanel(null);
            TextboxItemContent.Clear();
            PanelMapTool.Focus();
        }

        Point ConvertPositionToPoint(string Position)
        {
            string[] Parts = Position.Split(' ');
            int x = int.Parse(Parts[0]);
            int y = int.Parse(Parts[1]);

            return new Point(x, y);
        }

        int GetWidthFromName(string Width)
        {
            if (Width == "") return 0;
            else return int.Parse(Width);
        }
        #endregion

        #region Generate item in PanelMapTool
        Point ConvertRelativePosToAbsolutePos(Point RelativePos, int SzWidth, int SzHeight, string AnchorPoint = "center")
        {
            Point ScrollPosition = new Point(-PanelMapTool.AutoScrollPosition.X, -PanelMapTool.AutoScrollPosition.Y);
            Point AbsoluteLocation;

            if (AnchorPoint == "top_left")
                AbsoluteLocation = new Point(RelativePos.X + ScrollPosition.X, AdjHeight - (RelativePos.Y + ScrollPosition.Y));
            else if (AnchorPoint == "top_center")
                AbsoluteLocation = new Point(RelativePos.X + ScrollPosition.X + SzWidth / 2, AdjHeight - (RelativePos.Y + ScrollPosition.Y));
            else if (AnchorPoint == "middle_bottom")
                AbsoluteLocation = new Point(RelativePos.X + ScrollPosition.X + SzWidth / 2, AdjHeight - (RelativePos.Y + ScrollPosition.Y + SzHeight));
            else
                AbsoluteLocation = new Point(RelativePos.X + ScrollPosition.X + SzWidth / 2, AdjHeight - (RelativePos.Y + ScrollPosition.Y + SzHeight / 2));

            return AbsoluteLocation;
        }

        Point ConvertAbsolutePosToRelativePos(Point AbsolutePos, int SzWidth, int SzHeight, string AnchorPoint = "center")
        {
            Point ScrollPosition = new Point(-PanelMapTool.AutoScrollPosition.X, -PanelMapTool.AutoScrollPosition.Y);
            Point RelativeLocation;

            if (AnchorPoint == "top_left")
                RelativeLocation = new Point(AbsolutePos.X - ScrollPosition.X, AdjHeight - (AbsolutePos.Y - ScrollPosition.Y));
            else if (AnchorPoint == "top_center")
                RelativeLocation = new Point(AbsolutePos.X - ScrollPosition.X - SzWidth / 2, AdjHeight - (AbsolutePos.Y - ScrollPosition.Y));
            else if (AnchorPoint == "middle_bottom")
                RelativeLocation = new Point(AbsolutePos.X - ScrollPosition.X - SzWidth / 2, AdjHeight - (AbsolutePos.Y - ScrollPosition.Y + SzHeight));
            else
                RelativeLocation = new Point(AbsolutePos.X - ScrollPosition.X - SzWidth / 2, AdjHeight - (AbsolutePos.Y - ScrollPosition.Y + SzHeight / 2));

            return RelativeLocation;
        }

        void PanelMapTool_MouseDown(object? sender, MouseEventArgs e)
        {
            if (SelectedItemType != 0)
            {
                Item? It = OriginalItems.FirstOrDefault(i => i.Type == SelectedItemType);

                if (It != null)
                {
                    Item NewItem = new Item(It.Img, It.Sz, It.Type, It.Length, It.AnchorPoint);

                    if (It.AdditionalProperties != "")
                        NewItem.AdditionalProperties = It.AdditionalProperties;

                    FlagMoveItemAsSoonAsItIsCreated = true;

                    if (It.AnchorPoint == "center")
                        CreateItemAtLocation(NewItem, new Point(e.Location.X - It.Sz.Width / 2, e.Location.Y - It.Sz.Height / 2));
                    else if (It.AnchorPoint == "top_center")
                        CreateItemAtLocation(NewItem, new Point(e.Location.X - It.Sz.Width / 2, e.Location.Y));
                    else if (It.AnchorPoint == "middle_bottom")
                        CreateItemAtLocation(NewItem, new Point(e.Location.X - It.Sz.Width / 2, e.Location.Y - It.Sz.Height));
                    else
                        CreateItemAtLocation(NewItem, e.Location);
                }

                SelectedItemType = 0;

                if (SelectedPictureBox != null)
                    DrawBorderForItemType(SelectedPictureBox, "red");
            }
            else if (SelectedItemType == 0)
            {
                TextboxItemContent.Clear();
                PanelMapTool.Focus();
                SetActiveItemPanel(null);
            }

            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        void PanelMapTool_MouseUp(object? sender, MouseEventArgs e)
        {
            FlagMoveItemAsSoonAsItIsCreated = false;
            MouseUpp();
        }

        void PanelMapTool_MouseMove(object? sender, MouseEventArgs e)
        {
            MouseDrag();
        }

        PictureBox CreatePictureBoxItem(Item ItemType, int PosX)
        {
            PictureBox Pb = new PictureBox
            {
                Image = ItemType.Img,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(ItemType.Sz.Width, ItemType.Sz.Height),
                Location = new Point(PosX, Border)
            };
            Pb.MouseDown += Item_MouseDown;
            Pb.MouseMove += Item_MouseMove;
            Pb.MouseUp += Item_MouseUp;

            return Pb;
        }

        PictureBox CreateItemAttached(string Attached)
        {
            PictureBox? Pb = null;

            if (Attached.StartsWith("coin")) Attached = "coin";

            if (Attached != "")
            {
                int AttType = 0;
                switch (Attached)
                {
                    case "bomb":
                        AttType = 21;
                        break;
                    case "coin":
                        AttType = 22;
                        break;
                    case "diamond":
                        AttType = 23;
                        break;
                    case "shield":
                        AttType = 24;
                        break;
                    case "vial":
                        AttType = 25;
                        break;
                    default:
                        AttType = 0;
                        break;
                }

                Item? ItemAtt = OriginalItems.FirstOrDefault(i => i.Type == AttType);

                if (ItemAtt != null) Pb = CreatePictureBoxItem(ItemAtt, Border);
            }

#pragma warning disable CS8603
            return Pb;
        }

        void CreateItemAtLocation(Item NewItem, Point Pos, int Flag = 1)
        {
            if (Flag != 1)
            {
                PanelMapTool.AutoScrollPosition = new Point(0, 0);
                NewItem.Position = Pos;
                Pos = ConvertAbsolutePosToRelativePos(Pos, NewItem.Sz.Width, NewItem.Sz.Height, NewItem.AnchorPoint);
            }
            else
            {
                if (NewItem.Type == 11 || NewItem.Type == 12 || NewItem.Type == 13 || NewItem.Type == 14)
                    Pos = new Point(((-PanelMapTool.AutoScrollPosition.X + Pos.X) / 45) * 45 + PanelMapTool.AutoScrollPosition.X, (Pos.Y / 45) * 45);
                else if (NewItem.Type == 22)
                    Pos = new Point(((-PanelMapTool.AutoScrollPosition.X + Pos.X) / 45) * 45 + PanelMapTool.AutoScrollPosition.X + 17, (Pos.Y / 45) * 45 + 12);

                NewItem.Position = ConvertRelativePosToAbsolutePos(Pos, NewItem.Sz.Width, NewItem.Sz.Height, NewItem.AnchorPoint);
            }

            Panel ItemPanel = new Panel
            {
                Size = new Size(NewItem.Sz.Width * NewItem.Length + Border * 2, NewItem.Sz.Height + Border * 2),
                Location = Pos, // Position relative
                Tag = NewItem  // Store the item as a Tag in the Panel
            };

            if (NewItem.ItemAttached != "") ItemPanel.Controls.Add(CreateItemAttached(NewItem.ItemAttached));

            if (NewItem.Type == 34 && NewItem.OtherNotes != "") ItemPanel.Controls.Add(AssignAddressToRoot(NewItem.OtherNotes));

            SetActiveItemPanel(ItemPanel);
            ItemPanel.Paint += (sender, e) =>
            {
                int BorderThickness = Border;

                ControlPaint.DrawBorder(e.Graphics, new Rectangle(0, 0, ItemPanel.Width, ItemPanel.Height),
                    ItemPanel == ActivateItemPanel ? Color.Green : Color.Red, BorderThickness, ButtonBorderStyle.Solid,
                    ItemPanel == ActivateItemPanel ? Color.Green : Color.Red, BorderThickness, ButtonBorderStyle.Solid,
                    ItemPanel == ActivateItemPanel ? Color.Green : Color.Red, BorderThickness, ButtonBorderStyle.Solid,
                    ItemPanel == ActivateItemPanel ? Color.Green : Color.Red, BorderThickness, ButtonBorderStyle.Solid);
            };

            int OffsetX = Border;
            for (int i = 0; i < NewItem.Length; i++)
            {
                PictureBox? Pb = CreatePictureBoxItem(NewItem, OffsetX);
                ItemPanel.Controls.Add(Pb);
                OffsetX += NewItem.Sz.Width;
            }

            PanelMapTool.Controls.Add(ItemPanel);
            ItemPanel.BringToFront();
            DisplayItemInformation();

            if (FlagMoveItemAsSoonAsItIsCreated)
            {
                CurrentDragPanel = ItemPanel;
                IsDragging = true;
            }
        }

        void SetActiveItemPanel(Panel? Pn)
        {
            ActivateItemPanel = Pn;
            if (Pn != null) Pn.Invalidate();

            foreach (Control control in PanelMapTool.Controls)
            {
                if (control is Panel OtherPanel && OtherPanel != Pn)
                    OtherPanel.Invalidate();
            }
        }
        #endregion

        #region Movement item in PanelMapTool
        int FindWallOrGroundLocationClosestPlayerOrMonster(Item ItemCheck, Point NewLocation)
        {
            int[] ListGroundType = { 1, 2, 3, 4, 31 };
            int[] ListBrickType = { 11, 12, 13, 14 };
            int VerticalThreshold = 20;
            int HorizontalThresholdWall = 50;
            NewLocation.Y += (ItemCheck.Sz.Height + 2 * Border);

            foreach (Control control in PanelMapTool.Controls)
            {
                if (control is Panel && control.Tag is Item ExistingItem)
                {
                    if (ExistingItem == ItemCheck) continue;

                    foreach (int t in ListBrickType)
                    {
                        if (ExistingItem.Type == t)
                        {
                            Point ExistingItem_TopLeft = ConvertAbsolutePosToRelativePos(ExistingItem.Position, ExistingItem.Sz.Width, ExistingItem.Sz.Height);
                            int VerticalDistance = Math.Abs(NewLocation.Y - ExistingItem_TopLeft.Y);
                            int HorizontalDistance = Math.Abs(NewLocation.X - ExistingItem_TopLeft.X);

                            if (VerticalDistance < VerticalThreshold && HorizontalDistance < HorizontalThresholdWall)
                                return ExistingItem_TopLeft.Y - ItemCheck.Sz.Height - 2 * Border;
                        }
                    }

                    foreach (int t in ListGroundType)
                    {
                        if (ExistingItem.Type == t)
                        {
                            Point ExistingItem_TopLeft = ConvertAbsolutePosToRelativePos(ExistingItem.Position, ExistingItem.Sz.Width, ExistingItem.Sz.Height, ExistingItem.AnchorPoint);
                            int VerticalDistance = Math.Abs(NewLocation.Y - ExistingItem_TopLeft.Y);
                            bool Check = false;

                            if ((NewLocation.X + ItemCheck.Sz.Width) > ExistingItem_TopLeft.X && NewLocation.X < ExistingItem_TopLeft.X + ExistingItem.Sz.Width * ExistingItem.Length)
                                Check = true;

                            if (VerticalDistance < VerticalThreshold && Check)
                                return ExistingItem_TopLeft.Y - ItemCheck.Sz.Height - 2 * Border;
                        }
                    }
                }
            }

            return 0;
        }

        int FindWaterOrGroundLocationClosestGroundOrWater(Item ItemCheck, Point NewLocation)
        {
            int[] ListGroundAndWaterType = { 1, 2 };
            int HorizontalThreshold = 30;
            int VerticalThreshold = 500;

            foreach (Control control in PanelMapTool.Controls)
            {
                if (control is Panel && control.Tag is Item ExistingItem)
                {
                    if (ExistingItem == ItemCheck)
                        continue;

                    foreach (int t in ListGroundAndWaterType)
                    {
                        if (ExistingItem.Type == t)
                        {
                            Point ExistingItem_TopLeft = ConvertAbsolutePosToRelativePos(ExistingItem.Position, ExistingItem.Sz.Width, ExistingItem.Sz.Height, "top_left");
                            int VerticalDistance = Math.Abs(NewLocation.Y - ExistingItem_TopLeft.Y);
                            int HorizontalDistance = Math.Abs(NewLocation.X - (ExistingItem_TopLeft.X + ExistingItem.Sz.Width * ExistingItem.Length + Border));

                            if (HorizontalDistance < HorizontalThreshold && VerticalDistance < VerticalThreshold)
                            {
                                return ExistingItem_TopLeft.X + ExistingItem.Sz.Width * ExistingItem.Length + 2 * Border;
                            }

                            HorizontalDistance = Math.Abs(NewLocation.X + ItemCheck.Length * ItemCheck.Sz.Width + 2 * Border - ExistingItem_TopLeft.X);
                            if (HorizontalDistance < HorizontalThreshold && VerticalDistance < VerticalThreshold)
                            {
                                return ExistingItem_TopLeft.X - (ItemCheck.Sz.Width * ItemCheck.Length + 2 * Border);
                            }
                        }
                    }
                }
            }

            return 0;
        }

        void MouseDrag()
        {
            if (!IsDragging || CurrentDragPanel == null) return;

            Item? It = CurrentDragPanel.Tag as Item;
            Point ScreenPoint;

            if (FlagMoveItemAsSoonAsItIsCreated)
            {
                if (It.AnchorPoint == "center")
                    ScreenPoint = new Point(Cursor.Position.X - It.Sz.Width / 2, Cursor.Position.Y - It.Sz.Height / 2);
                else if (It.AnchorPoint == "top_center")
                    ScreenPoint = new Point(Cursor.Position.X - It.Sz.Width / 2, Cursor.Position.Y);
                else if (It.AnchorPoint == "middle_bottom")
                    ScreenPoint = new Point(Cursor.Position.X - It.Sz.Width / 2, Cursor.Position.Y - It.Sz.Height);
                else
                    ScreenPoint = Cursor.Position;
            }
            else ScreenPoint = Cursor.Position;

            Point NewLocation = PanelMapTool.PointToClient(ScreenPoint); // Convert to coordinates relative to PanelMapTool
            int Type = It.Type;

            NewLocation.Offset(-DragOffset.X, -DragOffset.Y);
            /*NewLocation.Y = Math.Max(0, NewLocation.Y);
            NewLocation.Y = Math.Min(PanelMapTool.ClientSize.Height - CurrentDragPanel.Height, NewLocation.Y);*/

            if (!IsF1KeyPress)
            {
                bool Check1 = false;
                int[] ListAnchorPointEqualMiddleBottom = { 100, 32, 33, 34, 35, 41, 44, 46, 48, 61, 63, 64, 71, 72, 82 };

                foreach (int i in ListAnchorPointEqualMiddleBottom)
                {
                    if (Type == i)
                    {
                        Check1 = true;
                        break;
                    }
                }

                int DeltaPosY = 30;
                if (Check1)
                {
                    int Y = FindWallOrGroundLocationClosestPlayerOrMonster(It, NewLocation);
                    if (Y != 0) NewLocation.Y = Y;
                }
                else if (Type == 1)
                {
                    if (NewLocation.Y < Tier1PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier1PosYGround_TopLeft - DeltaPosY)
                        NewLocation.Y = Tier1PosYGround_TopLeft;

                    if (NewLocation.Y < Tier2PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier2PosYGround_TopLeft - DeltaPosY)
                        NewLocation.Y = Tier2PosYGround_TopLeft;

                    if (NewLocation.Y < Tier3PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier3PosYGround_TopLeft - DeltaPosY)
                        NewLocation.Y = Tier3PosYGround_TopLeft;

                    if (NewLocation.Y < Tier4PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier4PosYGround_TopLeft - DeltaPosY)
                        NewLocation.Y = Tier4PosYGround_TopLeft;

                    if (NewLocation.Y < Tier5PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier5PosYGround_TopLeft - DeltaPosY)
                        NewLocation.Y = Tier5PosYGround_TopLeft;

                    if (NewLocation.Y < Tier6PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier6PosYGround_TopLeft - DeltaPosY)
                        NewLocation.Y = Tier6PosYGround_TopLeft;

                    int X = FindWaterOrGroundLocationClosestGroundOrWater(It, NewLocation);
                    if (X != 0) NewLocation.X = X;
                }
                else if (Type == 2)
                {
                    if (NewLocation.Y < Tier1PosYWater_TopLeft + DeltaPosY && NewLocation.Y > Tier1PosYWater_TopLeft - DeltaPosY)
                        NewLocation.Y = Tier1PosYWater_TopLeft;

                    if (NewLocation.Y < Tier2PosYWater_TopLeft + DeltaPosY && NewLocation.Y > Tier2PosYWater_TopLeft - DeltaPosY)
                        NewLocation.Y = Tier2PosYWater_TopLeft;

                    int X = FindWaterOrGroundLocationClosestGroundOrWater(It, NewLocation);
                    if (X != 0) NewLocation.X = X;
                }
            }

            if (Type == 11 || Type == 12 || Type == 13 || Type == 14)
                NewLocation = new Point(((-PanelMapTool.AutoScrollPosition.X + NewLocation.X) / 45) * 45 + PanelMapTool.AutoScrollPosition.X, (NewLocation.Y / 45) * 45);
            else if (Type == 22)
                NewLocation = new Point(((-PanelMapTool.AutoScrollPosition.X + NewLocation.X) / 45) * 45 + PanelMapTool.AutoScrollPosition.X + 17, (NewLocation.Y / 45) * 45 + 12);

            CurrentDragPanel.Location = NewLocation;
            It.Position = ConvertRelativePosToAbsolutePos(NewLocation, It.Sz.Width, It.Sz.Height, It.AnchorPoint);

            DisplayItemInformation();
            UpdateFormTitle(0);
        }

        void MouseUpp()
        {
            IsDragging = false;
            CurrentDragPanel = null;
            PanelMapTool.Invalidate();
            DragOffset = Point.Empty;
        }

        void Item_MouseDown(object? sender, MouseEventArgs e)
        {
            Control? control = sender as Control;
            CurrentDragPanel = control as Panel ?? control.Parent as Panel;

            if (CurrentDragPanel == null) return;

            MoveItemUsingKeysFlag = true;
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            if (e.Button == MouseButtons.Left)
            {
                IsDragging = true;
                Point ClientPoint = CurrentDragPanel.PointToClient(Cursor.Position);
                DragOffset = ClientPoint;
                CurrentDragPanel.BringToFront();

                if (sender is PictureBox Pb)
                {
                    Item? It = Pb.Parent.Tag as Item;
                    if (It != null)
                    {
                        ActivateItemPanel = (Panel)Pb.Parent;
                        DisplayItemInformation();
                        SetActiveItemPanel(ActivateItemPanel);
                    }
                }
            }
        }

        void Item_MouseMove(object? sender, MouseEventArgs e)
        {
            MouseDrag();
        }

        void Item_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MouseUpp();
                DisplayItemInformation();
            }
        }
        #endregion

        #region Handles events from the keyboard
        void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    {
                        IsF1KeyPress = true;
                        break;
                    }
                case Keys.F2:
                    {
                        TextboxItemContent.Focus();
                        break;
                    }
                case Keys.F3:
                    {
                        ApplyChange();
                        break;
                    }
                case Keys.F9:
                    {
                        LoadFile();
                        break;
                    }
                case Keys.F10:
                    {
                        SaveFile();
                        break;
                    }

                case Keys.Delete:
                    {
                        DeleteItem();
                        break;
                    }
                case Keys.Q:
                    {
                        SelectedItemType = OlderSelectedItemType;

                        if (SelectedPictureBox != null)
                            DrawBorderForItemType(SelectedPictureBox, "green");
                        break;
                    }
                case Keys.W:
                    {
                        if (MoveItemUsingKeysFlag)
                            MoveItemUseArrowKeys(1);
                        break;
                    }
                case Keys.A:
                    {
                        if (MoveItemUsingKeysFlag)
                            MoveItemUseArrowKeys(2);
                        break;
                    }
                case Keys.S:
                    {
                        if (MoveItemUsingKeysFlag)
                            MoveItemUseArrowKeys(3);
                        break;
                    }
                case Keys.D:
                    {
                        if (MoveItemUsingKeysFlag)
                            MoveItemUseArrowKeys(4);
                        break;
                    }
                case Keys.C:
                    {
                        if (MoveItemUsingKeysFlag)
                        {
                            WriteWayPoint("C");
                        }
                        break;
                    }
                case Keys.V:
                    {
                        if (MoveItemUsingKeysFlag)
                        {
                            WriteWayPoint("V");
                        }
                        break;
                    }
                case Keys.NumPad0:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(0, 0);
                        HorizontalScrollPosition = 0;
                        break;
                    }
                case Keys.NumPad1:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(2025, 0);
                        HorizontalScrollPosition = 2025;
                        break;
                    }
                case Keys.NumPad2:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(4050, 0);
                        HorizontalScrollPosition = 4050;
                        break;
                    }
                case Keys.NumPad3:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(6075, 0);
                        HorizontalScrollPosition = 6075;
                        break;
                    }
                case Keys.NumPad4:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(8100, 0);
                        HorizontalScrollPosition = 8100;
                        break;
                    }
                case Keys.NumPad5:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(10125, 0);
                        HorizontalScrollPosition = 10125;
                        break;
                    }
                case Keys.NumPad6:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(12150, 0);
                        HorizontalScrollPosition = 12150;
                        break;
                    }
                case Keys.NumPad7:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(14175, 0);
                        HorizontalScrollPosition = 14175;
                        break;
                    }
                case Keys.NumPad8:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(16200, 0);
                        HorizontalScrollPosition = 16200;
                        break;
                    }
                case Keys.NumPad9:
                    {
                        PanelMapTool.AutoScrollPosition = new Point(18225, 0);
                        HorizontalScrollPosition = 18225;
                        break;
                    }
            }

            if (e.Control && e.KeyCode == Keys.S) SaveFile();

            if (e.Control && e.KeyCode == Keys.O) LoadFile();
        }

        void MainForm_KeyUp(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    {
                        IsF1KeyPress = false;
                        break;
                    }
            }
        }
        #endregion

        #region Use WASD keys to move items
        void MoveItemUseArrowKeys(int Flag)
        {
            if (ActivateItemPanel != null)
            {
                Point RelativePos = ActivateItemPanel.Location; // Origin is top left
                Point AbsolutePos = (ActivateItemPanel.Tag as Item).Position; // Origin is bottom left

                switch (Flag)
                {
                    case 1: // up
                        {
                            RelativePos = new Point(RelativePos.X, RelativePos.Y - 1);
                            AbsolutePos = new Point(AbsolutePos.X, AbsolutePos.Y + 1);
                            break;
                        }
                    case 2: // left
                        {
                            RelativePos = new Point(RelativePos.X - 1, RelativePos.Y);
                            AbsolutePos = new Point(AbsolutePos.X - 1, AbsolutePos.Y);
                            break;
                        }
                    case 3: // down
                        {
                            RelativePos = new Point(RelativePos.X, RelativePos.Y + 1);
                            AbsolutePos = new Point(AbsolutePos.X, AbsolutePos.Y - 1);
                            break;
                        }
                    case 4: // right
                        {
                            RelativePos = new Point(RelativePos.X + 1, RelativePos.Y);
                            AbsolutePos = new Point(AbsolutePos.X + 1, AbsolutePos.Y);
                            break;
                        }
                }

                ActivateItemPanel.Location = RelativePos;
                (ActivateItemPanel.Tag as Item).Position = AbsolutePos;
                DisplayItemInformation();
                UpdateFormTitle(0);
            }
        }
        #endregion

        #region Displays file name in title
        void Form_Load(object? sender, EventArgs e)
        {
            UpdateFormTitle();
        }

        void UpdateFormTitle(int check = 1)
        {
            string FilePath = FileOpen;
            string[] Values = FilePath.Split('\\');

            if (check != 1)
            {
                Text = $"* {Values[Values.Length - 1]}";
                return;
            }
            Text = $"{Values[Values.Length - 1]}";
        }
        #endregion

        #region Display item in TextboxItemContent
        void DisplayItemInformation()
        {
            StringBuilder DisplayText = new StringBuilder();
            Item? ActivateItem = ActivateItemPanel.Tag as Item;

            if (ActivateItem != null)
            {
                int Type = ActivateItem.Type;

                DisplayText.AppendLine(GetNameFromType(Type));

                if (ActivateItem.AdditionalProperties != "")
                    DisplayText.AppendLine(ActivateItem.AdditionalProperties);

                DisplayText.AppendLine($"position {ActivateItem.Position.X} {ActivateItem.Position.Y}");

                if (DisplaysItemWidthProperty(Type))
                    DisplayText.AppendLine("width " + ActivateItem.Sz.Width * ActivateItem.Length);

                if (ActivateItem.ItemAttached != "")
                    DisplayText.AppendLine(ActivateItem.ItemAttached);

                if (ActivateItem.OtherNotes != "")
                {
                    string[] Lines = ActivateItem.OtherNotes.Split(" - ");
                    foreach (string Line in Lines)
                    {
                        DisplayText.AppendLine(Line);
                    }
                }
            }

            TextboxItemContent.Text = DisplayText.ToString();
            PanelMapTool.Focus();
        }

        void TextboxItemContent_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MoveItemUsingKeysFlag = false;

            if (timer == null)
            {
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 1000;
                timer.Tick += new EventHandler(ButtonApply_Click);
            }
            timer.Start();
        }
        #endregion

        #region Functions convert type -> name & name -> type
        string GetNameFromType(int Type)
        {
            switch (Type)
            {
                case 100:
                    return "[player]";

                case 1:
                case 3:
                case 4:
                    return "[ground]";
                case 2:
                    return "[water]";

                case 11:
                case 12:
                case 13:
                case 14:
                    return "[wall]";

                case 21:
                    return "[bomb]";
                case 22:
                    return "[coin]";
                case 23:
                    return "[diamond]";
                case 24:
                    return "[shield]";
                case 25:
                    return "[vial]";

                case 31:
                    return "[bridge]";
                case 32:
                    return "[castle]";
                case 33:
                    return "[flag]";
                case 34:
                    return "[root]";
                case 35:
                    return "[seaweed]";
                case 41:
                    return "[mushroom]";
                case 42:
                    return "[bee]";
                case 43:
                    return "[bird]";
                case 44:
                    return "[crab]";
                case 45:
                    return "[fish]";
                case 46:
                    return "[frog]";
                case 47:
                    return "[octopus]";
                case 48:
                    return "[snail]";
                case 49:
                    return "[spider]";
                case 50:
                    return "[ghost]";
                case 51:
                    return "[snakehead]";

                case 61:
                    return "[carnivorous_flower]";
                case 62:
                    return "[wood]";
                case 63:
                    return "[spike]";
                case 64:
                    return "[spring]";
                case 65:
                    return "[meteorite]";
                case 66:
                    return "[balloon]";
                case 67:
                    return "[spin]";
                case 68:
                    return "[mine]";
                case 69:
                    return "[nail_stick]";
                case 70:
                    return "[saw]";
                case 71:
                    return "[statue]";
                case 72:
                    return "[gun]";

                case 81:
                case 82:
                    return "[boss]";

                default:
                    return "";
            }
        }

        int GetTypeFromName(string Name, string AddProp = "")
        {
            switch (Name)
            {
                case "player":
                    return 100;

                case "ground":
                    {
                        switch (AddProp)
                        {
                            case "island":
                                return 3;
                            case "islet":
                                return 4;
                            default:
                                return 1;
                        }
                    }
                case "water":
                    return 2;

                case "wall":
                    {
                        switch (AddProp)
                        {
                            case "wood":
                                return 11;
                            case "gold":
                                return 12;
                            case "unbreakable":
                                return 13;
                            default:
                                return 14;
                        }
                    }

                case "bomb":
                    return 21;
                case "coin":
                    return 22;
                case "diamond":
                    return 23;
                case "shield":
                    return 24;
                case "vial":
                    return 25;

                case "bridge":
                    return 31;
                case "castle":
                    return 32;
                case "flag":
                    return 33;
                case "root":
                    return 34;
                case "seaweed":
                    return 35;

                case "mushroom":
                    return 41;
                case "bee":
                    return 42;
                case "bird":
                    return 43;
                case "crab":
                    return 44;
                case "fish":
                    return 45;
                case "frog":
                    return 46;
                case "octopus":
                    return 47;
                case "snail":
                    return 48;
                case "spider":
                    return 49;
                case "ghost":
                    return 50;
                case "snakehead":
                    return 51;

                case "carnivorous_flower":
                    return 61;
                case "wood":
                    return 62;
                case "spike":
                    return 63;
                case "spring":
                    return 64;
                case "meteorite":
                    return 65;
                case "balloon":
                    return 66;
                case "spin":
                    return 67;
                case "mine":
                    return 68;
                case "nail_stick":
                    return 69;
                case "saw":
                    return 70;
                case "statue":
                    return 71;
                case "gun":
                    return 72;

                case "boss":
                    {
                        switch (AddProp)
                        {
                            case "maybug":
                                return 81;
                            default:
                                return 82;
                        }
                    }

                default:
                    return 0;
            }
        }
        #endregion

        #region Button Save
        void ButtonSave_Click(object? sender, EventArgs e)
        {
            SaveFile();
        }

        bool DisplaysItemWidthProperty(int Type)
        {
            int[] ItemsWantToDisplayWidth = { 1, 2, 3, 31 };

            foreach (int i in ItemsWantToDisplayWidth)
                if (i == Type) return true;

            return false;
        }

        void SaveFile()
        {
            List<Panel> ItemPanels = new List<Panel>();

            foreach (Control control in PanelMapTool.Controls)
            {
                if (control is Panel Pn && Pn.Tag is Item)
                    ItemPanels.Add(Pn);
            }

            ItemPanels.Sort((x, y) =>
            {
                int Result = x.Top.CompareTo(y.Top);
                return (Result == 0) ? x.Left.CompareTo(y.Left) : Result;
            });

            StringBuilder ExportData = new StringBuilder();
            ExportData.AppendLine(OptionalData);

            foreach (Panel Pn in ItemPanels)
            {
                if (Pn.Tag is Item It)
                {
                    int Type = It.Type;
                    Point Pos = ConvertRelativePosToAbsolutePos(Pn.Location, It.Sz.Width, It.Sz.Height, It.AnchorPoint);

                    ExportData.AppendLine(GetNameFromType(Type));

                    if (It.AdditionalProperties != "")
                        ExportData.AppendLine(It.AdditionalProperties);

                    ExportData.AppendLine($"position {Pos.X} {Pos.Y}");

                    if (DisplaysItemWidthProperty(Type))
                    {
                        int Width = It.Sz.Width * It.Length;
                        ExportData.AppendLine($"width {Width}");
                    }

                    if (It.ItemAttached != "")
                        ExportData.AppendLine(It.ItemAttached);

                    if (It.OtherNotes != "")
                        ExportData.AppendLine(It.OtherNotes);
                }
                else
                {
                    MessageBox.Show("Panel's Tag property is not of type Item!", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string ExportFilePath = string.Join("@", FileOpen);
            try
            {
                File.WriteAllText(ExportFilePath, ExportData.ToString());
                UpdateFormTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting item data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Button Apply
        void ButtonApply_Click(object? sender, EventArgs e)
        {
            ApplyChange();
        }

        void WriteWayPoint(string Key)
        {
            string[] Lines = TextboxItemContent.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Item? It = ActivateItemPanel.Tag as Item;

            if (It != null)
            {
                string[] ListOfItemsThatCanHaveWayPointAdded = { "bee", "bird", "crab", "fish", "frog", "ghost", "mushroom", "octopus", 
                    "snail", "snakehead", "spider", "maybug", "monkey", "balloon", "flat", "meteorite", "wood", "saw" };
                bool C = false;
                ExtractItemInformation(Lines); //ItemInformation.Add((CurrentName, CurrentPosition, CurrentWidth, CurrentItemAttached, CurrentAddProp, CurrentOtherNotes));

                foreach (string Key_ in ListOfItemsThatCanHaveWayPointAdded)
                {
                    if (Key_ == ItemInformation[0].Item1)
                    {
                        C = true;
                        break;
                    }
                }

                if (!C)
                {
                    return;
                }

                Point Position = ConvertPositionToPoint(ItemInformation[0].Item2);
                string CurrentOtherNotes = ItemInformation[0].Item6;
                string[] S = CurrentOtherNotes.Split(" ");

                if (S.Length == 1)
                {
                    CurrentOtherNotes = $"way_point {Position.X} {Position.Y}";
                }
                else if (S.Length == 3 && Key == "C")
                {
                    CurrentOtherNotes = $"way_point {Position.X} {Position.Y}";
                }
                else if (S.Length == 3 && Key == "V")
                {
                    CurrentOtherNotes = CurrentOtherNotes + $" {Position.X} {Position.Y}";
                }
                else if ((S.Length == 5 || S.Length == 7) && Key == "C")
                {
                    CurrentOtherNotes = $"way_point {Position.X} {Position.Y} {S[3]} {S[4]}";
                }
                else if ((S.Length == 5 || S.Length == 7) && Key == "V")
                {
                    CurrentOtherNotes = $"way_point {S[1]} {S[2]} {Position.X} {Position.Y}";
                }

                StringBuilder DisplayText = new StringBuilder();
                DisplayText.AppendLine($"[{ItemInformation[0].Item1}]");
                DisplayText.AppendLine($"position {Position.X} {Position.Y}");
                if (ItemInformation[0].Item3 != "")
                    DisplayText.AppendLine(ItemInformation[0].Item3);
                if (ItemInformation[0].Item4 != "")
                    DisplayText.AppendLine(ItemInformation[0].Item4);
                if (ItemInformation[0].Item5 != "")
                    DisplayText.AppendLine(ItemInformation[0].Item5);
                DisplayText.AppendLine(CurrentOtherNotes);

                TextboxItemContent.Text = DisplayText.ToString();
                PanelMapTool.Focus();
                ApplyChange();
            }
        }

        void ApplyChange()
        {
            string[] Lines = TextboxItemContent.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Item? It = ActivateItemPanel.Tag as Item;

            if (It != null)
            {
                ActivateItemPanel.Controls.Clear();
                ExtractItemInformation(Lines); //ItemInformation.Add((CurrentName, CurrentPosition, CurrentWidth, CurrentItemAttached, CurrentAddProp, CurrentOtherNotes));

                Point Position = ConvertPositionToPoint(ItemInformation[0].Item2);
                string CurrentWidth = ItemInformation[0].Item3;
                int.TryParse(CurrentWidth.Trim(), out int Width);
                int Length = (int)Math.Round((float)Width / It.Sz.Width);

                if (Length < 1) Length = 1;

                string ItemAttached = ItemInformation[0].Item4;
                if (ItemAttached != "") ActivateItemPanel.Controls.Add(CreateItemAttached(ItemAttached));

                string OtherNotes = ItemInformation[0].Item6;
                if (OtherNotes != "" && It.Type == 34) // Assign an address to the root of the tree
                    ActivateItemPanel.Controls.Add(AssignAddressToRoot(OtherNotes));

                int OffsetX = Border;
                for (int i = 0; i < Length; i++)
                {
                    PictureBox Pb = CreatePictureBoxItem(It, OffsetX);
                    ActivateItemPanel.Controls.Add(Pb);
                    OffsetX += It.Sz.Width;
                }

                string AddProp = ItemInformation[0].Item5;

                It.Length = Length;
                It.Position = Position;
                It.OtherNotes = OtherNotes;
                It.AdditionalProperties = AddProp;
                It.ItemAttached = ItemAttached;

                ActivateItemPanel.Width = It.Sz.Width * Length + Border * 2;
                ActivateItemPanel.Location = ConvertAbsolutePosToRelativePos(It.Position, It.Sz.Width, It.Sz.Height, It.AnchorPoint);
                ActivateItemPanel.Refresh();
            }
        }

        Label AssignAddressToRoot(string Address)
        {
            string[] splitStrings = Address.Split(new string[] { " - " }, StringSplitOptions.None);
            while (splitStrings.Length > 0 && splitStrings[splitStrings.Length - 1] == "")
            {
                Array.Resize(ref splitStrings, splitStrings.Length - 1);
            }

            Label label = new Label();
            label.Text = string.Join(Environment.NewLine, splitStrings);
            label.AutoSize = true;
            label.Location = new Point(10, 10);

            return label;
        }
        #endregion

        #region Button Delete
        void ButtonDelete_Click(object? sender, EventArgs e)
        {
            DeleteItem();
        }

        void DeleteItem()
        {
            PanelMapTool.Controls.Remove(ActivateItemPanel);
            PanelMapTool.Refresh();
            TextboxItemContent.Clear();
            PanelMapTool.Focus();
        }
        #endregion

        #region Button Load
        void ButtonLoad_Click(object? sender, EventArgs e)
        {
            LoadFile();
        }

        void LoadFile()
        {
            OpenFileDialog FileDialog = new OpenFileDialog();
            string FolderPath = "";

            if (SwitchModeCommittingAndEditing == 2) FolderPath = Path.Combine(ProjectDirectory, "level\\");
            else if (SwitchModeCommittingAndEditing == 1) FolderPath = Path.Combine(ProjectDirectory, "Map Editor\\level\\");

            FileDialog.InitialDirectory = $"{FolderPath}";
            FileDialog.CheckFileExists = false;
            FileDialog.CheckPathExists = true;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                string SelectedFilePath = FileDialog.FileName;
                FileOpen = SelectedFilePath;

                ItemInformation.Clear();
                PanelMapTool.Controls.Clear();
                PanelMapTool.Refresh();
                HorizontalScrollPosition = 0;
                PanelMapTool.AutoScrollPosition = new Point(0, 0);

                UpdateFormTitle();
                ReadItemsFile(1);
                DisplayItems();
            }
        }
        #endregion
    }
}
