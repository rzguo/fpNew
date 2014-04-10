using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace fpNew
{
    public partial class AddPTDialog : Form
    {
        public int pID { get; private set; }
        private string proID;
        public AddPTDialog(string proID)
        {
            InitializeComponent();
            this.proID = proID;
            this.AcceptButton = btnOK;
            btnOK.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;
            pID = -1;
            ptList = new Dictionary<string, int>();
        }

        private Dictionary<string, int> ptList;
        private void AddPTDialog_Load(object sender, EventArgs e)
        {
            //载入已有坐标
            DataTable dt = commonOP.ReadData("select ID,pName from fp_" + proID + "_PT", login.conn);
            foreach (DataRow row in dt.Rows)
            {
                ptList.Add(row.Field<string>("pName"), row.Field<int>("ID"));
            }
            listBox1.DataSource = ptList.Keys.ToArray<string>();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pID = ptList[listBox1.SelectedItem.ToString()];
        }
    }
}
