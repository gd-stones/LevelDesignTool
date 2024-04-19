using System;

namespace LevelDesignTool
{
    internal class Item
    {
        public event EventHandler<int> ItemClicked; // Custom event to notify the type
        public Image Img;
        public Size Sz;
        public Point Position;
        public int Type;
        public int Length;
        public string ItemAttached = "";
        public string AnchorPoint = "";
        public string AdditionalProperties = "";
        public string OtherNotes = "";

#pragma warning disable CS8618
        public Item(Image Img, Size Sz, int Type, int Length, string AnchorPoint)
#pragma warning restore CS8618
        {
            this.Img = Img;
            this.Sz = Sz;
            this.Type = Type;
            this.Length = Length;
            this.AnchorPoint = AnchorPoint;
        }

        public PictureBox GenerateImage(Point Pos)
        {
            PictureBox Pb = new PictureBox();
            Pb.Image = Img;
            Pb.SizeMode = PictureBoxSizeMode.Zoom;
            Pb.Size = new Size(74, 74);
            Pb.Location = Pos;
            Pb.BackColor = Color.Black;

            Pb.Paint += (sender, e) =>
            {
                int borderThickness = 3;
                ControlPaint.DrawBorder(e.Graphics, new Rectangle(0, 0, Pb.Width, Pb.Height),
                    Color.Red, borderThickness, ButtonBorderStyle.Solid,
                    Color.Red, borderThickness, ButtonBorderStyle.Solid,
                    Color.Red, borderThickness, ButtonBorderStyle.Solid,
                    Color.Red, borderThickness, ButtonBorderStyle.Solid);
            };

            Pb.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    ItemClicked?.Invoke(this, Type);
            };

            return Pb;
        }
    }
}
