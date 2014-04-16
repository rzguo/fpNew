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
    public partial class addpro : Form
    {
        public event EventHandler refleshFP;

        //用于保存图片路径
        private string photoUrl;

        public addpro()
        {
            InitializeComponent();
        }

        private void addpro_Load(object sender, EventArgs e)
        {

        }

        private void bt_addpro_add_Click(object sender, EventArgs e)
        {
            try
            {
                if (login.conn.State == ConnectionState.Closed) login.conn.Open();
                //写入项目信息，先不插入图片数据
                SqlCommand sc = new SqlCommand("insert into fpPros values('" + tb_addpro_proName.Text + "','" + tb_addpro_coord.Text + "','" + tb_addpro_address.Text + "','" + tb_addpro_firstParty.Text + "','" + tb_addpro_geology.Text + "','" + tb_addpro_design.Text + "','" + tb_addpro_remark.Text + "',null)", login.conn);
                sc.ExecuteNonQuery();
                //获取刚插入的项目ID
                sc.CommandText = @"select @@identity";
                string proID = Convert.ToString(sc.ExecuteScalar());
                //下面创建所有相关表
                string[] relatedTable = new string[] {
                        "create table fp_"+proID+"_struc(ID int identity(0,1) primary key,strucName nvarchar(25) not null unique,height float,remark nvarchar(max))",
                        "create table fp_"+proID+"_mtype(ID int identity(0,1) primary key,mtype nvarchar(25) not null unique,innerName varchar(20) not null unique,remark nvarchar(max),limit1 float,limit2 float,limit3 float,limRemark nvarchar(max))",
                        "create table fp_"+proID+"_PT(ID int identity(0,1) primary key,pName varchar(25) not null unique,X float,Y float,Z float,strucID int foreign key references fp_"+proID+"_struc(ID),remark nvarchar(max))",
                        "create table fp_"+proID+"_dateRemark(ID int identity(0,1) primary key,dateT bigint not null,mtypeID int not null foreign key references fp_"+proID+"_mtype(ID),remark nvarchar(max),condition nvarchar(max),otherRem nvarchar(max))",
                        "insert into fp_"+proID+"_mtype(mtype,innerName) values('测孔位移','holesD')",
                        "insert into fp_"+proID+"_mtype(mtype,innerName) values('基坑压顶水平位移','pressD')",
                        "insert into fp_"+proID+"_mtype(mtype,innerName) values('沉降位移','subD')",
                        "insert into fp_"+proID+"_mtype(mtype,innerName) values('锚索内力','ropeN')",
                        "insert into fp_"+proID+"_mtype(mtype,innerName) values('支撑轴力','supportN')",
                        "insert into fp_"+proID+"_mtype(mtype,innerName) values('地下水水位位移','waterD')",
                        "create table fp_"+proID+"_0(ID int identity(0,1) primary key,pID int not null unique foreign key references fp_"+proID+"_PT(ID))",
                        "create table fp_"+proID+"_1(ID int identity(0,1) primary key,pID int not null unique foreign key references fp_"+proID+"_PT(ID))",
                        "create table fp_"+proID+"_2(ID int identity(0,1) primary key,pID int not null unique foreign key references fp_"+proID+"_PT(ID))",
                        "create table fp_"+proID+"_3(ID int identity(0,1) primary key,pID int not null unique foreign key references fp_"+proID+"_PT(ID))",
                        "create table fp_"+proID+"_4(ID int identity(0,1) primary key,pID int not null unique foreign key references fp_"+proID+"_PT(ID))",
                        "create table fp_"+proID+"_5(ID int identity(0,1) primary key,pID int not null unique foreign key references fp_"+proID+"_PT(ID))",
                    };
                //创建表
                foreach (string s in relatedTable)
                {
                    sc.CommandText = s;
                    sc.ExecuteNonQuery();
                }
                //下面把图片数据插入数据库
                if (photoUrl != null && photoUrl != "")
                {
                    //读入图片字节photo
                    byte[] photo = File.ReadAllBytes(photoUrl);
                    //利用参数化方法插入到数据库
                    sc.CommandText = "update fpPros set photo=@photo where ID='" + proID + "'";
                    sc.Parameters.Add("@photo", SqlDbType.Image).Value = photo;
                    sc.ExecuteNonQuery();
                }
                login.conn.Close();
                refleshFP(sender, e);
                this.Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 浏览图片按钮被单击
        /// </summary>
        private void bt_addpro_SelectPhoto_Click(object sender, EventArgs e)
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
                lb_addpro_photoUrl.Text = photoUrl.Substring(photoUrl.LastIndexOf('\\') + 1) + "(点击查看)";
            }
        }

        /// <summary>
        /// 点击了图片地址
        /// </summary>
        private void lb_addpro_photoUrl_Click(object sender, EventArgs e)
        {
            if (photoUrl != null && photoUrl != "")
            {
                photoViewer pv = new photoViewer();
                pv.photoUrl = photoUrl;
                pv.Show();
            }
        }
    }
}
