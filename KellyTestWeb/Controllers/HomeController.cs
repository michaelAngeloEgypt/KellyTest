using KellyTest.Models;
using KellyTestModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KellyTest.Controllers
{
    public class HomeController : Controller
    {
        private AraganModel db = new AraganModel();
        public ActionResult Index(string operatorFilter, string technologyFilter, string functionFilter, string actionFilter, string search = "")
        {
            OuterOrdersModel model = new OuterOrdersModel();
            model.PageSize = 10;
            ViewBag.search = search;
            if (!string.IsNullOrEmpty(operatorFilter))
                model.Operator = (AndOr)Enum.Parse(typeof(AndOr), operatorFilter);

            List<TestOrderProduct> orderProducts = db.TestOrderProducts.ToList();

            if (orderProducts != null)
            {
                /*
                model.Technologies = GetTechnologiesFilter(demands, technologyFilter);
                model.Technologies.Insert(0, new SelectListItem() { Value = "ANY", Text = "ANY" });

                model.Functions = GetFunctionsFilter(demands, functionFilter);
                model.Functions.Insert(0, new SelectListItem() { Value = "ANY", Text = "ANY" });

                model.Actions = GetActionsFilter(demands, actionFilter);
                model.Actions.Insert(0, new SelectListItem() { Value = "ANY", Text = "ANY" });
                */
            }

            //#0:make the change in radiobutton cause a postback
            var queryCondition = GetQueryCondition(operatorFilter, technologyFilter, functionFilter, actionFilter);
            orderProducts = db.Database.SqlQuery<TestOrderProduct>(
                $"select * from TestOrderProducts where {queryCondition}"
                , new SqlParameter(nameof(technologyFilter), technologyFilter ?? "ANY")
                , new SqlParameter(nameof(functionFilter), functionFilter ?? "ANY")
                , new SqlParameter(nameof(actionFilter), actionFilter ?? "ANY")).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                orderProducts = (
                 from a in orderProducts
                 /*
                 where
                   !string.IsNullOrEmpty(a.Technology) && a.Technology.ContainsString(search) ||
                   !string.IsNullOrEmpty(a.Function) && a.Function.ContainsString(search) ||
                   !string.IsNullOrEmpty(a.Action) && a.Action.ContainsString(search) ||
                   !string.IsNullOrEmpty(a.OpportunityNameOrDemandNumber) && a.OpportunityNameOrDemandNumber.ContainsString(search) ||
                   !string.IsNullOrEmpty(a.CoETeam) && a.CoETeam.ContainsString(search) ||
                   !string.IsNullOrEmpty(a.TSSCentreOrMarket) && a.TSSCentreOrMarket.ContainsString(search) ||
                   !string.IsNullOrEmpty(a.Team) && a.Team.ContainsString(search) ||
                   !string.IsNullOrEmpty(a.GOLiveonOrCompleted) && a.GOLiveonOrCompleted.ContainsString(search)
                */
                 select a
                 ).ToList();

            model.OrderProducts = orderProducts;
            model.TotalCount = orderProducts.Count();

            return View(model);
        }

        private string GetQueryCondition(string operatorFilter, string technologyFilter, string functionFilter, string actionFilter)
        {
            var res = new StringBuilder("1=1");

            var usedFilters = new Dictionary<string, bool>() {
                {nameof(technologyFilter), !string.IsNullOrWhiteSpace(technologyFilter) && technologyFilter != "ANY"},
                {nameof(functionFilter), !string.IsNullOrWhiteSpace(functionFilter) && functionFilter != "ANY"},
                {nameof(actionFilter), !string.IsNullOrWhiteSpace(actionFilter) && actionFilter != "ANY"}
            };
            var filters = new StringBuilder();
            if (usedFilters[nameof(technologyFilter)])
                filters.Append($" Technology = @{nameof(technologyFilter)}");

            var currentFilter = string.IsNullOrEmpty(filters.ToString()) ? "" : operatorFilter;
            if (usedFilters[nameof(functionFilter)])
                filters.Append($" {currentFilter} [function] = @{nameof(functionFilter)}");

            currentFilter = string.IsNullOrEmpty(filters.ToString()) ? "" : operatorFilter;
            if (usedFilters[nameof(actionFilter)])
                filters.Append($" {currentFilter} action = @{nameof(actionFilter)}");

            if (!string.IsNullOrEmpty(filters.ToString()))
                res.Append($" and ({filters.ToString()})");

            return res.ToString();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}