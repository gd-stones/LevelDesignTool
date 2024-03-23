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
            Item_Content = new Panel();
            Edit_Item = new Button();
            Delete_Item = new Button();
            Save_Item = new Button();
            Item_Content.SuspendLayout();
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
            ButtonExport.BackColor = Color.FromArgb(255, 224, 192);
            ButtonExport.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ButtonExport.Location = new Point(1456, 966);
            ButtonExport.Name = "ButtonExport";
            ButtonExport.Size = new Size(110, 32);
            ButtonExport.TabIndex = 2;
            ButtonExport.Text = "Export";
            ButtonExport.UseVisualStyleBackColor = false;
            ButtonExport.Click += ButtonExport_Click;
            // 
            // Item_Content
            // 
            Item_Content.AutoSize = true;
            Item_Content.BorderStyle = BorderStyle.FixedSingle;
            Item_Content.Controls.Add(Save_Item);
            Item_Content.Controls.Add(Delete_Item);
            Item_Content.Controls.Add(Edit_Item);
            Item_Content.Location = new Point(1456, 0);
            Item_Content.Name = "Item_Content";
            Item_Content.Size = new Size(450, 403);
            Item_Content.TabIndex = 3;
            // 
            // Edit_Item
            // 
            Edit_Item.BackColor = Color.FromArgb(255, 255, 192);
            Edit_Item.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            Edit_Item.Location = new Point(25, 368);
            Edit_Item.Name = "Edit_Item";
            Edit_Item.Size = new Size(90, 30);
            Edit_Item.TabIndex = 0;
            Edit_Item.Text = "Edit";
            Edit_Item.UseVisualStyleBackColor = false;
            // 
            // Delete_Item
            // 
            Delete_Item.BackColor = Color.FromArgb(255, 128, 128);
            Delete_Item.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            Delete_Item.Location = new Point(330, 368);
            Delete_Item.Name = "Delete_Item";
            Delete_Item.Size = new Size(90, 30);
            Delete_Item.TabIndex = 1;
            Delete_Item.Text = "Delete";
            Delete_Item.UseVisualStyleBackColor = false;
            // 
            // Save_Item
            // 
            Save_Item.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            Save_Item.Location = new Point(135, 368);
            Save_Item.Name = "Save_Item";
            Save_Item.Size = new Size(90, 30);
            Save_Item.TabIndex = 2;
            Save_Item.Text = "Save";
            Save_Item.UseVisualStyleBackColor = true;
            // 
            // LDT_v2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 1061);
            Controls.Add(Item_Content);
            Controls.Add(ButtonExport);
            Controls.Add(Item_List_View);
            Controls.Add(Map_Tool);
            Name = "LDT_v2";
            Text = "LDT_v2";
            Item_Content.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel Map_Tool;
        private Panel Item_List_View;
        private Button ButtonExport;
        private Panel Item_Content;
        private Button Save_Item;
        private Button Delete_Item;
        private Button Edit_Item;
    }
}