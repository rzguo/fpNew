﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text.RegularExpressions;


namespace fpNew
{
    public partial class editpro : Form
    {
        private string proID;
        private string proName;
        //当前正在执行的编辑内容名
        private string _editTypeName = "";
        public string editTypeName
        {
            get
            {
                return _editTypeName;
            }
            private set
            {
                _editTypeName = value;
                this.Text = "编辑项目:" + proName + " " + _editTypeName;
            }
        }
        private editType currentEditType;
        private bool isSave;
        //保存事件
        private event EventHandler save;
        //添加新行事件
        private event EventHandler NLine;
        //删除某行事件
        private event EventHandler DLine;
        //添加列事件
        private event EventHandler NColumn;
        //删除列事件
        private event EventHandler DColumn;
        //添加坐标事件
        private event EventHandler AddPT;
        //删除坐标事件
        private event EventHandler DelPT;

        //cellDoubleClick事件
        private event EventHandler<DataGridViewCellEventArgs> CellDoubleClick;
        //cellEndEdit事件
        private event EventHandler<DataGridViewCellEventArgs> CellEndEdit;
        //extraList的selectedIndexChanged事件
        private event EventHandler extraList_SelectedIndexChanged;

        //删除项目事件，在foundationPit处有订阅
        public event EventHandler delPro;

        //用于保存可能发生修改后的ID值的结构实例
        private List<editedCell> editedCells = new List<editedCell>();
        /// <summary>
        /// 用于保存可能发生修改后的ID值的结构
        /// </summary>
        private struct editedCell
        {
            public string ID;
            public int X;
            public int Y;
            public editedCell(string ID, int X, int Y)
            {
                this.ID = ID;
                this.X = X;
                this.Y = Y;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="proID">传入的项目ID值</param>
        /// <param name="proName">项目名称</param>
        public editpro(string proID, string proName)
        {
            InitializeComponent();
            this.proID = proID;
            this.proName = proName;
            this.Text = "编辑项目:" + proName;
            isSave = true;
        }

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        private void editpro_Load(object sender, EventArgs e)
        {
            prepareForEdit(false, false, null, false, false, false, false, false, false, false, false, editType.none, "");
            dgv_editpro.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv_editpro.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        }

        /// <summary>
        /// 删除项目
        /// </summary>
        private void 删除DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            delPro(sender, e);
        }

        /// <summary>
        /// 退出程序
        /// </summary>
        private void 退出QToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 项目编辑
        /// </summary>
        private void 编辑EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 录入准备
            prepareForEdit(true, false, null, true, false, false, false, false, false, false, false, editType.editPro, "项目信息");

            #region 建立表格样式
            dgv_editpro.Columns.Add("proInfo", "项目信息");
            dgv_editpro.Columns.Add("content", "内容");
            dgv_editpro.Columns[0].DefaultCellStyle = new DataGridViewCellStyle() { BackColor = Color.Gray };
            dgv_editpro.Columns[0].ReadOnly = true;
            dgv_editpro.Columns[0].Width = 150;
            dgv_editpro.Columns[1].Width = 500;
            dgv_editpro.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dgv_editpro.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            //添加行
            dgv_editpro.Rows.Add(7);
            setCell(0, 0, "坐标系统(国际ID)");
            setCell(1, 0, "地址");
            setCell(2, 0, "委托单位");
            setCell(3, 0, "地质基本情况");
            setCell(4, 0, "设计基本情况");
            setCell(5, 0, "备注");
            setCell(6, 0, "监测点位布置图");
            setCell(6, 1, "双击编辑点位图");
            dgv_editpro.Rows[6].Cells[1].ReadOnly = true;
            dgv_editpro.Rows[6].Cells[1].Style = new DataGridViewCellStyle() { BackColor = Color.Silver };
            #endregion

            #region 读出数据
            try
            {
                login.conn.Open();
                //先读出除图片外的数据
                SqlDataAdapter sda = new SqlDataAdapter("select coord,address,firstParty,geology,design,remark from fpPros where ID='" + proID + "'", login.conn);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    setCell(i, 1, dt.Rows[0][i]);
                }
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                login.conn.Close();
            }
            #endregion

            #region 设定双击单元格事件
            CellDoubleClick += (object o, DataGridViewCellEventArgs ea) =>
            {
                if (ea.ColumnIndex == 1 && ea.RowIndex == 6)
                {
                    //双击图片单元格后读出image并传递到photoViewer
                    photoViewer pv = new photoViewer();
                    pv.proID = proID;
                    pv.Show();
                }
            };
            #endregion

            #region 设定保存事件
            save += (object o, EventArgs ea) =>
            {
                if (isSave) return;
                try
                {
                    //从表中读取数据
                    string[] content = new string[6];
                    for (int i = 0; i < content.Length; i++)
                    {
                        content[i] = dgv_editpro.Rows[i].Cells[1].Value.ToString();
                    }
                    //保存数据
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand("update fpPros set coord='" + content[0] + "',address='" + content[1] + "',firstParty='" + content[2] + "',geology='" + content[3] + "',design='" + content[4] + "',remark='" + content[5] + "' where ID='" + proID + "'", login.conn);
                    sc.ExecuteNonQuery();
                    setSaveState(true);
                    //刷新视图
                    //login.conn.Close();
                    //编辑EToolStripMenuItem_Click(sender, e);
                }
                catch (Exception exc) { throw exc; }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion
        }

        /// <summary>
        /// 点位录入
        /// </summary>
        private void 点位PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            prepareForEdit(true, false, null, true, true, true, false, false, false, false, false, editType.pt, "点位录入");

            #region 设置表格样式
            dgv_editpro.Columns.Add(new DataGridViewTextBoxColumn() { Name = "ID", HeaderText = "点位ID", DefaultCellStyle = new DataGridViewCellStyle() { BackColor = Color.Gray }, ReadOnly = true });
            dgv_editpro.Columns.Add("pName", "点位名");
            dgv_editpro.Columns.Add("X", "X");
            dgv_editpro.Columns.Add("Y", "Y");
            dgv_editpro.Columns.Add("Z", "Z");
            dgv_editpro.Columns.Add(new DataGridViewTextBoxColumn() { Name = "strucID", HeaderText = "建筑ID", DefaultCellStyle = new DataGridViewCellStyle() { BackColor = Color.Gray }, ReadOnly = true });
            dgv_editpro.Columns.Add(new DataGridViewComboBoxColumn() { Name = "strucName", HeaderText = "建筑名" });
            dgv_editpro.Columns.Add("remark", "备注");
            //锁定排序
            for (int i = 0; i < dgv_editpro.Columns.Count; i++) dgv_editpro.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            #endregion

            #region 读入数据
            //建筑列表，用于选择
            Dictionary<string, int> strucList = new Dictionary<string, int>();
            try
            {
                login.conn.Open();
                //1 读入建筑列表;创建建筑名的选择列模板
                DataGridViewComboBoxCell strucNameTemp = new DataGridViewComboBoxCell();//建筑名模板
                SqlCommand sc = new SqlCommand("select ID,strucName from fp_" + proID + "_struc", login.conn);
                SqlDataReader sdr = sc.ExecuteReader();
                while (sdr.Read())
                {
                    strucList.Add(sdr.GetString(1), sdr.GetInt32(0));
                    strucNameTemp.Items.Add(sdr.GetString(1));
                }
                strucNameTemp.Items.Add(" ");
                sdr.Close();
                dgv_editpro.Columns["strucName"].CellTemplate = strucNameTemp;//设定模板
                //2 读入坐标数据
                sc.CommandText = "select fp_@proID_PT.ID,pName,X,Y,Z,strucID,strucName,fp_@proID_PT.remark from fp_@proID_PT left outer join fp_@proID_struc on fp_@proID_PT.strucID=fp_@proID_struc.ID order by pName ASC";
                sc.CommandText = sc.CommandText.Replace("@proID", proID);
                sdr = sc.ExecuteReader();
                object[] tempRowItem;
                while (sdr.Read())
                {
                    tempRowItem = new object[sdr.FieldCount];
                    sdr.GetValues(tempRowItem);
                    dgv_editpro.Rows.Add(tempRowItem);
                }
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                login.conn.Close();
            }
            #endregion

            #region 添加新行
            NLine += (object o, EventArgs ea) =>
            {
                string pName = inputBox("请输入坐标名");
                if (pName != null && pName != "")
                {
                    try
                    {
                        login.conn.Open();
                        SqlCommand sc = new SqlCommand("insert into fp_" + proID + "_PT(pName) values('" + pName + "')", login.conn);
                        sc.ExecuteNonQuery();
                        //采用直接插入新行到视图的方式代替刷新视图，提高性能
                        sc.CommandText = "select @@identity";
                        object currentID = sc.ExecuteScalar();
                        dgv_editpro.Rows.Add(currentID, pName);//这里后面还未添加的自动变为null，超坑爹，不是DBNull
                        //刷新视图
                        //login.conn.Close();
                        //点位PToolStripMenuItem_Click(sender, e);
                    }
                    catch (Exception exc) { throw exc; }
                    finally
                    {
                        login.conn.Close();
                    }
                }
            };
            #endregion

            #region 建筑列表选择变更事件
            CellEndEdit += (object o, DataGridViewCellEventArgs ea) =>
            {
                if (ea.ColumnIndex == dgv_editpro.Columns["strucName"].Index)
                {
                    string sn = dgv_editpro.Rows[ea.RowIndex].Cells[ea.ColumnIndex].Value.ToString();//选择的建筑名
                    if (sn != " ")
                        setCell(ea.RowIndex, dgv_editpro.Columns["strucID"].Index, strucList[sn]);
                    else
                        setCell(ea.RowIndex, dgv_editpro.Columns["strucID"].Index, DBNull.Value);
                }
            };
            #endregion

            #region 保存事件
            save += (object o, EventArgs ea) =>
            {
                if (isSave) return;
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;
                    //内层循环需要用到的参数
                    string columnName;//列名
                    object value;//改行对应列的值
                    foreach (editedCell ec in editedCells) //仅保存已修改的行
                    {
                        sc.CommandText = "update fp_" + proID + "_PT set ";
                        for (int i = 1; i < dgv_editpro.Columns.Count; i++)
                        {
                            columnName = dgv_editpro.Columns[i].Name;
                            value = dgv_editpro.Rows[ec.X].Cells[i].Value;
                            //如果取得当前单元为空，则设置为"null"，这里value可能出现为null的情况，详见添加新行事件
                            if ((value is DBNull) || value == null) value = "null";
                            if (!columnName.Equals("strucName"))
                            {
                                if (i == 1) sc.CommandText += columnName + "='" + value.ToString() + "'";
                                else sc.CommandText += "," + columnName + "='" + value.ToString() + "'";
                            }
                        }
                        sc.CommandText += " where ID=" + ec.ID;
                        //把 'null' 替换为 null
                        sc.CommandText = sc.CommandText.Replace("'null'", "null");
                        //执行
                        sc.ExecuteNonQuery();
                    }
                    setSaveState(true);
                    //刷新视图
                    //login.conn.Close();
                    //点位PToolStripMenuItem_Click(sender, e);
                }
                catch (Exception exc) { throw exc; }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion

            #region 删除行
            DLine += (object o, EventArgs ea) =>
            {
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;

                    List<int> rowindices = new List<int>();//用于存储选中的行index
                    for (int i = 0; i < dgv_editpro.SelectedCells.Count; i++)
                    {
                        rowindices.Add(dgv_editpro.SelectedCells[i].RowIndex);
                    }
                    //清除多余的index
                    var tempIndices = rowindices.Distinct();

                    //删除每一行
                    if (tempIndices.Count() > 0)
                    {
                        foreach (var n in tempIndices)
                        {
                            sc.CommandText = "delete from fp_" + proID + "_PT where ID=" + dgv_editpro.Rows[n].Cells[0].Value.ToString();
                            sc.ExecuteNonQuery();
                            //移除视图中的行
                            dgv_editpro.Rows.RemoveAt(n);
                        }
                        //刷新视图
                        //login.conn.Close();
                        //点位PToolStripMenuItem_Click(sender, e);
                    }
                }
                catch (Exception exc) { throw exc; }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion
        }

        /// <summary>
        /// 建筑录入
        /// </summary>
        private void 建筑SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            prepareForEdit(true, false, null, true, true, true, false, false, false, false, false, editType.struc, "建筑录入");

            #region 设置表格样式
            dgv_editpro.Columns.Add(new DataGridViewTextBoxColumn() { Name = "ID", HeaderText = "ID", DefaultCellStyle = new DataGridViewCellStyle() { BackColor = Color.Gray }, ReadOnly = true });
            dgv_editpro.Columns.Add("strucName", "建筑名");
            dgv_editpro.Columns.Add("height", "高度");
            dgv_editpro.Columns.Add("remark", "备注");
            //固定排序
            for (int i = 0; i < dgv_editpro.Columns.Count; i++) dgv_editpro.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            #endregion

            #region 读入数据
            try
            {
                login.conn.Open();
                SqlCommand sc = new SqlCommand("select * from fp_" + proID + "_struc", login.conn);
                SqlDataReader sdr = sc.ExecuteReader();
                object[] tempRowItem;
                while (sdr.Read())
                {
                    tempRowItem = new object[sdr.FieldCount];
                    sdr.GetValues(tempRowItem);
                    dgv_editpro.Rows.Add(tempRowItem);
                }
            }
            catch (Exception exc) { throw exc; }
            finally
            {
                login.conn.Close();
            }
            #endregion

            #region 新行
            NLine += (object o, EventArgs ea) =>
            {
                string strucName = inputBox("请输入建筑名");
                if (strucName != "" && strucName != null)
                {
                    try
                    {
                        login.conn.Open();
                        SqlCommand sc = new SqlCommand("insert into fp_" + proID + "_struc(strucName) values('" + strucName + "')", login.conn);
                        sc.ExecuteNonQuery();
                        sc.CommandText = "select @@identity";
                        object ID = sc.ExecuteScalar();
                        dgv_editpro.Rows.Add(ID, strucName);
                    }
                    catch (Exception exc) { throw exc; }
                    finally
                    {
                        login.conn.Close();
                    }
                }
            };
            #endregion

            #region 保存
            save += (object o, EventArgs ea) =>
            {
                if (isSave) return;
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;
                    string columnName;
                    object value;
                    foreach (editedCell ec in editedCells)
                    {
                        sc.CommandText = "update fp_" + proID + "_struc set ";
                        for (int i = 1; i < dgv_editpro.Columns.Count; i++)
                        {
                            columnName = dgv_editpro.Columns[i].Name;
                            value = dgv_editpro.Rows[ec.X].Cells[i].Value;
                            if ((value is DBNull) || value == null) value = "null";//注意下面会变为'null'不是null
                            if (i == 1) sc.CommandText += columnName + "='" + value.ToString() + "'";
                            else sc.CommandText += "," + columnName + "='" + value.ToString() + "'";
                        }
                        sc.CommandText += " where ID=" + ec.ID;
                        //把 'null' 替换为 null
                        sc.CommandText = sc.CommandText.Replace("'null'", "null");
                        sc.ExecuteNonQuery();
                    }
                    setSaveState(true);
                }
                catch (Exception exc) { throw exc; }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion

            #region 删除行
            DLine += (object o, EventArgs ea) =>
            {
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;

                    List<int> rowindices = new List<int>();//用于存储选中的行index
                    for (int i = 0; i < dgv_editpro.SelectedCells.Count; i++)
                    {
                        rowindices.Add(dgv_editpro.SelectedCells[i].RowIndex);
                    }
                    //清除多余的index
                    var tempIndices = rowindices.Distinct();

                    //删除每一行
                    if (tempIndices.Count() > 0)
                    {
                        foreach (var n in tempIndices)
                        {
                            sc.CommandText = "delete from fp_" + proID + "_struc where ID=" + dgv_editpro.Rows[n].Cells[0].Value.ToString();
                            sc.ExecuteNonQuery();
                            //移除视图中的行
                            dgv_editpro.Rows.RemoveAt(n);
                        }
                        //刷新视图
                        //login.conn.Close();
                        //点位PToolStripMenuItem_Click(sender, e);
                    }
                }
                catch (Exception exc) { throw exc; }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion
        }

        /// <summary>
        /// 测孔位移录入
        /// </summary>
        private void 测孔位移HToolStripMenuItem_Click(object sender, EventArgs e)
        {
            prepareForEdit(true, true, "本类别备注", true, true, true, true, true, true, true, true, editType.holesD, "测孔位移录入");
            Dictionary<string, int> ptList = new Dictionary<string, int>();//点位名-汇总表ID 字典
            #region 读入数据
            try
            {
                //1 读入已有测孔位移坐标，并组建 点位名-汇总表ID 字典，用于选择某个点位后检索数据
                login.conn.Open();
                SqlCommand sc = new SqlCommand("select fp_@proID_0.ID,pName from fp_@proID_PT,fp_@proID_0 where pID=fp_@proID_PT.ID", login.conn);
                sc.CommandText = sc.CommandText.Replace("@proID", proID);
                SqlDataReader sdr = sc.ExecuteReader();
                while (sdr.Read())
                {
                    ptList.Add(sdr.GetString(1), sdr.GetInt32(0));
                    //把坐标名添加到选择列表
                    tscb_editpro_extraList.Items.Add(sdr.GetString(1));
                }
                sdr.Close();
                tscb_editpro_extraList.Text = "请选择某个坐标";

                //2 读入总备注
                sc.CommandText = "select remark from fp_" + proID + "_mtype where ID='0'";
                tb_editpro_remarks.Text = sc.ExecuteScalar().ToString();//这里修改了输入框的值，会触发输入框被更改事件，从而把保存状态改为false，下面要重新改正
                setSaveState(true);
            }
            catch (Exception exc) { throw exc; }
            finally { login.conn.Close(); }
            #endregion

            #region 添加新斜测孔坐标
            AddPT += (object o, EventArgs ea) =>
            {
                try
                {
                    AddPTDialog apd = new AddPTDialog(proID);
                    if (apd.ShowDialog() == DialogResult.OK)
                    {
                        if (apd.pID != -1)
                        {
                            login.conn.Open();
                            //1 添加到汇总坐标中
                            SqlCommand sc = new SqlCommand("insert into fp_" + proID + "_0(pID) values('" + apd.pID.ToString() + "')", login.conn);
                            sc.ExecuteNonQuery();
                            //2 为该坐标创建数据表
                            sc.CommandText = "select @@identity";
                            int ID = Convert.ToInt32(sc.ExecuteScalar());
                            sc.CommandText = "create table fp_" + proID + "_0_" + ID.ToString() + "(ID int identity(0,1) primary key,dep float not null unique)";
                            sc.ExecuteNonQuery();
                            //3 添加到 点位名-汇总表ID 字典
                            ptList.Add(apd.pName, ID);
                            //4 添加到视图点位列表
                            tscb_editpro_extraList.Items.Add(apd.pName);
                        }
                    }
                }
                catch (Exception exc) { throw exc; }
                finally { login.conn.Close(); }
            };
            #endregion

            #region 根据选择的坐标读取数据
            extraList_SelectedIndexChanged += (object o, EventArgs ea) =>
            {
                try
                {
                    login.conn.Open();
                    Dictionary<int, long> dateList = new Dictionary<int, long>();//日期列表
                    //清空视图
                    dgv_editpro.Columns.Clear();
                    //1 设置表格样式
                    dgv_editpro.Columns.Add(new DataGridViewTextBoxColumn() { Name = "ID", HeaderText = "ID", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle() { BackColor = Color.Gray } });
                    dgv_editpro.Columns.Add("dep", "深度");
                    //2 读取日期列表，用于排序
                    SqlCommand sc = new SqlCommand("select ID,dateT from fp_" + proID + "_dateRemark", login.conn);
                    SqlDataReader sdr = sc.ExecuteReader();
                    while (sdr.Read())
                    {
                        dateList.Add(sdr.GetInt32(0), sdr.GetInt64(1));
                    }
                    sdr.Close();
                    //3 读出该坐标的所有数据
                    DataTable dt = new DataTable();
                    SqlDataAdapter sda = new SqlDataAdapter("select * from fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + " order by dep DESC", login.conn);
                    sda.Fill(dt);
                    //4 对列名日期排序，得到排序后的新表
                    DataTable sortedDt = null;//排序后的新表
                    if (dt.Columns.Count > 2)
                    {
                        //列名-日期值字典
                        Dictionary<string, long> columnDateT = new Dictionary<string, long>();
                        for (int i = 2; i < dt.Columns.Count; i++)
                        {
                            columnDateT.Add(dt.Columns[i].ColumnName, dateList[Convert.ToInt32(dt.Columns[i].ColumnName.Substring(1))]);
                        }
                        //排序
                        var sortedColumnDateT = columnDateT.OrderBy(n => n.Value);
                        //组出新的列名
                        List<string> columnNames = new List<string>();
                        columnNames.Add("ID");
                        columnNames.Add("dep");
                        foreach (var n in sortedColumnDateT)
                        {
                            columnNames.Add(n.Key);
                            //顺便添加到视图
                            dgv_editpro.Columns.Add(n.Key, DateTime.FromBinary(n.Value).ToString());
                        }
                        //组建新表
                        sortedDt = dt.DefaultView.ToTable(false, columnNames.ToArray());
                    }
                    //锁定列，防止排序
                    for (int i = 0; i < dgv_editpro.Columns.Count; i++)
                    {
                        dgv_editpro.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    DataTable finalDT = sortedDt == null ? dt : sortedDt;
                    //写入数据
                    foreach (DataRow row in finalDT.Rows)
                    {
                        dgv_editpro.Rows.Add(row.ItemArray);//这里的操作使读出的空值变为0
                    }
                }
                catch (Exception exc) { throw exc; }
                finally { login.conn.Close(); }
            };
            #endregion

            #region 添加新行
            NLine += (object o, EventArgs ea) =>
            {
                try
                {
                    if (tscb_editpro_extraList.SelectedItem == null)
                    {
                        throw new Exception("请选择一个坐标");
                    }
                    string dep = inputBox("请输入深度值");
                    if (dep != null && dep != "")
                    {
                        login.conn.Open();
                        SqlCommand sc = new SqlCommand();
                        sc.Connection = login.conn;
                        //匹配数字
                        Regex rx = new Regex(@"^\-?\d+(\.\d+)?$");
                        if (rx.IsMatch(dep))
                        {
                            sc.CommandText = "insert into fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + "(dep) values('" + dep + "')";
                            sc.ExecuteNonQuery();
                            login.conn.Close();
                            //刷新视图
                            extraList_SelectedIndexChanged(sender, e);
                        }
                        else
                        {
                            //匹配快速输入模式 start:end:step
                            rx = new Regex(@"^(?<start>\-?\d+(\.\d+)?)\:(?<end>\-?\d+(\.\d+)?)\:(?<step>\-?\d+(\.\d+)?)$");
                            Match m = rx.Match(dep);
                            if (m.Success)
                            {
                                GroupCollection gc = m.Groups;
                                double start = Convert.ToDouble(gc["start"].Value);
                                double end = Convert.ToDouble(gc["end"].Value);
                                double step = Convert.ToDouble(gc["step"].Value);
                                double temp;
                                //如果start比end大，交换之
                                if (start > end)
                                {
                                    temp = start;
                                    start = end;
                                    end = temp;
                                }
                                //如果step<0，取正
                                if (step < 0) step = -step;
                                for (double i = start; i <= end; i += step)
                                {
                                    sc.CommandText = "insert into fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + "(dep) values('" + i.ToString() + "')";
                                    sc.ExecuteNonQuery();
                                }
                                login.conn.Close();
                                //刷新视图
                                extraList_SelectedIndexChanged(o, ea);
                            }
                            else
                            {
                                throw new Exception("请输入正确的数值格式！");
                            }
                        }

                    }
                }
                catch (Exception exc) { MessageBox.Show(exc.Message); }
                finally { login.conn.Close(); }
            };
            #endregion

            #region 添加新列
            NColumn += (object o, EventArgs ea) =>
            {
                try
                {
                    if (tscb_editpro_extraList.SelectedItem == null)
                    {
                        throw new Exception("请选择一个坐标");
                    }
                    //取得要添加的日期
                    addDateDialog addia = new addDateDialog();
                    if (addia.ShowDialog() == DialogResult.OK)
                    {
                        if (addia.dateT != null)
                        {
                            login.conn.Open();
                            //1 插入到日期表
                            SqlCommand sc = new SqlCommand("insert into fp_" + proID + "_dateRemark(dateT,mtypeID) values('" + addia.dateT.ToBinary().ToString() + "','0')", login.conn);
                            sc.ExecuteNonQuery();
                            //2 取回插入ID
                            sc.CommandText = "select @@identity";
                            string ID = sc.ExecuteScalar().ToString();
                            //3 添加该日期列，列名格式为 d+日期ID
                            sc.CommandText = "alter table fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + " add d" + ID + " float";
                            sc.ExecuteNonQuery();
                            //刷新视图
                            login.conn.Close();
                            extraList_SelectedIndexChanged(o, ea);
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
                finally { login.conn.Close(); }
            };
            #endregion

            #region 保存
            save += (object o, EventArgs ea) =>
            {
                if (isSave) return;
                try
                {
                    login.conn.Open();
                    //先保存总备注
                    SqlCommand sc = new SqlCommand("update fp_" + proID + "_mtype set remark='" + tb_editpro_remarks.Text + "' where ID='0'", login.conn);
                    sc.ExecuteNonQuery();
                    //如果有选择某个坐标才进行坐标保存
                    if (tscb_editpro_extraList.SelectedItem != null)
                    {
                        //仅保存被修改的行
                        foreach (editedCell ec in editedCells)
                        {
                            sc.CommandText = "update fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + " set dep='" + dgv_editpro.Rows[ec.X].Cells[1].Value.ToString() + "'";
                            for (int j = 2; j < dgv_editpro.Columns.Count; j++)
                            {
                                sc.CommandText += "," + dgv_editpro.Columns[j].Name + "='" + dgv_editpro.Rows[ec.X].Cells[j].Value.ToString() + "'";
                            }
                            sc.CommandText += " where ID='" + ec.ID + "'";
                            sc.ExecuteNonQuery();
                        }
                    }
                    setSaveState(true);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion

            #region 删除点位
            DelPT += (object o, EventArgs ea) =>
            {
                if (tscb_editpro_extraList.SelectedIndex != -1)
                {
                    if (MessageBox.Show("是否真的要删除该测斜孔坐标？", "删除", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        try
                        {
                            login.conn.Open();
                            string selectedPtID = ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString();
                            //0 移除该坐标点所有的相关日期项
                            SqlCommand sc = new SqlCommand();
                            sc.Connection = login.conn;
                            for (int i = 2; i < dgv_editpro.Columns.Count; i++)
                            {
                                sc.CommandText = "delete from fp_" + proID + "_dateRemark where ID='" + dgv_editpro.Columns[i].Name.Substring(1) + "'";
                                sc.ExecuteNonQuery();
                            }
                            //1 移除数据表
                            sc.CommandText = "drop table fp_" + proID + "_0_" + selectedPtID;
                            sc.ExecuteNonQuery();
                            //2 在ID汇总表删除该ID行
                            sc.CommandText = "delete from fp_" + proID + "_0 where ID='" + selectedPtID + "'";
                            sc.ExecuteNonQuery();
                            login.conn.Close();
                            //刷新视图
                            测孔位移HToolStripMenuItem_Click(sender, e);
                        }
                        catch (Exception exc) { throw exc; }
                        finally
                        {
                            login.conn.Close();
                        }
                    }
                }
            };
            #endregion

            #region 删除行
            DLine += (object o, EventArgs ea) =>
            {
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;

                    List<int> rowindices = new List<int>();//用于存储选中的行index
                    for (int i = 0; i < dgv_editpro.SelectedCells.Count; i++)
                    {
                        rowindices.Add(dgv_editpro.SelectedCells[i].RowIndex);
                    }
                    //清除多余的index
                    var tempIndices = rowindices.Distinct();

                    //删除每一行
                    if (tempIndices.Count() > 0)
                    {
                        foreach (var n in tempIndices)
                        {
                            sc.CommandText = "delete from fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + " where ID=" + dgv_editpro.Rows[n].Cells[0].Value.ToString();
                            sc.ExecuteNonQuery();
                            //移除视图中的行
                            dgv_editpro.Rows.RemoveAt(n);
                        }
                        //刷新视图
                        //login.conn.Close();
                    }
                }
                catch (Exception exc) { throw exc; }
                finally { login.conn.Close(); }
            };
            #endregion

            #region 删除列
            DColumn += (object o, EventArgs ea) =>
            {
                List<string> selectedColumnIndices = new List<string>();
                foreach (DataGridViewCell cell in dgv_editpro.SelectedCells)
                {
                    selectedColumnIndices.Add(cell.OwningColumn.Name);
                }
                //取得唯一值
                var tempV = selectedColumnIndices.Distinct();
                //开始删除列
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;
                    foreach (var columnName in tempV)
                    {
                        //排除前两列
                        if (columnName != "ID" && columnName != "dep")
                        {
                            //删除数据表中的列
                            sc.CommandText = "alter table fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + " drop column " + columnName;
                            sc.ExecuteNonQuery();
                            //移除日期表中的项
                            sc.CommandText = "delete from fp_" + proID + "_dateRemark where ID='" + Convert.ToInt32(columnName.Substring(1)).ToString() + "'";
                            sc.ExecuteNonQuery();
                            //删除视图的列
                            dgv_editpro.Columns.Remove(columnName);
                        }
                    }
                    //login.conn.Close();
                    //刷新视图
                    //extraList_SelectedIndexChanged(o, ea);
                }
                catch (Exception exc)
                {
                    throw exc;
                }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion
        }

        /// <summary>
        /// 其他测量类型的录入
        /// </summary>
        /// <param name="mtypeID">类型，用类型表的ID表达</param>
        /// <param name="nameOfOtherRem">表示其他备注的名称（常规的备注、工况以外的）</param>
        private void otherMTypeImport(string mtypeID, string nameOfOtherRem)
        {
            prepareForEdit(true, true, "本类别备注", true, true, true, true, false, false, false, true, editType.none, "");
            //下面再修改当前的录入类型
            switch (mtypeID)
            {
                case "1":
                    currentEditType = editType.pressD;
                    editTypeName = "基坑压顶水平位移";
                    break;
                case "2":
                    currentEditType = editType.subD;
                    editTypeName = "沉降位移";
                    break;
                case "3":
                    currentEditType = editType.ropeN;
                    editTypeName = "锚索内力";
                    break;
                case "4":
                    currentEditType = editType.supportN;
                    editTypeName = "支撑轴力";
                    break;
                case "5":
                    currentEditType = editType.waterD;
                    editTypeName = "地下水水位位移";
                    break;
            }
            #region 读入数据

            try
            {
                login.conn.Open();
                //首先读入总备注
                SqlCommand sc = new SqlCommand("select remark from fp_" + proID + "_mtype where ID='" + mtypeID + "'", login.conn);
                tb_editpro_remarks.Text = sc.ExecuteScalar().ToString();
                setSaveState(true);//读入备注的时候会把状态改为已编辑，这里调整保存状态

                //读入坐标列表
                //坐标列表，用于读出数据时改写pID值
                Dictionary<int, string> ptList = new Dictionary<int, string>();
                sc.CommandText = "select ID,pName from fp_" + proID + "_PT";
                SqlDataReader sdr = sc.ExecuteReader();
                while (sdr.Read())
                {
                    ptList.Add(sdr.GetInt32(0), sdr.GetString(1));
                }
                sdr.Close();

                //读入数据表
                SqlDataAdapter sda = new SqlDataAdapter("select * from fp_" + proID + "_" + mtypeID + " order by pID", login.conn);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dgv_editpro.Columns.Add("ID", "ID");
                dgv_editpro.Columns.Add("pID", "点位");
                dgv_editpro.Columns[0].DefaultCellStyle = new DataGridViewCellStyle() { BackColor = Color.Gray };
                dgv_editpro.Columns[1].DefaultCellStyle = new DataGridViewCellStyle() { BackColor = Color.Silver };
                dgv_editpro.Columns[0].ReadOnly = true;
                dgv_editpro.Columns[1].ReadOnly = true;
                //排序后的数据表
                DataTable sortedDt = null;

                //调整日期列
                if (dt.Columns.Count > 2)
                {
                    //读出日期表，用于日期排序
                    Dictionary<int, long> dateList = new Dictionary<int, long>();
                    sc.CommandText = "select ID,dateT from fp_" + proID + "_dateRemark";
                    sdr = sc.ExecuteReader();
                    while (sdr.Read())
                    {
                        dateList.Add(sdr.GetInt32(0), sdr.GetInt64(1));
                    }
                    sdr.Close();

                    //组出 列名-日期值 的字典
                    Dictionary<string, long> columnList = new Dictionary<string, long>();
                    for (int i = 2; i < dt.Columns.Count; i++)
                    {
                        columnList.Add(dt.Columns[i].ColumnName, dateList[Convert.ToInt32(dt.Columns[i].ColumnName.Substring(1))]);
                    }
                    //排序
                    var sortedcolumnList = columnList.OrderBy(n => n.Value);
                    //组建排序后的列名顺序列表
                    List<string> newColumnList = new List<string>();
                    newColumnList.Add("ID");
                    newColumnList.Add("pID");
                    foreach (var n in sortedcolumnList)
                    {
                        newColumnList.Add(n.Key);
                        //顺便把排序后的列添加到视图
                        dgv_editpro.Columns.Add(n.Key, DateTime.FromBinary(n.Value).ToString());
                    }
                    //根据新的列名顺序，组出新的数据表
                    sortedDt = dt.DefaultView.ToTable(false, newColumnList.ToArray());
                }

                //锁定数据表排序
                for (int i = 0; i < dgv_editpro.Columns.Count; i++)
                {
                    dgv_editpro.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                //写入数据
                DataTable finalDt = sortedDt == null ? dt : sortedDt;
                foreach (DataRow row in finalDt.Rows)
                {
                    dgv_editpro.Rows.Add(row.ItemArray);
                }

                //写入"备注,工况,其他备注"三行
                dgv_editpro.Rows.Add("备注", "-------");
                dgv_editpro.Rows.Add("工况", "-------");
                dgv_editpro.Rows.Add(nameOfOtherRem, "-------");
                sda = new SqlDataAdapter("select remark,condition,otherRem from fp_" + proID + "_dateRemark where mtypeID='" + mtypeID + "' order by dateT", login.conn);
                DataTable remarkInfo = new DataTable();
                sda.Fill(remarkInfo);
                if (remarkInfo.Rows.Count > 0)
                {
                    for (int i = 2; i < dgv_editpro.Columns.Count; i++)
                    {
                        for (int j = dgv_editpro.Rows.Count - 3; j < dgv_editpro.Rows.Count; j++)
                        {
                            dgv_editpro.Rows[j].Cells[i].Value = remarkInfo.Rows[i - 2].Field<string>(j - dgv_editpro.Rows.Count + 3);
                        }
                    }
                }

                //改写pID
                for (int i = 0; i < dgv_editpro.Rows.Count - 3; i++)
                {
                    setCell(i, 1, ptList[Convert.ToInt32(dgv_editpro.Rows[i].Cells[1].Value)]);
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
            finally
            {
                login.conn.Close();
            }
            #endregion

            #region 添加行
            NLine += (object o, EventArgs ea) =>
            {
                try
                {
                    //要求选择某个点位
                    AddPTDialog aptd = new AddPTDialog(proID);
                    if (aptd.ShowDialog() == DialogResult.OK)
                    {
                        if (aptd.pID != -1) //默认值是-1
                        {
                            login.conn.Open();
                            //插入到数据表
                            SqlCommand sc = new SqlCommand("insert into fp_" + proID + "_" + mtypeID + "(pID) values('" + aptd.pID.ToString() + "')", login.conn);
                            sc.ExecuteNonQuery();
                            login.conn.Close();
                            //刷新视图
                            otherMTypeImport(mtypeID, nameOfOtherRem);
                        }
                    }
                }
                catch (Exception exc)
                {
                    throw exc;
                }
                finally { login.conn.Close(); }
            };
            #endregion

            #region 添加列
            NColumn += (object o, EventArgs ea) =>
            {
                try
                {
                    //选择某个日期
                    addDateDialog addia = new addDateDialog();
                    if (addia.ShowDialog() == DialogResult.OK)
                    {
                        if (addia.dateT != null)
                        {
                            //0 检查是否已经存在相同日期，若存在则抛出错误
                            login.conn.Open();
                            SqlCommand sc = new SqlCommand("select ID from fp_" + proID + "_dateRemark where dateT='" + addia.dateT.ToBinary().ToString() + "' and mtypeID='" + mtypeID + "'", login.conn);
                            if (sc.ExecuteScalar() != null)
                            {
                                throw new Exception("已存在相同的日期值");
                            }
                            //1 把日期插入dateRemark表并取回ID
                            sc.CommandText = "insert into fp_" + proID + "_dateRemark(dateT,mtypeID) values('" + addia.dateT.ToBinary().ToString() + "','" + mtypeID + "')";
                            sc.ExecuteNonQuery();
                            sc.CommandText = "select @@identity";
                            string currentID = sc.ExecuteScalar().ToString();
                            //2 插入到数据表
                            sc.CommandText = "alter table fp_" + proID + "_" + mtypeID + " add d" + currentID + " float";
                            sc.ExecuteNonQuery();
                            //3 刷新视图
                            login.conn.Close();
                            otherMTypeImport(mtypeID, nameOfOtherRem);//这里之前若不关闭连接会导致连接重复打开
                        }
                    }
                }
                catch (Exception exc)
                {
                    throw exc;
                }
                finally
                {
                    login.conn.Close();//即使throw出来也能关闭conn
                }
            };
            #endregion

            #region 保存
            save += (object o, EventArgs ea) =>
            {
                try
                {
                    if (isSave) return;
                    login.conn.Open();
                    //首先保存总备注
                    SqlCommand sc = new SqlCommand("update fp_" + proID + "_mtype set remark='" + tb_editpro_remarks.Text + "' where ID='" + mtypeID + "'", login.conn);
                    sc.ExecuteNonQuery();
                    //保存日备注及其他信息
                    for (int i = 2; i < dgv_editpro.Columns.Count; i++)
                    {
                        object cell1 = dgv_editpro.Rows[dgv_editpro.Rows.Count - 3].Cells[i].Value;
                        object cell2 = dgv_editpro.Rows[dgv_editpro.Rows.Count - 2].Cells[i].Value;
                        object cell3 = dgv_editpro.Rows[dgv_editpro.Rows.Count - 1].Cells[i].Value;
                        if (cell1 == null) cell1 = "";
                        if (cell2 == null) cell2 = "";
                        if (cell3 == null) cell3 = "";
                        sc.CommandText = "update fp_" + proID + "_dateRemark set remark='" + cell1.ToString() + "',condition='" + cell2.ToString() + "',otherRem='" + cell3.ToString() + "' where dateT='" + DateTime.Parse(dgv_editpro.Columns[i].HeaderText).ToBinary().ToString() + "' and mtypeID='" + mtypeID + "'";
                        sc.ExecuteNonQuery();
                    }

                    //保存数据
                    for (int i = 0; i < dgv_editpro.Rows.Count - 3; i++)
                    {
                        sc.CommandText = "update fp_" + proID + "_" + mtypeID + " set ";
                        //组建命令
                        for (int j = 2; j < dgv_editpro.Columns.Count; j++)
                        {
                            if (j == 2)
                            {
                                sc.CommandText += dgv_editpro.Columns[j].Name + "='" + dgv_editpro.Rows[i].Cells[j].Value.ToString() + "'";
                                continue;
                            }
                            sc.CommandText += "," + dgv_editpro.Columns[j].Name + "='" + dgv_editpro.Rows[i].Cells[j].Value.ToString() + "'";
                        }
                        sc.CommandText += " where ID='" + dgv_editpro.Rows[i].Cells[0].Value.ToString() + "'";
                        sc.ExecuteNonQuery();
                    }
                    //刷新视图
                    //login.conn.Close();
                    //otherMTypeImport(mtypeID, nameOfOtherRem);

                    setSaveState(true);
                }
                catch (Exception exc)
                {
                    throw exc;
                }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion

            #region 删除行
            DLine += (object o, EventArgs ea) =>
            {
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;

                    List<int> rowindices = new List<int>();//用于存储选中的行index
                    for (int i = 0; i < dgv_editpro.SelectedCells.Count; i++)
                    {
                        rowindices.Add(dgv_editpro.SelectedCells[i].RowIndex);
                    }
                    //清除多余的index
                    var tempIndices = rowindices.Distinct().Where(n => n < dgv_editpro.Rows.Count - 3);

                    //删除每一行
                    if (tempIndices.Count() > 0)
                    {
                        foreach (var n in tempIndices)
                        {
                            sc.CommandText = "delete from fp_" + proID + "_" + mtypeID + " where ID='" + dgv_editpro.Rows[n].Cells[0].Value.ToString() + "'";
                            sc.ExecuteNonQuery();
                            //从视图中移除该行
                            dgv_editpro.Rows.RemoveAt(n);
                        }
                        //刷新视图
                        //login.conn.Close();
                        //otherMTypeImport(mtypeID, nameOfOtherRem);
                    }
                }
                catch (Exception exc) { throw exc; }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion

            #region 删除列
            DColumn += (object o, EventArgs ea) =>
            {
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;

                    List<int> columnsIndices = new List<int>();//用于存储选中的列index
                    for (int i = 0; i < dgv_editpro.SelectedCells.Count; i++)
                    {
                        columnsIndices.Add(dgv_editpro.SelectedCells[i].ColumnIndex);
                    }
                    //清除多余的index
                    var tempIndices = columnsIndices.Distinct().Where(n => n > 1);
                    //删除每一列
                    if (tempIndices.Count() > 0)
                    {
                        foreach (var n in tempIndices)
                        {
                            //从数据表中移除
                            sc.CommandText = "alter table fp_" + proID + "_" + mtypeID + " drop column " + dgv_editpro.Columns[n].Name;
                            sc.ExecuteNonQuery();
                            //从日期表中移除
                            sc.CommandText = "delete from fp_" + proID + "_dateRemark where ID='" + dgv_editpro.Columns[n].Name.Substring(1) + "'";
                            sc.ExecuteNonQuery();
                            //从视图中移除该列
                            dgv_editpro.Columns.Remove(dgv_editpro.Columns[n].Name);
                        }
                        //刷新视图
                        //login.conn.Close();
                        //otherMTypeImport(mtypeID, nameOfOtherRem);
                    }

                }
                catch (Exception exc) { throw exc; }
                finally
                {
                    login.conn.Close();
                }
            };
            #endregion
        }

        /// <summary>
        /// 限差录入
        /// </summary>
        private void 限差TToolStripMenuItem_Click(object sender, EventArgs e)
        {
            prepareForEdit(true, false, null, true, false, false, false, false, false, false, false, editType.tolerance, "限差录入");

            #region 生成界面表格
            dgv_editpro.Columns.Add("ID", "ID");
            dgv_editpro.Columns.Add("mtype", "监测类型");
            dgv_editpro.Columns.Add("limit1", "单次容差");
            dgv_editpro.Columns.Add("limit2", "累计容差");
            dgv_editpro.Columns.Add("limit3", "变化速度容差");
            dgv_editpro.Columns[0].ReadOnly = true;
            dgv_editpro.Columns[0].DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.Gray };
            dgv_editpro.Columns[1].ReadOnly = true;
            dgv_editpro.Columns[1].DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.Gray };
            foreach (DataGridViewColumn dgvc in dgv_editpro.Columns)
                dgvc.SortMode = DataGridViewColumnSortMode.NotSortable;
            #endregion

            #region 读入数据
            try
            {
                SqlDataAdapter sda = new SqlDataAdapter("select ID,limit1,limit2,limit3 from fp_" + proID + "_mtype", login.conn);
                DataTable dt = new DataTable();
                sda.Fill(dt); //如果这里limit值有空缺，会自动在datatable里面补上DBNull
                string[] mtypeName = new string[] { "测孔位移", "基坑压顶水平位移", "沉降位移", "锚索内力", "支撑轴力", "地下水水位位移" };
                for (int i = 0; i < 6; i++)
                {
                    dgv_editpro.Rows.Add(dt.Rows[i][0], mtypeName[i], dt.Rows[i][1], dt.Rows[i][2], dt.Rows[i][3]);
                }
            }
            catch (Exception exc) { MessageBox.Show(exc.Message); }
            #endregion

            #region 保存事件
            save += (object o, EventArgs ea) =>
            {
                if (isSave) return;
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;
                    foreach (DataGridViewRow dgvr in dgv_editpro.Rows)
                    {
                        sc.CommandText = "update fp_" + proID + "_mtype set limit1=@lim1, limit2=@lim2, limit3=@lim3 where ID=@ID";
                        sc.CommandText = sc.CommandText.Replace("@ID", dgvr.Cells[0].Value.ToString());
                        object lim1 = dgvr.Cells[2].Value;
                        object lim2 = dgvr.Cells[3].Value;
                        object lim3 = dgvr.Cells[4].Value;
                        sc.CommandText = sc.CommandText.Replace("@lim1", (lim1 is DBNull) ? "null" : lim1.ToString());
                        sc.CommandText = sc.CommandText.Replace("@lim2", (lim2 is DBNull) ? "null" : lim2.ToString());
                        sc.CommandText = sc.CommandText.Replace("@lim3", (lim3 is DBNull) ? "null" : lim3.ToString());
                        sc.ExecuteNonQuery();
                    }
                    setSaveState(true);
                }
                catch (Exception exc) { throw exc; }
                finally { login.conn.Close(); }
            };
            #endregion
            //TODO 3 保存事件
        }

        /// <summary>
        /// 设置界面
        /// </summary>
        private void prepareForEdit(bool left, bool right, string rightBanner, bool save, bool addLine, bool delLine, bool delColumn, bool extraList, bool addPT, bool delPT, bool addColumn, editType et, string etName)
        {

            //界面设置
            splitContainer1.Panel1.Enabled = left;
            splitContainer1.Panel2.Enabled = right;
            lb_editpro_rightBanner.Text = rightBanner;
            tsb_editpro_save.Enabled = save;
            tsb_editpro_newline.Enabled = addLine;
            tsmi_editpro_DLine.Enabled = delLine;
            tsmi_editpro_DColumn.Enabled = delColumn;
            tscb_editpro_extraList.Visible = extraList;
            tsb_editpro_addPT.Visible = addPT;
            tsb_editpro_delPT.Visible = delPT;
            tsb_editpro_NColumn.Visible = addColumn;
            editTypeName = etName;
            //指定录入类型
            currentEditType = et;
            //事件清空
            this.save = null;
            NLine = null;
            DLine = null;
            NColumn = null;
            DColumn = null;
            AddPT = null;
            DelPT = null;
            CellDoubleClick = null;
            CellEndEdit = null;
            extraList_SelectedIndexChanged = null;
            //清空界面控件内容
            dgv_editpro.Columns.Clear();
            tscb_editpro_extraList.Items.Clear();
            tb_editpro_remarks.Text = "";
            //重置保存事件
            editedCells.Clear();
            setSaveState(true);//上面的设置有触发已编辑事件，这里重新改正
        }

        /// <summary>
        /// 用来表示当前编辑的项目
        /// </summary>
        public enum editType
        {
            holesD,
            pressD,
            subD,
            ropeN,
            supportN,
            waterD,
            editPro,
            pt,
            struc,
            none,
            tolerance
        }

        /// <summary>
        /// 创建输入框，并获取返回消息，用于添加新行
        /// </summary>
        /// <param name="caption">标题</param>
        /// <returns></returns>
        private string inputBox(string caption)
        {
            Form inputForm = new Form();
            inputForm.MaximizeBox = false;
            inputForm.MinimizeBox = false;
            inputForm.Height = 130;
            inputForm.Width = 220;
            inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputForm.Text = caption;
            inputForm.StartPosition = FormStartPosition.CenterParent;

            TextBox tb = new TextBox();
            tb.Left = 20;
            tb.Top = 20;
            tb.Width = 170;
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.Parent = inputForm;
            tb.Focus();

            Button btnOk = new Button();
            btnOk.Left = 20;
            btnOk.Top = 60;
            btnOk.Parent = inputForm;
            btnOk.Text = "确定";
            btnOk.FlatStyle = FlatStyle.Flat;
            inputForm.AcceptButton = btnOk;
            btnOk.DialogResult = DialogResult.OK;

            Button btnCancel = new Button();
            btnCancel.Left = 115;
            btnCancel.Top = 60;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Parent = inputForm;
            btnCancel.Text = "取消";
            btnCancel.DialogResult = DialogResult.Cancel;

            try
            {
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    return tb.Text;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                inputForm.Dispose();
            }
        }

        /// <summary>
        /// 设置某个单元格的值
        /// </summary>
        /// <param name="x">行</param>
        /// <param name="y">列</param>
        /// <param name="content">内容</param>
        private void setCell(int x, int y, object content)
        {
            try
            {
                dgv_editpro.Rows[x].Cells[y].Value = content;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 可能发生修改时
        /// </summary>
        private void dgv_editpro_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {

            //把所修改的行ID记录
            //检查是否有相同的记录
            string ID = dgv_editpro.Rows[e.RowIndex].Cells[0].Value.ToString();
            if (!editedCells.Exists(n => n.ID == ID))
            {
                editedCells.Add(new editedCell(ID, e.RowIndex, e.ColumnIndex));
            }
            //设置保存状态
            setSaveState(false);
        }

        /// <summary>
        /// 表格结束编辑
        /// </summary>
        private void dgv_editpro_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (CellEndEdit != null) CellEndEdit(sender, e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 添加新行被单击
        /// </summary>
        private void tsb_editpro_newline_Click(object sender, EventArgs e)
        {
            try
            {
                if (NLine != null) NLine(sender, e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 保存按钮被单击
        /// </summary>
        private void tsb_editpro_save_Click(object sender, EventArgs e)
        {
            if (!isSave)
            {
                try
                {
                    if (save != null) save(sender, e);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        /// <summary>
        /// 设置保存状态，并更新标题栏显示
        /// </summary>
        private void setSaveState(bool saved)
        {
            if (isSave ^ saved)
            {
                if (saved)
                {
                    this.Text = "编辑项目:" + proName + " " + editTypeName;
                    isSave = true;
                }
                else
                {
                    this.Text = "编辑项目:" + proName + " " + editTypeName + " *";
                    isSave = false;
                }
            }
        }

        /// <summary>
        /// extraList的Index改变
        /// </summary>
        private void tscb_editpro_extraList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (extraList_SelectedIndexChanged != null) extraList_SelectedIndexChanged(sender, e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 移除行被单击
        /// </summary>
        private void tsmi_editpro_DLine_Click(object sender, EventArgs e)
        {
            try
            {
                if (DLine != null) DLine(sender, e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 移除列被单击
        /// </summary>
        private void tsmi_editpro_DColumn_Click(object sender, EventArgs e)
        {
            try
            {
                if (DColumn != null) DColumn(sender, e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 添加列被单击
        /// </summary>
        private void tsb_editpro_NColumn_Click(object sender, EventArgs e)
        {
            try
            {
                if (NColumn != null) NColumn(sender, e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 添加坐标被单击
        /// </summary>
        private void tsb_editpro_addPT_Click(object sender, EventArgs e)
        {
            try
            {
                if (AddPT != null) AddPT(sender, e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 用于测孔位移录入，删除某个坐标
        /// </summary>
        private void tsb_editpro_delPT_Click(object sender, EventArgs e)
        {
            try
            {
                if (DelPT != null) DelPT(sender, e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 总备注发生改变
        /// </summary>
        private void tb_editpro_remarks_TextChanged(object sender, EventArgs e)
        {
            setSaveState(false);
            //TODO 读入时会触发，造成显示混乱
        }

        /// <summary>
        /// 基坑压顶录入按钮被点击
        /// </summary>
        private void 基坑压顶水平位移PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                otherMTypeImport("1", "其他");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 沉降位移按钮被点击
        /// </summary>
        private void 沉降位移CToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                otherMTypeImport("2", "荷载情况");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 锚索内力按钮被点击
        /// </summary>
        private void 锚索内力MToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                otherMTypeImport("3", "其他");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 支撑轴力按钮被点击
        /// </summary>
        private void 支撑轴力ZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                otherMTypeImport("4", "其他");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 地下水水位位移按钮被点击
        /// </summary>
        private void 地下水水位位移WToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                otherMTypeImport("5", "其他");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 窗体响应按键
        /// </summary>
        private void editpro_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true)
            {
                switch (e.KeyCode)
                {
                    case Keys.D1:
                        tsb_editpro_newline_Click(sender, e);
                        break;
                    case Keys.D2:
                        tsb_editpro_NColumn_Click(sender, e);
                        break;
                    case Keys.S:
                        tsb_editpro_save_Click(sender, e);
                        break;
                }
            }
        }

        /// <summary>
        /// 某个单元格被双击
        /// </summary>
        private void dgv_editpro_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (CellDoubleClick != null) CellDoubleClick(sender, e);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 输出测孔位移量结果表
        /// </summary>
        private void 测孔位移量表HToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                reportView rv = new reportView(editType.holesD, proID);
                rv.Show();
            }
            catch (Exception exc) { MessageBox.Show(exc.Message); }
        }

        private void 累计位移曲线图DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                chartView cv = new chartView(editType.holesD, proID, 0);
                cv.Show();
            }
            catch (Exception exc) { MessageBox.Show(exc.Message); }
        }

        private void 不同深度位移量与时间关系曲线图TToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                chartView cv = new chartView(editType.holesD, proID, 1);
                cv.Show();
            }
            catch (Exception exc) { MessageBox.Show(exc.Message); }
        }
    }
}
