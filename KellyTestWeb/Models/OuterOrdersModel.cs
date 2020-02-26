using KellyTestModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KellyTest.Models
{
    public class OuterOrdersModel
    {
        public AndOr DefaultOperator { get; set; }
        public AndOr Operator { get; set; }
        public List<TestOrderProduct> OrderProducts { get; set; }
        public List<SelectListItem> Technologies { get; set; }
        public List<SelectListItem> Functions { get; set; }
        public List<SelectListItem> Actions { get; set; }


        //pagination
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int PagerCount { get; set; }


        public OuterOrdersModel()
        {
        }
    }
}