using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BizMan.Models;
using BizMan.DAL;
using BizMan.Helpers.Security;

namespace BizMan.Controllers
{
    public class CustomerController : BaseController
    {

        DataContext db = new DataContext();

        //
        // GET: /Customer/
        [Authorize]
        public ActionResult Index()
        {
            ActionLog.WriteLog(ActionLog.VIEW_CUSTOMER_LIST, "", User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);
            return View();
        }

        [Authorize]
        public ActionResult Add()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Add(AddCustomerViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Create new customer
                    var newCustomer = new Customer();
                    newCustomer.CustomerName = model.CustomerName;
                    newCustomer.Description = model.Description;

                    // Save customer info
                    db.Customers.Add(newCustomer);
                    db.SaveChanges();

                    // Write action log
                    string actionLogData = "customerId:" + newCustomer.CustomerId + ", customerName:" + newCustomer.CustomerName;
                    ActionLog.WriteLog(ActionLog.ADD_CUSTOMER_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Thông tin đơn vị không hợp lệ!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            try
            {
                // get customer info
                var editedCustomer = db.Customers.Where(c => c.CustomerId == id).FirstOrDefault();
                if (editedCustomer != null)
                {
                    // Create view model
                    EditCustomerViewModel model = new EditCustomerViewModel();
                    model.CustomerId = editedCustomer.CustomerId;
                    model.CustomerName = editedCustomer.CustomerName;
                    model.Description = editedCustomer.Description;

                    return View(model);
                }
                else
                {
                    return RedirectToAction("Message", "Error", 
                        new RouteValueDictionary ( new { message = "đơn vị #" + id + " không tồn tại trong hệ thống!" }));
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Message", "Error",
                    new RouteValueDictionary(new { message = ex.Message }));
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(EditCustomerViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // get customer info
                    var editedCustomer = db.Customers.Where(c => c.CustomerId == model.CustomerId).FirstOrDefault();
                    if (editedCustomer != null)
                    {
                        // update customer info
                        editedCustomer.CustomerName = model.CustomerName;
                        editedCustomer.Description = model.Description;

                        db.SaveChanges();

                        // Write action log
                        string actionLogData = "customerId:" + editedCustomer.CustomerId + ", customerName:" + editedCustomer.CustomerName;
                        ActionLog.WriteLog(ActionLog.EDIT_CUSTOMER_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Message", "Error",
                        new RouteValueDictionary(new { message = "đơn vị #" + model.CustomerId + " không tồn tại trong hệ thống!" }));
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Thông tin đơn vị không hợp lệ!");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetAllCustomers()
        {
            try
            {
                var customers = db.Customers.OrderBy(c => c.CustomerName).ToList();

                return Json(new { Error = 0, Message = "Success", Customers = customers });
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
                // get customer
                var deletedCustomer = db.Customers.Where(c => c.CustomerId == id).FirstOrDefault();
                if (deletedCustomer != null)
                {
                    var order = db.Orders.Where(o => o.CustomerId == deletedCustomer.CustomerId).FirstOrDefault();
                    if (order == null)
                    {
                        // delete customer
                        db.Customers.Remove(deletedCustomer);
                        db.SaveChanges();

                        // Write action log
                        string actionLogData = "customerId:" + deletedCustomer.CustomerId + ", customerName:" + deletedCustomer.CustomerName;
                        ActionLog.WriteLog(ActionLog.DELETE_CUSTOMER_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                        return Json(new { Error = 0, Message = "Xóa đơn vị [" + deletedCustomer.CustomerName + "] thành công!" });
                    }
                    return Json(new { Error = 1, Message = "Xóa đơn vị thất bại! Bạn không thể xóa đơn vị đã được lập thông tin chuyến." });
                }
                return Json(new { Error = 1, Message = "Xóa đơn vị thất bại! đơn vị [" + deletedCustomer.CustomerName +  "] không tồn tại trong hệ thống." });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 2, Message = "Xóa tài khoản thất bại! Lỗi " + ex.Message });
            }
        }
    }
}
