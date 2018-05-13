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
    public class BoatController : Controller
    {

        DataContext db = new DataContext();

        //
        // GET: /Boat/

        [Authorize]
        public ActionResult Index()
        {
            // Write action log
            ActionLog.WriteLog(ActionLog.VIEW_BOAT_LIST, string.Empty, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);
            return View();
        }

        [Authorize]
        public ActionResult Add()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Add(AddBoatViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // create new boat info
                    var newBoat = new Boat();
                    newBoat.BoatCode = model.BoatCode;
                    newBoat.BoatOwner = model.BoatOwner;
                    newBoat.BoatVolume = model.BoatVolume;

                    // save boat info
                    db.Boats.Add(newBoat);
                    db.SaveChanges();

                    // Write action log
                    string actionLogData = "boat:" + newBoat.BoatId + ", boatCode:" + newBoat.BoatCode;
                    ActionLog.WriteLog(ActionLog.ADD_BOAT_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    return RedirectToAction("Index");
                }

                // invalid boat info
                ModelState.AddModelError("", "Thông tin Ghe không hợp lệ!");
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
                // get boat info
                var editedBoat = db.Boats.Where(b => b.BoatId == id).FirstOrDefault();
                if (editedBoat != null)
                {
                    // create edited boat model
                    EditBoatViewModel model = new EditBoatViewModel();
                    model.BoatId = editedBoat.BoatId;
                    model.BoatCode = editedBoat.BoatCode;
                    model.BoatOwner = editedBoat.BoatOwner;
                    model.BoatVolume = editedBoat.BoatVolume;

                    return View(model);
                }

                // boat not found
                return RedirectToAction("Message", "Error", new RouteValueDictionary(new { message = "Ghe số hiệu #" + id + " không tồn tại trong hệ thống!" }));
            }
            catch (Exception ex)
            {
                // error
                return RedirectToAction("Message", "Error", new RouteValueDictionary(new { message = ex.Message }));
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(EditBoatViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // get current boat info
                    var editedBoat = db.Boats.Where(b => b.BoatId == model.BoatId).FirstOrDefault();
                    if (editedBoat != null)
                    {
                        // update boat info
                        editedBoat.BoatCode = model.BoatCode;
                        editedBoat.BoatOwner = model.BoatOwner;
                        editedBoat.BoatVolume = model.BoatVolume;

                        db.SaveChanges();

                        // Write action log
                        string actionLogData = "boatId:" + editedBoat.BoatId + ", boatCode:" + editedBoat.BoatCode;
                        ActionLog.WriteLog(ActionLog.EDIT_BOAT_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                        return RedirectToAction("Index");
                    }

                    return RedirectToAction("Message", "Error", new RouteValueDictionary(new { message = "Ghe số hiệu #" + model.BoatCode + " không tồn tại trong hệ thống!" }));
                }

                // invalid boat info
                ModelState.AddModelError("", "Thông tin cập nhật sản phẩm không hợp lệ!!!");
            }
            catch (Exception ex)
            {
                // error
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetBoats(string filter, int pageIndex = 1)
        {
            try
            {
                // get boats from database
                IList<Boat> boats = null;
                if (string.IsNullOrWhiteSpace(filter))
                {
                    boats = db.Boats.OrderBy(b => b.BoatCode).ToList();
                }
                else
                {
                    boats = db.Boats
                        .Where(b => b.BoatCode.Contains(filter) || b.BoatOwner.Contains(filter))
                        .OrderBy(b => b.BoatCode).ToList();
                }

                // get rows count
                var rowCount = boats.Count;

                var pageSize = Configurations.DEFAULT_PAGE_SIZE;

                // calculate total pages
                var pagesTotal = (int)(rowCount / pageSize);
                if (rowCount % pageSize != 0)
                    pagesTotal++;

                // check valid page index
                if (pageIndex > pagesTotal && pagesTotal > 0)
                    pageIndex = pagesTotal;

                // do paging
                boats = boats.OrderBy(b => b.BoatCode)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // convert boats paged list to json
                var jsonBoats = boats.Select(q1 => new
                {
                    BoatId = q1.BoatId,
                    BoatCode = q1.BoatCode,
                    BoatOwner = q1.BoatOwner,
                    BoatVolume = q1.BoatVolume
                });

                return Json(new { 
                    Error = 0, 
                    Message = "Success",
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    PageTotal = pagesTotal,
                    BoatsCount = rowCount,
                    Boats = jsonBoats });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetAllBoats()
        {
            try
            {
                // get boats from database
                var boats = db.Boats.OrderBy(b => b.BoatCode).ToList();
                return Json(new { Error = 0, Message = "Success", Boats = boats });
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
                // find deleted boat
                var deletedBoat = db.Boats.Where(b => b.BoatId == id).FirstOrDefault();
                if (deletedBoat != null)
                {
                    // validate boat relationship to check boat which can delete or not
                    // ...
                    var details = db.OrderDetails.Where(b => b.BoatId == deletedBoat.BoatId).FirstOrDefault();
                    if (details == null)
                    {
                        db.Boats.Remove(deletedBoat);
                        db.SaveChanges();

                        // Write action log
                        string logData = "boatId:" + deletedBoat.BoatId + ",boatCode:" + deletedBoat.BoatCode;
                        string ip = Request.ServerVariables["REMOTE_ADDR"];
                        ActionLog.WriteLog("deleteBoat", logData, User.Identity.Name, ip);

                        return Json(new { Error = 0, Message = "Xóa ghe " + deletedBoat.BoatCode + " thành công!" });
                    }
                    return Json(new { Error = 1, Message = "Xóa ghe thất bại! Bạn không thể xóa ghe đã được lập thông tin chuyến" });
                }
                return Json(new { Error = 1, Message = "Ghe mã #" + id + " không tồn tại trong hệ thống!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }
    }
}
