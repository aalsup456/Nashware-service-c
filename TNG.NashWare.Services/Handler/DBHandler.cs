using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;

namespace TNG.NashWare.Services.Handler
{
    
    public class DBHandler
    {
        public SqlConnection connection;
        private static Object _lock = new Object();

        /// <summary>
        /// Initialize connection string
        /// </summary>
        public DBHandler()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NashwareDB"].ToString());
        }

        /// <summary>
        /// Check if Connection is Open
        /// </summary>
        public void ConnectionCheck()
        {
            bool lockWasTaken = false;
            try
            {
                Monitor.Enter(_lock, ref lockWasTaken);
                if (this.connection.State != ConnectionState.Open)
                {
                    try
                    {
                        if (this.connection.ConnectionString == "")
                            this.connection.ConnectionString = ConfigurationManager.ConnectionStrings["NashwareDB"].ToString();
                        this.connection.Open();
                    }
                    catch
                    {
                        SqlConnection.ClearPool(this.connection);
                        this.connection = new SqlConnection(ConfigurationManager.ConnectionStrings["NashwareDB"].ToString());
                        this.connection.Open();
                    }
                }
            }
            finally
            {
                if (lockWasTaken)
                {
                    Monitor.Exit(_lock);
                }
            }
        }
    }
}