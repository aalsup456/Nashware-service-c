using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TNG.NashWare.Services.Handler;

namespace TNG.NashWare.Services.Controllers
{
    /// <summary>
    /// Handle API Call for Timesheet Page
    /// </summary>
    public class ServiceController : ApiController
    {
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent(JsonConvert.SerializeObject("Success"), System.Text.Encoding.UTF8, "application/json")
            };
        }

        [HttpGet]
        public HttpResponseMessage GetTimesheetFilter(string timesheetfilter/*, string email*/)
        {
            try {                
                var controller = new TimeSheetHandler();
                var toReturn = JsonConvert.SerializeObject(controller.GetTimesheetFilter());

                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Content = new StringContent(toReturn, System.Text.Encoding.UTF8, "application/json")
                };
            } catch (Exception err)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(JsonConvert.SerializeObject(err.InnerException==null? err.Message:err.InnerException.Message), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        [HttpGet]
        public HttpResponseMessage CheckQBToken(string email)
        {
            try
            {
                var controller = new OauthController();
                var toReturn = JsonConvert.SerializeObject(controller.CheckToken(email));

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
                    Content = new StringContent(JsonConvert.SerializeObject(err.Message), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }

        [HttpPost]
        public HttpResponseMessage Login(string username, string password)
        {
            var tempSP = new SPController(username, password);
            try
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Content = new StringContent(JsonConvert.SerializeObject(tempSP.RequestToken()), System.Text.Encoding.UTF8, "application/json")
                };
            }
            catch (Exception err)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent(JsonConvert.SerializeObject(err.Message), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }


        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
