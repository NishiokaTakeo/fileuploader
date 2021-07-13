using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Data;


namespace fileuploader
{

    public class DaoBase : IDisposable
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        Logger backuplogger = LogManager.GetLogger("SQLBackup");
        SqlConnection _connection;


        protected SqlConnection DBConnection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public DaoBase(string connection)
        {
            DBConnection = new SqlConnection(connection);
            DBConnection.Open();
        }

        public IEnumerable<T> ExecuteSQL<T>(string sql, Dictionary<string, object> p = null)
        {

            List<T> list = new List<T>();

            try
            {
                using (SqlCommand sqlCommand = new SqlCommand(sql, DBConnection))
                {
                    sqlCommand.Parameters.Clear();


                    p?.ToList().ForEach(x =>
                    {
                        sqlCommand.Parameters.AddWithValue(x.Key, x.Value);
                    });

                    sqlCommand.ExecuteNonQuery();

                    using (SqlDataReader dr = sqlCommand.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add((T)Activator.CreateInstance(typeof(T), dr));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                backuplogger.Info("FAILED SELECT EXECUTE : {0} - {1}", sql, Newtonsoft.Json.JsonConvert.SerializeObject(p));
                logger.Error(ex, "DB Error : {0}",sql);
            }
            finally
            {
                logger.Debug("SQL Query - Raw : {0} - {1}", sql, Newtonsoft.Json.JsonConvert.SerializeObject(p));
            }

            return list;

        }

        /// <summary>
        /// Return List of Passed type using passed table paramter. Value should be other than default.
        /// It is prototype. Do NOT use if you get any error.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteSQLDynamic<T>(T table) where T: TableBase
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            string where = " ( 1 = 1 ) ";   // default is all

            //String sql = "SELECT * FROM [tblContact] WHERE [StudentId] = @id ORDER BY ContactId DESC";
            //string sql = "SELECT * FROM [{0}] WHERE {1} ORDER BY {2} DESC";
            string sql = "SELECT * FROM [{0}] WHERE {1} ORDER BY {2} DESC";

            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                table.GetColumns().ToList().ForEach(x =>
                {
                    if (x.Replace(" ", "") == prop.Name)
                    {
                        var value = prop.GetValue(table, null);

                        // set where if type is int or string and filled other than default.
                        if (
                            (prop.PropertyType == typeof(int) && (int)value > 0)
                            || (prop.PropertyType == typeof(string) && !string.IsNullOrWhiteSpace((string)value))
                        )
                        {
                            where += " AND [" + x + "] = @" + prop.Name + " ";
                            dic.Add("@" + prop.Name, value);
                        }
                    }
                });
            }

            //sql = sql.Replace("#1", where);
            sql = string.Format(sql, table.TableName, where, table.DBKey);

            IEnumerable<T> tbls = this.ExecuteSQL<T>(sql, dic);

            return tbls;
        }

        public bool ExecuteUpdateSQL<T>(T table) where T : TableBase
        {
            Dictionary<string, object> p = new Dictionary<string, object>();

            var keyvalue = ((T)table).GetType().GetProperty(table.DBKey).GetValue(((T)table), null);

            string sql = "UPDATE #0 SET #1 WHERE #2".Replace("#2", string.Format("{0} = {1};",table.DBKey,keyvalue));

            string v1 = "";
            int i = 0;

            table.GetColumns().Where(x => x != table.DBKey).ToList()
                .ForEach(x =>
                {
                    ++i;
                    x = x.Replace(" ", "");

                    string pn = "@P" + (i).ToString();

                    var pv = ((T)table).GetType().GetProperty(x).GetValue(((T)table), null);

                    if (pv == null) return;

                    if ( !p.ContainsKey(pn) ) p.Add(pn, pv);

                    v1 += (i == 1) ? "[" + x + "]" : ",[" + x + "]";
                    v1 += " = " +  pn;
                });

            // Marge columns name and its values to main SQL
            sql = sql.Replace("#0", table.TableName).Replace("#1", v1);

            try
            {
                using (SqlCommand sqlCommand = new SqlCommand(sql, DBConnection))
                {
                    sqlCommand.Parameters.Clear();

                    p.ToList().ForEach(x =>
                    {
                        sqlCommand.Parameters.AddWithValue(x.Key, x.Value);
                    });

                    sqlCommand.ExecuteScalar();

                }
            }
            catch (Exception ex)
            {
                backuplogger.Info("FAILED UPDATE EXECUTE : {0} - {1} - {2}", sql, Newtonsoft.Json.JsonConvert.SerializeObject(p), Newtonsoft.Json.JsonConvert.SerializeObject(table));
                logger.Error(ex, "DB Error: {0}",sql);
                return false;
            }
            finally
            {
                logger.Debug("SQL Query - Updated : {0} - {1}",sql, Newtonsoft.Json.JsonConvert.SerializeObject(p));
            }

            return true;

        }
        public int ExecuteInsertSQL<T>(T table) where T : TableBase
        {
            Dictionary<string, object> p = new Dictionary<string, object>();

            string sql = "INSERT INTO #0 (#1)  VALUES (#2);SELECT SCOPE_IDENTITY();";
            string v1 = "";
            string v2 = "";
            int i = 0;
            int lastID = 0;

            table.GetColumns().Where(x => x != table.DBKey).ToList()
                .ForEach(x =>
                {
                    ++i;
                    x = x.Replace(" ", "");

                    string pn = "@P" + (i).ToString();

                    var pv = ((T)table).GetType().GetProperty(x).GetValue(((T)table), null);

                    if (pv == null) return;

                    if (pv.GetType() == typeof(DateTime) || pv.GetType() == typeof(DateTime?))
                    {
                        string date = ((DateTime)pv).ToString("yyyy-MM-dd HH':'mm':'ss");

                        if (!p.ContainsKey(pn)) p.Add(pn, date);
                    }
                    else
                    {
                        if (!p.ContainsKey(pn)) p.Add(pn, pv);
                    }


                    v1 += (i == 1) ? "[" + x + "]" : ",[" + x + "]";
                    v2 += (i == 1) ? pn : "," + pn;

                });

            // Marge columns name and its values to main SQL
            sql = sql.Replace("#0", table.TableName).Replace("#1", v1).Replace("#2", v2);

            try
            {
                using (SqlCommand sqlCommand = new SqlCommand(sql, DBConnection))
                {
                    sqlCommand.Parameters.Clear();

                    p.ToList().ForEach(x =>
                    {
                        sqlCommand.Parameters.AddWithValue(x.Key, x.Value);
                    });

                    lastID = Convert.ToInt32(sqlCommand.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                backuplogger.Info("FAILED INSERT EXECUTE : {0} - {1} - {2}", sql, Newtonsoft.Json.JsonConvert.SerializeObject(p), Newtonsoft.Json.JsonConvert.SerializeObject(table));
                logger.Error(ex, "DB Error: {0}",sql);
            }
            finally
            {
                logger.Debug("SQL Query - Inserted : {0} - {1}", sql, Newtonsoft.Json.JsonConvert.SerializeObject(p));
            }

            return lastID;

        }

        public bool ExecuteDeleteSQL<T>(int id) where T : TableBase
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            var table = (T)Activator.CreateInstance(typeof(T));

            p.Add("@id", id);

            string sql = "DELETE FROM #0 WHERE #1".Replace("#0", table.TableName).Replace("#1", string.Format("{0} = @id;", table.DBKey));

            try
            {
                // Backup records before remove.
                if (!BackupRecods<T>(p)) return true;

                using (SqlCommand sqlCommand = new SqlCommand(sql, DBConnection))
                {
                    sqlCommand.Parameters.Clear();

                    p.ToList().ForEach(x =>
                    {
                        sqlCommand.Parameters.AddWithValue(x.Key, x.Value);
                    });

                    sqlCommand.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                backuplogger.Info("FAILED DELETE EXECUTE : {0} - {1}", sql, Newtonsoft.Json.JsonConvert.SerializeObject(p));
                logger.Error(ex, "DB Error: {0}",sql);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Backup records before remove it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <returns>true if the target is at least one</returns>
        public bool BackupRecods<T>(Dictionary<string, object> dic) where T : TableBase
        {
            //Logger backuplogger = LogManager.GetLogger("SQLBackup");

            var table = (T)Activator.CreateInstance(typeof(T));
            string slc = "SELECT * FROM #0 WHERE #1".Replace("#0", table.TableName).Replace("#1", string.Format("{0} = @id;", table.DBKey));
            var records = this.ExecuteSQL<T>(slc, dic);

            if (records.Count()> 0) backuplogger.Info("REMOVED : {0} - {1} - {2}", slc, Newtonsoft.Json.JsonConvert.SerializeObject(dic), Newtonsoft.Json.JsonConvert.SerializeObject(records));

            return (records.Count() > 0) ;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if( DBConnection?.State != System.Data.ConnectionState.Closed)
                    DBConnection.Close();

            if (!disposedValue)
            {
                if (disposing)
                {


                }


                disposedValue = true;
            }


        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }


}