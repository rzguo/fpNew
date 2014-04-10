﻿using System;
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
    public partial class editpro : Form
    {
        private string proID;
        private string proName;
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

        //cellEndEdit事件
        private event EventHandler<DataGridViewCellEventArgs> CellEndEdit;
        //extraList的selectedIndexChanged事件
        private event EventHandler extraList_SelectedIndexChanged;

        //删除项目事件，在foundationPit处有订阅
        public event EventHandler delPro;

        /// <summary>
        /// 读入日期列表，key为ID，value为值
        /// </summary>
        private Dictionary<int, long> readDate()
        {
            Dictionary<int, long> dateList = new Dictionary<int, long>();
            DataTable dt = commonOP.ReadData("select ID,dateT from fp_" + proID + "_dateRemark", login.conn);
            foreach (DataRow row in dt.Rows)
            {
                dateList.Add(row.Field<int>("ID"), row.Field<long>("dateT"));
            }
            return dateList;
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
            this.Text = "编辑项目-" + proName;
            isSave = true;
        }

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        private void editpro_Load(object sender, EventArgs e)
        {
            prepareForEdit(false, false, null, false, false, false, false, false, false, false, false, editType.none);
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
            prepareForEdit(true, false, null, true, false, false, false, false, false, false, false, editType.editPro);
            //建表
            createTable(
                new string[] { "proInfo", "content" },
                new string[] { "项目信息", "内容" },
                new int[] { 1, 1 },
                new Color[] { Color.LightGray, Color.White },
                new int[] { 150, 500 },
                new eCType[] { eCType.textBox, eCType.textBox },
                new bool[] { true, false }
                );

            #region 设置显示数据
            dgv_editpro.Rows.Add(6);
            setCell(0, 0, "坐标系统(国际ID)");
            setCell(1, 0, "地址");
            setCell(2, 0, "委托单位");
            setCell(3, 0, "地质基本情况");
            setCell(4, 0, "设计基本情况");
            setCell(5, 0, "备注");
            DataTable dt = commonOP.ReadData("select * from fpPros where ID='" + proID + "'", login.conn);
            for (int i = 0; i < 6; i++)
            {
                setCell(i, 1, dt.Rows[0][i + 2]);
            }
            #endregion

            #region 绑定保存事件
            save += new EventHandler((object s, EventArgs ea) =>
            {
                if (isSave) return;
                commonOP.formattedModData(
                    new string[] { "update fpPros set coord=@coord,address=@address,firstParty=@firstParty,geology=@geology,design=@design,remark=@remark where ID='" + proID + "'" },
                    new string[,] { { "@coord", "@address", "@firstParty", "@geology", "@design", "@remark" } },
                    new object[,] { { 
                                                dgv_editpro.Rows[0].Cells[1].Value,
                                                dgv_editpro.Rows[1].Cells[1].Value, 
                                                dgv_editpro.Rows[2].Cells[1].Value, 
                                                dgv_editpro.Rows[3].Cells[1].Value, 
                                                dgv_editpro.Rows[4].Cells[1].Value, 
                                                dgv_editpro.Rows[5].Cells[1].Value 
                                            } },
                    new SqlDbType[,] { { SqlDbType.Int, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar, SqlDbType.NVarChar } },
                    new int[,] { { 0, 100, 25, 0, 0, 0 } }, login.conn
                    );
                setSaveState(true);
            });
            #endregion
        }

        /// <summary>
        /// 点位录入
        /// </summary>
        private void 点位PToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                login.conn.Open();
                //设置界面
                prepareForEdit(true, false, null, true, true, true, false, false, false, false, false, editType.pt);
                #region 设置表格样式
                createTable(
                    new string[] { "ID", "pName", "X", "Y", "Z", "strucID", "strucName", "remark" },
                    new string[] { "ID", "点位", "X", "Y", "Z", "所属建筑ID", "所属建筑名", "备注" },
                    null,
                    new Color[] { Color.Gray, Color.White, Color.White, Color.White, Color.White, Color.Gray, Color.White, Color.White },
                    null,
                    new eCType[] { eCType.textBox, eCType.textBox, eCType.textBox, eCType.textBox, eCType.textBox, eCType.textBox, eCType.comboBox, eCType.textBox },
                    new bool[] { true, false, false, false, false, true, false, false }
                    );
                //设置strucName列的combobox模板
                DataGridViewComboBoxCell dgvcbc = new DataGridViewComboBoxCell();
                Dictionary<string, int> strucList = new Dictionary<string, int>(); //用于保存建筑名称对应的ID值***********
                SqlCommand sc = new SqlCommand("select ID,strucName from fp_" + proID + "_struc", login.conn);
                SqlDataReader sdr = sc.ExecuteReader();
                while (sdr.Read())
                {
                    strucList.Add(sdr.GetString(1), sdr.GetInt32(0));
                    dgvcbc.Items.Add(sdr.GetString(1));
                }
                //添加进无建筑选项
                dgvcbc.Items.Add("无");

                dgv_editpro.Columns["strucName"].CellTemplate = dgvcbc;
                login.conn.Close();
                #endregion


                #region 读入数据
                //设置join连接
                string vSqlCommand = "select @pt.ID,@pt.pName,@pt.X,@pt.Y,@pt.Z,@pt.strucID,@struc.strucName,@pt.remark from @pt left outer join @struc on @pt.strucID=@struc.ID";
                vSqlCommand = vSqlCommand.Replace("@pt", "fp_" + proID + "_PT");
                vSqlCommand = vSqlCommand.Replace("@struc", "fp_" + proID + "_struc");
                //读出数据
                DataTable dt = commonOP.ReadData(vSqlCommand, login.conn);
                foreach (DataRow row in dt.Rows)
                {
                    dgv_editpro.Rows.Add(row.ItemArray);
                }
                #endregion

                #region 添加新行事件
                NLine += (object o, EventArgs ea) =>
                {
                    string pName = inputBox("请输入点位名");
                    if (pName != null && pName != "")
                    {
                        try
                        {
                            login.conn.Open();
                            SqlCommand sc2 = new SqlCommand("insert into fp_" + proID + "_PT(pName) values('" + pName + "')", login.conn);
                            sc2.ExecuteNonQuery();
                            sc2.CommandText = "select @@identity";
                            int lastID = Convert.ToInt32(sc2.ExecuteScalar());
                            dgv_editpro.Rows.Add(lastID, pName);
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.Message);
                        }
                        finally
                        {
                            login.conn.Close();
                        }
                    }
                };
                #endregion

                #region 添加选择建筑时的相应事件
                CellEndEdit += (object o, DataGridViewCellEventArgs ea) =>
                {
                    if (currentEditType == editType.pt)
                    {
                        if (ea.ColumnIndex == dgv_editpro.Columns["strucName"].Index)
                        {
                            if (dgv_editpro.Rows[ea.RowIndex].Cells[ea.ColumnIndex].Value.ToString() == "无")
                                setCell(ea.RowIndex, ea.ColumnIndex - 1, null);
                            else setCell(ea.RowIndex, ea.ColumnIndex - 1, strucList[dgv_editpro.Rows[ea.RowIndex].Cells[ea.ColumnIndex].Value.ToString()]);
                        }
                    }
                };
                #endregion

                #region 添加保存事件
                save += (object o, EventArgs ea) =>
                {
                    if (isSave) return;
                    try
                    {
                        login.conn.Open();
                        //逐行更新
                        object X, Y, Z, strucID, remark;
                        for (int i = 0; i < dgv_editpro.Rows.Count; i++)
                        {
                            SqlCommand sc2 = new SqlCommand("update fp_" + proID + "_PT set pName=@pName,X=@X,Y=@Y,Z=@Z,strucID=@strucID,remark=@remark where ID='" + Convert.ToString(dgv_editpro.Rows[i].Cells["ID"].Value) + "'", login.conn);
                            //添加pName参数
                            sc2.Parameters.Add("@pName", SqlDbType.VarChar, 25).Value = dgv_editpro.Rows[i].Cells["pName"].Value;
                            //添加X参数
                            X = dgv_editpro.Rows[i].Cells["X"].Value;
                            if (X == null) sc2.CommandText = sc2.CommandText.Replace("@X", "null");
                            else sc2.Parameters.Add("@X", SqlDbType.Float).Value = X;
                            //添加Y参数
                            Y = dgv_editpro.Rows[i].Cells["Y"].Value;
                            if (Y == null) sc2.CommandText = sc2.CommandText.Replace("@Y", "null");
                            else sc2.Parameters.Add("@Y", SqlDbType.Float).Value = Y;
                            //添加Z参数
                            Z = dgv_editpro.Rows[i].Cells["Z"].Value;
                            if (Z == null) sc2.CommandText = sc2.CommandText.Replace("@Z", "null");
                            else sc2.Parameters.Add("@Z", SqlDbType.Float).Value = Z;
                            //添加strucID参数
                            strucID = dgv_editpro.Rows[i].Cells["strucID"].Value;
                            if (strucID == null) sc2.CommandText = sc2.CommandText.Replace("@strucID", "null");
                            else sc2.Parameters.Add("@strucID", SqlDbType.Int).Value = strucID;
                            //添加remark参数
                            remark = dgv_editpro.Rows[i].Cells["remark"].Value;
                            if (remark == null) sc2.CommandText = sc2.CommandText.Replace("@remark", "null");
                            else sc2.Parameters.Add("@remark", SqlDbType.NVarChar).Value = remark;
                            //执行
                            sc2.ExecuteNonQuery();
                        }
                        setSaveState(true);
                    }
                    catch (Exception exc)
                    {
                        throw (exc);
                    }
                    finally
                    {
                        login.conn.Close();
                    }
                };
                #endregion

                #region 添加删除行事件
                DLine += (object o, EventArgs ea) =>
                {
                    try
                    {
                        login.conn.Open();
                        SqlCommand sc2 = new SqlCommand();
                        sc2.Connection = login.conn;
                        for (int i = 0; i < dgv_editpro.SelectedRows.Count; i++)
                        {
                            //移除数据库条目
                            sc2.CommandText = "delete from fp_" + proID + "_PT where ID='" + dgv_editpro.SelectedRows[i].Cells["ID"].Value.ToString() + "'";
                            sc2.ExecuteNonQuery();
                            //移除界面条目
                            dgv_editpro.Rows.RemoveAt(dgv_editpro.SelectedRows[i].Index);
                        }
                    }
                    catch (Exception exc)
                    {
                        throw (exc);
                    }
                    finally
                    {
                        login.conn.Close();
                    }
                };
                #endregion
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            finally
            {
                login.conn.Close();
            }
        }

        /// <summary>
        /// 建筑录入
        /// </summary>
        private void 建筑SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //设定界面
            prepareForEdit(true, false, null, true, true, true, false, false, false, false, false, editType.struc);
            //设定表格
            createTable(
                new string[] { "ID", "strucName", "height", "remark" },
                new string[] { "ID", "建筑名", "高度", "备注" },
                null,
                new Color[] { Color.Gray, Color.White, Color.White, Color.White },
                null,
                null,
                new bool[] { true, false, false, false }
                );
            //读入数据
            DataTable dt = commonOP.ReadData("select * from fp_" + proID + "_struc", login.conn);
            foreach (DataRow row in dt.Rows)
            {
                dgv_editpro.Rows.Add(row.ItemArray);
            }

            #region 设置添加新行事件
            NLine += (object o, EventArgs ea) =>
            {
                string strucName = inputBox("请输入建筑名");
                if (strucName != null && strucName != "")
                {
                    //插入新行，数据库和界面，这里不能用commonOP，因为一旦关闭conn，就不能获得lastID
                    /*
                    commonOP.modifyData(new string[] { "insert into fp_"+proID+"_struc(strucName) values('"+strucName+"')" }, login.conn);
                    DataTable tempDt = commonOP.ReadData("select @@identity", login.conn);
                    int lastID = Convert.ToInt32(tempDt.Rows[0][0]);
                    dgv_editpro.Rows.Add(lastID, strucName);
                     * */
                    try
                    {
                        //插入新行，数据库和界面，这里不能用commonOP，因为一旦关闭conn，就不能获得lastID
                        login.conn.Open();
                        SqlCommand sc = new SqlCommand("insert into fp_" + proID + "_struc(strucName) values('" + strucName + "')", login.conn);
                        sc.ExecuteNonQuery();
                        sc.CommandText = "select @@identity";
                        int lastID = Convert.ToInt32(sc.ExecuteScalar());
                        dgv_editpro.Rows.Add(lastID, strucName);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.Message);
                    }
                    finally
                    {
                        login.conn.Close();
                    }
                }
            };
            #endregion

            #region 设置删除某行事件
            DLine += (object o, EventArgs ea) =>
            {
                try
                {
                    login.conn.Open();
                    //获取选择的所有行
                    string selectedID;
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;
                    for (int i = 0; i < dgv_editpro.SelectedRows.Count; i++)
                    {
                        selectedID = Convert.ToString(dgv_editpro.SelectedRows[i].Cells["ID"].Value);
                        //从界面删除
                        dgv_editpro.Rows.Remove(dgv_editpro.SelectedRows[i]);
                        //从数据库删除
                        sc.CommandText = "delete from fp_" + proID + "_struc where ID='" + selectedID + "'";
                        sc.ExecuteNonQuery();
                    }
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

            #region 设置保存事件
            save += (object o, EventArgs ea) =>
            {
                if (isSave) return;//保存过直接返回
                try
                {
                    login.conn.Open();
                    int allRowsCount = dgv_editpro.Rows.Count;
                    object height, remark;
                    //逐行插入
                    for (int i = 0; i < allRowsCount; i++)
                    {
                        SqlCommand sc = new SqlCommand();
                        sc.Connection = login.conn;
                        sc.CommandText = "update fp_" + proID + "_struc set strucName=@strucName,height=@height,remark=@remark where ID='" + Convert.ToString(dgv_editpro.Rows[i].Cells["ID"].Value) + "'";
                        //建筑名
                        sc.Parameters.Add("@strucName", SqlDbType.NVarChar, 25).Value = dgv_editpro.Rows[i].Cells["strucName"].Value;
                        //高度
                        height = dgv_editpro.Rows[i].Cells["height"].Value;
                        if (height == null)
                            sc.CommandText = sc.CommandText.Replace("@height", "null");//用替换为null的方法设置为null，如果加入了parameter，则不能设置为null
                        else
                            sc.Parameters.Add("@height", SqlDbType.Float).Value = height;
                        //备注
                        remark = dgv_editpro.Rows[i].Cells["remark"].Value;
                        if (remark == null)
                            sc.CommandText = sc.CommandText.Replace("@remark", "null");
                        else
                            sc.Parameters.Add("@remark", SqlDbType.NVarChar).Value = remark;
                        //执行
                        sc.ExecuteNonQuery();
                    }
                    setSaveState(true);
                }
                catch (Exception exc)
                {
                    throw (exc);
                }
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
            //设置界面
            prepareForEdit(true, true, "本类别备注", true, true, true, true, true, true, true, true, editType.holesD);
            #region 读入已有的测孔位移坐标
            DataTable dt = commonOP.ReadData("select fp_" + proID + "_0.ID,pName from fp_" + proID + "_PT,fp_" + proID + "_0 where pID=fp_" + proID + "_PT.ID", login.conn);
            Dictionary<string, int> ptList = new Dictionary<string, int>();//点位名-汇总表ID 的字典
            foreach (DataRow row in dt.Rows)
            {
                tscb_editpro_extraList.Items.Add(row.Field<string>("pName"));
                ptList.Add(row.Field<string>("pName"), row.Field<int>("ID"));//该ID是汇总表的ID
            }
            tscb_editpro_extraList.Text = "请选择某个坐标";
            #endregion

            #region 读入总备注
            dt = commonOP.ReadData("select remark from fp_" + proID + "_mtype where ID='0'", login.conn);
            tb_editpro_remarks.Text = dt.Rows[0].Field<string>(0);
            setSaveState(true);//如果有数据的话，读入时会触发一次修改，这里纠正过来
            #endregion

            #region 选中某个坐标 //TODO 日期排序
            extraList_SelectedIndexChanged += (object o, EventArgs ea) =>
            {
                dgv_editpro.Columns.Clear();
                //读取数据表
                DataTable dt2 = commonOP.ReadData("select * from fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + " order by dep desc", login.conn);
                //读取日期列表
                Dictionary<int, long> dateList = readDate();
                //添加列到视图
                dgv_editpro.Columns.Add("ID", "ID");
                dgv_editpro.Columns.Add("dep", "深度");
                dgv_editpro.Columns[0].ReadOnly = true;
                dgv_editpro.Columns[0].DefaultCellStyle = new DataGridViewCellStyle() { BackColor = Color.Gray };

                /*
                 * 按日期排序
                 * 由于日期是列，并且只有日期ID，所以首先要根据日期字典组建出列名和日期值的字典
                 * 用这个新的字典按日期值排序，得到排序后的字典，用途如下
                 * 1.用列名可以组建出新的datatable列名数组，用于创建排序后的新表，然后插入新表的数据到视图
                 * 2.列名和日期的字典恰好是视图里面对应的 列名-列表头名 表头名是给用户看的，而列名在保存时需要读取
                 * */
                DataTable sortedDt2 = null;
                if (dt2.Columns.Count > 2)
                {
                    //列名-日期值字典
                    Dictionary<string, long> columnDateT = new Dictionary<string, long>();
                    for (int i = 2; i < dt2.Columns.Count; i++)
                    {
                        columnDateT.Add(dt2.Columns[i].ColumnName, dateList[Convert.ToInt32(dt2.Columns[i].ColumnName.Substring(1))]);
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
                    sortedDt2 = dt2.DefaultView.ToTable(false, columnNames.ToArray());
                }
                //锁定列，防止排序
                for (int i = 0; i < dgv_editpro.Columns.Count; i++)
                {
                    dgv_editpro.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                DataTable finalDT = sortedDt2 == null ? dt2 : sortedDt2;
                //写入数据
                foreach (DataRow row in finalDT.Rows)
                {
                    dgv_editpro.Rows.Add(row.ItemArray);//这里的操作使读出的空值变为0
                }

            };
            #endregion

            #region 添加坐标
            AddPT += (object o, EventArgs ea) =>
            {
                AddPTDialog addptdialog = new AddPTDialog(proID);
                if (addptdialog.ShowDialog() == DialogResult.OK)
                {
                    if (addptdialog.pID != -1)
                    {
                        //为该坐标创建相应的表
                        try
                        {
                            login.conn.Open();
                            //1把该坐标插入到汇总表
                            SqlCommand sc = new SqlCommand("insert into fp_" + proID + "_0(pID) values('" + addptdialog.pID.ToString() + "')", login.conn);
                            sc.ExecuteNonQuery();
                            //2取出插入后的ID
                            sc.CommandText = "select @@identity";
                            string currentID = sc.ExecuteScalar().ToString();
                            //3创建相应的数据存储表
                            sc.CommandText = "create table fp_" + proID + "_0_" + currentID + "(ID int identity(0,1) primary key,dep float not null unique)";
                            sc.ExecuteNonQuery();
                            //4重新读入测孔位移坐标
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

            #region 删除坐标
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
                            //1 移除数据表
                            SqlCommand sc = new SqlCommand("drop table fp_" + proID + "_0_" + selectedPtID, login.conn);
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
                        float depFloat;
                        if (float.TryParse(dep, out depFloat))
                        {
                            //把深度值插入到数据库
                            commonOP.modifyData(new string[] { "insert into fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + "(dep) values('" + dep + "')" }, login.conn);
                            //刷新视图
                            extraList_SelectedIndexChanged(o, ea);
                        }
                        else
                        {
                            throw new Exception("请输入一个小数");
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            };
            #endregion

            #region 添加列
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
                            //插入到dateRemark表并取回ID
                            int currentID = commonOP.insertAndGetID("insert into fp_" + proID + "_dateRemark(dateT,mtypeID) values('" + addia.dateT.ToBinary().ToString() + "','0')", login.conn);
                            //为数据表添加该ID列
                            commonOP.modifyData(new string[] { "alter table fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + " add d" + currentID.ToString() + " float" }, login.conn);
                            //刷新视图
                            extraList_SelectedIndexChanged(o, ea);
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            };
            #endregion

            #region 添加保存事件
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
                        for (int i = 0; i < dgv_editpro.Rows.Count; i++)
                        {
                            //组建update语句
                            sc.CommandText = "update fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + " set dep='" + dgv_editpro.Rows[i].Cells[1].Value.ToString() + "'";
                            for (int j = 2; j < dgv_editpro.Columns.Count; j++)
                            {
                                sc.CommandText += "," + dgv_editpro.Columns[j].Name + "='" + dgv_editpro.Rows[i].Cells[j].Value.ToString() + "'";
                            }
                            sc.CommandText += " where ID='" + dgv_editpro.Rows[i].Cells[0].Value.ToString() + "'";
                            //执行
                            sc.ExecuteNonQuery();
                        }
                        login.conn.Close();
                        //刷新视图
                        extraList_SelectedIndexChanged(o, ea);
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

            #region 删除行事件
            DLine += (object o, EventArgs ea) =>
            {
                try
                {
                    login.conn.Open();
                    SqlCommand sc = new SqlCommand();
                    sc.Connection = login.conn;
                    sc.CommandText = "delete from fp_" + proID + "_0_" + ptList[tscb_editpro_extraList.SelectedItem.ToString()].ToString() + " where ID='@ID'";
                    foreach (DataGridViewRow row in dgv_editpro.SelectedRows)
                    {
                        //从数据表删除选中的一行
                        sc.CommandText = sc.CommandText.Replace("@ID", row.Cells[0].Value.ToString());
                        sc.ExecuteNonQuery();
                    }
                    login.conn.Close();
                    //刷新视图
                    extraList_SelectedIndexChanged(o, ea);
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

            #region 删除列事件
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
                        }
                    }
                    login.conn.Close();
                    //刷新视图
                    extraList_SelectedIndexChanged(o, ea);
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
            prepareForEdit(true, true, "本类别备注", true, true, true, true, false, false, false, true, editType.none);
            //下面再修改当前的录入类型
            switch (mtypeID)
            {
                case "1":
                    currentEditType = editType.pressD;
                    break;
                case "2":
                    currentEditType = editType.subD;
                    break;
                case "3":
                    currentEditType = editType.ropeN;
                    break;
                case "4":
                    currentEditType = editType.supportN;
                    break;
                case "5":
                    currentEditType = editType.waterD;
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
                    Dictionary<int, long> dateList = new Dictionary<int, long>();//这里不能直接readdate，否则conn被关闭
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
                            //插入到数据表
                            commonOP.modifyData(new string[] { "insert into fp_" + proID + "_" + mtypeID + "(pID) values('" + aptd.pID.ToString() + "')" }, login.conn);
                            //刷新视图
                            otherMTypeImport(mtypeID, nameOfOtherRem);
                        }
                    }
                }
                catch (Exception exc)
                {
                    throw exc;
                }
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
                            SqlCommand sc = new SqlCommand("select ID from fp_" + proID + "_dateRemark where dateT='" + addia.dateT.ToBinary().ToString() + "'", login.conn);
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
                    login.conn.Close();
                    otherMTypeImport(mtypeID, nameOfOtherRem);

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
                        }
                        //刷新视图
                        login.conn.Close();
                        otherMTypeImport(mtypeID, nameOfOtherRem);
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
                            sc.CommandText = "alter table fp_" + proID + "_" + mtypeID + " drop column " + dgv_editpro.Columns[n].Name;
                            sc.ExecuteNonQuery();
                        }
                        //刷新视图
                        login.conn.Close();
                        otherMTypeImport(mtypeID, nameOfOtherRem);
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
        /// 设置界面
        /// </summary>
        private void prepareForEdit(bool left, bool right, string rightBanner, bool save, bool addLine, bool delLine, bool delColumn, bool extraList, bool addPT, bool delPT, bool addColumn, editType et)
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
            CellEndEdit = null;
            extraList_SelectedIndexChanged = null;
            //清空界面控件内容
            dgv_editpro.Columns.Clear();
            tscb_editpro_extraList.Items.Clear();
            tb_editpro_remarks.Text = "";
            //重置保存事件
            setSaveState(true);//上面的设置有触发已编辑事件，这里重新改正
        }

        /// <summary>
        /// 添加表格列
        /// </summary>
        /// <param name="name">内部名称</param>
        /// <param name="headerText">列表名称</param>
        /// <param name="sortMode">排列方式0为自动，1为不排序，2为代码排序，null为不排序</param>
        /// <param name="ccolor">底色，null为默认</param>
        /// <param name="width">宽度，null为自动设置</param>
        /// <param name="type">列类型</param>
        private void createTable(string[] name, string[] headerText, int[] sortMode, Color[] ccolor, int[] width, eCType[] ctype, bool[] readOnly)
        {
            for (int i = 0; i < name.Length; i++)
            {
                //获取类型
                DataGridViewColumn co = null;
                if (ctype != null)
                {
                    switch (ctype[i])
                    {
                        case eCType.textBox:
                            co = new DataGridViewTextBoxColumn();
                            break;
                        case eCType.comboBox:
                            co = new DataGridViewComboBoxColumn();
                            break;
                        case eCType.checkbox:
                            co = new DataGridViewCheckBoxColumn();
                            break;
                        case eCType.image:
                            co = new DataGridViewImageColumn();
                            break;
                        case eCType.button:
                            co = new DataGridViewButtonColumn();
                            break;
                        default:
                            co = new DataGridViewLinkColumn();
                            break;
                    }
                }
                else co = new DataGridViewTextBoxColumn();
                //设置名称
                co.Name = name[i];
                //设置列名称
                co.HeaderText = headerText[i];
                //排序模式
                if (sortMode != null)
                {
                    if (sortMode[i] == 0) co.SortMode = DataGridViewColumnSortMode.Automatic;
                    else if (sortMode[i] == 1) co.SortMode = DataGridViewColumnSortMode.NotSortable;
                    else if (sortMode[i] == 2) co.SortMode = DataGridViewColumnSortMode.Programmatic;
                    else co.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                else co.SortMode = DataGridViewColumnSortMode.NotSortable;
                //底色
                if (ccolor != null)
                {
                    co.DefaultCellStyle = new DataGridViewCellStyle() { BackColor = ccolor[i] };
                }
                //宽度
                if (width != null)
                {
                    co.Width = width[i];
                }
                //设置只读属性
                if (readOnly != null)
                {
                    co.ReadOnly = readOnly[i];
                }
                //添加进表
                dgv_editpro.Columns.Add(co);
            }
        }

        /// <summary>
        /// 列类型
        /// </summary>
        enum eCType
        {
            button,
            checkbox,
            comboBox,
            image,
            link,
            textBox
        }

        /// <summary>
        /// 用来表示当前编辑的项目
        /// </summary>
        enum editType
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
            none
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
                catch (Exception)
                {
                    MessageBox.Show("所填写的数据不符合要求！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    this.Text = "编辑项目-" + proName;
                    isSave = true;
                }
                else
                {
                    this.Text = "编辑项目-" + proName + " *";
                    isSave = false;
                }
            }
        }

        /// <summary>
        /// 把某个sql指令字符串的@标签替换为相应的表名
        /// </summary>
        private string formatSqlcommand(string str)
        {
            string temp = "fp_" + proID;
            str = str.Replace("@PT", temp + "_PT");
            str = str.Replace("@struc", temp + "_struc");
            str = str.Replace("@mtype", temp + "_mtype");
            str = str.Replace("@dateRemark", temp + "_dateRemark");
            return str;
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


    }
}