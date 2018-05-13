using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BizMan.Models;
using BizMan.DAL;
using BizMan.Helpers;
using BizMan.Helpers.Security;

namespace BizMan.Controllers
{
    public class ActionLogController : BaseController
    {

        DataContext db = new DataContext();

        //
        // GET: /Role/

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Index()
        {
            return View();
        }


        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public JsonResult GetActionLogs(string dateFrom, string dateTo, string filterText = "", int pageIndex = 0)
        {
            try
            {
                // parse date range
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                var logs = (from t1 in db.ActionLogs
                            where
                                t1.ActionDate >= d1 &&
                                t1.ActionDate <= d2 &&
                                (string.IsNullOrEmpty(filterText.Trim()) ||
                                    t1.ActionName.Contains(filterText) ||
                                    t1.UserName.Contains(filterText) ||
                                    t1.IP.Contains(filterText))
                            orderby t1.ActionDate descending
                            select t1)
                            .ToList();

                // get rows count
                var rowCount = logs.Count;

                var pageSize = Configurations.DEFAULT_PAGE_SIZE;

                // calculate total pages
                var pagesTotal = (int)(rowCount / pageSize);
                if (rowCount % pageSize != 0)
                    pagesTotal++;

                // check valid page index
                if (pageIndex > pagesTotal && pagesTotal > 0)
                    pageIndex = pagesTotal;

                // do paging
                logs = logs.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                return Json(new
                {
                    Error = 0,
                    Message = "Success",
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    PageTotal = pagesTotal,
                    LogsCount = rowCount,
                    Logs = logs
                });

            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public JsonResult Delete(string logIds)
        {
            try
            {
                var ids = logIds.Split(',');
                foreach (var id in ids)
                {
                    var actionLogId = int.Parse(id);
                    var log = db.ActionLogs.Where(al => al.ActionLogId == actionLogId).FirstOrDefault();
                    if (log != null) db.ActionLogs.Remove(log);
                }
                db.SaveChanges();
                return Json(new { Error = 0, Message = logIds });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }
    }
}
