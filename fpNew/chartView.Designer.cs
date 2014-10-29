namespace fpNew
{
    partial class chartView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(chartView));
            this.cv_ts = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.cv_tscb_holesDlist = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cv_tsb_openDepList = new System.Windows.Forms.ToolStripButton();
            this.cv_zgc_content = new ZedGraph.ZedGraphControl();
            this.cv_clb_List = new System.Windows.Forms.CheckedListBox();
            this.cv_ts.SuspendLayout();
            this.SuspendLayout();
            // 
            // cv_ts
            // 
            this.cv_ts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.cv_tscb_holesDlist,
            this.toolStripSeparator1,
            this.cv_tsb_openDepList});
            this.cv_ts.Location = new System.Drawing.Point(0, 0);
            this.cv_ts.Name = "cv_ts";
            this.cv_ts.Size = new System.Drawing.Size(636, 25);
            this.cv_ts.TabIndex = 0;
            this.cv_ts.Text = "toolStrip1";
            this.cv_ts.Visible = false;
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(92, 22);
            this.toolStripLabel1.Text = "请选择测斜孔：";
            // 
            // cv_tscb_holesDlist
            // 
            this.cv_tscb_holesDlist.Name = "cv_tscb_holesDlist";
            this.cv_tscb_holesDlist.Size = new System.Drawing.Size(121, 25);
            this.cv_tscb_holesDlist.SelectedIndexChanged += new System.EventHandler(this.cv_tscb_holesDlist_SelectedIndexChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // cv_tsb_openDepList
            // 
            this.cv_tsb_openDepList.BackColor = System.Drawing.SystemColors.Control;
            this.cv_tsb_openDepList.CheckOnClick = true;
            this.cv_tsb_openDepList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cv_tsb_openDepList.Image = ((System.Drawing.Image)(resources.GetObject("cv_tsb_openDepList.Image")));
            this.cv_tsb_openDepList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cv_tsb_openDepList.Name = "cv_tsb_openDepList";
            this.cv_tsb_openDepList.Size = new System.Drawing.Size(108, 22);
            this.cv_tsb_openDepList.Text = "选择要绘制的曲线";
            this.cv_tsb_openDepList.Visible = false;
            this.cv_tsb_openDepList.Click += new System.EventHandler(this.cv_tsb_openDepList_Click);
            // 
            // cv_zgc_content
            // 
            this.cv_zgc_content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cv_zgc_content.Location = new System.Drawing.Point(0, 25);
            this.cv_zgc_content.Name = "cv_zgc_content";
            this.cv_zgc_content.ScrollGrace = 0D;
            this.cv_zgc_content.ScrollMaxX = 0D;
            this.cv_zgc_content.ScrollMaxY = 0D;
            this.cv_zgc_content.ScrollMaxY2 = 0D;
            this.cv_zgc_content.ScrollMinX = 0D;
            this.cv_zgc_content.ScrollMinY = 0D;
            this.cv_zgc_content.ScrollMinY2 = 0D;
            this.cv_zgc_content.Size = new System.Drawing.Size(636, 714);
            this.cv_zgc_content.TabIndex = 3;
            // 
            // cv_clb_List
            // 
            this.cv_clb_List.CheckOnClick = true;
            this.cv_clb_List.Dock = System.Windows.Forms.DockStyle.Left;
            this.cv_clb_List.FormattingEnabled = true;
            this.cv_clb_List.Location = new System.Drawing.Point(0, 25);
            this.cv_clb_List.Name = "cv_clb_List";
            this.cv_clb_List.Size = new System.Drawing.Size(87, 714);
            this.cv_clb_List.TabIndex = 4;
            this.cv_clb_List.Visible = false;
            this.cv_clb_List.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cv_clb_List_ItemCheck);
            // 
            // chartView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 739);
            this.Controls.Add(this.cv_clb_List);
            this.Controls.Add(this.cv_zgc_content);
            this.Controls.Add(this.cv_ts);
            this.Name = "chartView";
            this.Text = "图表输出";
            this.Load += new System.EventHandler(this.chartView_Load);
            this.cv_ts.ResumeLayout(false);
            this.cv_ts.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip cv_ts;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox cv_tscb_holesDlist;
        private ZedGraph.ZedGraphControl cv_zgc_content;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton cv_tsb_openDepList;
        private System.Windows.Forms.CheckedListBox cv_clb_List;
    }
}