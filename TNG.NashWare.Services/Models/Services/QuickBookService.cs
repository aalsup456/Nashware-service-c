using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using TNG.NashWare.Services.Controllers;
using TNG.NashWare.Services.Handler;

namespace TNG.NashWare.Services.Models.Services
{
    public class QuickBookService
    {
        private DBHandler dbController;
        public OAuthRequestValidator tokenValidator { get; set; }
        public ServiceContext serviceContext { get; set; }
        public DataService dataService { get; set; }

        public QuickBookService()
        {
            dbController = new DBHandler();

            var appToken = ConfigurationManager.AppSettings["AppToken"];
            var consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
            var consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
            var serviceType = IntuitServicesType.QBO;
            var Email = ConfigurationManager.AppSettings["AdminEmail"];
            var accessToken = "";
            var accessTokenSecret = "";
            var realmId = "";

            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                var query = string.Format("SELECT * FROM QBO_QUICKBOOK_OAUTH WHERE QBO_EMP_ID = " +
                                "(SELECT EMP_ID FROM EMP_EMPLOYEES WHERE EMP_EMAIL_ADDRESS = '{0}')", Email);
                using (SqlCommand com = new SqlCommand(query, dbController.connection))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            accessToken = reader["QBO_ACCESS_TOKEN"].ToString();
                            accessTokenSecret = reader["QBO_ACCESS_SECRET"].ToString();
                            realmId = reader["QBO_REALMID"].ToString();
                            //datasource = reader["QBO_DATASOURCE"].ToString();
                        }
                    }
                }
            }

            accessToken = TNG.NashWare.Services.Models.Utilities.Utility.Decrypt(accessToken, ConfigurationManager.AppSettings["securityKey"]);
            accessTokenSecret = TNG.NashWare.Services.Models.Utilities.Utility.Decrypt(accessTokenSecret, ConfigurationManager.AppSettings["securityKey"]);

            tokenValidator = new OAuthRequestValidator(accessToken, accessTokenSecret, consumerKey, consumerSecret);
            serviceContext = new ServiceContext(appToken, realmId, serviceType, tokenValidator);
            //serviceContext.IppConfiguration.BaseUrl.Qbo = ConfigurationManager.AppSettings["ServiceContext.BaseUrl.Qbo"];
            //serviceContext.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";

            dataService = new DataService(serviceContext);
            
        }
    }
}