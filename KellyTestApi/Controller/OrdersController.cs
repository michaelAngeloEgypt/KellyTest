using KellyTestModel;
using KellyTestModel.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Web.Http;

namespace KellyTestApi.Controller
{
    public static class BLQueries
    {
        public const string GetShipmentDtos =
    @"
Exec ComputeShipments";
    }

    public class OrdersController : ApiController
    {
        private AraganModel db = new AraganModel();

        public IEnumerable<ShimpentDto> Get()
        {
            var res = new List<ShimpentDto>();
            try
            {
                res = db.Database.SqlQuery<ShimpentDto>(BLQueries.GetShipmentDtos).ToList();
                //var res = new List<VouchersDto>();
            }
            catch (Exception x)
            {
                ExceptionDispatchInfo.Capture(x).Throw();
            }
            return res;
        }


    }
}
