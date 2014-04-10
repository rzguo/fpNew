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
using System.Security.Cryptography;

namespace fpNew
{
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
            userFileName = "user.data";
            desKey=new byte[] { 101, 231, 45, 12, 78, 23, 67, 35 };
            desIV = new byte[] { 77, 75, 53, 45, 152, 75, 42, 85 };
        }

        private void login_Load(object sender, EventArgs e)
        {
            fnLogin();
        }

        private string userFileName;
        private byte[] desKey;
        private byte[] desIV;
        public static SqlConnection conn = null;

        /// <summary>
        /// 登陆处理
        /// </summary>
        private void fnLogin()
        {
            if (File.Exists(userFileName))
            {
                FileStream fs = null;
                MemoryStream ms = null;
                try
                {
                    fs = new FileStream(userFileName, FileMode.Open, FileAccess.Read);
                    ms = new MemoryStream();
                    cryptIt(fs, ms, desKey, desIV, false);

                    BinaryReader br = new BinaryReader(ms);
                    tb_login_server.Text = br.ReadString();
                    tb_login_database.Text = br.ReadString();
                    tb_login_user.Text = br.ReadString();
                    tb_login_pw.Text = br.ReadString();
                }
                catch (Exception exc) { MessageBox.Show(exc.Message, "错误"); }
                finally
                {
                    fs.Close();
                    ms.Close();
                }
            }
        }


        private void bt_login_login_Click(object sender, EventArgs e)
        {
            //若选中‘记住’
            if (ch_login_remeber.Checked)
            {
                MemoryStream inS = null;
                FileStream outS = null;
                try
                {
                    inS = new MemoryStream();
                    outS = new FileStream(userFileName, FileMode.Create, FileAccess.Write);
                    //写入inS
                    BinaryWriter bw = new BinaryWriter(inS);
                    bw.Write(tb_login_server.Text);
                    bw.Write(tb_login_database.Text);
                    bw.Write(tb_login_user.Text);
                    bw.Write(tb_login_pw.Text);

                    inS.Position = 0;
                    //加密inS并输入到outS
                    cryptIt(inS, outS, desKey, desIV, true);

                }
                catch (Exception exc) { MessageBox.Show(exc.Message, "错误"); }
                finally
                {
                    inS.Close();
                    outS.Close();
                }
            }
            else
            {
                if (File.Exists(userFileName))
                {
                    File.Delete(userFileName);
                }
            }

            //下面连接数据库
            string connStr = "server=" + tb_login_server.Text + ";uid=" + tb_login_user.Text + ";pwd=" + tb_login_pw.Text + ";database=" + tb_login_database.Text + ";";
            conn = null;
            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                conn.Close();
                foundationPit fp = new foundationPit();
                this.Visible = false;
                //打开主界面（用showdialog占用本窗体的技巧）
                fp.ShowDialog();
                this.Close();
            }
            catch (Exception exc) { MessageBox.Show(exc.Message, "错误"); }
        }


        /// <summary>
        /// 对流进行加密或解密
        /// </summary>
        /// <param name="inS">输入流</param>
        /// <param name="outS">输出流</param>
        /// <param name="desKey">密钥</param>
        /// <param name="desIV">向量</param>
        /// <param name="isEncrypt">是否加密</param>
        /// <returns>成功返回true，失败返回false</returns>
        private static bool cryptIt(Stream inS, Stream outS, byte[] desKey, byte[] desIV, bool isEncrypt)
        {
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                ICryptoTransform mode = null;
                if (isEncrypt)
                    mode = des.CreateEncryptor(desKey, desIV);
                else
                    mode = des.CreateDecryptor(desKey, desIV);
                CryptoStream cs = new CryptoStream(inS, mode, CryptoStreamMode.Read);

                byte[] bin = new byte[100];
                int len;
                while ((len = cs.Read(bin, 0, bin.Length)) != 0)
                {
                    outS.Write(bin, 0, len);
                }

                //流位置重置
                inS.Position = 0;
                outS.Position = 0;
                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "错误");
                return false;
            }

        }
    }
}
