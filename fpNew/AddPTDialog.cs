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
    public partial class AddPTDialog : Form
    {
        private Dictionary<string, int> ptList = new Dictionary<string, int>();//用于存储从坐标表读取出来的所有坐标
        /// <summary>
        /// 取得所选择的坐标ID值，若没有选择，返回-1
        /// </summary>
        public int pID { get; private set; }
        public string pName { get; private set; }
        private string _proID;
        public string proID
        {
            set
            {
                _proID = value;
                //读入坐标列表
                if (login.conn.State == ConnectionState.Closed) login.conn.Open();
                SqlCommand sc = new SqlCommand("select ID,pName from fp_" + proID + "_PT order by pName ASC", login.conn);
                SqlDataReader sdr = sc.ExecuteReader();
                while (sdr.Read())
                {
                    ptList.Add(sdr.GetString(1), sdr.GetInt32(0));
                }
                //在视图上显示
                listBox1.DataSource = ptList.Keys.ToArray();
                login.conn.Close();
            }
            get
            {
                return _proID;
            }
        }
        public AddPTDialog(string proID)
        {
            InitializeComponent();
            this.AcceptButton = btnOK;
            btnOK.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;
            // 设定proID并读入坐标列表
            this.proID = proID;
            if (ptList.Count > 0) pID = ptList[listBox1.Items[0].ToString()];
            else pID = -1;
        }

        /// <summary>
        /// 窗口加载
        /// </summary>
        private void AddPTDialog_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 选择某个项
        /// </summary>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pName = listBox1.SelectedItem.ToString();
            pID = ptList[pName];
        }
    }
}
