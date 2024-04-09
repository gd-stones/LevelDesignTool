using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace LevelDesignTool
{
    public partial class LDT_v2 : Form
    {
        const int GridSize = 20;
        const int MapToolWidth = 32000;
        const int MapToolHeight = 1000;

        List<Item> OriginalItems = new List<Item>();
        Panel? ActivateItemPanel = null;
        int SelectedItemType = 0;
        int OlderSelectedItemType = 0;
        Panel? CurrentDragPanel;
        Point DragOffset;
        bool IsDragging;
        string ProjectDirectory = "";
        int SwitchModeCommittingAndEditing = 2; // 2 = Editing && 1 = Committing; Can only be set to 1 or 2

        int MaxPosYGround_BottomLeft = 362;
        int MinPosYGround_BottomLeft = 180;
        int MaxPosYWater_BottomLeft = 115;
        int MaxPosYGround_TopLeft;
        int MinPosYGround_TopLeft;
        int MaxPosYWater_TopLeft;
        int Border = 3;

        bool IsF1KeyPress = false;

        int BrickSize = 80;
        List<Point> BrickPoints = new List<Point>();

        bool EditItemFlag = false;

        public LDT_v2()
        {
            InitializeComponent();

            if (SwitchModeCommittingAndEditing == 2)
#pragma warning disable CS8602
                ProjectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            else if (SwitchModeCommittingAndEditing == 1)
                ProjectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;

            ReadItemsTypeFile();
            DisplayItemsType();
            InitializeMapToolPanel();
            ReadItemsFile();
            DisplayItems();

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            this.KeyUp += MainForm_KeyUp;
            this.Load += Form_Load;
        }


        #region Display item type in panel Item_List_View
        void DisplayItemsType()
        {
            int x = 10;
            int y = 10;

            foreach (Item ItemType in OriginalItems)
            {
                PictureBox Pb = new PictureBox();
                Pb = ItemType.GenerateImage(new Point(x, y));
                Pb.MouseDown += PictureBox_MouseDown;
                Item_List_View.Controls.Add(Pb);

                x += Pb.Width + 10;
                if (x + Pb.Width > Item_List_View.Width)
                {
                    x = 10;
                    y += Pb.Height + 10;
                }
            }

            Item_List_View.MouseDown += ItemListView_MouseDown;
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
                        Item It = new Item(Img, Sz, Type, Length, AnchorPoint, AdditionalProperties);
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

        PictureBox? SelectedPictureBox = null;

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

        #region Initialize panel Map_Tool
        void InitializeMapToolPanel()
        {
            if (SwitchModeCommittingAndEditing == 2)
                Map_Tool.Paint += Map_Tool_DrawGrid;

            MaxPosYGround_TopLeft = 1000 - MaxPosYGround_BottomLeft - 2 * Border + 1;
            MinPosYGround_TopLeft = 1000 - MinPosYGround_BottomLeft - 2 * Border + 1;
            MaxPosYWater_TopLeft = 1000 - MaxPosYWater_BottomLeft - 2 * Border + 1;
            Map_Tool.Paint += DrawLandAndWaterBoundaries;

            Map_Tool.MouseClick += Map_Tool_MouseClick;
            Map_Tool.AutoScrollMinSize = new Size(MapToolWidth, MapToolHeight);
            Map_Tool.AutoScroll = true;
            Map_Tool.AllowDrop = true;
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

            g.DrawLine(p1, 0, MaxPosYGround_TopLeft, MapToolWidth, MaxPosYGround_TopLeft);
            g.DrawLine(p1, 0, MinPosYGround_TopLeft, MapToolWidth, MinPosYGround_TopLeft);
            g.DrawLine(p2, 0, MaxPosYWater_TopLeft, MapToolWidth, MaxPosYWater_TopLeft);

            p1.Dispose();
            p2.Dispose();
        }

        void Map_Tool_DrawGrid(object? sender, PaintEventArgs e)
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
        #endregion

        #region Display items in Map_Tool panel
        string FileOpen = "";
        List<(string, string, string, string, string)> ItemInformation = new List<(string, string, string, string, string)>();

        void ExtractItemInformation(string[] Lines)
        {
            ItemInformation.Clear();

            string CurrentName = "";
            string CurrentPosition = "";
            string CurrentWidth = "";
            string CurrentItemAttached = "";
            string CurrentAddProp = "";

            foreach (string line in Lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (!string.IsNullOrEmpty(CurrentName) && !string.IsNullOrEmpty(CurrentPosition))
                        ItemInformation.Add((CurrentName, CurrentPosition, CurrentWidth, CurrentItemAttached, CurrentAddProp));

                    CurrentName = line.Trim('[', ']');
                    CurrentPosition = "";
                    CurrentWidth = "";
                    CurrentItemAttached = "";
                    CurrentAddProp = "";
                }
                else if (line.StartsWith("position"))
                    CurrentPosition = line.Replace("position ", "");
                else if (line.StartsWith("width"))
                    CurrentWidth = line.Replace("width ", "");
                else if (line.StartsWith("bomb") || line.StartsWith("coin") || line.StartsWith("diamond") || line.StartsWith("vial") || line.StartsWith("shield"))
                    CurrentItemAttached = line;
                else if (line.StartsWith("wood") || line.StartsWith("gold") || line.StartsWith("unbreakable") || line.StartsWith("island") || line.StartsWith("islet")
                    || line.StartsWith("high") || line.StartsWith("big") || line.StartsWith("shell"))
                    CurrentAddProp = line;
            }

            if (!string.IsNullOrEmpty(CurrentName) && !string.IsNullOrEmpty(CurrentPosition))
                ItemInformation.Add((CurrentName, CurrentPosition, CurrentWidth, CurrentItemAttached, CurrentAddProp));
        }

        void ReadItemsFile(int Flag = 0)
        {
            if (Flag == 0)
            {
                if (SwitchModeCommittingAndEditing == 2)
                    FileOpen = Path.Combine(ProjectDirectory, "level\\level_009.txt");
                else if (SwitchModeCommittingAndEditing == 1)
                    FileOpen = Path.Combine(ProjectDirectory, "Map Editor\\level\\level_009.txt");
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
                string Prop = It.Item5;

                Item? OriginalItem = OriginalItems.FirstOrDefault(i => i.Type == Type);
                if (OriginalItem != null)
                {
                    Item NewItem = new Item(OriginalItem.Img, OriginalItem.Sz, OriginalItem.Type,
                        Width > 0 ? Width / OriginalItem.Sz.Width : OriginalItem.Length, OriginalItem.AnchorPoint, Prop);
                    NewItem.ItemAttached = Attached;
                    CreateItemAtLocation(NewItem, Position, 0);
                }
            }

            SetActiveItemPanel(null);
            Item_Content.Clear();
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

        #region Handles events from the keyboard
        void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    DeleteItem();
                    break;
                case Keys.Q:
                    {
                        MessageBox.Show("qqqqqq");

                        SelectedItemType = OlderSelectedItemType;

                        if (SelectedPictureBox != null)
                            DrawBorderForItemType(SelectedPictureBox, "green");
                        break;
                    }
                case Keys.F1:
                    {
                        IsF1KeyPress = true;
                        break;
                    }
                case Keys.W:
                    {
                        if (!EditItemFlag)
                        {
                            MoveItemUseArrowKeys(1);
                        }
                        break;
                    }
                case Keys.A:
                    {
                        if (!EditItemFlag)
                        {
                            MoveItemUseArrowKeys(2);
                        }
                        break;
                    }
                case Keys.S:
                    {
                        if (!EditItemFlag)
                        {
                            MoveItemUseArrowKeys(3);
                        }
                        break;
                    }
                case Keys.D:
                    {
                        if (!EditItemFlag)
                        {
                            MoveItemUseArrowKeys(4);
                        }
                        break;
                    }
            }
            //if (e.Alt)
            //{
            //    IsF1KeyPress = true;
            //    MessageBox.Show("KeyDown Alt");
            //}
        }

        void MainForm_KeyUp(object? sender, KeyEventArgs e)
        {
            //if (e.Alt)
            //{
            //    IsF1KeyPress = false;
            //    MessageBox.Show($"{IsF1KeyPress}");
            //}
            switch (e.KeyCode)
            {
                case Keys.F1:
                    {
                        IsF1KeyPress = false;
                        //MessageBox.Show($"{IsF1KeyPress}");
                        break;
                    }
                    //case Keys.Alt:
                    //    {
                    //        IsF1KeyPress = false;
                    //        MessageBox.Show($"{IsF1KeyPress}");
                    //        break;
                    //    }
            }
        }

        void LDT_v2_KeyUp(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Enter)
            //{
            //    IsF1KeyPress = true;
            //    MessageBox.Show("KeyUp Enter");
            //}
            //if (e.KeyCode == Keys.Alt)
            //{
            //    IsF1KeyPress = false;
            //    MessageBox.Show("KeyUp Alt");
            //}
            //if (ModifierKeys == (Keys.Control | Keys.Alt))
            //{
            //    IsF1KeyPress = false;
            //    MessageBox.Show("KeyUp Alt");
            //}
            //if (Control.ModifierKeys.ToString() == "Alt" || Control.ModifierKeys == (Keys.Control | Keys.Alt))
            //{
            //    MessageBox.Show("The Alt key is being pressed");
            //}
        }

        void LDT_v2_KeyPress(object sender, KeyPressEventArgs e)
        {

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
                            AbsolutePos = new Point(AbsolutePos.X -1 , AbsolutePos.Y);
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

        #region Generate item in Map_Top
        int AdjHeight = 995;

        Point ConvertRelativePosToAbsolutePos(Point RelativePos, int SzWidth, int SzHeight, string AnchorPoint = "center")
        {
            Point ScrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);
            Point AbsoluteLocation;

            if (AnchorPoint == "top_left")
                AbsoluteLocation = new Point(RelativePos.X + ScrollPosition.X, AdjHeight - (RelativePos.Y + ScrollPosition.Y));
            else
                AbsoluteLocation = new Point(RelativePos.X + ScrollPosition.X + SzWidth / 2, AdjHeight - (RelativePos.Y + ScrollPosition.Y + SzHeight / 2));

            return AbsoluteLocation;
        }

        Point ConvertAbsolutePosToRelativePos(Point AbsolutePos, int SzWidth, int SzHeight, string AnchorPoint = "center")
        {
            Point ScrollPosition = new Point(-Map_Tool.AutoScrollPosition.X, -Map_Tool.AutoScrollPosition.Y);
            Point RelativeLocation;

            if (AnchorPoint == "top_left")
                RelativeLocation = new Point(AbsolutePos.X - ScrollPosition.X, AdjHeight - (AbsolutePos.Y - ScrollPosition.Y));
            else
                RelativeLocation = new Point(AbsolutePos.X - ScrollPosition.X - SzWidth / 2, AdjHeight - (AbsolutePos.Y - ScrollPosition.Y + SzHeight / 2));

            return RelativeLocation;
        }

        void Map_Tool_MouseClick(object? sender, MouseEventArgs e)
        {
            if (SelectedItemType != 0)
            {
                Item? It = OriginalItems.FirstOrDefault(i => i.Type == SelectedItemType);

                if (It != null)
                {
                    Item NewItem = new Item(It.Img, It.Sz, It.Type, It.Length, It.AnchorPoint, It.AdditionalProperties);

                    if (It.AnchorPoint == "center")
                        CreateItemAtLocation(NewItem, new Point(e.Location.X - It.Sz.Width / 2, e.Location.Y - It.Sz.Height / 2));
                    else
                        CreateItemAtLocation(NewItem, e.Location);
                }

                SelectedItemType = 0;
                if (SelectedPictureBox != null)
                    DrawBorderForItemType(SelectedPictureBox, "red");
            }
            else if (SelectedItemType == 0)
            {
                Item_Content.Clear();
                SetActiveItemPanel(null);

                foreach (Control control in Map_Tool.Controls)
                {
                    if (control is Panel Pn)
                        Pn.Invalidate();
                }
            }
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
                Map_Tool.AutoScrollPosition = new Point(0, 0);
                NewItem.Position = Pos;
                Pos = ConvertAbsolutePosToRelativePos(Pos, NewItem.Sz.Width, NewItem.Sz.Height, NewItem.AnchorPoint);
            }
            else
                NewItem.Position = ConvertRelativePosToAbsolutePos(Pos, NewItem.Sz.Width, NewItem.Sz.Height, NewItem.AnchorPoint);

            if (NewItem.Type == 4 || NewItem.Type == 5 || NewItem.Type == 6 || NewItem.Type == 7)
            {

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

            Map_Tool.Controls.Add(ItemPanel);
            DisplayItemInformation();
        }

        void SetActiveItemPanel(Panel? Pn)
        {
            ActivateItemPanel = Pn;
            if (Pn != null)
                Pn.Invalidate();

            foreach (Control control in Map_Tool.Controls)
            {
                if (control is Panel OtherPanel && OtherPanel != Pn)
                    OtherPanel.Invalidate(); // Redraw the inactive panels
            }
        }
        #endregion

        #region Display item in textbox (Item_Content)
        void DisplayItemInformation()
        {
            StringBuilder DisplayText = new StringBuilder();
            Item? ActivateItem = ActivateItemPanel.Tag as Item;

            if (ActivateItem != null)
            {
                int Type = ActivateItem.Type;

                if (Type >= 1 && Type <= 33)
                    DisplayText.AppendLine(GetNameFromType(Type));

                if (ActivateItem.AdditionalProperties != "")
                    DisplayText.AppendLine(ActivateItem.AdditionalProperties);

                DisplayText.AppendLine($"position {ActivateItem.Position.X} {ActivateItem.Position.Y}");

                if (Type == 18 || Type == 33 || Type == 8 || Type == 21)
                    DisplayText.AppendLine("width " + ActivateItem.Sz.Width * ActivateItem.Length);

                if (ActivateItem.ItemAttached != "")
                    DisplayText.AppendLine(ActivateItem.ItemAttached);
            }

            Item_Content.Text = DisplayText.ToString();
            Item_Content.Enabled = false;
        }
        #endregion

        #region Movement item in panel Map_Tool
        void Item_MouseDown(object? sender, MouseEventArgs e)
        {
            Control? control = sender as Control;
            CurrentDragPanel = control as Panel ?? control.Parent as Panel;
            EditItemFlag = false;

            if (CurrentDragPanel == null) return;

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

                        if (It.Type == 4 || It.Type == 5 || It.Type == 6 || It.Type == 7)
                        {
                        }
                    }
                }
            }
        }

        void Item_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!IsDragging || CurrentDragPanel == null)
                return;

            Point ScreenPoint = Cursor.Position;
            Point NewLocation = Map_Tool.PointToClient(ScreenPoint); // Convert to coordinates relative to Map_Tool
            Item? It = ActivateItemPanel.Tag as Item;
            int Type = It.Type;

            NewLocation.Offset(-DragOffset.X, -DragOffset.Y);
            //NewLocation.Y = Math.Max(0, NewLocation.Y);
            //NewLocation.Y = Math.Min(Map_Tool.ClientSize.Height - CurrentDragPanel.Height, NewLocation.Y);

            if (!IsF1KeyPress)
            {
                // Set min/max posY for ground
                int DeltaPosY = 30;
                if (Type == 18 || Type == 21)
                {
                    if (NewLocation.Y < MaxPosYGround_TopLeft + DeltaPosY && NewLocation.Y > MaxPosYGround_TopLeft - DeltaPosY)
                        NewLocation.Y = MaxPosYGround_TopLeft;
                    else if (NewLocation.Y < MinPosYGround_TopLeft + DeltaPosY && NewLocation.Y > MinPosYGround_TopLeft - DeltaPosY)
                        NewLocation.Y = MinPosYGround_TopLeft;
                }
                // Set max posY for water
                else if (Type == 33)
                {
                    if (NewLocation.Y < MaxPosYWater_TopLeft + DeltaPosY && NewLocation.Y > MaxPosYWater_TopLeft - DeltaPosY)
                        NewLocation.Y = MaxPosYWater_TopLeft;
                }
            }

            if (Type == 4 || Type == 5 || Type == 6 || Type == 7)
            {

            }

            CurrentDragPanel.Location = NewLocation;
            It.Position = ConvertRelativePosToAbsolutePos(NewLocation, It.Sz.Width, It.Sz.Height, It.AnchorPoint);

            DisplayItemInformation();
            UpdateFormTitle(0);
        }

        void Item_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsDragging = false;
                CurrentDragPanel = null;
                Map_Tool.Invalidate(); // Redraw the Map_Tool
                DisplayItemInformation();
            }
        }
        #endregion

        #region Align the position of the bricks


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
                    return "[item_brick]";
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
                    return "[item_ground]";
                case 20:
                    return "[item_ground]";
                case 21:
                    return "[item_ground]";
                case 22:
                case 23:
                    return "[item_mario]";
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
                case "item_brick":
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
                            case "":
                                return 18;
                            case "island":
                                return 19;
                            case "islet":
                                return 20;
                            default:
                                return 21;
                        }
                    }
                case "item_mario":
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
                    return 33;
                default:
                    return 0;
            }
        }
        #endregion

        #region Button Save
        void ButtonExport_Click(object? sender, EventArgs e)
        {
            SaveFile();
            UpdateFormTitle();
        }

        void SaveFile()
        {
            List<Panel> ItemPanels = new List<Panel>();

            foreach (Control control in Map_Tool.Controls)
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

                    if (Type >= 1 && Type <= 33)
                    {
                        ExportData.AppendLine(GetNameFromType(Type));

                        if (It.AdditionalProperties != "")
                            ExportData.AppendLine(It.AdditionalProperties);

                        ExportData.AppendLine($"position {Pos.X} {Pos.Y}");

                        if (Type == 18 || Type == 33 || Type == 8 || Type == 21)
                        {
                            int Width = It.Sz.Width * It.Length;
                            ExportData.AppendLine($"width {Width}");
                        }

                        if (It.ItemAttached != "")
                            ExportData.AppendLine(It.ItemAttached);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting item data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Button Apply
        void Save_Item_Click(object? sender, EventArgs e)
        {
            string[] Lines = Item_Content.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Item? It = ActivateItemPanel.Tag as Item;

            if (It != null)
            {
                ActivateItemPanel.Controls.Clear();
                ExtractItemInformation(Lines); //ItemInformation.Add((CurrentName, CurrentPosition, CurrentWidth, CurrentItemAttached, CurrentAddProp));

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

                It.Length = Length;
                It.Position = Position;
                It.ItemAttached = ItemAttached;

                ActivateItemPanel.Width = It.Sz.Width * Length + Border * 2;
                ActivateItemPanel.Location = ConvertAbsolutePosToRelativePos(It.Position, It.Sz.Width, It.Sz.Height, It.AnchorPoint);
                ActivateItemPanel.Refresh();
            }
        }
        #endregion

        #region Button Delete
        void Delete_Item_Click(object? sender, EventArgs e)
        {
            DeleteItem();
        }

        void DeleteItem()
        {
            Map_Tool.Controls.Remove(ActivateItemPanel);
            Map_Tool.Refresh();
            Item_Content.Clear();
        }
        #endregion

        #region Button Load
        void ButtonLoad_Click(object? sender, EventArgs e)
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
                Map_Tool.Controls.Clear();
                Map_Tool.Refresh();
                Map_Tool.AutoScrollPosition = new Point(0, 0);
                UpdateFormTitle();
                ReadItemsFile(1);
                DisplayItems();
            }
        }
        #endregion

        #region Do not use - But deleting will cause program errors
        void Item_Content_TextChanged(object? sender, EventArgs e)
        {

        }

        void Item_List_View_Paint(object? sender, PaintEventArgs e)
        {

        }

        void Map_Tool_Paint(object? sender, PaintEventArgs e)
        {

        }
        #endregion

        private void EditItem_Click(object sender, EventArgs e)
        {
            Item_Content.Enabled = true;
            EditItemFlag = true;
        }
    }
}
