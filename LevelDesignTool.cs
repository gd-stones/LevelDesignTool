using System;
using System.IO;
using System.Windows.Forms;

namespace LevelDesignTool
{
    // Create a custom class to store additional properties
    //public class ImageProperties
    //{
    //    public string ImagePath { get; set; }
    //    public Size ImageSize { get; set; }
    //}

    public partial class LevelDesignTool : Form
    {
        private const int gridSize = 20; // Size of each square in the grid

        public LevelDesignTool()
        {
            InitializeComponent();

            // Map tool
            MapTool.Paint += MapTool_DrawGrid;
            MapTool.AllowDrop = true;
            MapTool.DragEnter += MapTool_DragEnter;
            MapTool.DragDrop += MapTool_DragDrop;

            DisplayImagesFromItemsFile();
        }

        // Map editor
        private void MapTool_Paint(object sender, PaintEventArgs e) { }

        private void MapTool_DrawGrid(object sender, PaintEventArgs e)
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

        private void MapTool_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void MapTool_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Image"))
            {
                Image droppedImage = (Image)e.Data.GetData("Image");
                string imagePath = e.Data.GetData("ImagePath") as string;
                Size imageSize = (Size)e.Data.GetData("ImageSize");

                PictureBox newPictureBox = new PictureBox();
                newPictureBox.Image = new Bitmap(droppedImage); // Clone the image
                newPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                newPictureBox.Size = imageSize;

                Point dropPoint = MapTool.PointToClient(new Point(e.X, e.Y));
                newPictureBox.Location = dropPoint;
                MapTool.Controls.Add(newPictureBox);
            }
        }

        // Button export
        private void ButtonExport_Click(object sender, EventArgs e) { }

        // List item
        private void ListItem_Paint(object sender, PaintEventArgs e) { }

        private void DisplayImagesFromItemsFile()
        {
            string filePath = @"D:\All Test and Learn Cocos Projects\LevelDesignTool\Items.txt";

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                List<(string path, Size size)> imageInfoList = new List<(string, Size)>();

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("path:"))
                    {
                        string imagePath = lines[i].Substring(6).Trim();

                        if (File.Exists(imagePath))
                        {
                            try
                            {
                                string sizeLine = lines[i + 1].Substring(6).Trim();
                                string[] sizeParts = sizeLine.Split(',');
                                Size imageSize = new Size(int.Parse(sizeParts[0]), int.Parse(sizeParts[1]));

                                imageInfoList.Add((imagePath, imageSize));
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error parsing image size: " + ex.Message);
                            }
                        }
                    }
                }

                DisplayImagesInListItem(imageInfoList);
            }
            else
            {
                MessageBox.Show("Items.txt file not found.");
            }

            foreach (PictureBox pictureBox in ListItem.Controls)
            {
                pictureBox.MouseDown += PictureBox_MouseDown;
                pictureBox.MouseMove += PictureBox_MouseMove;
                pictureBox.MouseUp += PictureBox_MouseUp;
            }
        }

        private void DisplayImagesInListItem(List<(string path, Size size)> imageInfoList)
        {
            int x = 10; // Initial x position
            int y = 10; // Initial y position

            foreach (var imageInfo in imageInfoList)
            {
                if (File.Exists(imageInfo.path))
                {
                    try
                    {
                        //ImageProperties _properties = new ImageProperties
                        //{
                        //    ImagePath = imageInfo.path,
                        //    ImageSize = imageInfo.size
                        //};

                        //MessageBox.Show("image path: " + _properties.ImageSize.Width);

                        Image img = Image.FromFile(imageInfo.path);
                        PictureBox pictureBox = new PictureBox();
                        pictureBox.Image = img;
                        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox.Location = new Point(x, y);
                        pictureBox.BorderStyle = BorderStyle.FixedSingle;
                        pictureBox.BackColor = Color.Black;
                        //pictureBox.Tag = _properties;
                        ListItem.Controls.Add(pictureBox);

                        // Update x and y positions for the next item
                        x += pictureBox.Width + 10; // Add some spacing between items
                        if (x + pictureBox.Width > ListItem.Width)
                        {
                            x = 10; // Reset x position
                            y += pictureBox.Height + 10; // Move to the next row
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image: " + ex.Message);
                    }
                }
            }
        }

        // Drag and drop item from ListItem to MapTool
        private bool isDragging = false;
        private Point startPoint;

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            //ImageProperties _properties = pictureBox.Tag as ImageProperties;

            //if (_properties != null)
            //{
            //    DataObject data = new DataObject();
            //    data.SetData("Image", pictureBox.Image);
            //    data.SetData("ImagePath", _properties.ImagePath);
            //    data.SetData("ImageSize", _properties.ImageSize);

            //    pictureBox.DoDragDrop(data, DragDropEffects.Copy);
            //}
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            //MessageBox.Show("MouseMove");

            if (isDragging)
            {
                PictureBox pictureBox = sender as PictureBox;
                pictureBox.BringToFront();
                pictureBox.Location = new Point(pictureBox.Left + e.X - startPoint.X, pictureBox.Top + e.Y - startPoint.Y);
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            MessageBox.Show("MouseUp");


            isDragging = false;
            string imagePath;
            Size imageSize = new Size(1000, 1000);

            PictureBox pictureBox = sender as PictureBox;
            //ImageProperties retrievedProperties = pictureBox.Tag as ImageProperties;
            //if (retrievedProperties != null)
            //{
            //    imagePath = retrievedProperties.ImagePath;
            //    imageSize = retrievedProperties.ImageSize;
            //}

            if (MapTool.ClientRectangle.Contains(MapTool.PointToClient(Cursor.Position)))
            {
                // Clone the image before assigning it to the new PictureBox
                Image clonedImage = new Bitmap(pictureBox.Image);

                PictureBox newPictureBox = new PictureBox();
                newPictureBox.Image = clonedImage;
                newPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                newPictureBox.Size = imageSize;

                newPictureBox.Location = MapTool.PointToClient(Cursor.Position);
                MapTool.Controls.Add(newPictureBox);
            }
        }
    }
}
