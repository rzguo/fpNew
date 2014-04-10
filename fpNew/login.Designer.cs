namespace fpNew
{
    partial class login
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(login));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tb_login_pw = new System.Windows.Forms.TextBox();
            this.tb_login_user = new System.Windows.Forms.TextBox();
            this.tb_login_database = new System.Windows.Forms.TextBox();
            this.tb_login_server = new System.Windows.Forms.TextBox();
            this.ch_login_remeber = new System.Windows.Forms.CheckBox();
            this.bt_login_login = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.76271F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 74.23729F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(335, 295);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(329, 69);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tb_login_pw);
            this.groupBox1.Controls.Add(this.tb_login_user);
            this.groupBox1.Controls.Add(this.tb_login_database);
            this.groupBox1.Controls.Add(this.tb_login_server);
            this.groupBox1.Controls.Add(this.ch_login_remeber);
            this.groupBox1.Controls.Add(this.bt_login_login);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 78);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(329, 214);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "登陆信息";
            // 
            // tb_login_pw
            // 
            this.tb_login_pw.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_login_pw.Location = new System.Drawing.Point(141, 128);
            this.tb_login_pw.Name = "tb_login_pw";
            this.tb_login_pw.PasswordChar = '*';
            this.tb_login_pw.Size = new System.Drawing.Size(127, 21);
            this.tb_login_pw.TabIndex = 9;
            // 
            // tb_login_user
            // 
            this.tb_login_user.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_login_user.Location = new System.Drawing.Point(141, 98);
            this.tb_login_user.Name = "tb_login_user";
            this.tb_login_user.Size = new System.Drawing.Size(127, 21);
            this.tb_login_user.TabIndex = 8;
            // 
            // tb_login_database
            // 
            this.tb_login_database.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_login_database.Location = new System.Drawing.Point(141, 68);
            this.tb_login_database.Name = "tb_login_database";
            this.tb_login_database.Size = new System.Drawing.Size(127, 21);
            this.tb_login_database.TabIndex = 7;
            // 
            // tb_login_server
            // 
            this.tb_login_server.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_login_server.Location = new System.Drawing.Point(141, 38);
            this.tb_login_server.Name = "tb_login_server";
            this.tb_login_server.Size = new System.Drawing.Size(127, 21);
            this.tb_login_server.TabIndex = 6;
            // 
            // ch_login_remeber
            // 
            this.ch_login_remeber.AutoSize = true;
            this.ch_login_remeber.Checked = true;
            this.ch_login_remeber.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ch_login_remeber.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ch_login_remeber.Location = new System.Drawing.Point(177, 169);
            this.ch_login_remeber.Name = "ch_login_remeber";
            this.ch_login_remeber.Size = new System.Drawing.Size(45, 16);
            this.ch_login_remeber.TabIndex = 5;
            this.ch_login_remeber.Text = "记住";
            this.ch_login_remeber.UseVisualStyleBackColor = true;
            // 
            // bt_login_login
            // 
            this.bt_login_login.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_login_login.Location = new System.Drawing.Point(96, 166);
            this.bt_login_login.Name = "bt_login_login";
            this.bt_login_login.Size = new System.Drawing.Size(75, 23);
            this.bt_login_login.TabIndex = 4;
            this.bt_login_login.Text = "登陆";
            this.bt_login_login.UseVisualStyleBackColor = true;
            this.bt_login_login.Click += new System.EventHandler(this.bt_login_login_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(58, 132);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "密      码：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(58, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "用  户  名：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(58, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "数  据  库：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(58, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器地址：";
            // 
            // login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(335, 295);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "登陆";
            this.Load += new System.EventHandler(this.login_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox ch_login_remeber;
        private System.Windows.Forms.Button bt_login_login;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_login_pw;
        private System.Windows.Forms.TextBox tb_login_user;
        private System.Windows.Forms.TextBox tb_login_database;
        private System.Windows.Forms.TextBox tb_login_server;

    }
}

