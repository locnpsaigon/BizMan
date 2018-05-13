using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Routing;
using System.Data;
using BizMan.Models;
using BizMan.DAL;
using BizMan.Helpers.Security;

namespace BizMan.Controllers
{
    public class ExpenseController : BaseController
    {

        DataContext db = new DataContext();

        //
        // GET: /Role/


        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Index()
        {
            // Write action log
            ActionLog.WriteLog(ActionLog.VIEW_EXPENSE_LIST, "", User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);
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
        public ActionResult Add(AddExpenseViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // save expense info
                    var expense = new Expense();
                    expense.Date = DateTime.ParseExact(model.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    expense.Name = model.Name;
                    expense.Amount = model.Amount;
                    expense.Description = model.Description;
                    db.Expenses.Add(expense);
                    db.SaveChanges();

                    // write action log
                    string actionLogData = "expense_id=" + expense.ExpenseId + ", name=" + expense.Name + ", amount=" + expense.Amount;
                    ActionLog.WriteLog(ActionLog.ADD_EXPENSE_INFO, actionLogData,
                        User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Thông tin chi phí không hợp lệ.");
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
                // get edited expense
                var editedExpense = db.Expenses.Where(ex => ex.ExpenseId == id).FirstOrDefault();
                if (editedExpense != null)
                {
                    EditExpenseViewModel model = new EditExpenseViewModel();
                    model.ExpenseId = editedExpense.ExpenseId;
                    model.Date = editedExpense.Date.ToString("dd/MM/yyyy");
                    model.Name = editedExpense.Name;
                    model.Amount = editedExpense.Amount;
                    model.Description = editedExpense.Description;

                    return View(model);
                }

                return RedirectToAction("Message", "Error",
                    new RouteValueDictionary(new { message = "Không tìm thấy chi phí cần chỉnh sửa!" }));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Message", "Error",
                    new RouteValueDictionary(new { message = ex.Message }));
            }
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpPost]
        public ActionResult Edit(EditExpenseViewModel model)
        {
            try
            {
                // get edited expense
                var editedExpense = db.Expenses.Where(ex => ex.ExpenseId == model.ExpenseId).FirstOrDefault();
                if (editedExpense != null)
                {
                    // create action log
                    var actionLogData =
                        "expense_id=" + editedExpense.ExpenseId +
                        ", date=" + editedExpense.Date.ToString("dd/MM/yyyy HH:mm") +
                        ", name=" + editedExpense.Name +
                        ", amount=" + editedExpense.Amount;

                    // update expense info
                    editedExpense.Date = DateTime.ParseExact(model.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    editedExpense.Name = model.Name;
                    editedExpense.Amount = model.Amount;
                    editedExpense.Description = model.Description;
                    db.SaveChanges();

                    // write action log
                    actionLogData +=
                        ", new_date=" + editedExpense.Date.ToString("dd/MM/yyyy HH:mm") +
                        ", new_name=" + editedExpense.Name +
                        ", new_amount=" + editedExpense.Amount;
                    ActionLog.WriteLog(
                        ActionLog.EDIT_EXPENSE_INFO, actionLogData,
                        User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    return RedirectToAction("Index");
                }

                return RedirectToAction("Message", "Error",
                    new RouteValueDictionary(new { message = "Không tìm thấy chi phí cần chỉnh sửa!" }));
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
        public JsonResult Delete(int id)
        {
            try
            {
                // find deleted expense
                var deletedExpense = db.Expenses.Where(ex => ex.ExpenseId == id).FirstOrDefault();
                if (deletedExpense != null)
                {
                    db.Expenses.Remove(deletedExpense);
                    db.SaveChanges();
                }

                return Json(new { Error = 0, Message = "Xóa khoản chi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetExpenses(string dateFrom, string dateTo, string filterText = "")
        {
            try
            {
                // parse date range
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                filterText = filterText.Trim().ToLower();

                // query expenses
                IQueryable expenses;
                if (string.IsNullOrEmpty(filterText))
                {
                    expenses = from t1 in db.Expenses
                               where t1.Date >= d1 && t1.Date <= d2
                               orderby t1.Date descending
                               select t1;
                }
                else
                {
                    expenses = from t1 in db.Expenses
                               where t1.Date >= d1 && t1.Date <= d2 && t1.Name.Contains(filterText)
                               orderby t1.Date descending
                               select t1;
                }

                return Json(new { Error = 0, Message = "Success", Expenses = expenses });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpGet]
        public ActionResult ExportExcel(string dateFrom, string dateTo, string filterText = "")
        {
            try
            {
                // parse date range
                var d1 = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var d2 = DateTime.ParseExact(dateTo + " 23:59:59", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                filterText = filterText.Trim().ToLower();

                // query expenses
                IQueryable query;
                if (string.IsNullOrEmpty(filterText))
                {
                    query = from t1 in db.Expenses
                               where t1.Date >= d1 && t1.Date <= d2
                               orderby t1.Date descending
                               select t1;
                }
                else
                {
                    query = from t1 in db.Expenses
                               where t1.Date >= d1 && t1.Date <= d2 && t1.Name.Contains(filterText)
                               orderby t1.Date descending
                               select t1;
                }

                var expenses = query.Cast<Expense>().ToList();

                var dtExcel = new DataTable("Expenses");
                dtExcel.Columns.Add("Ngày", typeof(string));
                dtExcel.Columns.Add("Tên khoản chi", typeof(string));
                dtExcel.Columns.Add("Ghi chú", typeof(string));
                dtExcel.Columns.Add("Số tiền", typeof(string));

                // add lines
                var SumAmount = (double)0;
                foreach (var line in expenses)
                {
                    var row = dtExcel.NewRow();
                    row["Ngày"] = line.Date.ToString("yyyy/MM/dd");
                    row["Tên khoản chi"] = line.Name;
                    row["Ghi chú"] = line.Description;
                    row["Số tiền"] = line.Amount;
                    dtExcel.Rows.Add(row);

                    SumAmount += (double)line.Amount;
                }

                // add total line
                var row_summary = dtExcel.NewRow();
                row_summary["Ngày"] = "Tổng:";
                row_summary["Tên khoản chi"] = "";
                row_summary["Ghi chú"] = "";
                row_summary["Số tiền"] = SumAmount;
                dtExcel.Rows.Add(row_summary);

                var grid = new GridView();
                grid.DataSource = dtExcel;
                grid.DataBind();

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=BaoCao_DanhSach_KhoanChi_" + DateTime.Now.ToString("yyyyMMdd") + ".xls");
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
                    ", filter_text=" + filterText;
                ActionLog.WriteLog(ActionLog.EXPORT_EXCEL_EXPENSES_LIST, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

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
