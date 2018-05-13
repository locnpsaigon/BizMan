using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using BizMan.DAL;
using BizMan.Helpers.Security;

namespace BizMan.Controllers
{
    public class ReportController : Controller
    {

        DataContext db = new DataContext();

        #region Boat reports

        [Authorize]
        public ActionResult ReportBoatsData()
        {
            // Write action log
            ActionLog.WriteLog(ActionLog.VIEW_BOAT_REPORT, "", User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

            return View();
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetBoatsTransportData(string dateFrom, string dateTo, int sandId = 0)
        {
            try
            {
                // filter report by date range
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                var transportData = from t1 in db.OrderDetails
                                    join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                    join t3 in db.Boats on t1.BoatId equals t3.BoatId
                                    join t4 in db.Barges on t2.BargeId equals t4.BargeId
                                    where t2.OrderDate >= d1 && t2.OrderDate <= d2 && t2.SandId == sandId
                                    orderby t2.OrderDate ascending, t3.BoatId ascending, t3.BoatCode ascending
                                    select new
                                    {
                                        t1.OrderId,
                                        t2.OrderDate,
                                        t4.BargeCode,
                                        t2.VolumeRevenue,
                                        t3.BoatCode,
                                        t3.BoatOwner,
                                        t1.TransportTimes,
                                        t1.ExtraVolume,
                                        t1.BoatVolume,
                                        TotalBoatVolumes = t1.TransportTimes * t1.BoatVolume + t1.ExtraVolume
                                    };

                // Write action logs
                var actionLogData =
                    "  d1=" + dateFrom +
                    ", d2=" + dateTo +
                    ", sand_id=" + sandId;
                ActionLog.WriteLog(ActionLog.VIEW_BOAT_REPORT, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                return Json(new { Error = 0, Message = "Success", ReportData = transportData });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetBoatsTransportCostData(string dateFrom, string dateTo, int sandId = 0)
        {
            try
            {
                // filter report by date range
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                var transportCost = from t1 in db.OrderDetails
                                    join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                    join t3 in db.Boats on t1.BoatId equals t3.BoatId
                                    where t2.OrderDate >= d1 && t2.OrderDate <= d2 && t2.SandId == sandId
                                    group t1 by new
                                    {
                                        t1.BoatId,
                                        t3.BoatCode,
                                        t3.BoatOwner,
                                        t1.BoatVolume,
                                        t1.TransportPrice
                                    } into g
                                    select new
                                    {
                                        g.Key.BoatId,
                                        g.Key.BoatCode,
                                        g.Key.BoatOwner,
                                        g.Key.BoatVolume,
                                        g.Key.TransportPrice,
                                        TransportTimes = g.Sum(s => s.TransportTimes),
                                        ExtraVolumes = g.Sum(s => s.ExtraVolume),
                                        TotalBoatVolumes = g.Sum(s => s.TransportTimes) * g.Key.BoatVolume + g.Sum(s => s.ExtraVolume),
                                        TransportCost = (g.Sum(s => s.TransportTimes) * g.Key.BoatVolume + g.Sum(s => s.ExtraVolume)) * (double)g.Key.TransportPrice
                                    };

                return Json(new { Error = 0, Message = "Success", ReportData = transportCost });

            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpGet]
        public ActionResult ExportExcelBoatsData(string dateFrom, string dateTo, int sandId = 0)
        {
            try
            {
                // filter report by date range
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                #region Transport Data

                // BOAT TRANSPORT DATA
                var transportData = (from t1 in db.OrderDetails
                                     join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                     join t3 in db.Boats on t1.BoatId equals t3.BoatId
                                     join t4 in db.Barges on t2.BargeId equals t4.BargeId
                                     where t2.OrderDate >= d1 && t2.OrderDate <= d2 && t2.SandId == sandId
                                     orderby t2.OrderDate ascending, t3.BoatId ascending, t3.BoatCode ascending
                                     select new
                                     {
                                         t1.OrderId,
                                         t2.OrderDate,
                                         t4.BargeCode,
                                         t2.VolumeRevenue,
                                         t3.BoatCode,
                                         t3.BoatOwner,
                                         t1.TransportTimes,
                                         t1.ExtraVolume,
                                         t1.BoatVolume,
                                         TotalBoatVolumes = t1.TransportTimes * t1.BoatVolume + t1.ExtraVolume
                                     }).ToList();

                // export transport data
                DataTable dtExcel = new DataTable("TransportData");
                dtExcel.Columns.Add("Ngày", typeof(string));
                dtExcel.Columns.Add("Sà lan", typeof(string));
                dtExcel.Columns.Add("Khối lượng", typeof(string));
                dtExcel.Columns.Add("Ghe", typeof(string));
                dtExcel.Columns.Add("Số chuyến", typeof(string));
                dtExcel.Columns.Add("Khối lẻ", typeof(string));
                dtExcel.Columns.Add("Khối ghe", typeof(string));
                dtExcel.Columns.Add("Tổng khối", typeof(string));

                var total_boat_volumes = (double)0;
                var grand_total_boat_volumes = (double)0;
                var grand_total_barge_volumes = (double)0;

                for (int i = 0; i < transportData.Count; i++)
                {
                    // is a new transport group start?
                    if (i > 0 && transportData[i - 1].OrderId != transportData[i].OrderId)
                    {
                        // add transport group summary row
                        var group_summary_row = dtExcel.NewRow();
                        group_summary_row["Ngày"] = "Tổng:";
                        group_summary_row["Sà lan"] = "";
                        group_summary_row["Khối lượng"] = transportData[i - 1].VolumeRevenue;
                        group_summary_row["Ghe"] = "";
                        group_summary_row["Số chuyến"] = "";
                        group_summary_row["Khối lẻ"] = "";
                        group_summary_row["Khối ghe"] = "";
                        group_summary_row["Tổng khối"] = total_boat_volumes.ToString();

                        dtExcel.Rows.Add(group_summary_row);

                        // update grand total volumes
                        grand_total_barge_volumes += transportData[i - 1].VolumeRevenue;
                        grand_total_boat_volumes += total_boat_volumes;

                        total_boat_volumes = 0;
                    }

                    if (i == 0 || (transportData[i - 1].OrderId != transportData[i].OrderId))
                    {
                        var group_first_row = dtExcel.NewRow();
                        group_first_row["Ngày"] = transportData[i].OrderDate.ToString("yyyy/MM/dd");
                        group_first_row["Sà lan"] = transportData[i].BargeCode;
                        group_first_row["Khối lượng"] = transportData[i].VolumeRevenue;
                        group_first_row["Ghe"] = transportData[i].BoatOwner;
                        group_first_row["Số chuyến"] = transportData[i].TransportTimes;
                        group_first_row["Khối lẻ"] = transportData[i].ExtraVolume;
                        group_first_row["Khối ghe"] = transportData[i].BoatVolume;
                        group_first_row["Tổng khối"] = transportData[i].TotalBoatVolumes;

                        dtExcel.Rows.Add(group_first_row);
                    }
                    else
                    {
                        var group_row = dtExcel.NewRow();
                        group_row["Ngày"] = "";
                        group_row["Sà lan"] = "";
                        group_row["Khối lượng"] = "";
                        group_row["Ghe"] = transportData[i].BoatOwner;
                        group_row["Số chuyến"] = transportData[i].TransportTimes;
                        group_row["Khối lẻ"] = transportData[i].ExtraVolume;
                        group_row["Khối ghe"] = transportData[i].BoatVolume;
                        group_row["Tổng khối"] = transportData[i].TotalBoatVolumes;

                        dtExcel.Rows.Add(group_row);
                    }

                    total_boat_volumes += transportData[i].TotalBoatVolumes;
                } // .end for

                // update last grand total volumes
                grand_total_barge_volumes += transportData[transportData.Count - 1].VolumeRevenue;
                grand_total_boat_volumes += total_boat_volumes;

                // add last transport group summary row
                var last_group_summary_row = dtExcel.NewRow();
                last_group_summary_row["Ngày"] = "Tổng:";
                last_group_summary_row["Sà lan"] = "";
                last_group_summary_row["Khối lượng"] = transportData[transportData.Count - 1].VolumeRevenue;
                last_group_summary_row["Ghe"] = "";
                last_group_summary_row["Số chuyến"] = "";
                last_group_summary_row["Khối lẻ"] = "";
                last_group_summary_row["Khối ghe"] = "";
                last_group_summary_row["Tổng khối"] = total_boat_volumes;

                dtExcel.Rows.Add(last_group_summary_row);

                // add grand total row
                var grand_total_row = dtExcel.NewRow();
                grand_total_row["Ngày"] = "Tổng cộng:";
                grand_total_row["Sà lan"] = "";
                grand_total_row["Khối lượng"] = grand_total_barge_volumes;
                grand_total_row["Ghe"] = "";
                grand_total_row["Số chuyến"] = "";
                grand_total_row["Khối lẻ"] = "";
                grand_total_row["Khối ghe"] = "";
                grand_total_row["Tổng khối"] = grand_total_boat_volumes;

                dtExcel.Rows.Add(grand_total_row);

                #endregion

                #region Transport Cost

                // TRANSPORT COST INFO
                var transportCost = (from t1 in db.OrderDetails
                                     join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                     join t3 in db.Boats on t1.BoatId equals t3.BoatId
                                     where t2.OrderDate >= d1 && t2.OrderDate <= d2 && t2.SandId == sandId
                                     group t1 by new
                                     {
                                         t1.BoatId,
                                         t3.BoatCode,
                                         t3.BoatOwner,
                                         t1.BoatVolume,
                                         t1.TransportPrice
                                     } into g
                                     select new
                                     {
                                         g.Key.BoatId,
                                         g.Key.BoatCode,
                                         g.Key.BoatOwner,
                                         g.Key.BoatVolume,
                                         g.Key.TransportPrice,
                                         TransportTimes = g.Sum(s => s.TransportTimes),
                                         ExtraVolumes = g.Sum(s => s.ExtraVolume),
                                         TotalBoatVolumes = g.Sum(s => s.TransportTimes) * g.Key.BoatVolume + g.Sum(s => s.ExtraVolume),
                                         TransportCost = (g.Sum(s => s.TransportTimes) * g.Key.BoatVolume + g.Sum(s => s.ExtraVolume)) * (double)g.Key.TransportPrice
                                     }).ToList();

                // export transport cost data
                DataTable dtExcel2 = new DataTable("TransportCostData");
                dtExcel2.Columns.Add("Số ghe", typeof(string));
                dtExcel2.Columns.Add("Tên ghe", typeof(string));
                dtExcel2.Columns.Add("Số chuyến", typeof(string));
                dtExcel2.Columns.Add("Khối lẻ", typeof(string));
                dtExcel2.Columns.Add("Khối ghe", typeof(string));
                dtExcel2.Columns.Add("Tổng khối", typeof(string));
                dtExcel2.Columns.Add("Đơn giá", typeof(string));
                dtExcel2.Columns.Add("Thành tiền", typeof(string));

                var sum_boat_volumes = (double)0;
                var sum_transport_cost = (double)0;

                foreach (var item in transportCost)
                {
                    var row = dtExcel2.NewRow();
                    row["Số ghe"] = item.BoatCode;
                    row["Tên ghe"] = item.BoatOwner;
                    row["Số chuyến"] = item.TransportTimes;
                    row["Khối lẻ"] = item.ExtraVolumes;
                    row["Khối ghe"] = item.BoatVolume;
                    row["Tổng khối"] = item.TotalBoatVolumes;
                    row["Đơn giá"] = item.TransportPrice;
                    row["Thành tiền"] = item.TransportCost;

                    dtExcel2.Rows.Add(row);

                    sum_boat_volumes += item.TotalBoatVolumes;
                    sum_transport_cost += item.TransportCost;
                }

                // add summary row
                var summary_row = dtExcel2.NewRow();
                summary_row["Số ghe"] = "Tổng cộng:";
                summary_row["Tên ghe"] = "";
                summary_row["Số chuyến"] = "";
                summary_row["Khối lẻ"] = "";
                summary_row["Khối ghe"] = "";
                summary_row["Tổng khối"] = sum_boat_volumes;
                summary_row["Đơn giá"] = "";
                summary_row["Thành tiền"] = sum_transport_cost;
                dtExcel2.Rows.Add(summary_row);

                #endregion

                var grid = new GridView();
                grid.DataSource = dtExcel;
                grid.DataBind();

                var grid2 = new GridView();
                grid2.DataSource = dtExcel2;
                grid2.DataBind();

                // Export excel
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=BaoCao_SoLieuGhe_" + DateTime.Now.ToString("yyyyMMdd") + ".xls");
                Response.ContentType = "application/ms-excel";

                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);
                htw.WriteBreak();
                htw.WriteBreak();
                grid2.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

                // Write action logs
                var actionLogData =
                    "  d1=" + dateFrom +
                    ", d2=" + dateTo +
                    ", sand_id=" + sandId;
                ActionLog.WriteLog(ActionLog.EXPORT_EXCEL_BOAT_REPORT, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                return View();

            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return View();
        }

        #endregion

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult ReportFinance()
        {
            // Write action log
            ActionLog.WriteLog(ActionLog.VIEW_BOAT_REPORT, "", User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

            return View();
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpPost]
        public JsonResult GetFinanceData(string dateFrom, string dateTo)
        {
            double amountGoldSandRevenue = 0;
            double amountFillindSandRevenue = 0;
            double amountPurchase = 0;
            double amountTransport = 0;

            try
            {
                // parse date range
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                // calculate GOLD SAND amount revenue
                var goldSand = db.Sands
                    .Where(s => s.SandName.ToLower().Contains("vàng") || s.SandName.ToLower().Contains("vang"))
                    .FirstOrDefault();
                var goldSandId = (goldSand == null) ? 0 : goldSand.SandId;
                amountGoldSandRevenue = (from t1 in db.OrderDetails
                                         join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                         where
                                           t2.OrderDate >= d1 &&
                                           t2.OrderDate <= d2 &&
                                           t2.SandId == goldSandId
                                         group t1 by new
                                         {
                                             t2.OrderId,
                                             t2.VolumePromotion,
                                             t2.CustomerPrice,
                                             t2.BaseTransportPrice
                                         } into g
                                         select new
                                         {
                                             AmountRevenue = (g.Sum(s => s.BoatVolume * s.TransportTimes + s.ExtraVolume) - g.Key.VolumePromotion) * (double)g.Key.CustomerPrice + g.Key.VolumePromotion * (double)g.Key.BaseTransportPrice
                                         })
                                         .Sum(s => s.AmountRevenue);


                // calculate FILLING SAND amount revenue
                var fillingSand = db.Sands
                    .Where(s => s.SandName.ToLower().Contains("lấp") || s.SandName.ToLower().Contains("lap") || s.SandName.ToLower().Contains("trộn") || s.SandName.ToLower().Contains("tron"))
                    .FirstOrDefault();
                var fillingSandId = (fillingSand == null) ? 0 : fillingSand.SandId;
                amountFillindSandRevenue = (from t1 in db.Orders
                                            where
                                              t1.OrderDate >= d1 &&
                                              t1.OrderDate <= d2 &&
                                              t1.SandId == fillingSandId
                                            select new
                                            {
                                                AmountRevenue = t1.VolumeRevenue * (double)t1.CustomerPrice
                                            })
                                            .Sum(s => s.AmountRevenue);


                // calculate amount purchase
                amountPurchase = (from t1 in db.Orders
                                  where t1.OrderDate >= d1 && t1.OrderDate <= d2
                                  select new
                                  {
                                      AmountPurchase = (t1.VolumePurchase - t1.VolumePurchaseDecrease) * (double)t1.ProviderPrice
                                  })
                                  .Sum(s => s.AmountPurchase);

                // get transport cost
                amountTransport = (from t1 in db.Orders
                                   join t2 in db.OrderDetails on t1.OrderId equals t2.OrderId
                                   where t1.OrderDate >= d1 && t1.OrderDate <= d2
                                   select new
                                   {
                                       TransportCost = (t2.TransportTimes * t2.BoatVolume + t2.ExtraVolume) * (double)t2.TransportPrice
                                   })
                                   .Sum(s => s.TransportCost);


                // get expenses
                var expenses = (from t1 in db.Expenses
                                where t1.Date >= d1 && t1.Date <= d2
                                select new
                                {
                                    t1.Name,
                                    t1.Amount
                                }).ToList();

                var jsonResult = new
                {
                    RevenueGoldSand = amountGoldSandRevenue,
                    RevenueFillingSand = amountFillindSandRevenue,
                    PurchaseAmount = amountPurchase,
                    TransportAmount = amountTransport,
                    Expenses = expenses
                };

                return Json(new { Error = 0, Message = "Success", FinanceData = jsonResult });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }


        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpGet]
        public ActionResult ExportExcelFinanceData(string dateFrom, string dateTo)
        {
            double amountGoldSandRevenue = 0;
            double amountFillindSandRevenue = 0;
            double amountPurchase = 0;
            double amountTransport = 0;
            double amountExpense = 0;

            try
            {
                // filter report by date range
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                // calculate GOLD SAND amount revenue
                var goldSand = db.Sands
                    .Where(s => s.SandName.ToLower().Contains("vàng") || s.SandName.ToLower().Contains("vang"))
                    .FirstOrDefault();
                var goldSandId = (goldSand == null) ? 0 : goldSand.SandId;
                amountGoldSandRevenue = (from t1 in db.OrderDetails
                                         join t2 in db.Orders on t1.OrderId equals t2.OrderId
                                         where
                                           t2.OrderDate >= d1 &&
                                           t2.OrderDate <= d2 &&
                                           t2.SandId == goldSandId
                                         group t1 by new
                                         {
                                             t2.OrderId,
                                             t2.VolumePromotion,
                                             t2.CustomerPrice,
                                             t2.BaseTransportPrice
                                         } into g
                                         select new
                                         {
                                             AmountRevenue = (g.Sum(s => s.BoatVolume * s.TransportTimes + s.ExtraVolume) - g.Key.VolumePromotion) * (double)g.Key.CustomerPrice + g.Key.VolumePromotion * (double)g.Key.BaseTransportPrice
                                         })
                                         .Sum(s => s.AmountRevenue);


                // calculate FILLING SAND amount revenue
                var fillingSand = db.Sands
                    .Where(s => s.SandName.ToLower().Contains("lấp") || s.SandName.ToLower().Contains("lap") || s.SandName.ToLower().Contains("trộn") || s.SandName.ToLower().Contains("tron"))
                    .FirstOrDefault();
                var fillingSandId = (fillingSand == null) ? 0 : fillingSand.SandId;
                amountFillindSandRevenue = (from t1 in db.Orders
                                            where
                                              t1.OrderDate >= d1 &&
                                              t1.OrderDate <= d2 &&
                                              t1.SandId == fillingSandId
                                            select new
                                            {
                                                AmountRevenue = t1.VolumeRevenue * (double)t1.CustomerPrice
                                            })
                                            .Sum(s => s.AmountRevenue);


                // calculate amount purchase
                amountPurchase = (from t1 in db.Orders
                                  where t1.OrderDate >= d1 && t1.OrderDate <= d2
                                  select new
                                  {
                                      AmountPurchase = (t1.VolumePurchase - t1.VolumePurchaseDecrease) * (double)t1.ProviderPrice
                                  })
                                  .Sum(s => s.AmountPurchase);

                // get transport cost
                amountTransport = (from t1 in db.Orders
                                   join t2 in db.OrderDetails on t1.OrderId equals t2.OrderId
                                   where t1.OrderDate >= d1 && t1.OrderDate <= d2
                                   select new
                                   {
                                       TransportCost = (t2.TransportTimes * t2.BoatVolume + t2.ExtraVolume) * (double)t2.TransportPrice
                                   })
                                   .Sum(s => s.TransportCost);

                // get expenses
                var expenses = (from t1 in db.Expenses
                                where t1.Date >= d1 && t1.Date <= d2
                                select new
                                {
                                    t1.Name,
                                    t1.Amount
                                });
                amountExpense = expenses.Sum(s => (double)s.Amount);

                var dtExcel = new DataTable("Finance");
                dtExcel.Columns.Add("HẠNG MỤC", typeof(string));
                dtExcel.Columns.Add("SỐ TIỀN", typeof(string));

                // Revenue gold sand
                var row1 = dtExcel.NewRow();
                row1["HẠNG MỤC"] = "TỔNG THU CÁT VÀNG";
                row1["SỐ TIỀN"] = amountGoldSandRevenue;
                dtExcel.Rows.Add(row1);

                // Revenue filling sand
                var row2 = dtExcel.NewRow();
                row2["HẠNG MỤC"] = "TỔNG THU CÁT LẤP";
                row2["SỐ TIỀN"] = amountFillindSandRevenue;
                dtExcel.Rows.Add(row2);

                // Reveneue summary
                var row3 = dtExcel.NewRow();
                row3["HẠNG MỤC"] = "TỔNG THU";
                row3["SỐ TIỀN"] = amountGoldSandRevenue + amountFillindSandRevenue;
                dtExcel.Rows.Add(row3);

                // Transport cost
                var row4 = dtExcel.NewRow();
                row4["HẠNG MỤC"] = "TRẢ TIỀN GHE";
                row4["SỐ TIỀN"] = amountTransport;
                dtExcel.Rows.Add(row4);

                // Purchase cost
                var row5 = dtExcel.NewRow();
                row5["HẠNG MỤC"] = "TIỀN MỎ";
                row5["SỐ TIỀN"] = amountPurchase;
                dtExcel.Rows.Add(row5);

                //foreach (var item in expenses)
                //{
                //    var row6 = dtExcel.NewRow();
                //    row6["Hạng mục"] = item.Name;
                //    row6["Số tiền"] = item.Amount;

                //    dtExcel.Rows.Add(row6);
                //}

                var row7 = dtExcel.NewRow();
                row7["HẠNG MỤC"] = "KHOẢN CHI KHÁC";
                row7["SỐ TIỀN"] = amountExpense;
                dtExcel.Rows.Add(row7);

                // expenses summary
                var row8 = dtExcel.NewRow();
                row8["HẠNG MỤC"] = "TỔNG CHI";
                row8["SỐ TIỀN"] = amountPurchase + amountTransport + amountExpense;

                dtExcel.Rows.Add(row8);

                // profit summary
                var row9 = dtExcel.NewRow();
                row9["HẠNG MỤC"] = "LỢI NHUẬN";
                row9["SỐ TIỀN"] = amountGoldSandRevenue + amountFillindSandRevenue - amountPurchase - amountTransport - amountExpense;
                dtExcel.Rows.Add(row9);

                var grid = new GridView();
                grid.DataSource = dtExcel;
                grid.DataBind();

                // Export excel
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=BaoCao_TaiChinh_" + DateTime.Now.ToString("yyyyMMdd") + ".xls");
                Response.ContentType = "application/ms-excel";

                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                htw.WriteLine("BÁO CÁO TÀI CHÍNH");
                htw.WriteBreak();
                htw.WriteLine("Từ ngày: " + dateFrom);
                htw.WriteBreak();
                htw.WriteLine("Đến ngày: " + dateTo);
                htw.WriteBreak();

                grid.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

                // Write action logs
                var actionLogData =
                    "  d1=" + dateFrom +
                    ", d2=" + dateTo;
                ActionLog.WriteLog(ActionLog.EXPORT_EXCEL_FINANCE_REPORT, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

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
