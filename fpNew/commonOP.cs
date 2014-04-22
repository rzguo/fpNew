using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace fpNew
{
    /// <summary>
    /// 通用数据库操作类
    /// </summary>
    class commonOP
    {
        /// <summary>
        /// 根据sql返回一个dataTable
        /// </summary>
        /// <param name="command">需要执行的sql命令</param>
        /// <param name="connection">数据库连接</param>
        public static DataTable ReadData(string command, SqlConnection connection)
        {
            try
            {
                SqlDataAdapter sda = new SqlDataAdapter(command, connection);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                return dt;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        /// <summary>
        /// 执行sql命令[]，并返回受影响的行数[]
        /// </summary>
        /// <param name="command">sql命令[]</param>
        /// <param name="connection">数据库连接</param>
        /// <returns>受影响的行数[]</returns>
        public static int[] modifyData(string[] command, SqlConnection connection)
        {
            try
            {
                if (connection.State == ConnectionState.Closed) connection.Open();
                SqlCommand sc = new SqlCommand();
                sc.Connection = connection;
                int[] affectLines = new int[command.Length];
                for (int i = 0; i < command.Length; i++)
                {
                    sc.CommandText = command[i];
                    affectLines[i] = sc.ExecuteNonQuery();
                }
                return affectLines;
            }
            catch (Exception exc)
            {
                throw exc;
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
