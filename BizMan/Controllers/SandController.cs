using System;
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
    public class SandController : Controller
    {

        DataContext db = new DataContext();

        //
        // GET: /Product/

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Index()
        {
            ActionLog.WriteLog(ActionLog.VIEW_SAND_LIST, "", User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);
            return View();
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Add()
        {
            return View();
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpPost]
        public ActionResult Add(AddSandViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // create new sand info object
                    var newSand = new Sand();
                    newSand.SandName = model.SandName;
                    newSand.ProviderPrice = model.ProviderPrice;
                    newSand.CustomerPrice = model.CustomerPrice;
                    newSand.TransportPrice = model.TransportPrice;
                    newSand.Description = model.Description;
                    db.Sands.Add(newSand);

                    // save new sand info
                    db.SaveChanges();

                    // Write action log
                    string actionLogData = "sandId:" + newSand.SandId + ", name:" + newSand.SandName;
                    ActionLog.WriteLog(ActionLog.ADD_SAND_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Thông tin sản phẩm cát không hợp lệ!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Edit(int id)
        {
            try
            {
                // get sand info
                var editedSand = db.Sands.Where(s => s.SandId == id).FirstOrDefault();
                if (editedSand != null)
                {
                    // create view model
                    EditSandViewModel model = new EditSandViewModel();
                    model.SandId = editedSand.SandId;
                    model.SandName = editedSand.SandName;
                    model.ProviderPrice = editedSand.ProviderPrice;
                    model.CustomerPrice = editedSand.CustomerPrice;
                    model.TransportPrice = editedSand.TransportPrice;
                    model.Description = editedSand.Description;

                    return View(model);
                }

                // sand info not found
                return RedirectToAction("Message", "Error",
                    new RouteValueDictionary(
                        new { message = "Sản phẩm cát #" + id + " không tồn tại trong hệ thống!!!" }));
            }
            catch (Exception ex)
            {
                // error
                return RedirectToAction("Message", "Error",
                    new RouteValueDictionary(
                        new { message = ex.Message }));
            }
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpPost]
        public ActionResult Edit(EditSandViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // get sand info
                    var editedSand = db.Sands.Where(s => s.SandId == model.SandId).FirstOrDefault();

                    // update sand info 
                    if (editedSand != null)
                    {
                        string actionLogData =
                            "  sandId=" + editedSand.SandId +
                            ", oldSandName=" + editedSand.SandName +
                            ", oldProviderPrice=" + editedSand.ProviderPrice +
                            ", oldCustomerPrice=" + editedSand.CustomerPrice +
                            ", oldTransportPrice=" + editedSand.TransportPrice; 

                        editedSand.SandName = model.SandName;
                        editedSand.ProviderPrice = model.ProviderPrice;
                        editedSand.CustomerPrice = model.CustomerPrice;
                        editedSand.TransportPrice = model.TransportPrice;
                        editedSand.Description = model.Description;
                        db.SaveChanges();

                        // Write action log
                        actionLogData +=
                            ", new_SandName=" + editedSand.SandName +
                            ", new_ProviderPrice=" + editedSand.ProviderPrice +
                            ", new_CustomerPrice=" + editedSand.CustomerPrice +
                            ", new_TransportPrice=" + editedSand.TransportPrice;

                        ActionLog.WriteLog(ActionLog.EDIT_SAND_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                        return RedirectToAction("Index");
                    }

                    return RedirectToAction("Message", "Error",
                        new RouteValueDictionary(
                            new { message = "Sản phẩm cát #" + model.SandId + " không tồn tại trong hệ thống!!!" }));
                }

                ModelState.AddModelError("", "Thông tin cập nhật không hợp lệ!!!");
            }
            catch (Exception ex)
            {
                // error
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpPost]
        public JsonResult GetSands(int pageIndex = 1)
        {
            try
            {
                var sandsCount = db.Sands.ToList().Count();

                // get page Size
                var pageSize = Configurations.DEFAULT_PAGE_SIZE;

                // calculate total page
                var pagesTotal = (int)(sandsCount / Configurations.DEFAULT_PAGE_SIZE);
                if (sandsCount % Configurations.DEFAULT_PAGE_SIZE != 0)
                    pagesTotal++;

                // validate page index
                if (pageIndex > pagesTotal && pagesTotal > 0)
                    pageIndex = pagesTotal;

                var sands = db.Sands.OrderBy(p => p.SandName)
                   .Skip((pageIndex - 1) * Configurations.DEFAULT_PAGE_SIZE)
                   .Take(Configurations.DEFAULT_PAGE_SIZE);

                return Json(new
                {
                    Error = 0,
                    Message = "Success",
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    PageTotal = pagesTotal,
                    SandsCount = sandsCount,
                    Sands = sands
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetAllSands()
        {
            try
            {
                var sands = db.Sands
                    .OrderBy(s => s.SandName)
                    .ToList();

                return Json(new
                {
                    Error = 0,
                    Message = "Success",
                    Sands = sands
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                // find product
                var deletedSand = db.Sands.Where(p => p.SandId == id).FirstOrDefault();
                if (deletedSand != null)
                {
                    // verify product
                    var sandOrders = db.Orders.Where(o => o.SandId == deletedSand.SandId).FirstOrDefault();
                    if (sandOrders == null)
                    {
                        // delete product
                        db.Sands.Remove(deletedSand);
                        db.SaveChanges();

                        // Write action log
                        string actionLogData =
                            "  sandId=" + deletedSand.SandId +
                            ", sandName=" + deletedSand.SandName +
                            ", ProviderPrice=" + deletedSand.ProviderPrice +
                            ", CustomerPrice=" + deletedSand.CustomerPrice +
                            ", TransportPrice" + deletedSand.TransportPrice;
                        ActionLog.WriteLog(ActionLog.DELETE_SAND_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                        return Json(new { Error = 0, Message = "Xóa sản phẩm cát thành công!!!" });
                    }
                    return Json(new { Error = 1, Message = "Xóa sản phẩm cát thất bại! Bạn không thể xóa loại cát đã được nhập chuyến." });
                }
                return Json(new { Error = 1, Message = "Sản phẩm cát #" + id + " không tồn tại trong hệ thống!!!" });
            }
            catch (Exception ex)
            {

                return Json(new { Error = 1, Message = ex.Message });
            }
        }
    }
}
