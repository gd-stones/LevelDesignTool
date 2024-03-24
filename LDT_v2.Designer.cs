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
            Item_Editor = new Panel();
            Item_Content = new TextBox();
            Save_Item = new Button();
            Delete_Item = new Button();
            Edit_Item = new Button();
            Item_Editor.SuspendLayout();
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
            ButtonExport.Size = new Size(130, 40);
            ButtonExport.TabIndex = 2;
            ButtonExport.Text = "Save/Export";
            ButtonExport.UseVisualStyleBackColor = false;
            ButtonExport.Click += ButtonExport_Click;
            // 
            // Item_Editor
            // 
            Item_Editor.AutoSize = true;
            Item_Editor.BorderStyle = BorderStyle.FixedSingle;
            Item_Editor.Controls.Add(Item_Content);
            Item_Editor.Controls.Add(Save_Item);
            Item_Editor.Controls.Add(Delete_Item);
            Item_Editor.Controls.Add(Edit_Item);
            Item_Editor.Location = new Point(1456, 0);
            Item_Editor.Name = "Item_Editor";
            Item_Editor.Size = new Size(450, 403);
            Item_Editor.TabIndex = 3;
            Item_Editor.Paint += Item_Editor_Paint;
            // 
            // Item_Content
            // 
            Item_Content.Location = new Point(25, 20);
            Item_Content.Multiline = true;
            Item_Content.Name = "Item_Content";
            Item_Content.Size = new Size(395, 325);
            Item_Content.TabIndex = 3;
            Item_Content.TextChanged += Item_Content_TextChanged;
            // 
            // Save_Item
            // 
            Save_Item.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            Save_Item.Location = new Point(135, 360);
            Save_Item.Name = "Save_Item";
            Save_Item.Size = new Size(90, 30);
            Save_Item.TabIndex = 2;
            Save_Item.Text = "Save";
            Save_Item.UseVisualStyleBackColor = true;
            Save_Item.Click += Save_Item_Click;
            // 
            // Delete_Item
            // 
            Delete_Item.BackColor = Color.FromArgb(255, 128, 128);
            Delete_Item.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            Delete_Item.Location = new Point(330, 360);
            Delete_Item.Name = "Delete_Item";
            Delete_Item.Size = new Size(90, 30);
            Delete_Item.TabIndex = 1;
            Delete_Item.Text = "Delete";
            Delete_Item.UseVisualStyleBackColor = false;
            Delete_Item.Click += Delete_Item_Click;
            // 
            // Edit_Item
            // 
            Edit_Item.BackColor = Color.FromArgb(255, 255, 192);
            Edit_Item.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            Edit_Item.Location = new Point(25, 360);
            Edit_Item.Name = "Edit_Item";
            Edit_Item.Size = new Size(90, 30);
            Edit_Item.TabIndex = 0;
            Edit_Item.Text = "Edit";
            Edit_Item.UseVisualStyleBackColor = false;
            Edit_Item.Click += Edit_Item_Click;
            // 
            // LDT_v2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 1061);
            Controls.Add(Item_Editor);
            Controls.Add(ButtonExport);
            Controls.Add(Item_List_View);
            Controls.Add(Map_Tool);
            Name = "LDT_v2";
            Text = "LDT_v2";
            Item_Editor.ResumeLayout(false);
            Item_Editor.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel Map_Tool;
        private Panel Item_List_View;
        private Button ButtonExport;
        private Panel Item_Editor;
        private Button Save_Item;
        private Button Delete_Item;
        private Button Edit_Item;
        private TextBox Item_Content;
    }
}