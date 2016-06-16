using Intuit.Ipp.Data;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using TNG.NashWare.Services.Models;
using TNG.NashWare.Services.Models.Services;

namespace TNG.NashWare.Services.Handler
{
    /// <summary>
    /// Handle QB Syncing Data
    /// </summary>
    public class DataHandler
    {
        private DBHandler dbController;
        private QuickBookService qbService;

        public DataHandler()
        {
            dbController = new DBHandler();
            qbService = new QuickBookService();
        }

        /// <summary>
        /// Sync SQL Customer with Quick Book Customer
        /// </summary>
        /// <returns></returns>
        public int CheckSyncQBCustomer()
        {
            var SqlList = new List<ClientModel>();

            //Get SQL List
            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                var query = @"SELECT * FROM CL_CLIENTS";
                using (SqlCommand com = new SqlCommand(query, dbController.connection))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tempCustomer = new ClientModel();
                            tempCustomer.Id = new Guid(reader["CL_ID"].ToString());
                            tempCustomer.Name = reader["CL_NAME"].ToString();
                            if (DBNull.Value != reader["CL_QB_ID"])
                                tempCustomer.QB_Id = Convert.ToInt64(reader["CL_QB_ID"]);
                            SqlList.Add(tempCustomer);
                            //datasource = reader["QBO_DATASOURCE"].ToString();
                        }
                    }
                }
            }

            //Get QB List
            QueryService<Customer> customerQueryService = new QueryService<Customer>(qbService.serviceContext);
            var listClient = customerQueryService.Select(c => c).OrderBy(p => p.DisplayName).ToList();

            //Different between QB and SQL
            var diffList = listClient.Select(p => p.DisplayName).Except(SqlList.Select(s => s.Name)).ToList();
            if (diffList.Count() > 0)
            {
                SyncClient(listClient, diffList);
            }

            return diffList.Count();
        }

        public void SyncClient(List<Customer> listClient, List<string> diffList)
        {
            var queryList = "";
            foreach (var client in listClient.Where(p => diffList.Contains(p.DisplayName)))
            {
                var clientName = client.DisplayName.Replace("'", "''");
                var id = client.Id;

                var line1 = "";
                var line2 = "";
                var subdiv = "";
                var city = "";
                var state = "";
                var zip = "";
                if (client.BillAddr != null)
                {
                    line1 = client.BillAddr.Line1 != null ? client.BillAddr.Line1.Replace("'", "''") : "";
                    line2 = client.BillAddr.Line2 != null ? client.BillAddr.Line2.Replace("'", "''") : "";
                    subdiv = client.BillAddr.CountrySubDivisionCode != null ? client.BillAddr.CountrySubDivisionCode.Replace("'", "''") : "";
                    city = client.BillAddr.City != null ? client.BillAddr.City.Replace("'", "''") : "";
                    state = subdiv;
                    zip = client.BillAddr.PostalCode != null ? client.BillAddr.PostalCode : "";
                }


                var email = client.PrimaryEmailAddr != null ? client.PrimaryEmailAddr.Address : "";
                var phone = client.PrimaryPhone != null ? client.PrimaryPhone.FreeFormNumber : "";
                var contact_email = client.PrimaryEmailAddr != null ? client.PrimaryEmailAddr.Address : "";
                var contact_phone = client.PrimaryPhone != null ? client.PrimaryPhone.FreeFormNumber : "";
                var contactname = client.ContactName;
                var isactive = client.Active ? 1 : 0;

                var query = string.Format("INSERT INTO CL_CLIENTS (" +
                    "CL_NAME, CL_QB_ID, CL_BILLING_ADDRESS_1, CL_BILLING_ADDRESS_2, " +
                    "CL_BILLING_COUNTRY_SUBDIVCODE, CL_BILLING_CITY, CL_BILLING_STATE, CL_BILLING_ZIP, " +
                    "CL_EMAIL, CL_PHONE, CL_BILLING_CONTACT_NAME, CL_BILLING_CONTACT_EMAIL, CL_BILLING_CONTACT_PHONE, CL_IS_ACTIVE, CL_CC_ID) VALUES (" +
                    "'{0}',{1},'{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}',{13}, 'Z');",
                    clientName, id, line1, line2, subdiv, city, state, zip, email, phone, contactname, contact_email, contact_phone, isactive);

                queryList += query;
            }

            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                using (SqlCommand com = new SqlCommand())
                {
                    com.Connection = dbController.connection;
                    com.CommandText = queryList;
                    com.CommandType = System.Data.CommandType.Text;
                    com.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Sync SQL Service with Quick Book Service
        /// </summary>
        /// <returns></returns>
        public int CheckSyncQBService()
        {
            var SqlList = new List<ServiceModel>();

            //Get SQL List
            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                var query = @"SELECT * FROM SRV_SERVICES";
                using (SqlCommand com = new SqlCommand(query, dbController.connection))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tempService = new ServiceModel();
                            tempService.Id = new Guid(reader["SRV_ID"].ToString());
                            tempService.Name = reader["SRV_NAME"].ToString();
                            if (DBNull.Value != reader["SRV_QB_ID"])
                                tempService.QB_Id = Convert.ToInt64(reader["SRV_QB_ID"]);
                            SqlList.Add(tempService);
                            //datasource = reader["QBO_DATASOURCE"].ToString();
                        }
                    }
                }
            }

            //Get QB List
            QueryService<Item> customerQueryService = new QueryService<Item>(qbService.serviceContext);
            var listService = customerQueryService.Select(c => c).OrderBy(p => p.Name).ToList();

            //Different between QB and SQL
            var diffList = listService.OrderBy(p=>p.FullyQualifiedName).Select(p => p.Name).Except(SqlList.Select(s => s.Name)).ToList();
            if (diffList.Count() > 0)
            {
                SyncService(listService, diffList);
            }

            return diffList.Count();
        }

        public void SyncService(List<Item> listService, List<string> diffList)
        {
            var queryList = "";
            var parentLink = new Dictionary<string, string>();
            foreach (var service in listService.Where(p => diffList.Contains(p.Name)))
            {
                var serviceName = service.Name.Replace("'", "''");
                var id = service.Id;
                var description = service.Description;
                if(service.ParentRef!= null)
                    parentLink.Add(id, service.ParentRef.name);

                var isactive = service.Active ? 1 : 0;

                var query = string.Format("INSERT INTO SRV_SERVICES (" +
                    "SRV_NAME, SRV_QB_ID, SRV_DESCRIPTION, SRV_IS_ACTIVE) VALUES (" +
                    "'{0}',{1}, '{2}', {3});",
                    serviceName, id, description, isactive);
                queryList += query;
            }

            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                using (SqlCommand com = new SqlCommand())
                {
                    com.Connection = dbController.connection;
                    com.CommandText = queryList;
                    com.CommandType = System.Data.CommandType.Text;
                    com.ExecuteNonQuery();
                }
            }

            UpdateServiceHierachy(parentLink);
        }

        public void UpdateServiceHierachy(Dictionary<string,string> updateList)
        {
            foreach(var toupdate in updateList)
            {
                var getquery = @"UPDATE SRV_SERVICES SET SRV_PARENTREF_QB_ID = (SELECT SRV_QB_ID FROM SRV_SERVICES WHERE SRV_NAME = '"+toupdate.Value+"') WHERE SRV_QB_ID = "+toupdate.Key;
                using (dbController.connection)
                {
                    dbController.ConnectionCheck();
                    using (SqlCommand com = new SqlCommand(getquery, dbController.connection))
                    {
                        com.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Sync SQL Work Class with Quick Book Work Class
        /// </summary>
        /// <returns></returns>
        public int CheckSyncQBWorkClass()
        {
            var SqlList = new List<WorkClassModel>();

            //Get SQL List
            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                var query = @"SELECT * FROM WC_WORK_CLASS";
                using (SqlCommand com = new SqlCommand(query, dbController.connection))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tempWC = new WorkClassModel();
                            tempWC.Id = new Guid(reader["WC_ID"].ToString());
                            tempWC.Name = reader["WC_NAME"].ToString();
                            if (DBNull.Value != reader["WC_QB_ID"])
                                tempWC.QB_Id = Convert.ToInt64(reader["WC_QB_ID"]);
                            SqlList.Add(tempWC);
                            //datasource = reader["QBO_DATASOURCE"].ToString();
                        }
                    }
                }
            }

            //Get QB List
            QueryService<Class> customerQueryService = new QueryService<Class>(qbService.serviceContext);
            var listClass = customerQueryService.Select(c => c).OrderBy(p => p.Name).ToList();

            //Different between QB and SQL
            var diffList = listClass.Select(p => p.Name).Except(SqlList.Select(s => s.Name)).ToList();
            if (diffList.Count() > 0)
            {
                SyncWClass(listClass, diffList);
            }

            return diffList.Count();
        }

        public void SyncWClass(List<Class> listCLass, List<string> diffList)
        {
            var queryList = "";
            foreach (var wclass in listCLass.Where(p => diffList.Contains(p.Name)))
            {
                var wclassName = wclass.Name.Replace("'", "''");
                var id = wclass.Id;

                var isactive = wclass.Active ? 1 : 0;

                var query = string.Format("INSERT INTO WC_WORK_CLASS (" +
                    "WC_NAME, WC_QB_ID, WC_IS_ACTIVE) VALUES (" +
                    "'{0}',{1}, {2});",
                    wclassName, id, isactive);
                queryList += query;
            }

            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                using (SqlCommand com = new SqlCommand())
                {
                    com.Connection = dbController.connection;
                    com.CommandText = queryList;
                    com.CommandType = System.Data.CommandType.Text;
                    com.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Pull List of Client from SQL DB
        /// </summary>
        /// <returns></returns>
        public List<ClientModel> GetClientList()
        {
            var toReturn = new List<ClientModel>();
            dbController.ConnectionCheck();
            var query = @"SELECT * FROM CL_CLIENTS";
            using (SqlCommand com = new SqlCommand(query, dbController.connection))
            {
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tempClient = new ClientModel();
                        tempClient.Name = reader["CL_NAME"].ToString();
                        tempClient.Id = new Guid(reader["CL_ID"].ToString());
                        if (DBNull.Value != reader["CL_BILLING_ADDRESS_1"])
                            tempClient.AddressLine1 = reader["CL_BILLING_ADDRESS_1"].ToString();
                        if(DBNull.Value!= reader["CL_BILLING_ADDRESS_2"])
                        tempClient.AddressLine2 = reader["CL_BILLING_ADDRESS_2"].ToString();
                        if (DBNull.Value != reader["CL_BILLING_STATE"])
                            tempClient.State = reader["CL_BILLING_STATE"].ToString();
                        if (DBNull.Value != reader["CL_BILLING_CITY"])
                            tempClient.City = reader["CL_BILLING_CITY"].ToString();
                        if (DBNull.Value != reader["CL_BILLING_ZIP"])
                            tempClient.PostalCode = reader["CL_BILLING_ZIP"].ToString();
                        if (DBNull.Value != reader["CL_BILLING_CONTACT_EMAIL"])
                            tempClient.Email = reader["CL_BILLING_CONTACT_EMAIL"].ToString();
                        if (DBNull.Value != reader["CL_PHONE"])
                            tempClient.Phone = reader["CL_PHONE"].ToString();
                        tempClient.isActive = (int)reader["CL_IS_ACTIVE"];

                        toReturn.Add(tempClient);
                    }
                }
            }

            return toReturn;
        }

        public List<WorkClassModel> GetClassList()
        {
            var toReturn = new List<WorkClassModel>();
            dbController.ConnectionCheck();
            var query = @"SELECT * FROM WC_WORK_CLASS";
            using (SqlCommand com = new SqlCommand(query, dbController.connection))
            {
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tempWC = new WorkClassModel();
                        tempWC.Name = reader["WC_NAME"].ToString();
                        tempWC.Id = new Guid(reader["WC_ID"].ToString());
                        tempWC.isActive = (int)reader["CL_IS_ACTIVE"];
                        toReturn.Add(tempWC);
                    }
                }
            }

            return toReturn;
        }

        public List<ServiceModel> GetServiceList()
        {
            var toReturn = new List<ServiceModel>();
            dbController.ConnectionCheck();
            var query = @"SELECT * FROM SRV_SERVICES";
            using (SqlCommand com = new SqlCommand(query, dbController.connection))
            {
                using (SqlDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tempService = new ServiceModel();
                        tempService.Name = reader["SRV_NAME"].ToString();
                        tempService.Id = new Guid(reader["SRV_ID"].ToString());
                        tempService.Description = reader["SRV_DESCRIPTION"].ToString();
                        tempService.isBillable = (int)reader["SRV_IS_BILLABLE"];
                        tempService.isActive = (int)reader["SRV_IS_ACTIVE"];
                        toReturn.Add(tempService);
                    }
                }
            }

            return toReturn;
        }
    }
}