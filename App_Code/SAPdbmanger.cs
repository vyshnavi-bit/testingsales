using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using Npgsql;
using System.Configuration;

/// <summary>
/// Summary description for AccessControldbmanger
/// </summary>
public class SAPdbmanger
{

     string ConnectionString;
     public NpgsqlConnection conn = new NpgsqlConnection();
      List<string> data = new List<string>();
      public NpgsqlCommand cmd;
    //  public string UserName = "";
      object obj_lock = new object();
      string strpgcon = ConfigurationManager.ConnectionStrings["pgsql"].ConnectionString;
      public SAPdbmanger()
      {
          conn.ConnectionString = strpgcon;
      }
      public bool insert(NpgsqlCommand _cmd)
      {
          try
          {
              cmd = _cmd;
              lock (cmd)
              {
                  cmd.Connection = conn;
                  cmd.Connection.Open();
                  cmd.ExecuteNonQuery();
                  cmd.Connection.Close();
              }
              return true;
          }
          catch (Exception ex)
          {
              cmd.Connection.Close();
              throw new ApplicationException(ex.Message);
          }
      }
      public DataSet SelectQuery(NpgsqlCommand _cmd)
      {
          lock (obj_lock)
          {
              cmd = _cmd;

              lock (cmd)
              {
                  try
                  {
                      DataSet ds = new DataSet();
                      cmd.Connection = conn;
                      conn.Open();

                      //cmd.ExecuteNonQuery();
                      NpgsqlDataAdapter sda = new NpgsqlDataAdapter();
                      sda.SelectCommand = cmd;
                      sda.Fill(ds, "Table");
                      conn.Close();
                      return ds;
                  }
                  catch (Exception ex)
                  {
                      conn.Close();
                     throw new ApplicationException(ex.Message);
                  }
              }
          }
      }
      public long insertScalar(NpgsqlCommand _cmd)
      {
          //long sno = 0;
          long newId = 0;
          try
          {
              cmd = _cmd;
              lock (cmd)
              {
                  cmd.Connection = conn;
                  cmd.Connection.Open();
                  cmd.ExecuteNonQuery();
                  //sno = 1;
                  newId = (long)cmd.ExecuteScalar();
                  //sno = (long)cmd.ExecuteScalar();
                  cmd.Connection.Close();
              }
              return newId;
          }
          catch (Exception ex)
          {
              cmd.Connection.Close();
              throw new ApplicationException(ex.Message);
          }
          //}
      }
    public  bool insertVehicleData(string TableName, string feildsseparatedbycomma)
    {
        lock (obj_lock)
        {
            try
            {
                cmd = new NpgsqlCommand("insert into " + TableName + " values(" + feildsseparatedbycomma + ")", conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SqlException exe)
            {
                conn.Close();
                throw new ApplicationException(exe.ErrorCode.ToString());
            }
            return true;
        }
    }

    public bool Update(string table, string Updatestring, string condition)
    {
        lock (obj_lock)
        {
            bool flag = false;
            try
            {
                cmd = new NpgsqlCommand("update " + table + " set " + Updatestring + "where " + condition, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
                conn.Close();
                throw new AccessViolationException(ex.Message);
            }
            return flag;
        }
    }

    public bool Update(string table, string[] fieldNames, string[] fieldValues, string[] conFieldNames, string[] conFieldValues, string[] Operators)
    {
        lock (obj_lock)
        {
            string UpdateString = "";
            string ConditionStr = "";
            cmd = new NpgsqlCommand();
            NpgsqlParameter param;
            try
            {
                if (fieldNames.Count() == fieldValues.Count() && conFieldNames.Count() == conFieldValues.Count())
                {
                    for (int i = 0; i < fieldNames.Count(); i++)
                    {
                        UpdateString += fieldNames[i] + ", ";
                        param = new NpgsqlParameter(fieldNames[i].Split('=')[1], fieldValues[i]);
                        cmd.Parameters.Add(param);
                    }
                    UpdateString = UpdateString.Substring(0, UpdateString.LastIndexOf(","));

                    for (int j = 0; j < conFieldNames.Count(); j++)
                    {
                        ConditionStr += conFieldNames[j] + Operators[j];
                        param = new NpgsqlParameter(conFieldNames[j].Split('=')[1], conFieldValues[j]);
                        cmd.Parameters.Add(param);
                    }

                    cmd.CommandText = "update " + table + " set " + UpdateString + " where " + ConditionStr;
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    return true;
                }
                return false;
            }
            catch (NpgsqlException exe)
            {
                conn.Close();
                throw new ApplicationException(exe.ErrorCode.ToString());
            }
        }

    }

    public   bool Delete(string table, string condition)
    {
        lock (obj_lock)
        {
            bool flag = false;
            try
            {
                cmd = new NpgsqlCommand("delete from " + table + " where " + condition, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                flag = true;

            }
            catch (Exception ex)
            {
                flag = false;
                conn.Close();
                throw new ApplicationException(ex.Message);
            }
            return flag;
        }
    }

    public   DataSet GetReports(string table, string columnNames, string condition)
    {
        lock (obj_lock)
        {
            try
            {
                DataSet ds = new DataSet();
                NpgsqlDataAdapter sda = new NpgsqlDataAdapter("select " + columnNames + " from " + table + " where " + condition, conn);
                conn.Open();
                sda.Fill(ds, table);
                conn.Close();
                return ds;
            }
            catch (Exception ex)
            {
                conn.Close();
                throw new ApplicationException(ex.Message);
            }
        }
    }
    

    ///    <summary>
    ///    Converts a MySqlDataReader to a DataSet
    ///    <param name='reader'>
    /// MySqlDataReader to convert.</param>
    ///    <returns>
    /// DataSet filled with the contents of the reader.</returns>
    ///    </summary>
    public DataSet DataReaderToDataSet(NpgsqlDataReader reader)
    {
        lock (obj_lock)
        {
            DataSet dataSet = new DataSet(); 
            do
            {
                // Create new data table

                DataTable schemaTable = reader.GetSchemaTable();
                DataTable dataTable = new DataTable();

                if (schemaTable != null)
                {
                    // A query returning records was executed

                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        DataRow dataRow = schemaTable.Rows[i];
                        // Create a column name that is unique in the data table
                        string columnName = (string)dataRow["ColumnName"]; //+ "<C" + i + "/>";
                        // Add the column definition to the data table
                        DataColumn column = new DataColumn(columnName, (Type)dataRow["DataType"]);
                        dataTable.Columns.Add(column);
                    }

                    dataSet.Tables.Add(dataTable);

                    // Fill the data table we just created

                    while (reader.Read())
                    {
                        DataRow dataRow = dataTable.NewRow();

                        for (int i = 0; i < reader.FieldCount; i++)
                            dataRow[i] = reader.GetValue(i);

                        dataTable.Rows.Add(dataRow);
                    }
                }
                else
                {
                    // No records were returned

                    DataColumn column = new DataColumn("RowsAffected");
                    dataTable.Columns.Add(column);
                    dataSet.Tables.Add(dataTable);
                    DataRow dataRow = dataTable.NewRow();
                    dataRow[0] = reader.RecordsAffected;
                    dataTable.Rows.Add(dataRow);
                }
            }
            while (reader.NextResult());
            return dataSet;
        }
    }
    public int Update(NpgsqlCommand _cmd) 
    {
        lock (obj_lock)
        { 
            try
            {
                int i = 0;
                cmd = _cmd;
                lock (cmd)
                {
                    cmd.Connection = conn;
                    cmd.Connection.Open();
                    i=cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    return i;
                }
            }
            catch (Exception ex)
            {
                cmd.Connection.Close();
                throw new ApplicationException(ex.Message);
            }
        }
    }

    public void Delete(NpgsqlCommand _cmd)
    {
        lock (obj_lock)
        {
            try
            {
                cmd = _cmd;
                cmd.Connection = conn;
                cmd.Connection.Open();
             int ii=   cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                cmd.Connection.Close();
                throw new ApplicationException(ex.Message);
            }
        }
    }
    public static DateTime GetTime(NpgsqlConnection conn) 
    {
        
        DataSet ds = new DataSet();
        DateTime dt = DateTime.Now;
        NpgsqlCommand cmd = new NpgsqlCommand("SELECT GETDATE()");
        cmd.Connection = conn;
        if (cmd.Connection.State == ConnectionState.Open)
        {
            cmd.Connection.Close();
        }
        conn.Open();
        //cmd.ExecuteNonQuery(); npgsqlda
        NpgsqlDataAdapter sda = new NpgsqlDataAdapter();
        sda.SelectCommand = cmd; 
        sda.Fill(ds, "Table");
        conn.Close();
        if (ds.Tables[0].Rows.Count > 0)
        {
            dt = (DateTime)ds.Tables[0].Rows[0][0];
        }
        return dt;
    }
}