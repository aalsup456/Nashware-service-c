using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TNG.NashWare.Services.Handler;

namespace TNG.NashWare.Services.Controllers
{
    /// <summary>
    /// Handle API Call that involve Syncing Data
    /// </summary>
    public class DataController : ApiController
    {

        [HttpGet]
        public HttpResponseMessage GetDataList(string dataType)
        {
            try
            {

                var controller = new DataHandler();
                var toReturn = "";
                switch (dataType)
                {
                    case "client":
                        toReturn = JsonConvert.SerializeObject(controller.GetClientList());
                        break;
                    case "service":
                        toReturn = JsonConvert.SerializeObject(controller.GetServiceList());
                        break;
                    case "class":
                        toReturn = JsonConvert.SerializeObject(controller.GetClassList());
                        break;
                    default: break;
                }


                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Content = new StringContent(toReturn, System.Text.Encoding.UTF8, "application/json")
                };
            }
            catch (Exception err)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(err.InnerException == null ? err.Message : err.InnerException.Message), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        // POST: api/Data
        [HttpPost]
        public HttpResponseMessage CheckSync(string synctype)
        {
            try
            {
                var controller = new DataHandler();
                var toReturn = "";
                switch(synctype)
                {
                    case "client":
                        toReturn = JsonConvert.SerializeObject(controller.CheckSyncQBCustomer());
                        break;
                    case "service":
                        toReturn = JsonConvert.SerializeObject(controller.CheckSyncQBService());
                        break;
                    case "class":
                        toReturn = JsonConvert.SerializeObject(controller.CheckSyncQBWorkClass());
                        break;
                    default:break;
                }
                

                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Content = new StringContent(toReturn, System.Text.Encoding.UTF8, "application/json")
                };
            }
            catch (Exception err)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(err.InnerException == null ? err.Message : err.InnerException.Message), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }
    }
}
