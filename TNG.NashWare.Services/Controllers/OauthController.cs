using System;
using System.Configuration;
using System.Web.Mvc;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using TNG.NashWare.Services.Models.DTO;
using TNG.NashWare.Services.Models.Services;
using TNG.NashWare.Services.Models.Utilities;
using System.Data.SqlClient;
using TNG.NashWare.Services.Handler;

namespace TNG.NashWare.Services.Controllers
{
    /// <summary>
    /// OauthController is responsible for connecting to quick books api
    /// 
    /// </summary>
    public class OauthController : Controller
    {
        /// <summary>
        /// Sequence : 
        /// CosumerSecret, ConsumerKey, OAuthLink, RequestToken, TokenSecret, OAuthCallbackUrl
        /// </summary>
        OAuthorizationdto oAuthorizationdto = null;
        OAuthTokens oAuthorizationDB = null;
        OAuthService oAuthService = null;
        /// <summary>
        /// Action Result for Index, This flow will create OAuthConsumer Context using Consumer key and Consuler Secret key
        /// obtained when Application is added at intuit workspace. It creates OAuth Session out of OAuthConsumer and Calls 
        /// Intuit Workpsace endpoint for OAuth.
        /// </summary>
        /// <returns>Redirect Result.</returns>
        public RedirectResult Index(string Email)
        {
            oAuthorizationdto = new OAuthorizationdto();
            oAuthService = new OAuthService(oAuthorizationdto);
            oAuthorizationdto.CallBackUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/Oauth/Response?Email="+Email;
            return Redirect(oAuthService.GrantUrl(this));
        }
        /// <summary>
        /// Sequence:
        /// -->Retrieve the Request token
        /// -->Retrieve the value from query string
        /// -->Retrieve acces token
        /// -->Retrieve acces secret
        /// -->Redirect to close
        /// </summary>
        /// <returns></returns>
        public ActionResult Response(string Email)
        {
            oAuthorizationDB = new OAuthTokens();
            oAuthService = new OAuthService(oAuthorizationdto);
            oAuthorizationdto = oAuthService.GetRequestToken(this);
            if (Request.QueryString.HasKeys())
            {
                oAuthorizationdto.OauthVerifyer = Request.QueryString["oauth_verifier"].ToString();
                oAuthorizationDB.realmid = Convert.ToInt64(Request.QueryString["realmId"].ToString());
                oAuthorizationdto.Realmid = oAuthorizationDB.realmid;
                oAuthorizationDB.datasource = Request.QueryString["dataSource"].ToString();
                oAuthorizationdto.DataSource = oAuthorizationDB.datasource;
                oAuthorizationdto = oAuthService.GetAccessTokenFromServer(this, oAuthorizationdto);
                //encrypt the tokens
                oAuthorizationDB.access_secret = Utility.Encrypt(oAuthorizationdto.AccessTokenSecret, oAuthorizationdto.SecurityKey);
                oAuthorizationDB.access_token = Utility.Encrypt(oAuthorizationdto.AccessToken, oAuthorizationdto.SecurityKey);
                var dbController = new DBHandler();
                using (dbController.connection)
                {
                    dbController.ConnectionCheck();
                    var query = string.Format("INSERT INTO QBO_QUICKBOOK_OAUTH VALUES ( " +
                                "(SELECT EMP_ID FROM EMP_EMPLOYEES WHERE EMP_EMAIL = '{0}'), " +
                                "{1},'{2}','{3}','{4}',GETDATE()) ",
                        Email,oAuthorizationDB.realmid, oAuthorizationDB.access_secret, oAuthorizationDB.access_token, oAuthorizationDB.datasource);
                    using (SqlCommand com = new SqlCommand(query, dbController.connection))
                    {
                        com.ExecuteNonQuery();
                    }
                }
            }
            return RedirectToAction("Close", "Home");
        }

        public int CheckToken(string Email)
        {
            var dbController = new DBHandler();
            using (dbController.connection)
            {
                dbController.ConnectionCheck();
                var query = string.Format("SELECT COUNT(*) FROM QBO_QUICKBOOK_OAUTH " +
                    "WHERE QBO_EMP_ID = (SELECT EMP_ID FROM EMP_EMPLOYEES WHERE EMP_EMAIL = '{0}')",
                    Email);
                using (SqlCommand com = new SqlCommand(query, dbController.connection))
                {
                    return (int)com.ExecuteScalar();
                }
            }
        }
    }
}
