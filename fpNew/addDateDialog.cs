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
    public partial class addDateDialog : Form
    {
        public DateTime dateT { get; private set; }
        public addDateDialog()
        {
            InitializeComponent();
        }

        private void addDateDialog_Load(object sender, EventArgs e)
        {
            btnOk.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;
            this.AcceptButton = btnOk;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            dateT = new DateTime(dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, Convert.ToInt32(nud_hour.Value), Convert.ToInt32(nud_minute.Value),0);
        }
    }
}
