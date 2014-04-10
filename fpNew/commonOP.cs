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

        /// <summary>
        /// 格式化执行sql指令，用于insert和update
        /// </summary>
        /// <param name="cmd">sql命令</param>
        /// <param name="format">格式化的名称，需要带@</param>
        /// <param name="value">值</param>
        /// <param name="dbType">其在数据库的类型</param>
        /// <param name="dbSize">在数据库的长度，0为忽略该参数</param>
        /// <param name="connection">sqlconnection</param>
        /// <returns></returns>
        public static int[] formattedModData(string[] cmd, string[,] format, object[,] value, SqlDbType[,] dbType, int[,] dbSize, SqlConnection connection)
        {
            try
            {
                int[] affectLines = new int[cmd.Length];
                if (connection.State == ConnectionState.Closed) connection.Open();
                for (int i = 0; i < cmd.Length; i++)
                {
                    SqlCommand sc = new SqlCommand(cmd[i], connection);
                    for (int j = 0; j < format.GetLength(1); j++)
                    {
                        //假设<=0为不设长度
                        if (dbSize[i, j] <= 0)
                        {
                            sc.Parameters.Add(format[i, j], dbType[i, j]).Value = value[i, j];
                        }
                        else
                        {
                            sc.Parameters.Add(format[i, j], dbType[i, j], dbSize[i, j]).Value = value[i, j];
                        }
                    }
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


        /// <summary>
        /// 执行一个插入命令并返回插入后的ID值
        /// </summary>
        /// <param name="sqlcmd">sql命令</param>
        /// <param name="connection">连接</param>
        /// <returns>ID值</returns>
        public static int insertAndGetID(string sqlcmd, SqlConnection connection)
        {
            try
            {
                connection.Open();
                SqlCommand sc = new SqlCommand(sqlcmd, connection);
                sc.ExecuteNonQuery();
                sc.CommandText = "select @@identity";
                return Convert.ToInt32(sc.ExecuteScalar());
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
