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
    public class BargeController : Controller
    {

        DataContext db = new DataContext();

        //
        // GET: /Barge/

        [Authorize]
        public ActionResult Index()
        {
            // Write action log
            ActionLog.WriteLog(ActionLog.VIEW_BARGE_LIST, "", User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);
            return View();
        }

        [Authorize]
        public ActionResult Add()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Add(AddBargeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // was barge info exsited?
                    var existed = db.Barges.Where(b => string.Compare(b.BargeCode, model.BargeCode, true) == 0).FirstOrDefault();
                    if (existed != null)
                    {
                        ModelState.AddModelError("", "Sà lan số [" + model.BargeCode + "] đã tồn tại trong hệ thống!");
                        return View(model);
                    }

                    // create new barge info
                    var newBarge = new Barge();
                    newBarge.BargeCode = model.BargeCode;
                    newBarge.VolumeRevenue = model.VolumeRevenue;
                    newBarge.VolumePurchaseGoldSand = model.VolumePurchaseGoldSand;
                    newBarge.VolumePurchaseFillingSand = model.VolumePurchaseFillingSand;
                    newBarge.Description = model.Description;

                    // save new barge info
                    db.Barges.Add(newBarge);
                    db.SaveChanges();

                    // write action logs data
                    var log_data = "id=" + newBarge.BargeId + ",code=" + newBarge.BargeCode + 
                        ",v1=" + newBarge.VolumeRevenue + 
                        ",v2=" + newBarge.VolumePurchaseFillingSand + 
                        ",v3=" + newBarge.VolumePurchaseGoldSand;
                    ActionLog.WriteLog(ActionLog.ADD_BARGE_INFO, log_data, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    return RedirectToAction("Index");
                }

                // invalid boat info
                ModelState.AddModelError("", "Thông tin sà lan không hợp lệ!");
            }
            catch (Exception ex)
            {
                // error
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            try
            {
                // Get barges info
                var editedBarge = db.Barges.Where(b => b.BargeId == id).FirstOrDefault();
                if (editedBarge != null)
                {
                    EditBargeViewModel model = new EditBargeViewModel();
                    model.BargeId = editedBarge.BargeId;
                    model.BargeCode = editedBarge.BargeCode;
                    model.VolumeRevenue = editedBarge.VolumeRevenue;
                    model.VolumePurchaseFillingSand = editedBarge.VolumePurchaseFillingSand;
                    model.VolumePurchaseGoldSand = editedBarge.VolumePurchaseGoldSand;
                    model.Description = editedBarge.Description;

                    return View(model);
                }

                // Barges not found
                return RedirectToAction("Message", "Error", new RouteValueDictionary(new { message = "Sà lan số hiệu #" + id + " không tồn tại trong hệ thống!" }));
            }
            catch (Exception ex)
            {
                // error
                return RedirectToAction("Message", "Error", new RouteValueDictionary(new { message = ex.Message }));
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(EditBargeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var editedBarge = db.Barges.Where(b => b.BargeId == model.BargeId).FirstOrDefault();
                    if (editedBarge != null)
                    {
                        // create action log
                        string log_data = "id=" + editedBarge.BargeId +
                            ", code=" + editedBarge.BargeCode + ",ncode=" + model.BargeCode +
                            ", v1=" + editedBarge.VolumeRevenue + ",nv1=" + model.VolumeRevenue +
                            ", v2=" + editedBarge.VolumePurchaseFillingSand + ",nv2=" + model.VolumePurchaseFillingSand +
                            ", v3=" + editedBarge.VolumePurchaseGoldSand + ",nv3=" + model.VolumePurchaseGoldSand;

                        // update barge info
                        editedBarge.BargeCode = model.BargeCode;
                        editedBarge.VolumeRevenue = model.VolumeRevenue;
                        editedBarge.VolumePurchaseFillingSand = model.VolumePurchaseFillingSand;
                        editedBarge.VolumePurchaseGoldSand = model.VolumePurchaseGoldSand;
                        editedBarge.Description = model.Description;
                        db.SaveChanges();

                        // save action log
                        ActionLog.WriteLog(ActionLog.ADD_BARGE_INFO, log_data, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                        // redirect to barges list page
                        return RedirectToAction("Index");
                    }
                    ModelState.AddModelError("", "Không tìm thấy thông tin sà lan!");
                }
                ModelState.AddModelError("", "Thông tin sà lan không hợp lệ!");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetBarges(string filter = "", int pageIndex = 1)
        {
            try
            {
                // get barges
                filter = filter.Trim();
                var barges = (from t1 in db.Barges
                              where String.IsNullOrEmpty(filter) || t1.BargeCode.Contains(filter)
                              orderby t1.BargeCode
                              select t1)
                              .ToList();

                // barge paging setup
                var rowCount = barges.Count;
                var pageSize = Configurations.DEFAULT_PAGE_SIZE;
                var pagesTotal = (int)(rowCount / pageSize);

                if (rowCount % pageSize != 0)
                    pagesTotal++;

                if (pageIndex > pagesTotal && pagesTotal > 0)
                    pageIndex = pagesTotal;

                var pagedBarges = barges.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                // return barges
                return Json(new
                {
                    Error = 0,
                    Message = "Success",
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    PageTotal = pagesTotal,
                    BargesCount = rowCount,
                    Barges = pagedBarges
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetAllBarges()
        {
            try
            {
                var barges = db.Barges.OrderBy(b => b.BargeCode).ToList();
                return Json(new { Error = 0, Message = "Success", Barges = barges });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var deletedBarge = db.Barges.Where(b => b.BargeId == id).FirstOrDefault();
                if (deletedBarge != null)
                {
                    // barge existed in any orders?
                    var order = db.Orders.Where(o => o.BargeId == deletedBarge.BargeId).FirstOrDefault();
                    if (order != null)
                        return Json(new { Error = 1, Message = "Xóa sà lan thất bại! Bạn không thể xóa sà lan đã được lập chuyến." });

                    // delete barge
                    db.Barges.Remove(deletedBarge);
                    db.SaveChanges();

                    // write action log
                    var log_data = "id=" + deletedBarge.BargeId + 
                        ", code=" + deletedBarge.BargeCode +
                        ", v1=" + deletedBarge.VolumeRevenue +
                        ", v2=" + deletedBarge.VolumePurchaseFillingSand +
                        ", v3=" + deletedBarge.VolumePurchaseGoldSand;
                    ActionLog.WriteLog(ActionLog.DELETE_BARGE_INFO, log_data, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);
                }
                return Json(new { Error = 0, Message = "Xóa sà làn " + deletedBarge.BargeCode + " thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }
    }
}
