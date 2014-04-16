using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace fpNew
{
    public partial class photoViewer : Form
    {
        //设置photoUrl时
        private string _photoUrl;
        public string photoUrl
        {
            set
            {
                pictureBox1.ImageLocation = value;
                _photoUrl = value;
            }
            get
            {
                return _photoUrl;
            }
        }

        //传递过来的项目ID，用于编辑图片时使用
        private string _proID;
        public string proID
        {
            set
            {
                _proID = value;
                //传递项目ID过来的时候，直接从数据库读出图片
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand("select photo from fpPros where ID='" + proID + "'", login.conn);
                    SqlDataReader sdr = sc.ExecuteReader();
                    if (sdr.Read())
                    {
                        object oImg = sdr.GetValue(0);
                        if (!(oImg is DBNull))
                        {
                            MemoryStream ms = new MemoryStream((byte[])oImg);
                            pictureBox1.Image = Image.FromStream(ms);
                        }
                    }
                }
                catch (Exception exc) { throw exc; }
                finally
                {
                    login.conn.Close();
                }
                //打开编辑工具条
                toolStrip1.Visible = true;
            }
            get
            {
                return _proID;
            }
        }
        public photoViewer()
        {
            InitializeComponent();
            toolStrip1.Visible = false;
        }

        private void photoViewer_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// 换图片
        /// </summary>
        private void tsb_replacePhoto_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPG图像(*.jpg)|*.jpg";
            ofd.Multiselect = false;
            ofd.Title = "请选择图片文件";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                photoUrl = ofd.FileName;
                this.Text += " [未保存]";
            }
        }

        /// <summary>
        /// 保存按钮被单击
        /// </summary>
        private void tsb_save_Click(object sender, EventArgs e)
        {
            try
            {
                //根据url读入图片
                byte[] img = File.ReadAllBytes(photoUrl);
                login.conn.Open();
                SqlCommand sc = new SqlCommand("update fpPros set photo=@photo where ID='" + proID + "'", login.conn);
                sc.Parameters.Add("@photo", SqlDbType.Image).Value = img;
                sc.ExecuteNonQuery();
                this.Text = "图片浏览";
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                login.conn.Close();
            }
        }
    }
}
