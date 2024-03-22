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
            components = new System.ComponentModel.Container();
            imageList1 = new ImageList(components);
            Map_Tool = new Panel();
            Item_List_View = new Panel();
            ButtonExport = new Button();
            SuspendLayout();
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageSize = new Size(16, 16);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // Map_Tool
            // 
            Map_Tool.AutoScroll = true;
            Map_Tool.BorderStyle = BorderStyle.FixedSingle;
            Map_Tool.Location = new Point(0, 0);
            Map_Tool.Name = "Map_Tool";
            Map_Tool.Size = new Size(1920, 621);
            Map_Tool.TabIndex = 0;
            Map_Tool.Paint += Map_Tool_Paint;
            // 
            // Item_List_View
            // 
            Item_List_View.AutoScroll = true;
            Item_List_View.BorderStyle = BorderStyle.FixedSingle;
            Item_List_View.Location = new Point(1, 680);
            Item_List_View.Name = "Item_List_View";
            Item_List_View.Size = new Size(600, 320);
            Item_List_View.TabIndex = 1;
            Item_List_View.Paint += Item_List_View_Paint;
            // 
            // ButtonExport
            // 
            ButtonExport.BackColor = Color.FromArgb(255, 224, 192);
            ButtonExport.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ButtonExport.Location = new Point(5, 634);
            ButtonExport.Name = "ButtonExport";
            ButtonExport.Size = new Size(110, 32);
            ButtonExport.TabIndex = 2;
            ButtonExport.Text = "Export";
            ButtonExport.UseVisualStyleBackColor = false;
            ButtonExport.Click += ButtonExport_Click;
            // 
            // LDT_v2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 1061);
            Controls.Add(ButtonExport);
            Controls.Add(Item_List_View);
            Controls.Add(Map_Tool);
            Name = "LDT_v2";
            Text = "LDT_v2";
            ResumeLayout(false);
        }

        #endregion

        private ImageList imageList1;
        private Panel Map_Tool;
        private Panel Item_List_View;
        private Button ButtonExport;
    }
}