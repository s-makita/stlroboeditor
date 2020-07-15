namespace StellarRoboEditor
{
    partial class FormMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.NewtoolStripButton = new System.Windows.Forms.ToolStripButton();
            this.LoadtoolStripButton = new System.Windows.Forms.ToolStripButton();
            this.SavetoolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ExecutetoolStripButton = new System.Windows.Forms.ToolStripButton();
            this.DebugExecutetoolStripButton = new System.Windows.Forms.ToolStripButton();
            this.WriteFiletoolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.RecordingtoolStripButton = new System.Windows.Forms.ToolStripButton();
            this.StoptoolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.panelEditor = new System.Windows.Forms.Panel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusCoordinate = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ExecuteStoptoolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewtoolStripButton,
            this.LoadtoolStripButton,
            this.SavetoolStripButton,
            this.toolStripSeparator1,
            this.ExecutetoolStripButton,
            this.DebugExecutetoolStripButton,
            this.ExecuteStoptoolStripButton,
            this.toolStripSeparator4,
            this.WriteFiletoolStripButton,
            this.toolStripSeparator2,
            this.RecordingtoolStripButton,
            this.StoptoolStripButton,
            this.toolStripSeparator3});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(978, 25);
            this.toolStrip.TabIndex = 3;
            this.toolStrip.Text = "toolStrip1";
            // 
            // NewtoolStripButton
            // 
            this.NewtoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.NewtoolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("NewtoolStripButton.Image")));
            this.NewtoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.NewtoolStripButton.Name = "NewtoolStripButton";
            this.NewtoolStripButton.Size = new System.Drawing.Size(35, 22);
            this.NewtoolStripButton.Text = "新規";
            this.NewtoolStripButton.Click += new System.EventHandler(this.NewtoolStripButton_Click);
            // 
            // LoadtoolStripButton
            // 
            this.LoadtoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.LoadtoolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("LoadtoolStripButton.Image")));
            this.LoadtoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.LoadtoolStripButton.Name = "LoadtoolStripButton";
            this.LoadtoolStripButton.Size = new System.Drawing.Size(35, 22);
            this.LoadtoolStripButton.Text = "読込";
            this.LoadtoolStripButton.Click += new System.EventHandler(this.LoadtoolStripButton_Click);
            // 
            // SavetoolStripButton
            // 
            this.SavetoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SavetoolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("SavetoolStripButton.Image")));
            this.SavetoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SavetoolStripButton.Name = "SavetoolStripButton";
            this.SavetoolStripButton.Size = new System.Drawing.Size(35, 22);
            this.SavetoolStripButton.Text = "保存";
            this.SavetoolStripButton.Click += new System.EventHandler(this.SavetoolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // ExecutetoolStripButton
            // 
            this.ExecutetoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ExecutetoolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("ExecutetoolStripButton.Image")));
            this.ExecutetoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ExecutetoolStripButton.Name = "ExecutetoolStripButton";
            this.ExecutetoolStripButton.Size = new System.Drawing.Size(35, 22);
            this.ExecutetoolStripButton.Text = "実行";
            this.ExecutetoolStripButton.Click += new System.EventHandler(this.ExecutetoolStripButton_Click);
            // 
            // DebugExecutetoolStripButton
            // 
            this.DebugExecutetoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.DebugExecutetoolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("DebugExecutetoolStripButton.Image")));
            this.DebugExecutetoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DebugExecutetoolStripButton.Name = "DebugExecutetoolStripButton";
            this.DebugExecutetoolStripButton.Size = new System.Drawing.Size(71, 22);
            this.DebugExecutetoolStripButton.Text = "デバッグ実行";
            this.DebugExecutetoolStripButton.Click += new System.EventHandler(this.DebugExecutetoolStripButton_Click);
            // 
            // WriteFiletoolStripButton
            // 
            this.WriteFiletoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.WriteFiletoolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("WriteFiletoolStripButton.Image")));
            this.WriteFiletoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WriteFiletoolStripButton.Name = "WriteFiletoolStripButton";
            this.WriteFiletoolStripButton.Size = new System.Drawing.Size(35, 22);
            this.WriteFiletoolStripButton.Text = "書出";
            this.WriteFiletoolStripButton.Click += new System.EventHandler(this.WriteFiletoolStripButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // RecordingtoolStripButton
            // 
            this.RecordingtoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.RecordingtoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RecordingtoolStripButton.Name = "RecordingtoolStripButton";
            this.RecordingtoolStripButton.Size = new System.Drawing.Size(35, 22);
            this.RecordingtoolStripButton.Text = "録画";
            this.RecordingtoolStripButton.Click += new System.EventHandler(this.RecordingtoolStripButton_Click);
            // 
            // StoptoolStripButton
            // 
            this.StoptoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.StoptoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StoptoolStripButton.Name = "StoptoolStripButton";
            this.StoptoolStripButton.Size = new System.Drawing.Size(35, 22);
            this.StoptoolStripButton.Text = "停止";
            this.StoptoolStripButton.Click += new System.EventHandler(this.StoptoolStripButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // panelEditor
            // 
            this.panelEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEditor.Location = new System.Drawing.Point(0, 25);
            this.panelEditor.Name = "panelEditor";
            this.panelEditor.Size = new System.Drawing.Size(978, 509);
            this.panelEditor.TabIndex = 4;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.toolStripStatusCoordinate});
            this.statusStrip.Location = new System.Drawing.Point(0, 534);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(978, 22);
            this.statusStrip.TabIndex = 5;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(941, 17);
            this.toolStripStatusLabel.Spring = true;
            // 
            // toolStripStatusCoordinate
            // 
            this.toolStripStatusCoordinate.Name = "toolStripStatusCoordinate";
            this.toolStripStatusCoordinate.Size = new System.Drawing.Size(22, 17);
            this.toolStripStatusCoordinate.Text = "0:0";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // ExecuteStoptoolStripButton
            // 
            this.ExecuteStoptoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ExecuteStoptoolStripButton.Enabled = false;
            this.ExecuteStoptoolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("ExecuteStoptoolStripButton.Image")));
            this.ExecuteStoptoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ExecuteStoptoolStripButton.Name = "ExecuteStoptoolStripButton";
            this.ExecuteStoptoolStripButton.Size = new System.Drawing.Size(35, 22);
            this.ExecuteStoptoolStripButton.Text = "停止";
            this.ExecuteStoptoolStripButton.Click += new System.EventHandler(this.ExecuteStoptoolStripButton_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(978, 556);
            this.Controls.Add(this.panelEditor);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.Text = "StellarRoboEditor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton ExecutetoolStripButton;
        private System.Windows.Forms.ToolStripButton WriteFiletoolStripButton;
        private System.Windows.Forms.ToolStripButton LoadtoolStripButton;
        private System.Windows.Forms.ToolStripButton SavetoolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripButton NewtoolStripButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton RecordingtoolStripButton;
        private System.Windows.Forms.ToolStripButton StoptoolStripButton;
        private System.Windows.Forms.Panel panelEditor;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusCoordinate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton DebugExecutetoolStripButton;
        private System.Windows.Forms.ToolStripButton ExecuteStoptoolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}

