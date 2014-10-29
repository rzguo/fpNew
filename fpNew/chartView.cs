using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using System.Data.SqlClient;

namespace fpNew
{
    public partial class chartView : Form
    {
        /// <summary>
        /// 列表选项改变事件
        /// </summary>
        private event EventHandler checkedListChange;

        /// <summary>
        /// 测孔位移坐标汇总列表
        /// </summary>
        private Dictionary<string, int> holesList = new Dictionary<string, int>();

        public string proID { get; set; }

        /// <summary>
        /// 要绘制的图形的子分类标识
        /// </summary>
        private int typeFlag = -1;

        private editpro.editType _mType;
        private editpro.editType mType
        {
            get
            {
                return _mType;
            }
            set
            {
                _mType = value;
                //根据传入的类型来生成图表
                switch (_mType)
                {

                    case editpro.editType.holesD:
                        {
                            //显示上方工具条
                            cv_ts.Visible = true;
                            //检查子分类typeFlag
                            switch (typeFlag)
                            {
                                case 0:
                                    {
                                        this.Text += "：测斜孔的累计位移曲线图";
                                        break;
                                    }
                                case 1:
                                    {
                                        this.Text += "：不同深度位移量与时间关系曲线图";
                                        cv_tsb_openDepList.Visible = true;
                                        break;
                                    }
                            }
                            //读入所有测孔位移坐标到列表
                            try
                            {
                                login.conn.Open();
                                SqlCommand sc = new SqlCommand("select fp_" + proID + "_0.ID, pName from fp_" + proID + "_PT, fp_" + proID + "_0 where pID=fp_" + proID + "_PT.ID", login.conn);
                                SqlDataReader sdr = sc.ExecuteReader();
                                while (sdr.Read())
                                {
                                    holesList.Add(sdr.GetString(1), sdr.GetInt32(0));
                                    cv_tscb_holesDlist.Items.Add(sdr.GetString(1));
                                }
                            }
                            catch (Exception exc) { throw (exc); }
                            finally { login.conn.Close(); }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// 构造chartView
        /// </summary>
        /// <param name="mType">要绘制的监测类型</param>
        /// <param name="proID">项目ID</param>
        /// <param name="typeFlag">要绘制的图形的子分类标识</param>
        public chartView(editpro.editType mType, string proID, int typeFlag)
        {
            //先初始化控件
            InitializeComponent();

            this.proID = proID;
            this.typeFlag = typeFlag;
            //设置mType时读取数据并显示
            this.mType = mType;
        }

        /// <summary>
        /// 构造chartView
        /// </summary>
        /// <param name="mType">要绘制的监测类型</param>
        /// <param name="proID">项目ID</param>
        public chartView(editpro.editType mType, string proID)
        {
            //先初始化控件
            InitializeComponent();

            this.proID = proID;
            //设置mType时读取数据并显示
            this.mType = mType;
        }

        private void chartView_Load(object sender, EventArgs e)
        {

        }


        private void cv_tscb_holesDlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //1 取出相应点位的所有数据
                SqlDataAdapter sda = new SqlDataAdapter("select * from fp_" + proID + "_0_" + holesList[cv_tscb_holesDlist.SelectedItem.ToString()].ToString(), login.conn);
                DataTable allData = new DataTable();
                sda.Fill(allData);
                //没有数据直接返回
                if (allData.Columns.Count < 3)
                {
                    MessageBox.Show("没有数据！");
                    return;
                }

                //2 获得 列名-日期值 的匿名可枚举类型
                //取出测斜的所有日期值
                sda.SelectCommand.CommandText = "select ID, dateT from fp_" + proID + "_dateRemark where mtypeID='0'";
                DataTable allDates = new DataTable();
                sda.Fill(allDates);
                //用数据表中的日期id组建一个临时的datatable，用于和上面的表做连接，取出本测斜点的日期值
                DataTable dtTemp = new DataTable();
                dtTemp.Columns.Add("ID", typeof(int));
                dtTemp.Columns.Add("name", typeof(string));
                for (int i = 2; i < allData.Columns.Count; i++)
                {
                    dtTemp.Rows.Add(Convert.ToInt32(allData.Columns[i].ColumnName.Substring(1)), allData.Columns[i].ColumnName);
                }
                //连接以上两个表，得到结果
                //var dateList = allDates.AsEnumerable().Join(dtTemp.AsEnumerable(), n => n.Field<int>(0), m => m.Field<int>(0), (n, m) => new { name=m.Field<string>(1), dataT=n.Field<long>(1) });
                var dateList = from n in allDates.AsEnumerable()
                               join m in dtTemp.AsEnumerable()
                               on n.Field<int>(0) equals m.Field<int>(0)
                               select new { name = m.Field<string>(1), dataT = n.Field<long>(1) };
                //日期排序一下，保证作图出来的图例日期连贯
                dateList = from n in dateList orderby n.dataT ascending select n;
                //allData里面的数据，也要按日期的先后顺序排列一下，不然累计图和时间图都会有问题
                List<string> columnNames = new List<string>();
                columnNames.Add("ID");
                columnNames.Add("dep");
                foreach (var temp in dateList) { columnNames.Add(temp.name); }
                allData = allData.DefaultView.ToTable(false, columnNames.ToArray());
                //对数据进行累加，如果没有按日期排序，则可能产生错乱
                for (int i = 0; i < allData.Rows.Count; i++)
                {
                    for (int j = 2; j < allData.Columns.Count - 1; j++)
                    {
                        allData.Rows[i][j + 1] = (double)allData.Rows[i][j + 1] + (double)allData.Rows[i][j];
                    }
                }
                //3 判别要绘制的子类
                switch (typeFlag)
                {
                    case 0: //0为 测斜孔的累计位移曲线图
                        {
                            GraphPane gp = cv_zgc_content.GraphPane;
                            gp.CurveList.Clear();
                            //画图
                            gp.Title.Text = "测斜孔" + cv_tscb_holesDlist.SelectedItem.ToString() + "累计位移曲线图";
                            gp.XAxis.Title.Text = "位移量(cm)";
                            gp.YAxis.Title.Text = "深度(m)";
                            Random colorRand = new Random();
                            foreach (var temp in dateList)
                            {
                                PointPairList ppl = new PointPairList();
                                for (int i = 0; i < allData.Rows.Count; i++)
                                {
                                    ppl.Add(Convert.ToDouble(allData.Rows[i][temp.name]), Convert.ToDouble(allData.Rows[i]["dep"]));
                                }

                                LineItem li = gp.AddCurve(DateTime.FromBinary(temp.dataT).ToShortDateString(), ppl, Color.FromArgb(colorRand.Next(0xFFFFFF) + (int.MaxValue - 0xFFFFFF)), SymbolType.Circle);
                            }

                            cv_zgc_content.AxisChange();
                            cv_zgc_content.Refresh();
                            break;
                        }
                    case 1: //1为 不同深度位移量与时间关系曲线图
                        {
                            //清空之前的事件，列表
                            checkedListChange = null;
                            cv_clb_List.Items.Clear();
                            //插入列表
                            for (int i = 0; i < allData.Rows.Count; i++)
                            {
                                cv_clb_List.Items.Add(allData.Rows[i]["dep"], true);
                            }
                            //设置基本数据
                            GraphPane gp = cv_zgc_content.GraphPane;
                            gp.Title.Text = "测斜孔" + cv_tscb_holesDlist.SelectedItem.ToString() + "不同深度位移量与时间关系曲线图";
                            gp.XAxis.Title.Text = "时间";
                            gp.XAxis.Type = AxisType.Date;
                            gp.YAxis.Title.Text = "位移(cm)";
                            //添加事件
                            checkedListChange += (object o, EventArgs ea) =>
                            {
                                //作图之前清理一下
                                gp.CurveList.Clear();
                                //根据选中的项作出曲线
                                Random colorRand = new Random();
                                foreach (var t in cv_clb_List.CheckedItems)
                                {
                                    //取出一行
                                    var row = allData.AsEnumerable().First(n => n.Field<double>("dep") == (double)t);
                                    PointPairList ppl = new PointPairList();
                                    foreach (var temp in dateList)
                                    {
                                        ppl.Add((double)new XDate(DateTime.FromBinary(temp.dataT)), row.Field<double>(temp.name));
                                    }
                                    LineItem li = gp.AddCurve(t.ToString(), ppl, Color.FromArgb(colorRand.Next(0xFFFFFF) + (int.MaxValue - 0xFFFFFF)), SymbolType.Circle);
                                    //如果是列表触发的事件，则有可能多划一条线，这里作删除处理
                                    if (ea is ItemCheckEventArgs)
                                    {
                                        ItemCheckEventArgs icea = ea as ItemCheckEventArgs;
                                        if (icea.NewValue == CheckState.Unchecked)
                                        {
                                            if(t.ToString()==cv_clb_List.Items[icea.Index].ToString())
                                            {
                                                gp.CurveList.Remove(li);
                                            }
                                        }
                                    }
                                }
                                //假如是列表触发的事件，额外处理 --ItemCheck事件的限制
                                if (ea is ItemCheckEventArgs)
                                {
                                    ItemCheckEventArgs icea = ea as ItemCheckEventArgs;
                                    if (icea.NewValue == CheckState.Checked) //如果新值是checked则画多一条线
                                    {
                                        var row = allData.AsEnumerable().First(n => n.Field<double>("dep") == (double)cv_clb_List.Items[icea.Index]);
                                        PointPairList ppl = new PointPairList();
                                        foreach (var temp in dateList)
                                        {
                                            ppl.Add((double)new XDate(DateTime.FromBinary(temp.dataT)), row.Field<double>(temp.name));
                                        }
                                        LineItem li = gp.AddCurve(cv_clb_List.Items[icea.Index].ToString(), ppl, Color.FromArgb(colorRand.Next(0xFFFFFF) + (int.MaxValue - 0xFFFFFF)), SymbolType.Circle);
                                    }
                                }

                                cv_zgc_content.AxisChange();
                                cv_zgc_content.Refresh();
                            };
                            checkedListChange(sender, e);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

            }
            catch (Exception exc) { throw exc; }
            finally { login.conn.Close(); }
        }

        private void cv_tsb_openDepList_Click(object sender, EventArgs e)
        {
            ToolStripButton tsb = (ToolStripButton)sender;
            if (tsb.CheckState == CheckState.Checked)
            {
                cv_clb_List.Visible = true;
            }
            else
            {
                cv_clb_List.Visible = false;
            }
        }

        private void cv_clb_List_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                if (checkedListChange != null) checkedListChange(sender, e);
            }
            catch (Exception exc) { throw exc; }
        }
    }
}
