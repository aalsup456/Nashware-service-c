using Microsoft.SharePoint.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Web;
using TNG.NashWare.Services.Models;

namespace TNG.NashWare.Services.Handler
{
    /// <summary>
    /// Sharepoint Integration Handler
    /// </summary>
    public class SPController
    {
        private string HOST = ConfigurationManager.AppSettings["SPHost"];
        private string Username;
        private System.Security.SecureString Password;

        public SPController(string username, string password)
        {
            Username = username;
            GeneratePassword(DecodePasswordBase64(password));
        }

        /// <summary>
        /// Retrieve Control Token from Sharepoint
        /// </summary>
        /// <returns></returns>
        public string GetToken()
        {
            ClientContext context = new ClientContext(HOST);
            SharePointOnlineCredentials creds = new SharePointOnlineCredentials(Username, Password);
            context.Credentials = creds;
            
            Uri sharepointuri = new Uri(HOST);
            string authCookie = creds.GetAuthenticationCookie(sharepointuri);

            return authCookie.Replace("SPOIDCRL=", string.Empty);
        }


        /// <summary>
        /// Retrieve profile info from sharepoint
        /// </summary>
        /// <returns></returns>
        public Microsoft.SharePoint.Client.Utilities.PrincipalInfo GetProfileInfo()
        {
            ClientContext context = new ClientContext(HOST);
            SharePointOnlineCredentials creds = new SharePointOnlineCredentials(Username, Password);
            context.Credentials = creds;

            ClientResult<Microsoft.SharePoint.Client.Utilities.PrincipalInfo> persons = Microsoft.SharePoint.Client.Utilities.Utility.ResolvePrincipal(context, context.Web, Username, Microsoft.SharePoint.Client.Utilities.PrincipalType.User, Microsoft.SharePoint.Client.Utilities.PrincipalSource.All, null, true);
            context.ExecuteQuery();
            Microsoft.SharePoint.Client.Utilities.PrincipalInfo person = persons.Value;

            return person;
        }

        /// <summary>
        /// Get Form Digest from sharepoint - need it when submiting form info to sharepoint
        /// </summary>
        /// <returns></returns>
        public string GetFormDigest()
        {
            var endpointUri = new Uri(HOST + "/_api/contextinfo");
            var request = (HttpWebRequest)WebRequest.Create(endpointUri);
            request.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
            request.Method = WebRequestMethods.Http.Post;
            request.Accept = "application/json;odata=verbose";
            request.ContentType = "application/json;odata=verbose";
            request.ContentLength = 0;

            request.Credentials = new SharePointOnlineCredentials(Username, Password);

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var content = streamReader.ReadToEnd();
                    var t = JToken.Parse(content);
                    return t["d"]["GetContextWebInformation"]["FormDigestValue"].ToString();
                }
            }
        }

        /// <summary>
        /// Get TokenModel to send back to View
        /// </summary>
        /// <returns></returns>
        public SPTokenModel RequestToken()
        {
            var returnToken = new SPTokenModel();

            returnToken.nw_SPOIDCRL = GetToken();
            //returnToken.nw_DIGEST = GetFormDigest();

            var profile = GetProfileInfo();
            returnToken.nw_LoginName = profile.LoginName;
            returnToken.nw_DisplayName = profile.DisplayName;
            returnToken.nw_Email = profile.Email;

            CheckExistingEmployee(profile);

            return returnToken;
        }

        /// <summary>
        /// Check if Employee already in the DB, else add the local copy
        /// </summary>
        /// <param name="model"></param>
        public void CheckExistingEmployee(Microsoft.SharePoint.Client.Utilities.PrincipalInfo model)
        {
            var dbController = new DBHandler();
            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                var query = string.Format("BEGIN " +
                            "IF NOT EXISTS (SELECT * FROM EMP_EMPLOYEES  " +
                                    "WHERE EMP_LOGIN_NAME = '{0}' " +
                                    "AND EMP_EMAIL = '{1}') " +
                                "BEGIN " +
                                    "INSERT INTO EMP_EMPLOYEES (EMP_LOGIN_NAME, EMP_DISPLAY_NAME, EMP_EMAIL, EMP_TITLE, EMP_DEPARTMENT) " +
                                    "VALUES ('{0}','{2}','{1}','{3}','{4}') " +
                                "END " +
                            "END", model.LoginName, model.Email, model.DisplayName, model.JobTitle, model.Department);
                using (SqlCommand com = new SqlCommand(query, dbController.connection))
                {
                    com.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Generate Secure String from plain Text
        /// </summary>
        /// <param name="plainPassword"></param>
        public void GeneratePassword(string plainPassword)
        {
            Password = new SecureString();
            if (plainPassword.Length > 0)
            {
                foreach (var c in plainPassword.ToCharArray()) Password.AppendChar(c);
            }
        }

        /// <summary>
        /// Decode Hashed Password from View
        /// </summary>
        /// <param name="encodedpassword"></param>
        /// <returns></returns>
        public string DecodePasswordBase64(string encodedpassword)
        {
            byte[] data = Convert.FromBase64String(encodedpassword);
            string decodedString = Encoding.UTF8.GetString(data);

            return decodedString;
        }
    }
}