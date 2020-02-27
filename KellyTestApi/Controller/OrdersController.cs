using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace KellyTestApi.Controller
{
    public class OrdersController : ApiController
    {
        public string Get()
        {
            return "Hello World";
        }
    }
}
