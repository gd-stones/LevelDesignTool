namespace LevelDesignTool
{
    partial class LDT_v2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.IContainer components = null;

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
        void InitializeComponent()
        {
            PanelMapTool = new Panel();
            PanelItemListView = new Panel();
            TextboxItemContent = new TextBox();
            ButtonApply = new Button();
            ButtonDelete = new Button();
            ButtonSave = new Button();
            ButtonLoad = new Button();
            SuspendLayout();
            // 
            // PanelMapTool
            // 
            PanelMapTool.AllowDrop = true;
            PanelMapTool.AutoScroll = true;
            PanelMapTool.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PanelMapTool.BorderStyle = BorderStyle.FixedSingle;
            PanelMapTool.Location = new Point(0, 0);
            PanelMapTool.Name = "PanelMapTool";
            PanelMapTool.Size = new Size(1440, 1010);
            PanelMapTool.TabIndex = 0;
            // 
            // PanelItemListView
            // 
            PanelItemListView.AllowDrop = true;
            PanelItemListView.AutoScroll = true;
            PanelItemListView.BorderStyle = BorderStyle.FixedSingle;
            PanelItemListView.Location = new Point(1452, 54);
            PanelItemListView.Name = "PanelItemListView";
            PanelItemListView.Size = new Size(440, 600);
            PanelItemListView.TabIndex = 1;
            // 
            // TextboxItemContent
            // 
            TextboxItemContent.Location = new Point(1452, 666);
            TextboxItemContent.Multiline = true;
            TextboxItemContent.Name = "TextboxItemContent";
            TextboxItemContent.Size = new Size(440, 255);
            TextboxItemContent.TabIndex = 3;
            // 
            // ButtonApply
            // 
            ButtonApply.BackColor = SystemColors.ControlLight;
            ButtonApply.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ButtonApply.Location = new Point(1452, 938);
            ButtonApply.Name = "ButtonApply";
            ButtonApply.Size = new Size(60, 30);
            ButtonApply.TabIndex = 2;
            ButtonApply.Text = "Apply";
            ButtonApply.UseVisualStyleBackColor = false;
            ButtonApply.Click += ButtonApply_Click;
            // 
            // ButtonDelete
            // 
            ButtonDelete.BackColor = SystemColors.ControlLight;
            ButtonDelete.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ButtonDelete.Location = new Point(1528, 938);
            ButtonDelete.Name = "ButtonDelete";
            ButtonDelete.Size = new Size(60, 30);
            ButtonDelete.TabIndex = 10;
            ButtonDelete.Text = "Delete";
            ButtonDelete.UseVisualStyleBackColor = false;
            ButtonDelete.Click += ButtonDelete_Click;
            // 
            // ButtonSave
            // 
            ButtonSave.BackColor = SystemColors.ControlLight;
            ButtonSave.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ButtonSave.Location = new Point(1524, 12);
            ButtonSave.Name = "ButtonSave";
            ButtonSave.Size = new Size(60, 30);
            ButtonSave.TabIndex = 4;
            ButtonSave.Text = "Save";
            ButtonSave.UseVisualStyleBackColor = false;
            ButtonSave.Click += ButtonSave_Click;
            // 
            // ButtonLoad
            // 
            ButtonLoad.BackColor = SystemColors.ControlLight;
            ButtonLoad.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ButtonLoad.Location = new Point(1452, 12);
            ButtonLoad.Name = "ButtonLoad";
            ButtonLoad.Size = new Size(60, 30);
            ButtonLoad.TabIndex = 5;
            ButtonLoad.Text = "Open";
            ButtonLoad.UseVisualStyleBackColor = false;
            ButtonLoad.Click += ButtonLoad_Click;
            // 
            // LDT_v2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 1061);
            Controls.Add(ButtonSave);
            Controls.Add(ButtonLoad);
            Controls.Add(TextboxItemContent);
            Controls.Add(ButtonApply);
            Controls.Add(PanelItemListView);
            Controls.Add(PanelMapTool);
            Controls.Add(ButtonDelete);
            Name = "LDT_v2";
            Text = "LDT_v2";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        Panel PanelMapTool;
        Panel PanelItemListView;
        public TextBox TextboxItemContent;
        Button ButtonSave;
        Button ButtonApply;
        Button ButtonLoad;
        Button ButtonDelete;
    }
}