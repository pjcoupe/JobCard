namespace Job_Card
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class DataAccess
    {
        private static BindingSource binder = new BindingSource();

        public static Image Base64ToImage(string base64)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64)))
                {
                    return Image.FromStream(stream);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to convert base64 to image - " + exception.Message);
            }
            return null;
        }

        public static Dictionary<string, System.Type> GetFieldDataTypes(string tableName = "jobs")
        {
            Dictionary<string, System.Type> dictionary = new Dictionary<string, System.Type>();
            try
            {
                OleDbConnection selectConnection = new OleDbConnection(ConnectionString);
                OleDbDataAdapter adapter = new OleDbDataAdapter("Select top 1 * from " + tableName, selectConnection);
                DataSet dataSet = new DataSet();
                selectConnection.Open();
                adapter.Fill(dataSet, tableName + "_table");
                selectConnection.Close();
                DataColumnCollection columns = dataSet.Tables[0].Columns;
                foreach (DataColumn column in columns)
                {
                    string columnName = column.ColumnName;
                    System.Type dataType = column.DataType;
                    dictionary.Add(columnName, dataType);
                    Console.WriteLine(columnName + " " + dataType.ToString());
                }
            }
            catch (Exception)
            {
            }
            return dictionary;
        }

        public static string ImageFileToBase64(string path)
        {
            try
            {
                Image image = JobCard.FromFile(path);
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Jpeg);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to get image into string " + path + " - " + exception.Message);
            }
            return null;
        }

        public static string ImageToBase64(Image image)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Jpeg);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to get image" + exception.Message);
            }
            return null;
        }

        public static string StripPhoneAndEmailToSqlSuitable(string phone, string email)
        {            
            List<string> all = new List<string>();
            phone = phone.Trim();
            email = email.Trim();
            int len = 0;
            int nonLen = 0;
        
            string x = "";
            for (int i=0; i < phone.Length; i++)
            {
                var c = phone[i];
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        len++;
                        nonLen = 0;
                        x = x + c;
                        break;
                    case ' ':
                        nonLen++;
                        if (len >= 9)
                        {
                            all.Add(x);
                            x = "";
                            len = 0;
                        }
                        break;
                    default:
                        nonLen++;
                        break;
                }
                if (nonLen >= 2)
                {
                    len = 0;
                    x = "";
                }                
            }
            if (len >= 9)
            {
                all.Add(x);
            }
            string retVal = "";
            for (int i=0; i < all.Count; i++)
            {
                string s = all[i];
                retVal += ((i > 0) ? ",'" : "'") + s + "'"; 
            }
            return retVal;
        }

        public static bool isFussyCustomers(string phone, string email)
        {
            int count = 0;
            OleDbConnection myConnection = new OleDbConnection(ConnectionString);
            myConnection.Open();
            try {
                OleDbCommand myCommand = new OleDbCommand();
                myCommand.Connection = myConnection;
                myCommand.CommandText = "CREATE TABLE fussyCustomer(phoneOrEmail TEXT(50))";
                int result1 = myCommand.ExecuteNonQuery();

                myCommand.CommandText = "CREATE INDEX idxFussyPhoneOrEmail ON fussyCustomer(phoneOrEmail)";
                int result2 = myCommand.ExecuteNonQuery();                
            }
            catch (Exception err)
            {

            };
            try
            {
                OleDbCommand myCommand = new OleDbCommand();
                myCommand.Connection = myConnection;
                string data = StripPhoneAndEmailToSqlSuitable(phone, email);
                if (data != "")
                {
                    string t = "SELECT COUNT(phoneOrEmail) FROM fussyCustomer WHERE phoneOrEmail IN (" + data + ")";
                    myCommand.CommandText = t;
                    count = (int)myCommand.ExecuteScalar();
                }
            } catch (Exception err)
            {
                //MessageBox.Show("PJC REMOVE err " + err);   
            };
            myConnection.Close();
            return count > 0;
        }

        public static DataRowCollection ReadRecords(string sql)
        {
            OleDbConnection selectConnection = null;
            try
            {
                try
                {
                    selectConnection = new OleDbConnection(ConnectionString);
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sql, selectConnection);
                    DataSet dataSet = new DataSet();
                    selectConnection.Open();
                    adapter.Fill(dataSet, "jobs_table");
                    selectConnection.Close();
                    return dataSet.Tables[0].Rows;
                }
                catch (Exception exception)
                {
                    if (selectConnection != null)
                    {
                        selectConnection.Close();
                    }
                    ShowError(exception.Message);
                }
            }
            finally
            {
            }
            return null;
        }

        public static void ReadRecords(DataGridView datagrid, string sql)
        {
            OleDbConnection selectConnection = null;
            try
            {
                try
                {
                    selectConnection = new OleDbConnection(ConnectionString);
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sql, selectConnection);
                    DataSet dataSet = new DataSet();
                    selectConnection.Open();
                    adapter.Fill(dataSet, "jobs_table");
                    int count = 0;
                    if (dataSet.Tables.Count == 1)
                    {
                        count = dataSet.Tables[0].Rows.Count;
                    }
                    selectConnection.Close();
                    datagrid.DataSource = dataSet;
                    datagrid.DataMember = "jobs_table";
                }
                catch (Exception exception)
                {
                    if (selectConnection != null)
                    {
                        selectConnection.Close();
                    }
                    MessageBox.Show("Query failed " + exception.Message);
                }
            }
            finally
            {
            }
        }

        public static object ReadSingleValue(string sql)
        {
            OleDbConnection selectConnection = null;
            try
            {
                try
                {
                    selectConnection = new OleDbConnection(ConnectionString);
                    OleDbDataAdapter adapter = new OleDbDataAdapter(sql, selectConnection);
                    DataSet dataSet = new DataSet();
                    selectConnection.Open();
                    adapter.Fill(dataSet, "jobs_table");
                    selectConnection.Close();
                    int num = 0;
                    while (num < dataSet.Tables[0].Rows.Count)
                    {
                        return dataSet.Tables[0].Rows[num][0];
                    }
                }
                catch (Exception exception)
                {
                    if (selectConnection != null)
                    {
                        selectConnection.Close();
                    }
                    ShowError(exception.Message);
                }
            }
            finally
            {
            }
            return null;
        }

        private static void ShowError(string msg)
        {
            MessageBox.Show(msg, "Database connection error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        public static bool Update(string sql)
        {
            int num = 0;
            OleDbConnection connection = null;
            try
            {
                connection = new OleDbConnection(ConnectionString);
                
                connection.Open();
                using (OleDbCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;
                    num = command.ExecuteNonQuery();
                }
                connection.Close();
                if (num == 0)
                {
                    MessageBox.Show("Error No records updated");
                    num = 1;
                    //throw new Exception("Failed to update " + sql);
                }
                
            }
            catch (Exception exception)
            {
                if (connection != null)
                {
                    if (exception.Message.Contains("Null"))
                    {
                        using (OleDbCommand command = connection.CreateCommand())
                        {
                            sql = sql.Replace("null", "\"\"");
                            command.CommandType = CommandType.Text;
                            command.CommandText = sql;
                            try
                            {
                                num = command.ExecuteNonQuery();
                            }
                            catch (Exception err)
                            {

                            }
                        }                        
                    }
                    connection.Close();
                }
                if (num == 0)
                {
                    MessageBox.Show("Failed to update error " + exception.Message);
                    //ShowError(exception.Message);
                    num = 1;
                }                
            }
            return (num > 0);
        }

        public static void InsertFussyCustomer(string phone, string email = "")
        {
            int num = 0;
            OleDbConnection connection = null;
            try
            {
                connection = new OleDbConnection(ConnectionString);

                connection.Open();
                using (OleDbCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    string phones = StripPhoneAndEmailToSqlSuitable(phone, email);
                    if (phones != "")
                    {
                        string[] split = phones.Split(',');
                        for (int i = 0; i < split.Length; i++)
                        {
                            try
                            {
                                command.CommandText = "INSERT INTO fussyCustomer VALUES (" + split[i] + ")";
                                num = command.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
                connection.Close();
                if (num == 0)
                {
                    MessageBox.Show("Error No records updated");
                    num = 1;
                    //throw new Exception("Failed to update " + sql);
                }

            }
            catch (Exception exception)
            {
            }
        }

        private static string ConnectionString =>
            ("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + JobCard.DBPath + ";User Id=admin;Password=;");
    }
}

