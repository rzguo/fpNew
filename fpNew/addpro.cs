using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace fpNew
{
    public partial class addpro : Form
    {
        public event EventHandler refleshFP;

        public addpro()
        {
            InitializeComponent();
        }

        private void bt_addpro_add_Click(object sender, EventArgs e)
        {
            try
            {
                if (login.conn.State == ConnectionState.Closed) login.conn.Open();
                SqlCommand sc = new SqlCommand("insert into fpPros values('" + tb_addpro_proName.Text + "','" + tb_addpro_coord.Text + "','" + tb_addpro_address.Text + "','" + tb_addpro_firstParty.Text + "','" + tb_addpro_geology.Text + "','" + tb_addpro_design.Text + "','" + tb_addpro_remark.Text + "')", login.conn);
                if (sc.ExecuteNonQuery() == 1)
                {
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
                    commonOP.modifyData(relatedTable, login.conn);//这里顺便关闭conn
                    refleshFP(this, EventArgs.Empty);
                    this.Close();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void addpro_Load(object sender, EventArgs e)
        {

        }
    }
}
