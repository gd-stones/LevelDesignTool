namespace LevelDesignTool
{
    partial class LDT_v2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Map_Tool = new Panel();
            Item_List_View = new Panel();
            ButtonExport = new Button();
            Item_Content = new TextBox();
            Save_Item = new Button();
            Delete_Item = new Button();
            ButtonLoad = new Button();
            SuspendLayout();
            // 
            // Map_Tool
            // 
            Map_Tool.AllowDrop = true;
            Map_Tool.AutoScroll = true;
            Map_Tool.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Map_Tool.BorderStyle = BorderStyle.FixedSingle;
            Map_Tool.Location = new Point(0, 0);
            Map_Tool.Name = "Map_Tool";
            Map_Tool.Size = new Size(1440, 1020);
            Map_Tool.TabIndex = 0;
            Map_Tool.Paint += Map_Tool_Paint;
            // 
            // Item_List_View
            // 
            Item_List_View.AllowDrop = true;
            Item_List_View.AutoScroll = true;
            Item_List_View.BorderStyle = BorderStyle.FixedSingle;
            Item_List_View.Location = new Point(1456, 420);
            Item_List_View.Name = "Item_List_View";
            Item_List_View.Size = new Size(450, 530);
            Item_List_View.TabIndex = 1;
            Item_List_View.Paint += Item_List_View_Paint;
            // 
            // ButtonExport
            // 
            ButtonExport.BackColor = Color.Cyan;
            ButtonExport.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ButtonExport.Location = new Point(1456, 966);
            ButtonExport.Name = "ButtonExport";
            ButtonExport.Size = new Size(75, 30);
            ButtonExport.TabIndex = 2;
            ButtonExport.Text = "Save file";
            ButtonExport.UseVisualStyleBackColor = false;
            ButtonExport.Click += ButtonExport_Click;
            // 
            // Item_Content
            // 
            Item_Content.Location = new Point(1456, 0);
            Item_Content.Multiline = true;
            Item_Content.Name = "Item_Content";
            Item_Content.Size = new Size(450, 325);
            Item_Content.TabIndex = 3;
            Item_Content.TextChanged += Item_Content_TextChanged;
            // 
            // Save_Item
            // 
            Save_Item.BackColor = Color.FromArgb(128, 255, 128);
            Save_Item.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            Save_Item.Location = new Point(1456, 360);
            Save_Item.Name = "Save_Item";
            Save_Item.Size = new Size(90, 30);
            Save_Item.TabIndex = 2;
            Save_Item.Text = "Apply";
            Save_Item.UseVisualStyleBackColor = false;
            Save_Item.Click += Save_Item_Click;
            // 
            // Delete_Item
            // 
            Delete_Item.BackColor = Color.Red;
            Delete_Item.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            Delete_Item.Location = new Point(1816, 360);
            Delete_Item.Name = "Delete_Item";
            Delete_Item.Size = new Size(90, 30);
            Delete_Item.TabIndex = 1;
            Delete_Item.Text = "Delete";
            Delete_Item.UseVisualStyleBackColor = false;
            Delete_Item.Click += Delete_Item_Click;
            // 
            // ButtonLoad
            // 
            ButtonLoad.BackColor = Color.Cyan;
            ButtonLoad.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ButtonLoad.Location = new Point(1554, 966);
            ButtonLoad.Name = "ButtonLoad";
            ButtonLoad.Size = new Size(75, 30);
            ButtonLoad.TabIndex = 4;
            ButtonLoad.Text = "Load file";
            ButtonLoad.UseVisualStyleBackColor = false;
            ButtonLoad.Click += ButtonLoad_Click;
            // 
            // LDT_v2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 1061);
            Controls.Add(ButtonLoad);
            Controls.Add(Item_Content);
            Controls.Add(Save_Item);
            Controls.Add(Delete_Item);
            Controls.Add(ButtonExport);
            Controls.Add(Item_List_View);
            Controls.Add(Map_Tool);
            Name = "LDT_v2";
            Text = "LDT_v2";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel Map_Tool;
        private Panel Item_List_View;
        private Button ButtonExport;
        private Button Save_Item;
        private Button Delete_Item;
        public TextBox Item_Content;
        private Button ButtonLoad;
    }
}