using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BizMan.DAL;
using BizMan.Models;
using BizMan.Helpers.Security;

namespace BizMan.Controllers
{
    public class UserController : BaseController
    {

        DataContext db = new DataContext();

        //
        // GET: /User/
        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Index()
        {
            ActionLog.WriteLog(ActionLog.VIEW_USER_LIST, "", User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);
            return View();
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Add()
        {
            try
            {
                AddUserViewModel model = new AddUserViewModel();

                ViewBag.RolesList = db.Roles.OrderBy(r => r.RoleName).ToList();

                return View(model);
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
        public ActionResult Add(AddUserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // check user existed
                    bool isUserExisted = (db.Users
                        .Where(u => String.Compare(u.Username, model.Username, true) == 0)
                        .FirstOrDefault() != null); 

                    if (!isUserExisted)
                    {
                        // create new user
                        User newUser = new User();
                        SaltedHash sh = new SaltedHash(model.Password);

                        newUser.Username = model.Username;
                        newUser.Password = sh.Hash;
                        newUser.Salt = sh.Salt;
                        newUser.FirstName = model.FirstName;
                        newUser.LastName = model.LastName;
                        newUser.Email = model.Email;
                        newUser.IsActive = model.IsActive;
                        newUser.CreateDate = DateTime.Now;
                        newUser.Roles = new List<Role>();

                        db.Users.Add(newUser);

                        foreach (var roleId in model.RolesId)
                        {
                            var selectedRole = db.Roles.Where(r => r.RoleId == roleId).FirstOrDefault();
                            if (selectedRole != null)
                            {
                                newUser.Roles.Add(selectedRole);
                            }
                        }

                        db.SaveChanges();

                        // Write action log
                        string actionLogData = "username=" + newUser.Username;
                        ActionLog.WriteLog(ActionLog.ADD_USER_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Tài khoản tên [" + model.Username + "] đã tồn tại trong hệ thống!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Thông tin tài khoản không hợp lệ!");
                }

                ViewBag.RolesList = db.Roles.OrderBy(r => r.RoleName).ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi " + ex.Message);
            }

            return View(model);
        }

        [Authorize]
        [BizManAuthorize(Roles = "Administrators")]
        public ActionResult Edit(int id)
        {
            try
            {
                // get edited user info
                var editedUser = db.Users.Where(u => u.UserId == id).FirstOrDefault();
                if (editedUser != null)
                {
                    // create view model
                    EditUserViewModel model = new EditUserViewModel();
                    model.UserId = editedUser.UserId;
                    model.Username = editedUser.Username;
                    model.Password = string.Empty;
                    model.ConfirmedPassword = string.Empty;
                    model.FirstName = editedUser.FirstName;
                    model.LastName = editedUser.LastName;
                    model.Email = editedUser.Email;
                    model.IsActive = editedUser.IsActive;
                    model.RolesId = new int[editedUser.Roles.Count];
                    for (int i = 0; i < editedUser.Roles.Count; i++)
                    {
                        model.RolesId[i] = editedUser.Roles.ElementAt(i).RoleId;
                    }

                    // put roles list into viewbag
                    ViewBag.RolesList = db.Roles.OrderBy(r => r.RoleName).ToList();

                    return View(model);
                }

                throw new Exception("Tài khoản #" + id + " không tồn tại trong hệ thống!!!");
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
        public ActionResult Edit(EditUserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // get user info to update
                    var editedUser = db.Users.Where(u => u.UserId == model.UserId).FirstOrDefault();
                    if (editedUser == null)
                    {
                        return RedirectToAction("Message", "Error",
                            new RouteValueDictionary(
                                new { message = "Không tìm thấy thông tin tài khoản" }));
                    }

                    // update new user info
                    editedUser.FirstName = model.FirstName;
                    editedUser.LastName = model.LastName;
                    editedUser.Email = model.Email;
                    editedUser.IsActive = model.IsActive;
                    if (String.IsNullOrWhiteSpace(model.Password) == false)
                    {
                        SaltedHash sh = new SaltedHash(model.Password);
                        editedUser.Salt = sh.Salt;
                        editedUser.Password = sh.Hash;
                    }

                    // clear user roles
                    editedUser.Roles.Clear();

                    // update new user roles
                    if (model.RolesId != null)
                    {
                        foreach (var roleId in model.RolesId)
                        {
                            var role = db.Roles.Where(r => r.RoleId == roleId).FirstOrDefault();
                            if (role != null)
                            {
                                editedUser.Roles.Add(role);
                            }
                        }
                    }

                    db.SaveChanges();

                    // Write action log
                    string actionLogData = "username=" + editedUser.Username;
                    ActionLog.WriteLog(ActionLog.EDIT_USER_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Thông tin cập nhật tài khoản không hợp lệ!");
                }

                // set viewbag data 
                ViewBag.RolesList = db.Roles.OrderBy(r => r.RoleName).ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [Authorize]
        public ActionResult ChangePass()
        {
            try
            {
                // get currnet login user info
                var userLogin = db.Users.Where(u => u.UserId == User.UserId).FirstOrDefault();
                if (userLogin == null) {
                    string errorMsg = string.Format("Tài khoản {0} không tồn tại!", User.UserId);
                    return RedirectToAction("Message", "Error", new RouteValueDictionary(new { message = errorMsg }));
                }

                ChangePassViewModel model = new ChangePassViewModel();
                model.UserId = userLogin.UserId;
                model.Username = userLogin.Username;
                model.FirstName = userLogin.FirstName;
                model.LastName = userLogin.LastName;
                model.Email = userLogin.Email;

                

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Message", "Error",
                    new RouteValueDictionary(new { message = ex.Message }));
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePass(ChangePassViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // get current user info
                    var userInfo = db.Users.Where(u => u.UserId == model.UserId).FirstOrDefault();
                    if (userInfo == null)
                        return RedirectToAction("Login", "Auth");

                    // verify old password
                    var isCorrectOldPass = SaltedHash.Verify(userInfo.Salt, userInfo.Password, model.OldPassword);
                    if (isCorrectOldPass)
                    {
                        // new password must be different to old passowrd
                        if (String.Compare(userInfo.Password, SaltedHash.ComputeHash(userInfo.Salt, model.NewPassword) , false) != 0)
                        {
                            // update new user password
                            SaltedHash sh = new SaltedHash(model.NewPassword);
                            userInfo.Salt = sh.Salt;
                            userInfo.Password = sh.Hash;
                            userInfo.FirstName = model.FirstName;
                            userInfo.LastName = model.LastName;
                            userInfo.Email = model.Email;

                            db.SaveChanges();

                            // write action log
                            string actionLogData = "username=" + userInfo.Username;
                            ActionLog.WriteLog(ActionLog.CHANGE_PASSWORD, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Mật khẩu mới không được trùng mật khẩu cũ");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Mật khẩu cũ chưa chính xác");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Thông tin đổi mật khẩu không hợp lệ!");
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
                // find deleted user
                var deletedUser = db.Users.Where(u => u.UserId == id).FirstOrDefault();
                if (deletedUser != null)
                {
                    if (String.Compare(deletedUser.Username, "admin", true) == 0)
                    {
                        return Json(new { Error = 1, Message = "Tài khoản " + deletedUser.Username + " là tài khoản hệ thống. Bạn không thể xóa tài khoản này." });
                    }

                    // remove user roles
                    deletedUser.Roles.Clear();

                    // delete user
                    db.Users.Remove(deletedUser);
                    db.SaveChanges();

                    // Write action log
                    string actionLogData = "username=" + deletedUser.Username;
                    ActionLog.WriteLog(ActionLog.DELETE_USER_INFO, actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);
                }

                return Json(new { Error = 0, Message = "Delete user success!!!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = 1, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetUsers()
        {
            try
            {
                // select all users
                var users = db.Users.OrderBy(u => u.Username)
                    .Select(q1 => new
                    {
                        UserId = q1.UserId,
                        UserName = q1.Username,
                        FullName = q1.FirstName + " " + q1.LastName,
                        Email = q1.Email,
                        IsActive = q1.IsActive,
                        CreationDate = q1.CreateDate
                    });

                // return users json result
                return Json(new { Error = 0, Message = "Success", Users = users });

            }
            catch (Exception ex)
            {
                // error
                return Json(new { Error = 1, Message = ex.Message });
            }
        }
    }
}
