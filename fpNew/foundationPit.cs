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
    public partial class foundationPit : Form
    {
        public foundationPit()
        {
            InitializeComponent();
        }

        private void 新建项目NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Addpro();
        }

        private void 退出QToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void foundationPit_Load(object sender, EventArgs e)
        {
            try
            {
                //检查fpPros是否存在
                DataTable dt = commonOP.ReadData("select * from sysobjects where name='fpPros'", login.conn);
                if (dt.Rows.Count == 0)//不存在
                {
                    commonOP.modifyData(new string[] { "create table fpPros(ID int identity(0,1) primary key,proName nvarchar(25) not null unique,coord int,address nvarchar(100),firstParty nvarchar(25),geology nvarchar(max),design nvarchar(max),remark nvarchar(max),photo Image)" }, login.conn);
                }

                //更新主窗体的项目显示列表
                reflesh_lv_fp_pros(this, EventArgs.Empty);
            }
            catch (Exception exc) { MessageBox.Show(exc.Message); }
            finally
            {
                login.conn.Close();
            }
        }

        private void lv_fp_pros_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView.SelectedListViewItemCollection slvic = lv_fp_pros.SelectedItems;
            if (slvic[0].Name == "newPro")
            {
                Addpro();
            }
            else //打开编辑项目窗口
            {
                editpro ep = new editpro(slvic[0].Name, slvic[0].Text);
                ep.delPro += 删除ToolStripMenuItem_Click;
                ep.Show();
            }
        }

        private void Addpro()
        {
            addpro ap = new addpro();
            ap.refleshFP += reflesh_lv_fp_pros;
            ap.Show();
        }

        /// <summary>
        /// 刷新本窗口的项目列表
        /// </summary>
        public void reflesh_lv_fp_pros(object sender, EventArgs e)
        {
            DataTable dt = commonOP.ReadData("select ID,proName from fpPros", login.conn);
            lv_fp_pros.BeginUpdate();
            lv_fp_pros.Items.Clear();
            foreach (DataRow dr in dt.Rows)
            {
                lv_fp_pros.Items.Add(Convert.ToString(dr[0]), Convert.ToString(dr[1]), 1);
            }
            this.lv_fp_pros.Items.Add("newPro", "新建项目", 0);
            this.lv_fp_pros.EndUpdate();
            login.conn.Close();
        }

        /// <summary>
        /// 弹出右键菜单
        /// </summary>
        private void lv_fp_pros_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (lv_fp_pros.SelectedItems[0].Name != "newPro")
                {
                    cms_fp.Show(lv_fp_pros, e.X, e.Y);
                }
            }
        }

        /// <summary>
        /// 删除某个项目
        /// </summary>
        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("真的要删除整个项目吗？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    //取得选中的项目ID
                    string proID = lv_fp_pros.SelectedItems[0].Name;
                    //删除项目以及相关表
                    List<string> cmds = new List<string>();
                    cmds.Add("delete from fpPros where ID='" + proID + "'");
                    cmds.Add("drop table fp_" + proID + "_dateRemark");
                    cmds.Add("drop table fp_" + proID + "_mtype");
                    //添加所有测孔类型数据表
                    DataTable dt = commonOP.ReadData("select ID from fp_"+proID+"_0",login.conn);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow r in dt.Rows)
                        {
                            cmds.Add("drop table fp_"+proID+"_0_"+r.Field<Int32>(0).ToString());
                        }
                    }
                    //添加所有测量类型数据表
                    for (int i = 0; i < 6; i++)
                    {
                        cmds.Add("drop table fp_" + proID + "_" + i.ToString());
                    }
                    cmds.Add("drop table fp_" + proID + "_PT");
                    cmds.Add("drop table fp_" + proID + "_struc");
                    commonOP.modifyData(cmds.ToArray<string>(), login.conn);
                    reflesh_lv_fp_pros(this, EventArgs.Empty);

                    if (sender is editpro)
                    {
                        (sender as editpro).Close();
                    }
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MouseEventArgs mea = new MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 2, 0, 0, 0);
            lv_fp_pros_MouseDoubleClick(this, mea);
        }


    }
}
