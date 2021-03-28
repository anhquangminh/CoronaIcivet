using CoronaIcivet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CoronaIcivet.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        
        [HttpPost]
        public string apiCheckHealth(apiHealthModels apiHealth)
        {

            string res = " ok";

            if (apiHealth.SucKhoe == null) res = "ko tot";
            if (apiHealth.Ten != null) res += "Khang beo";

            return res;
        }
    }
}
