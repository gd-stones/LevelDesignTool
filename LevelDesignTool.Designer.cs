namespace LevelDesignTool
{
    partial class LevelDesignTool
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MapTool = new Panel();
            ButtonExport = new Button();
            ListItem = new Panel();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // MapTool
            // 
            MapTool.AutoScroll = true;
            MapTool.BorderStyle = BorderStyle.FixedSingle;
            MapTool.Location = new Point(0, 0);
            MapTool.Name = "MapTool";
            MapTool.Size = new Size(1920, 621);
            MapTool.TabIndex = 0;
            MapTool.Paint += MapTool_Paint;
            // 
            // ButtonExport
            // 
            ButtonExport.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ButtonExport.Location = new Point(12, 632);
            ButtonExport.Name = "ButtonExport";
            ButtonExport.Size = new Size(106, 36);
            ButtonExport.TabIndex = 1;
            ButtonExport.Text = "Export";
            ButtonExport.UseVisualStyleBackColor = true;
            ButtonExport.Click += ButtonExport_Click;
            // 
            // ListItem
            // 
            ListItem.AutoScroll = true;
            ListItem.BorderStyle = BorderStyle.FixedSingle;
            ListItem.Location = new Point(12, 684);
            ListItem.Name = "ListItem";
            ListItem.Size = new Size(399, 318);
            ListItem.TabIndex = 2;
            ListItem.Paint += ListItem_Paint;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(651, 767);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(100, 50);
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // LevelDesignTool
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 1061);
            Controls.Add(pictureBox1);
            Controls.Add(ListItem);
            Controls.Add(ButtonExport);
            Controls.Add(MapTool);
            Name = "LevelDesignTool";
            Text = "LevelDesignTool";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel MapTool;
        private Button ButtonExport;
        private Panel ListItem;
        private PictureBox pictureBox1;
    }
}
