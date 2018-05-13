using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data;
using BizMan.Models;
using BizMan.DAL;
using BizMan.Helpers;
using BizMan.Helpers.Security;

namespace BizMan.Controllers
{

    /// <summary>
    /// 
    /// Controller for order
    /// 
    /// Finance Notes:
    /// 
    ///     GOLD SAND
    /// 
    ///         Revenue     = (VolumeTransport - VolumePromotion) * CustomerPrice + VolumePromotion * BaseTransportPrice 
    ///         Purchase    = (VolumePurchase - VolumePurchaseDescrease) * ProviderPrice
    ///         Transport   = (BoatVolume * TransportTimes + ExtraVolume) * Lines
    ///   
    ///     FILLING SAND
    /// 
    ///         Revenue     = (VolumeRevenue * CustomerPrice)
    ///         Purchase    = (VolumePurchase - VolumePurchaseDescrease) * ProviderPrice
    ///         Transport   = (BoatVolume * TransportTimes + ExtraVolume) * Lines
    /// 
    /// Descriptions:
    /// 
    ///     Revenue                     : Doanh thu
    ///     Purchase                    : Trả tiền mỏ
    ///     Transport                   : Trả tiền ghe
    ///     VolumeTransport             : Tổng khối ghe
    ///     VolumePromotion             : Khối gia công
    ///     BaseTransportPrice          : Giá gia công ghe
    ///     VolumePurchase              : Khối tính mỏ
    ///     VolumePurchaseDescrease     : Khối giảm tạp chất
    ///     ProviderPrice               : Giá trả mỏ
    ///     BoatVolume                  : Khối ghe
    ///     TransportTimes              : Số chuyến
    ///     ExtraVolume                 : Khối lẻ
    ///     Lines                       : Số line đơn hàng
    ///     VolumeRevenue               : Khối tính doanh số
    ///     CustomerPrice               : Giá bán
    ///     
    /// </summary>
    public class OrderController : BaseController
    {

        DataContext db = new DataContext();

        //
        // GET: /Order/

        [Authorize]
        public ActionResult Index()
        {
            // Write action log
            ActionLog.WriteLog(ActionLog.VIEW_ORDER_LIST, "", User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

            return View();
        }

        [Authorize]
        public ActionResult Add(int customerId = 0)
        {
            AddOrderViewModel model = new AddOrderViewModel();
            model.OrderDate = DateTime.Now.ToString("dd/MM/yyyy");
            model.CustomerId = customerId;

            // set viewbag data
            var sands = db.Sands
                .OrderBy(s => s.SandName)
                .Select(q1 => new SelectListItem
                {
                    Value = q1.SandId.ToString(),
                    Text = "\u00A0" + q1.SandName
                })
                .ToList();

            var boats = db.Boats
                .OrderBy(b => b.BoatCode)
                .Select(q1 => new SelectListItem
                {
                    Value = q1.BoatId.ToString(),
                    Text = "\u00A0" + q1.BoatCode + " - " + q1.BoatOwner
                })
                .ToList();
            ViewBag.Sands = sands;
            ViewBag.Boats = boats;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Add(AddOrderViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // save header
                    var newOrder = new Order();
                    newOrder.OrderDate = DateTime.ParseExact(model.OrderDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    newOrder.CustomerId = model.CustomerId;
                    newOrder.BargeId = model.BargeId;
                    newOrder.SandId = model.SandId;
                    newOrder.VolumeRevenue = model.VolumeRevenue;
                    newOrder.VolumePromotion = model.VolumePromotion;
                    newOrder.VolumePurchase = model.VolumePurchase;
                    newOrder.VolumePurchaseDecrease = model.VolumePurchaseDecrease;
                    newOrder.CustomerPrice = model.CustomerPrice;
                    newOrder.ProviderPrice = model.ProviderPrice;
                    newOrder.BaseTransportPrice = model.BaseTransportPrice;

                    db.Orders.Add(newOrder);
                    db.SaveChanges();

                    // save lines
                    List<OrderDetails> lines = new List<OrderDetails>();
                    foreach (var details in model.OrderDetails)
                    {
                        // group lines (gộp line)
                        var wasLineExisted = false;
                        foreach (var line in lines)
                        {
                            if (line.BoatId == details.BoatId &&
                                line.BoatVolume == details.BoatVolume &&
                                line.TransportPrice == details.TransportPrice)
                            {
                                wasLineExisted = true;
                                line.TransportTimes += details.TransportTimes;
                                line.ExtraVolume += details.ExtraVolume;
                            }
                        }
                        if (!wasLineExisted)
                        {
                            // line not existed, add new line
                            var newLine = new OrderDetails();
                            newLine.OrderId = newOrder.OrderId;
                            newLine.BoatId = details.BoatId;
                            newLine.TransportTimes = details.TransportTimes;
                            newLine.ExtraVolume = details.ExtraVolume;
                            newLine.BoatVolume = details.BoatVolume;
                            newLine.TransportPrice = details.TransportPrice;
                            lines.Add(newLine);
                        } // end if
                    } // end foreach

                    db.OrderDetails.AddRange(lines);
                    db.SaveChanges();

                    // Write action log
                    string logData =
                        "  orderId=" + newOrder.OrderId +
                        ", bargeId=" + newOrder.BargeId +
                        ", sandId=" + newOrder.SandId +
                        ", volume_revenue=" + newOrder.VolumeRevenue +
                        ", volume_promotion=" + newOrder.VolumePromotion +
                        ", volume_purchase=" + newOrder.VolumePurchase +
                        ", volume_purchase_decrease=" + newOrder.VolumePurchaseDecrease +
                        ", customer_price=" + newOrder.CustomerPrice +
                        ", provider_price=" + newOrder.ProviderPrice +
                        ", base_tránsport_price=" + newOrder.BaseTransportPrice;

                    ActionLog.WriteLog(ActionLog.ADD_ORDER_INFO, logData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    return RedirectToAction("Index");
                }

                // set viewbag data
                var sands = db.Sands
                    .OrderBy(s => s.SandName)
                    .Select(q1 => new SelectListItem
                    {
                        Value = q1.SandId.ToString(),
                        Text = "\u00A0" + q1.SandName
                    })
                    .ToList();

                var boats = db.Boats
                    .OrderBy(b => b.BoatCode)
                    .Select(q1 => new SelectListItem
                    {
                        Value = q1.BoatId.ToString(),
                        Text = "\u00A0" + q1.BoatCode + " - " + q1.BoatOwner
                    })
                    .ToList();
                ViewBag.Sands = sands;
                ViewBag.Boats = boats;

                ModelState.AddModelError("", "Thông tin HĐ không hợp lệ!");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                // rollback transaction
                DataContext.UndoingChangesDbContextLevel(db);
            }

            return View(model);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            try
            {
                // get order info
                var editedOrder = db.Orders.Where(o => o.OrderId == id).FirstOrDefault();
                if (editedOrder != null)
                {
                    // create view model
                    var model = new EditOrderViewModel();
                    model.OrderId = editedOrder.OrderId;
                    model.OrderDate = editedOrder.OrderDate.ToString("dd/MM/yyyy");
                    model.CustomerId = editedOrder.CustomerId;
                    model.BargeId = editedOrder.BargeId;
                    model.SandId = editedOrder.SandId;
                    model.VolumeRevenue = editedOrder.VolumeRevenue;
                    model.VolumePromotion = editedOrder.VolumePromotion;
                    model.VolumePurchase = editedOrder.VolumePurchase;
                    model.VolumePurchaseDecrease = editedOrder.VolumePurchaseDecrease;
                    model.CustomerPrice = editedOrder.CustomerPrice;
                    model.ProviderPrice = editedOrder.ProviderPrice;
                    model.BaseTransportPrice = editedOrder.BaseTransportPrice;

                    var lines = db.OrderDetails.Where(od => od.OrderId == editedOrder.OrderId).ToList();

                    foreach (var line in lines)
                    {
                        var childModel = new EditOrderDetailsViewModel();

                        childModel.OrderDetailsId = line.OrderDetailsId;
                        childModel.BoatId = line.BoatId;
                        childModel.BoatVolume = line.BoatVolume;
                        childModel.TransportTimes = line.TransportTimes;
                        childModel.ExtraVolume = line.ExtraVolume;
                        childModel.TotalVolume = line.TransportTimes * line.BoatVolume + line.ExtraVolume;
                        childModel.TransportPrice = line.TransportPrice;
                        childModel.Amount = (decimal)childModel.TotalVolume * line.TransportPrice;

                        // add order details view models
                        model.OrderDetails.Add(childModel);
                    }

                    // set view bage data
                    ViewBag.Boats = db.Boats.OrderBy(b => b.BoatCode)
                        .Select(b => new SelectListItem()
                        {
                            Text = b.BoatCode + " - " + b.BoatOwner,
                            Value = b.BoatId.ToString()
                        });

                    return View(model);
                }

                return RedirectToAction("Message", "Error",
                    new RouteValueDictionary(new { message = "Thông tin chuyến #" + id + " không tồn tại trong hệ thống!" }));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Message", "Error",
                    new RouteValueDictionary(new { message = ex.Message }));
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(EditOrderViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // get current order
                    var currentOrder = db.Orders.Where(o => o.OrderId == model.OrderId).FirstOrDefault();
                    if (currentOrder != null)
                    {

                        string logData =
                            "  orderId=" + currentOrder.OrderId +
                            ", date" + currentOrder.OrderDate.ToString("dd/MM/yyyy") +
                            ", new_customer_id=" + currentOrder.CustomerId +
                            ", bargeId=" + currentOrder.BargeId +
                            ", sandId=" + currentOrder.SandId +
                            ", volume_revenue=" + currentOrder.VolumeRevenue +
                            ", volume_promotion=" + currentOrder.VolumePromotion +
                            ", volume_purchase=" + currentOrder.VolumePurchase +
                            ", volume_purchase_decrease=" + currentOrder.VolumePurchaseDecrease +
                            ", customer_price=" + currentOrder.CustomerPrice +
                            ", provider_price=" + currentOrder.ProviderPrice +
                            ", base_transport_price=" + currentOrder.BaseTransportPrice;

                        // update order info
                        currentOrder.OrderDate = DateTime.ParseExact(model.OrderDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        currentOrder.CustomerId = model.CustomerId;
                        currentOrder.BargeId = model.BargeId;
                        currentOrder.SandId = model.SandId;
                        currentOrder.VolumeRevenue = model.VolumeRevenue;
                        currentOrder.VolumePromotion = model.VolumePromotion;
                        currentOrder.VolumePurchase = model.VolumePurchase;
                        currentOrder.VolumePurchaseDecrease = model.VolumePurchaseDecrease;
                        currentOrder.CustomerPrice = model.CustomerPrice;
                        currentOrder.ProviderPrice = model.ProviderPrice;
                        currentOrder.BaseTransportPrice = model.BaseTransportPrice;

                        // remove current lines
                        var currentLines = db.OrderDetails.Where(od => od.OrderId == currentOrder.OrderId).ToList();
                        db.OrderDetails.RemoveRange(currentLines);

                        // add new lines
                        List<OrderDetails> lines = new List<OrderDetails>();
                        foreach (var details in model.OrderDetails)
                        {
                            // group lines (gộp line)
                            var wasLineExisted = false;
                            foreach (var line in lines)
                            {
                                if (line.BoatId == details.BoatId &&
                                    line.BoatVolume == details.BoatVolume &&
                                    line.TransportPrice == details.TransportPrice)
                                {
                                    wasLineExisted = true;
                                    line.TransportTimes += details.TransportTimes;
                                    line.ExtraVolume += details.ExtraVolume;
                                }
                            }
                            if (!wasLineExisted)
                            {
                                // line not existed, add new line
                                var newLine = new OrderDetails();
                                newLine.OrderId = currentOrder.OrderId;
                                newLine.BoatId = details.BoatId;
                                newLine.TransportTimes = details.TransportTimes;
                                newLine.ExtraVolume = details.ExtraVolume;
                                newLine.BoatVolume = details.BoatVolume;
                                newLine.TransportPrice = details.TransportPrice;
                                lines.Add(newLine);
                            } // end if
                        } // end foreach

                        db.OrderDetails.AddRange(lines);
                        db.SaveChanges();

                        // Write action log
                        logData +=
                            ",new_date" + currentOrder.OrderDate.ToString("dd/MM/yyyy") +
                            ",new_customer_id=" + currentOrder.CustomerId +
                            ",new_barge_id=" + currentOrder.BargeId +
                            ",new_sand_id=" + currentOrder.SandId +
                            ",new_volume_revenue=" + currentOrder.VolumeRevenue +
                            ",new_volume_purchase=" + currentOrder.VolumePurchase +
                            ",new_volume_purchase_decrease=" + currentOrder.VolumePurchaseDecrease +
                            ",new_customer_price=" + currentOrder.CustomerPrice +
                            ",new_provider_price=" + currentOrder.ProviderPrice +
                            ",new_ base_transport_price=" + currentOrder.BaseTransportPrice;

                        ActionLog.WriteLog(ActionLog.EDIT_ORDER_INFO, logData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Message", "Error",
                            new RouteValueDictionary(new { message = "Không tìm thấy thông tin chuyến cần cập nhật!" }));
                    }
                }

                // set viewbag data
                ViewBag.Boats = db.Boats
                    .OrderBy(b => b.BoatCode)
                    .Select(q1 => new SelectListItem
                    {
                        Value = q1.BoatId.ToString(),
                        Text = "\u00A0" + q1.BoatCode + " - " + q1.BoatOwner
                    }).ToList();

                ModelState.AddModelError("", "Thông tin chuyến không hợp lệ!");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpPost]
        public JsonResult Delete(int orderId)
        {
            try
            {
                var order = db.Orders.Where(o => o.OrderId == orderId).FirstOrDefault();
                var orderDetails = db.OrderDetails.Where(od => od.OrderId == orderId).ToList();

                // remove header
                if (order != null)
                {
                    db.Orders.Remove(order);
                }

                // remove lines
                if (orderDetails != null)
                {
                    db.OrderDetails.RemoveRange(orderDetails);
                }

                db.SaveChanges();

                return Json(new { Error = 0, Message = "Success" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetOrders(string dateFrom, string dateTo, int sandId = 0, int customerId = 0, string filterText = "")
        {
            try
            {
                // parse date range
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                filterText = filterText.Trim().ToLower();

                // get sand info
                var sand = db.Sands.Where(s => s.SandId == sandId).FirstOrDefault();
                if (sand == null)
                {
                    return Json(new { Error = 1, Message = "Sản phẩm cát #" + sandId + " không tồn tại trong hệ thống!" });
                }

                // get sand type
                Sand.SandTypes sandType = 0;
                if (sand.SandName.ToLower().Contains("vàng") || sand.SandName.ToLower().Contains("vang") ||
                    sand.SandName.ToLower().Contains("trộn") || sand.SandName.ToLower().Contains("tron"))
                {
                    sandType = Sand.SandTypes.GoldSand;
                }
                if (sand.SandName.ToLower().Contains("lấp") || sand.SandName.ToLower().Contains("lap"))
                {
                    sandType = Sand.SandTypes.FillingSand;
                }

                // query orders
                switch (sandType)
                {
                    case Sand.SandTypes.FillingSand:
                        var query_1 = from t1 in db.OrderDetails
                                      join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                      join t3 in db.Customers on t2.CustomerId equals t3.CustomerId
                                      join t4 in db.Barges on t2.BargeId equals t4.BargeId
                                      where
                                        t2.OrderDate >= d1 &&
                                        t2.OrderDate <= d2 &&
                                        t2.SandId == sandId &&
                                        (customerId == 0 || t2.CustomerId == customerId) &&
                                        (string.IsNullOrEmpty(filterText) || t3.CustomerName.Contains(filterText) || t4.BargeCode.Contains(filterText))
                                      orderby
                                        t2.OrderDate descending,
                                        t4.BargeCode ascending
                                      group t1 by new
                                      {
                                          t1.OrderId,
                                          t2.OrderDate,
                                          t2.CustomerId,
                                          t3.CustomerName,
                                          t2.BargeId,
                                          t4.BargeCode,
                                          t2.VolumeRevenue,
                                          t2.VolumePurchase,
                                          t2.VolumePurchaseDecrease,
                                          t2.CustomerPrice,
                                          t2.ProviderPrice,
                                          AmountRevenue = t2.VolumeRevenue * (double)t2.CustomerPrice,
                                          AmountPurchase = (t2.VolumePurchase - t2.VolumePurchaseDecrease) * (double)t2.ProviderPrice
                                      } into g
                                      select new
                                      {
                                          g.Key.OrderId,
                                          g.Key.OrderDate,
                                          g.Key.CustomerId,
                                          g.Key.CustomerName,
                                          g.Key.BargeId,
                                          g.Key.BargeCode,
                                          g.Key.VolumeRevenue,
                                          g.Key.VolumePurchase,
                                          g.Key.VolumePurchaseDecrease,
                                          VolumeTransport = g.Sum(s => s.BoatVolume * s.TransportTimes + s.ExtraVolume),
                                          g.Key.CustomerPrice,
                                          g.Key.ProviderPrice,
                                          g.Key.AmountRevenue,
                                          g.Key.AmountPurchase,
                                          AmountTransport = g.Sum(s => (s.BoatVolume * s.TransportTimes + s.ExtraVolume) * (double)s.TransportPrice)
                                      };

                        return Json(new { Error = 0, Message = "Success", Orders = query_1 });

                    case Sand.SandTypes.GoldSand:
                        var query_2 = from t1 in db.OrderDetails
                                      join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                      join t3 in db.Customers on t2.CustomerId equals t3.CustomerId
                                      join t4 in db.Barges on t2.BargeId equals t4.BargeId
                                      where
                                        t2.OrderDate >= d1 &&
                                        t2.OrderDate <= d2 &&
                                        t2.SandId == sandId &&
                                        (customerId == 0 || t2.CustomerId == customerId) &&
                                        (string.IsNullOrEmpty(filterText) || t3.CustomerName.Contains(filterText) || t4.BargeCode.Contains(filterText))
                                      orderby
                                        t2.OrderDate descending,
                                        t4.BargeCode ascending
                                      group t1 by new
                                      {
                                          t1.OrderId,
                                          t2.OrderDate,
                                          t2.CustomerId,
                                          t3.CustomerName,
                                          t2.BargeId,
                                          t4.BargeCode,
                                          t2.VolumePurchase,
                                          t2.VolumePurchaseDecrease,
                                          t2.VolumePromotion,
                                          t2.CustomerPrice,
                                          t2.ProviderPrice,
                                          t2.BaseTransportPrice,
                                          AmountPurchase = (t2.VolumePurchase - t2.VolumePurchaseDecrease) * (double)t2.ProviderPrice
                                      } into g
                                      select new
                                      {
                                          g.Key.OrderId,
                                          g.Key.OrderDate,
                                          g.Key.CustomerId,
                                          g.Key.CustomerName,
                                          g.Key.BargeId,
                                          g.Key.BargeCode,
                                          g.Key.VolumePurchase,
                                          g.Key.VolumePurchaseDecrease,
                                          VolumeTransport = g.Sum(s => s.BoatVolume * s.TransportTimes + s.ExtraVolume),
                                          g.Key.VolumePromotion,
                                          g.Key.CustomerPrice,
                                          g.Key.ProviderPrice,
                                          g.Key.BaseTransportPrice,
                                          AmountRevenue = (g.Sum(s => s.BoatVolume * s.TransportTimes + s.ExtraVolume) - g.Key.VolumePromotion) * (double)g.Key.CustomerPrice + g.Key.VolumePromotion * (double)g.Key.BaseTransportPrice,
                                          g.Key.AmountPurchase,
                                          AmountTransport = g.Sum(s => (s.BoatVolume * s.TransportTimes + s.ExtraVolume) * (double)s.TransportPrice)
                                      };

                        return Json(new { Error = 0, Message = "Success", Orders = query_2 });
                }

                return Json(new { Error = 1, Message = "Loại cát không phù hợp!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpGet]
        public ActionResult ExportExcel(string dateFrom, string dateTo, int sandId = 0, int customerId = 0, string filterText = "")
        {
            var dtExcel = new DataTable("DanhSachChuyen");

            try
            {
                // get request parameters
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                filterText = filterText.Trim().ToLower();

                // get sand info
                var sand = db.Sands.Where(s => s.SandId == sandId).FirstOrDefault();
                if (sand == null)
                {
                    return Json(new { Error = 1, Message = "Sản phẩm cát #" + sandId + " không tồn tại trong hệ thống!" });
                }

                // get sand type
                Sand.SandTypes sandType = 0;
                if (sand.SandName.ToLower().Contains("vàng") || sand.SandName.ToLower().Contains("vang") ||
                    sand.SandName.ToLower().Contains("trộn") || sand.SandName.ToLower().Contains("tron"))
                {
                    sandType = Sand.SandTypes.GoldSand;
                }
                if (sand.SandName.ToLower().Contains("lấp") || sand.SandName.ToLower().Contains("lap"))
                {
                    sandType = Sand.SandTypes.FillingSand;
                }

                // query orders
                switch (sandType)
                {
                    case Sand.SandTypes.FillingSand:
                        var query_1 = from t1 in db.OrderDetails
                                      join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                      join t3 in db.Customers on t2.CustomerId equals t3.CustomerId
                                      join t4 in db.Barges on t2.BargeId equals t4.BargeId
                                      where
                                        t2.OrderDate >= d1 &&
                                        t2.OrderDate <= d2 &&
                                        t2.SandId == sandId &&
                                        (customerId == 0 || t2.CustomerId == customerId) &&
                                        (string.IsNullOrEmpty(filterText) || t3.CustomerName.Contains(filterText) || t4.BargeCode.Contains(filterText))
                                      orderby
                                        t2.OrderDate descending,
                                        t4.BargeCode ascending
                                      group t1 by new
                                      {
                                          t1.OrderId,
                                          t2.OrderDate,
                                          t2.CustomerId,
                                          t3.CustomerName,
                                          t2.BargeId,
                                          t4.BargeCode,
                                          t2.VolumeRevenue,
                                          t2.VolumePurchase,
                                          t2.VolumePurchaseDecrease,
                                          t2.CustomerPrice,
                                          t2.ProviderPrice,
                                          AmountRevenue = t2.VolumeRevenue * (double)t2.CustomerPrice,
                                          AmountPurchase = (t2.VolumePurchase - t2.VolumePurchaseDecrease) * (double)t2.ProviderPrice
                                      } into g
                                      select new
                                      {
                                          g.Key.OrderId,
                                          g.Key.OrderDate,
                                          g.Key.CustomerId,
                                          g.Key.CustomerName,
                                          g.Key.BargeId,
                                          g.Key.BargeCode,
                                          g.Key.VolumeRevenue,
                                          g.Key.VolumePurchase,
                                          g.Key.VolumePurchaseDecrease,
                                          VolumeTransport = g.Sum(s => s.BoatVolume * s.TransportTimes + s.ExtraVolume),
                                          g.Key.CustomerPrice,
                                          g.Key.ProviderPrice,
                                          g.Key.AmountRevenue,
                                          g.Key.AmountPurchase,
                                          AmountTransport = g.Sum(s => (s.BoatVolume * s.TransportTimes + s.ExtraVolume) * (double)s.TransportPrice)
                                      };

                        dtExcel.Columns.Add("Ngày", typeof(string));
                        dtExcel.Columns.Add("Đơn vị", typeof(string));
                        dtExcel.Columns.Add("Sà lan", typeof(string));
                        dtExcel.Columns.Add("Khối doanh số", typeof(string));
                        dtExcel.Columns.Add("Khối mỏ", typeof(string));
                        dtExcel.Columns.Add("Khối giảm", typeof(string));
                        dtExcel.Columns.Add("Khối ghe", typeof(string));
                        dtExcel.Columns.Add("Giá bán", typeof(string));
                        dtExcel.Columns.Add("Giá mua", typeof(string));
                        dtExcel.Columns.Add("Doanh số", typeof(string));
                        dtExcel.Columns.Add("Trả tiền mỏ", typeof(string));
                        dtExcel.Columns.Add("Trả tiền ghe", typeof(string));
                        dtExcel.Columns.Add("Lợi nhuận", typeof(string));

                        double sumVolumeRevenue_1 = 0;
                        double sumVolumePurchase_1 = 0;
                        double sumVolumePurchaseDescrease_1 = 0;
                        double sumVolumeTransport_1 = 0;
                        double sumAmountRevenue_1 = 0;
                        double sumAmountPurchase_1 = 0;
                        double sumAmountTransport_1 = 0;
                        double sumAmountProfit_1 = 0;

                        foreach (var item in query_1)
                        {
                            var row_1 = dtExcel.NewRow();
                            row_1["Ngày"] = item.OrderDate;
                            row_1["Đơn vị"] = item.CustomerName;
                            row_1["Sà lan"] = item.BargeCode;
                            row_1["Khối doanh số"] = item.VolumeRevenue;
                            row_1["Khối mỏ"] = item.VolumePurchase;
                            row_1["Khối giảm"] = item.VolumePurchaseDecrease;
                            row_1["Khối ghe"] = item.VolumeTransport;
                            row_1["Giá bán"] = item.CustomerPrice;
                            row_1["Giá mua"] = item.ProviderPrice;
                            row_1["Doanh số"] = item.AmountRevenue;
                            row_1["Trả tiền mỏ"] = item.AmountPurchase;
                            row_1["Trả tiền ghe"] = item.AmountTransport;
                            row_1["Lợi nhuận"] = item.AmountRevenue - item.AmountPurchase - item.AmountTransport;

                            sumVolumeRevenue_1 += item.VolumeRevenue;
                            sumVolumePurchase_1 += item.VolumePurchase;
                            sumVolumePurchaseDescrease_1 += item.VolumePurchaseDecrease;
                            sumVolumeTransport_1 += item.VolumeTransport;
                            sumAmountRevenue_1 += item.AmountRevenue;
                            sumAmountPurchase_1 += item.AmountPurchase;
                            sumAmountTransport_1 += item.AmountTransport;
                            sumAmountProfit_1 += item.AmountRevenue - item.AmountPurchase - item.AmountTransport;

                            dtExcel.Rows.Add(row_1);
                        }

                        var rowSummary_1 = dtExcel.NewRow();
                        rowSummary_1["Ngày"] = "Tổng cộng";
                        rowSummary_1["Đơn vị"] = string.Empty;
                        rowSummary_1["Sà lan"] = string.Empty;
                        rowSummary_1["Khối doanh số"] = sumVolumeRevenue_1;
                        rowSummary_1["Khối mỏ"] = sumVolumePurchase_1;
                        rowSummary_1["Khối giảm"] = sumVolumePurchaseDescrease_1;
                        rowSummary_1["Khối ghe"] = sumVolumeTransport_1;
                        rowSummary_1["Giá bán"] = string.Empty;
                        rowSummary_1["Giá mua"] = string.Empty;
                        rowSummary_1["Doanh số"] = sumAmountRevenue_1;
                        rowSummary_1["Trả tiền mỏ"] = sumAmountPurchase_1;
                        rowSummary_1["Trả tiền ghe"] = sumAmountTransport_1;
                        rowSummary_1["Lợi nhuận"] = sumAmountProfit_1;

                        dtExcel.Rows.Add(rowSummary_1);

                        break;

                    case Sand.SandTypes.GoldSand:
                        var query_2 = from t1 in db.OrderDetails
                                      join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                      join t3 in db.Customers on t2.CustomerId equals t3.CustomerId
                                      join t4 in db.Barges on t2.BargeId equals t4.BargeId
                                      where
                                        t2.OrderDate >= d1 &&
                                        t2.OrderDate <= d2 &&
                                        t2.SandId == sandId &&
                                        (customerId == 0 || t2.CustomerId == customerId) &&
                                        (string.IsNullOrEmpty(filterText) || t3.CustomerName.Contains(filterText) || t4.BargeCode.Contains(filterText))
                                      orderby
                                        t2.OrderDate descending,
                                        t4.BargeCode ascending
                                      group t1 by new
                                      {
                                          t1.OrderId,
                                          t2.OrderDate,
                                          t2.CustomerId,
                                          t3.CustomerName,
                                          t2.BargeId,
                                          t4.BargeCode,
                                          t2.VolumePurchase,
                                          t2.VolumePurchaseDecrease,
                                          t2.VolumePromotion,
                                          t2.CustomerPrice,
                                          t2.ProviderPrice,
                                          t2.BaseTransportPrice,
                                          AmountPurchase = (t2.VolumePurchase - t2.VolumePurchaseDecrease) * (double)t2.ProviderPrice
                                      } into g
                                      select new
                                      {
                                          g.Key.OrderId,
                                          g.Key.OrderDate,
                                          g.Key.CustomerId,
                                          g.Key.CustomerName,
                                          g.Key.BargeId,
                                          g.Key.BargeCode,
                                          g.Key.VolumePurchase,
                                          g.Key.VolumePurchaseDecrease,
                                          VolumeTransport = g.Sum(s => s.BoatVolume * s.TransportTimes + s.ExtraVolume),
                                          g.Key.VolumePromotion,
                                          g.Key.CustomerPrice,
                                          g.Key.ProviderPrice,
                                          g.Key.BaseTransportPrice,
                                          AmountRevenue = (g.Sum(s => s.BoatVolume * s.TransportTimes + s.ExtraVolume) - g.Key.VolumePromotion) * (double)g.Key.CustomerPrice + g.Key.VolumePromotion * (double)g.Key.BaseTransportPrice,
                                          g.Key.AmountPurchase,
                                          AmountTransport = g.Sum(s => (s.BoatVolume * s.TransportTimes + s.ExtraVolume) * (double)s.TransportPrice)
                                      };

                        dtExcel.Columns.Add("Ngày", typeof(string));
                        dtExcel.Columns.Add("Đơn vị", typeof(string));
                        dtExcel.Columns.Add("Sà lan", typeof(string));
                        dtExcel.Columns.Add("Khối mỏ", typeof(string));
                        dtExcel.Columns.Add("Khối giảm", typeof(string));
                        dtExcel.Columns.Add("Khối ghe", typeof(string));
                        dtExcel.Columns.Add("Khối gia công", typeof(string));
                        dtExcel.Columns.Add("Giá bán", typeof(string));
                        dtExcel.Columns.Add("Giá mua", typeof(string));
                        dtExcel.Columns.Add("Giá gia công", typeof(string));
                        dtExcel.Columns.Add("Doanh số", typeof(string));
                        dtExcel.Columns.Add("Trả tiền mỏ", typeof(string));
                        dtExcel.Columns.Add("Trả tiền ghe", typeof(string));
                        dtExcel.Columns.Add("Lợi nhuận", typeof(string));

                        double sumVolumePromotion_2 = 0;
                        double sumVolumePurchase_2 = 0;
                        double sumVolumePurchaseDescrease_2 = 0;
                        double sumVolumeTransport_2 = 0;
                        double sumAmountRevenue_2 = 0;
                        double sumAmountPurchase_2 = 0;
                        double sumAmountTransport_2 = 0;
                        double sumAmountProfit_2 = 0;

                        foreach (var item in query_2)
                        {
                            var row_2 = dtExcel.NewRow();
                            row_2["Ngày"] = item.OrderDate;
                            row_2["Đơn vị"] = item.CustomerName;
                            row_2["Sà lan"] = item.BargeCode;
                            row_2["Khối mỏ"] = item.VolumePurchase;
                            row_2["Khối giảm"] = item.VolumePurchaseDecrease;
                            row_2["Khối ghe"] = item.VolumeTransport;
                            row_2["Khối gia công"] = item.VolumePromotion;
                            row_2["Giá bán"] = item.CustomerPrice;
                            row_2["Giá mua"] = item.ProviderPrice;
                            row_2["Giá gia công"] = item.BaseTransportPrice;
                            row_2["Doanh số"] = item.AmountRevenue;
                            row_2["Trả tiền mỏ"] = item.AmountPurchase;
                            row_2["Trả tiền ghe"] = item.AmountTransport;
                            row_2["Lợi nhuận"] = item.AmountRevenue - item.AmountPurchase - item.AmountTransport;
                            
                            sumVolumePurchase_2 += item.VolumePurchase;
                            sumVolumePurchaseDescrease_2 += item.VolumePurchaseDecrease;
                            sumVolumeTransport_2 += item.VolumeTransport;
                            sumVolumePromotion_2 += item.VolumePromotion;
                            sumAmountRevenue_2 += item.AmountRevenue;
                            sumAmountPurchase_2 += item.AmountPurchase;
                            sumAmountTransport_2 += item.AmountTransport;
                            sumAmountProfit_2 += item.AmountRevenue - item.AmountPurchase - item.AmountTransport;

                            dtExcel.Rows.Add(row_2);

                        }

                        var rowSummary_2 = dtExcel.NewRow();
                        rowSummary_2["Ngày"] = "Tổng cộng";
                        rowSummary_2["Đơn vị"] = string.Empty;
                        rowSummary_2["Sà lan"] = string.Empty;
                        rowSummary_2["Khối mỏ"] = sumVolumePurchase_2;
                        rowSummary_2["Khối giảm"] = sumVolumePurchaseDescrease_2;
                        rowSummary_2["Khối ghe"] = sumVolumeTransport_2;
                        rowSummary_2["Khối gia công"] = sumVolumePromotion_2;
                        rowSummary_2["Giá bán"] = string.Empty;
                        rowSummary_2["Giá mua"] = string.Empty;
                        rowSummary_2["Giá gia công"] = string.Empty;
                        rowSummary_2["Doanh số"] = sumAmountRevenue_2;
                        rowSummary_2["Trả tiền mỏ"] = sumAmountPurchase_2;
                        rowSummary_2["Trả tiền ghe"] = sumAmountTransport_2;
                        rowSummary_2["Lợi nhuận"] = sumAmountProfit_2;

                        dtExcel.Rows.Add(rowSummary_2);

                        break;

                }


                var grid = new GridView();
                grid.DataSource = dtExcel;
                grid.DataBind();

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=BaoCao_DanhSach_Chuyen_" + DateTime.Now.ToString("yyyyMMdd") + ".xls");
                Response.ContentType = "application/ms-excel";

                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

                // Write action log
                var actionLogData =
                    "  d1=" + dateFrom +
                    ", d2=" + dateTo +
                    ", sand_id=" + sandId +
                    ", customer_id=" + customerId +
                    ", filter_text=" + filterText;
                ActionLog.WriteLog(ActionLog.EXPORT_EXCEL_ORDER_REPORT, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                return View();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return View();
        }
    }
}
