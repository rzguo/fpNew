using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using Microsoft.Reporting.WinForms;

namespace fpNew
{
    public partial class reportView : Form
    {
        //测孔位移坐标汇总列表
        private Dictionary<string, int> holesList = new Dictionary<string, int>();
        private editpro.editType _mType = editpro.editType.none;
        public editpro.editType mType
        {
            get
            {
                return _mType;
            }
            set
            {
                _mType = value;
                //根据传入的类别来生成数据
                switch (_mType)
                {
                    #region 测孔位移量表
                    case editpro.editType.holesD:
                        {
                            //显示上方工具条
                            toolStrip1.Visible = true;
                            //读入所有测孔位移坐标到列表
                            try
                            {
                                login.conn.Open();
                                SqlCommand sc = new SqlCommand("select fp_" + proID + "_0.ID, pName from fp_" + proID + "_PT, fp_" + proID + "_0 where pID=fp_" + proID + "_PT.ID", login.conn);
                                SqlDataReader sdr = sc.ExecuteReader();
                                while (sdr.Read())
                                {
                                    holesList.Add(sdr.GetString(1), sdr.GetInt32(0));
                                    tsc_holesDlist.Items.Add(sdr.GetString(1));
                                }
                            }
                            catch (Exception exc) { throw (exc); }
                            finally { login.conn.Close(); }
                            break;
                        }
                    #endregion
                    default:
                        {
                            break;
                        }
                }
            }
        }

        public string proID { get; set; }

        public reportView(editpro.editType mType, string proID)
        {
            //如果这里不现初始化控件而直接使用，则会导致空指针问题
            InitializeComponent();
            //先设置proID，否则设置mType时没有proID
            this.proID = proID;
            this.mType = mType; //这一步使用了控件，所以必须先初始化控件
        }

        private void reportView_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 载入rdlc
        /// </summary>
        /// <param name="rdlcName">资源文件名</param>
        private void loadRdlc(string rdlcName)
        {
            try
            {
                //代码逻辑未到最好，重写<_<

                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly(); //获取正在运行中的程序集
                Stream st = asm.GetManifestResourceStream("fpNew." + rdlcName); //读出流
                //不存在或者长度不对则重新写出文件
                if (!(File.Exists(rdlcName) && File.OpenRead(rdlcName).Length == st.Length))
                {
                    FileStream fs = File.Create(rdlcName);
                    byte[] buf = new byte[st.Length];
                    st.Read(buf, 0, buf.Length);
                    fs.Write(buf, 0, buf.Length);
                    fs.Close();
                }
                st.Close();
                //载入到控件
                rv_content.Reset();
                rv_content.LocalReport.ReportPath = rdlcName;
                rv_content.RefreshReport();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// 当选择某测孔坐标时
        /// </summary>
        private void tsc_holesDlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //先加载rdlc
                loadRdlc("holesDreport.rdlc");

                string ID = holesList[tsc_holesDlist.SelectedItem.ToString()].ToString(); //这里的ID是汇总ID，不是点位ID
                //读入相应的数据并显示
                holesDcolumnDataClass test = new holesDcolumnDataClass();
                test.data = DateTime.Now;
                test.value = new int[] { 0, 2, 4, 6, 7, 3, 2, 4, 65, 67, 54 };
                List<holesDcolumnDataClass> ls = new List<holesDcolumnDataClass>();
                ls.Add(test);
                ReportDataSource rds = new ReportDataSource("holesDcolumnData", ls);
                rv_content.LocalReport.DataSources.Clear();
                rv_content.LocalReport.DataSources.Add(rds);
                rv_content.RefreshReport();

                
                
            }
            catch (Exception exc) { throw exc; }
            finally { login.conn.Close(); }
        }
    }
}
