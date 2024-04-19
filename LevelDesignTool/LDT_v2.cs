using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace LevelDesignTool
{
    public partial class LDT_v2 : Form
    {
        const int GridSize = 45;
        const int MapToolWidth = 31500;
        const int MapToolHeight = 990;

        int SwitchModeCommittingAndEditing = 2; // 2 = Editing && 1 = Committing; Can only be set to 1 or 2
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
                FilePath = Path.Combine(ProjectDirectory, "item\\items.txt");
            else if (SwitchModeCommittingAndEditing == 1)
                FilePath = Path.Combine(ProjectDirectory, "Map Editor\\item\\items.txt");

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

            g.DrawLine(p1, 0, Tier2PosYGround_TopLeft, MapToolWidth, Tier2PosYGround_TopLeft);
            g.DrawLine(p1, 0, Tier4PosYGround_TopLeft, MapToolWidth, Tier4PosYGround_TopLeft);

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

        int HorizontalScrollPosition = 0;
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
                else if (line.StartsWith("wood") || line.StartsWith("gold") || line.StartsWith("unbreakable") || line.StartsWith("island") || line.StartsWith("islet")
                    || line.StartsWith("high") || line.StartsWith("low") || line.StartsWith("big") || line.StartsWith("shell"))
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
                    FileOpen = Path.Combine(ProjectDirectory, "level\\level_009_1.txt");
                else if (SwitchModeCommittingAndEditing == 1)
                    FileOpen = Path.Combine(ProjectDirectory, "Map Editor\\level\\level_009_1.txt");
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

                    if (AddProp != "")
                        NewItem.AdditionalProperties = AddProp;

                    if (Attached != "")
                        NewItem.ItemAttached = Attached;

                    if (OtherNotes != "")
                        NewItem.OtherNotes = OtherNotes;

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
            if (Width == "")
                return 0;
            else
                return int.Parse(Width);
        }
        #endregion

        #region Generate item in PanelMapTool
        Point ConvertRelativePosToAbsolutePos(Point RelativePos, int SzWidth, int SzHeight, string AnchorPoint = "center")
        {
            Point ScrollPosition = new Point(-PanelMapTool.AutoScrollPosition.X, -PanelMapTool.AutoScrollPosition.Y);
            Point AbsoluteLocation;

            if (AnchorPoint == "top_left")
                AbsoluteLocation = new Point(RelativePos.X + ScrollPosition.X, AdjHeight - (RelativePos.Y + ScrollPosition.Y));
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
            else if (AnchorPoint == "middle_bottom")
                RelativeLocation = new Point(AbsolutePos.X - ScrollPosition.X - SzWidth / 2, AdjHeight - (AbsolutePos.Y - ScrollPosition.Y + SzHeight));
            else
                RelativeLocation = new Point(AbsolutePos.X - ScrollPosition.X - SzWidth / 2, AdjHeight - (AbsolutePos.Y - ScrollPosition.Y + SzHeight / 2));

            return RelativeLocation;
        }

        bool FlagMoveItemAsSoonAsItIsCreated = false;

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

                foreach (Control control in PanelMapTool.Controls)
                {
                    if (control is Panel Pn)
                        Pn.Invalidate();
                }
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

            if (Attached != "")
            {
                int AttType = 0;
                switch (Attached)
                {
                    case "bomb":
                        AttType = 3;
                        break;
                    case "coin":
                        AttType = 11;
                        break;
                    case "diamond":
                        AttType = 13;
                        break;
                    case "vial":
                        AttType = 32;
                        break;
                    case "shield":
                        AttType = 27;
                        break;
                    default:
                        AttType = 0;
                        break;
                }
                Item? ItemAtt = OriginalItems.FirstOrDefault(i => i.Type == AttType);

                if (ItemAtt != null)
                    Pb = CreatePictureBoxItem(ItemAtt, Border);
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
                if (NewItem.Type == 4 || NewItem.Type == 5 || NewItem.Type == 6 || NewItem.Type == 7)
                    Pos = new Point(((-PanelMapTool.AutoScrollPosition.X + Pos.X) / 45) * 45 + PanelMapTool.AutoScrollPosition.X, (Pos.Y / 45) * 45);
                else if (NewItem.Type == 11)
                    Pos = new Point((-PanelMapTool.AutoScrollPosition.X + Pos.X / 45) * 45 + PanelMapTool.AutoScrollPosition.X + 17, (Pos.Y / 45) * 45 + 12);

                NewItem.Position = ConvertRelativePosToAbsolutePos(Pos, NewItem.Sz.Width, NewItem.Sz.Height, NewItem.AnchorPoint);
            }

            Panel ItemPanel = new Panel
            {
                Size = new Size(NewItem.Sz.Width * NewItem.Length + Border * 2, NewItem.Sz.Height + Border * 2),
                Location = Pos, // Position relative
                Tag = NewItem  // Store the item as a Tag in the Panel
            };

            if (NewItem.ItemAttached != "")
                ItemPanel.Controls.Add(CreateItemAttached(NewItem.ItemAttached));

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
            if (Pn != null)
                Pn.Invalidate();

            foreach (Control control in PanelMapTool.Controls)
            {
                if (control is Panel OtherPanel && OtherPanel != Pn)
                    OtherPanel.Invalidate(); // Redraw the inactive panels
            }
        }
        #endregion

        #region Movement item in PanelMapTool
        int FindWallOrGroundLocationClosestPlayerOrMonster(Item ItemCheck, Point NewLocation)
        {
            int[] ListGroundType = { 35, 18, 36, 21, 37, 38, 19, 20 };
            int[] ListBrickType = { 4, 5, 6, 7 };
            int VerticalThreshold = 20;
            int HorizontalThresholdWall = 20;

            foreach (Control control in PanelMapTool.Controls)
            {
                if (control is Panel && control.Tag is Item existingItem)
                {
                    if (existingItem == ItemCheck)
                        continue;

                    foreach (int t in ListBrickType)
                    {
                        if (existingItem.Type == t)
                        {
                            Point existingItem_TopLeft = ConvertAbsolutePosToRelativePos(existingItem.Position, existingItem.Sz.Width, existingItem.Sz.Height);
                            int verticalDistance = Math.Abs(NewLocation.Y - (existingItem_TopLeft.Y - existingItem.Sz.Height / 2));
                            int horizontalDistance = Math.Abs(NewLocation.X - existingItem_TopLeft.X);

                            if (verticalDistance < VerticalThreshold && horizontalDistance < HorizontalThresholdWall)
                            {
                                return existingItem_TopLeft.Y - ItemCheck.Sz.Height - 2 * Border;
                            }
                        }
                    }

                    foreach (int t in ListGroundType)
                    {
                        if (existingItem.Type == t)
                        {
                            Point existingItem_TopLeft = ConvertAbsolutePosToRelativePos(existingItem.Position, existingItem.Sz.Width, existingItem.Sz.Height, "top_left");
                            int verticalDistance = Math.Abs(NewLocation.Y - (existingItem_TopLeft.Y - existingItem.Sz.Height / 2));
                            bool Check = false;
                            if (NewLocation.X > existingItem_TopLeft.X && NewLocation.X < existingItem_TopLeft.X + existingItem.Sz.Width * existingItem.Length)
                                Check = true;

                            if (verticalDistance < VerticalThreshold && Check)
                            {
                                return existingItem_TopLeft.Y - ItemCheck.Sz.Height - 2 * Border;
                            }
                        }
                    }
                }
            }

            return 0;
        }

        int FindWaterOrGroundLocationClosestGroundOrWater(Item ItemCheck, Point NewLocation)
        {
            int[] ListGroundAndWaterType = { 35, 18, 36, 21, 37, 38, 19, 20, 33, 34 };
            int HorizontalThreshold = 30;
            int VerticalThreshold = 20;

            foreach (Control control in PanelMapTool.Controls)
            {
                if (control is Panel && control.Tag is Item existingItem)
                {
                    if (existingItem == ItemCheck)
                        continue;

                    foreach (int t in ListGroundAndWaterType)
                    {
                        if (existingItem.Type == t)
                        {
                            Point existingItem_TopLeft = ConvertAbsolutePosToRelativePos(existingItem.Position, existingItem.Sz.Width, existingItem.Sz.Height, "top_left");
                            int verticalDistance = Math.Abs(NewLocation.Y - existingItem_TopLeft.Y);

                            int horizontalDistance = Math.Abs(NewLocation.X - (existingItem_TopLeft.X + existingItem.Sz.Width * existingItem.Length + Border));
                            if (horizontalDistance < HorizontalThreshold && verticalDistance < VerticalThreshold)
                            {
                                return existingItem_TopLeft.X + existingItem.Sz.Width * existingItem.Length + 2 * Border;
                            }

                            horizontalDistance = Math.Abs(NewLocation.X + ItemCheck.Length * ItemCheck.Sz.Width + 2 * Border - existingItem_TopLeft.X);
                            if (horizontalDistance < HorizontalThreshold && verticalDistance < VerticalThreshold)
                            {
                                return existingItem_TopLeft.X - (ItemCheck.Sz.Width * ItemCheck.Length + 2 * Border);
                            }
                        }
                    }
                }
            }

            return 0;
        }

        void MouseDrag()
        {
            if (!IsDragging || CurrentDragPanel == null)
                return;

            Item? It = CurrentDragPanel.Tag as Item;
            Point ScreenPoint;

            if (FlagMoveItemAsSoonAsItIsCreated)
            {
                if (It.AnchorPoint == "center")
                    ScreenPoint = new Point(Cursor.Position.X - It.Sz.Width / 2, Cursor.Position.Y - It.Sz.Height / 2);
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
                bool Check = false;
                int[] ListMarioAndMonsterType = { 9, 10, 15, 17, 22, 23, 24, 26, 28, 29, 30, 31 };

                foreach (int i in ListMarioAndMonsterType)
                {
                    if (Type == i) Check = true;
                }

                int DeltaPosY = 30;
                if (Check)
                {
                    int Y = FindWallOrGroundLocationClosestPlayerOrMonster(It, NewLocation);
                    if (Y != 0) NewLocation.Y = Y;
                }
                else
                {
                    switch (Type)
                    {
                        case 35:
                            {
                                if (NewLocation.Y < Tier1PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier1PosYGround_TopLeft - DeltaPosY)
                                    NewLocation.Y = Tier1PosYGround_TopLeft;
                                break;
                            }
                        case 18:
                        case 20:
                            {
                                if (NewLocation.Y < Tier2PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier2PosYGround_TopLeft - DeltaPosY)
                                    NewLocation.Y = Tier2PosYGround_TopLeft;
                                break;
                            }
                        case 36:
                            {
                                if (NewLocation.Y < Tier3PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier3PosYGround_TopLeft - DeltaPosY)
                                    NewLocation.Y = Tier3PosYGround_TopLeft;
                                break;
                            }
                        case 21:
                            {
                                if (NewLocation.Y < Tier4PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier4PosYGround_TopLeft - DeltaPosY)
                                    NewLocation.Y = Tier4PosYGround_TopLeft;
                                break;
                            }
                        case 37:
                            {
                                if (NewLocation.Y < Tier5PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier5PosYGround_TopLeft - DeltaPosY)
                                    NewLocation.Y = Tier5PosYGround_TopLeft;
                                break;
                            }
                        case 38:
                            {
                                if (NewLocation.Y < Tier6PosYGround_TopLeft + DeltaPosY && NewLocation.Y > Tier6PosYGround_TopLeft - DeltaPosY)
                                    NewLocation.Y = Tier6PosYGround_TopLeft;
                                break;
                            }
                        case 33:
                            {
                                if (NewLocation.Y < Tier2PosYWater_TopLeft + DeltaPosY && NewLocation.Y > Tier2PosYWater_TopLeft - DeltaPosY)
                                    NewLocation.Y = Tier2PosYWater_TopLeft;
                                break;
                            }
                        case 34:
                            {
                                if (NewLocation.Y < Tier1PosYWater_TopLeft + DeltaPosY && NewLocation.Y > Tier1PosYWater_TopLeft - DeltaPosY)
                                    NewLocation.Y = Tier1PosYWater_TopLeft;
                                break;
                            }
                    }

                    int X = FindWaterOrGroundLocationClosestGroundOrWater(It, NewLocation);
                    if (X != 0) NewLocation.X = X;
                }

            }

            if (Type == 4 || Type == 5 || Type == 6 || Type == 7)
                NewLocation = new Point(((-PanelMapTool.AutoScrollPosition.X + NewLocation.X) / 45) * 45 + PanelMapTool.AutoScrollPosition.X, (NewLocation.Y / 45) * 45);
            else if (Type == 11)
                NewLocation = new Point((-PanelMapTool.AutoScrollPosition.X + NewLocation.X / 45) * 45 + PanelMapTool.AutoScrollPosition.X + 17, (NewLocation.Y / 45) * 45 + 12);

            CurrentDragPanel.Location = NewLocation;
            It.Position = ConvertRelativePosToAbsolutePos(NewLocation, It.Sz.Width, It.Sz.Height, It.AnchorPoint);

            DisplayItemInformation();
            UpdateFormTitle(0);

            if (FlagMoveItemAsSoonAsItIsCreated) DragOffset = Point.Empty; // Reset DragOffset
        }

        void MouseUpp()
        {
            IsDragging = false;
            CurrentDragPanel = null;
            PanelMapTool.Invalidate();
        }

        void Item_MouseDown(object? sender, MouseEventArgs e)
        {
            Control? control = sender as Control;
            CurrentDragPanel = control as Panel ?? control.Parent as Panel;

            if (CurrentDragPanel == null)
                return;

            MoveItemUsingKeysFlag = true;

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

        #region Use arrow keys to move items
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
                this.Text = $"* {Values[Values.Length - 1]}";
                return;
            }
            this.Text = $"{Values[Values.Length - 1]}";
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

                if (Type >= 1 && Type <= 38)
                    DisplayText.AppendLine(GetNameFromType(Type));

                if (ActivateItem.AdditionalProperties != "")
                    DisplayText.AppendLine(ActivateItem.AdditionalProperties);

                DisplayText.AppendLine($"position {ActivateItem.Position.X} {ActivateItem.Position.Y}");

                if (Type == 18 || Type == 19 || Type == 33 || Type == 35 || Type == 36 || Type == 37 || Type == 38 || Type == 8 || Type == 21)
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
        }
        #endregion

        #region Functions convert type -> name & name -> type
        string GetNameFromType(int Type)
        {
            switch (Type)
            {
                case 1:
                    return "[item_bee]";
                case 2:
                    return "[item_bird]";
                case 3:
                    return "[item_bomb]";
                case 4:
                case 5:
                case 6:
                case 7:
                    return "[item_wall]";
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
                case 35:
                case 36:
                case 37:
                case 38:
                case 19:
                case 20:
                case 21:
                    return "[item_ground]";
                case 22:
                case 23:
                    return "[item_player]";
                case 24:
                    return "[item_mushroom]";
                case 25:
                    return "[item_octopus]";
                case 26:
                    return "[item_root]";
                case 27:
                    return "[item_shield]";
                case 28:
                case 29:
                    return "[item_snail]";
                case 30:
                    return "[item_spike]";
                case 31:
                    return "[item_spring]";
                case 32:
                    return "[item_vial]";
                case 33:
                case 34:
                    return "[item_water]";
                default:
                    return "";
            }
        }

        int GetTypeFromName(string Name, string AddProp = "")
        {
            switch (Name)
            {
                case "item_bee":
                    return 1;
                case "item_bird":
                    return 2;
                case "item_bomb":
                    return 3;
                case "item_wall":
                    {
                        switch (AddProp)
                        {
                            case "wood":
                                return 4;
                            case "gold":
                                return 5;
                            case "unbreakable":
                                return 6;
                            default:
                                return 7;
                        }
                    }
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
                    {
                        switch (AddProp)
                        {
                            case "low mound":
                                return 35;
                            case "high mound tier 3":
                                return 36;
                            case "high mound tier 5":
                                return 37;
                            case "high mound tier 6":
                                return 38;
                            case "island":
                                return 19;
                            case "islet":
                                return 20;
                            case "high mound":
                                return 21;
                            case "":
                                return 18;
                            default:
                                return 0;
                        }
                    }
                case "item_player":
                    {
                        if (AddProp == "")
                            return 22;
                        else
                            return 23;
                    }
                case "item_mushroom":
                    return 24;
                case "item_octopus":
                    return 25;
                case "item_root":
                    return 26;
                case "item_shield":
                    return 27;
                case "item_snail":
                    {
                        if (AddProp == "")
                            return 28;
                        else
                            return 29;
                    }
                case "item_spike":
                    return 30;
                case "item_spring":
                    return 31;
                case "item_vial":
                    return 32;
                case "item_water":
                    {
                        if (AddProp == "")
                            return 33;
                        else
                            return 34;
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

            foreach (Panel Pn in ItemPanels)
            {
                if (Pn.Tag is Item It)
                {
                    int Type = It.Type;
                    Point Pos = ConvertRelativePosToAbsolutePos(Pn.Location, It.Sz.Width, It.Sz.Height, It.AnchorPoint);

                    if (Type >= 1 && Type <= 38)
                    {
                        ExportData.AppendLine(GetNameFromType(Type));

                        if (It.AdditionalProperties != "")
                            ExportData.AppendLine(It.AdditionalProperties);

                        ExportData.AppendLine($"position {Pos.X} {Pos.Y}");

                        if (Type == 18 || Type == 19 || Type == 33 || Type == 35 || Type == 36 || Type == 37 || Type == 38 || Type == 8 || Type == 21)
                        {
                            int Width = It.Sz.Width * It.Length;
                            ExportData.AppendLine($"width {Width}");
                        }

                        if (It.ItemAttached != "")
                            ExportData.AppendLine(It.ItemAttached);

                        if (It.OtherNotes != "")
                            ExportData.AppendLine(It.OtherNotes);
                    }
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
                if (Length < 1)
                    Length = 1;

                string ItemAttached = ItemInformation[0].Item4;
                if (ItemAttached != "")
                    ActivateItemPanel.Controls.Add(CreateItemAttached(ItemAttached));

                int OffsetX = Border;
                for (int i = 0; i < Length; i++)
                {
                    PictureBox Pb = CreatePictureBoxItem(It, OffsetX);
                    ActivateItemPanel.Controls.Add(Pb);
                    OffsetX += It.Sz.Width;
                }

                string AddProp = ItemInformation[0].Item5;
                string OtherNotes = ItemInformation[0].Item6;

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

            if (SwitchModeCommittingAndEditing == 2)
                FolderPath = Path.Combine(ProjectDirectory, "level\\");
            else if (SwitchModeCommittingAndEditing == 1)
                FolderPath = Path.Combine(ProjectDirectory, "Map Editor\\level\\");

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
