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
    public class RoleController : BaseController
    {

        DataContext db = new DataContext();

        //
        // GET: /Role/


        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Index()
        {
            // Write action log
            ActionLog.WriteLog(ActionLog.VIEW_ROLE_LIST, "", User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

            var model = db.Roles.OrderBy(r => r.RoleName).ToList();
            return View(model);
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Add()
        {
            AddRoleViewModel model = new AddRoleViewModel();
            return View(model);
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        [HttpPost]
        public ActionResult Add(AddRoleViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("Thông tin vai trò không hợp lệ");
                }

                Role role = new Role();
                role.RoleName = model.RoleName;
                role.Description = model.Description;

                db.Roles.Add(role);
                db.SaveChanges();

                // Write action log
                string actionLogData = "roleId=" + role.RoleId + ", name=" + role.RoleName;
                ActionLog.WriteLog(ActionLog.ADD_ROLE_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                return RedirectToAction("Index");
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
                // find role info
                var role = db.Roles.Where(r => r.RoleId == id).FirstOrDefault();
                if (role != null)
                {
                    // create view model to edit role
                    EditRoleViewModel model = new EditRoleViewModel();
                    model.RoleId = role.RoleId;
                    model.RoleName = role.RoleName;
                    model.Description = role.Description;

                    return View(model);
                }
                else
                {
                    throw new Exception("Vai trò #" + id + " không tồn tại trong hệ thống!!!");
                }
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
        public ActionResult Edit(EditRoleViewModel model)
        {
            try
            {
                // find edited role
                var editedRole = db.Roles.Where(r => r.RoleId == model.RoleId).FirstOrDefault();
                if (editedRole != null)
                {
                    var oldRoleName = editedRole.RoleName;
                    // update role info
                    editedRole.RoleName = model.RoleName;
                    editedRole.Description = model.Description;

                    // save info
                    db.SaveChanges();

                    // Write action log
                    string actionLogData = "roleId" + editedRole.RoleId + ", oldRoleName:" + oldRoleName + ", newRoleName:" + editedRole.RoleName;
                    ActionLog.WriteLog(ActionLog.EDIT_ROLE_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    return RedirectToAction("Index");
                }
                else
                {
                    throw new Exception("Vai trò #" + model.RoleId  + " không tồn tại trong hệ thống!!!");
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
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                // find deleted role
                var deletedRole = db.Roles.Where(r => r.RoleId == id).FirstOrDefault();
                if (deletedRole != null)
                {
                    if (String.Compare(deletedRole.RoleName, "Administrators", true) == 0 ||
                        String.Compare(deletedRole.RoleName, "Users", true) == 0)
                    {
                        return Json(new { Error = 1, Message = "Chức danh " + deletedRole.RoleName + " là chức danh hệ thống! Bạn không thể xóa." });
                    }

                    // delete role
                    db.Roles.Remove(deletedRole);
                    db.SaveChanges();

                    // Write action log
                    string actionLogData = "roldId:" + deletedRole.RoleId + ", name:" + deletedRole.RoleName;
                    ActionLog.WriteLog(ActionLog.DELETE_ROLE_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    string message = string.Format("Xóa chức danh ({0}) thành công!!!", deletedRole.RoleName);
                    return Json(new { Error = 0, Message = message });
                }
                else
                {
                    throw new Exception("Xóa chức danh thất bại! Chức danh #" + id + " không tồn tại trong hệ thống");
                }
            }
            catch(Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetRoles()
        {
            try
            {
                // select roles from database
                var roles = db.Roles
                    .OrderBy(r => r.RoleName)
                    .Select(q1 => new
                    {
                        RoleId = q1.RoleId,
                        RoleName = q1.RoleName,
                        Description = q1.Description
                    });

                return Json(new { Error = 0, Message = "Success", Roles = roles });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

    }
}
