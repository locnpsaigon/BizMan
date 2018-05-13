using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BizMan.Models;
using BizMan.DAL;
using BizMan.Helpers.Security;
using Newtonsoft.Json;

namespace BizMan.Controllers
{
    public class AuthController : BaseController
    {

        DataContext db = new DataContext();

        public ActionResult Login()
        {
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            var actionLogData = "";

            try
            {
                if (ModelState.IsValid)
                {
                    // get user info
                    var userLogin = db.Users.Where(u => String.Compare(u.Username, model.UserName, true) == 0).FirstOrDefault();
                    if (userLogin != null)
                    {
                        // verify user password
                        var loginSuccess = SaltedHash.Verify(userLogin.Salt, userLogin.Password, model.Password);
                        if (loginSuccess)
                        {
                            BizManPrincipalSerialize principal = new BizManPrincipalSerialize();
                            principal.UserId = userLogin.UserId;
                            principal.FirstName = userLogin.FirstName;
                            principal.LastName = userLogin.LastName;
                            principal.CreationDate = userLogin.CreateDate;
                            principal.Roles = userLogin.Roles.Select(r => r.RoleName).ToArray();

                            string jsonPrincipal = JsonConvert.SerializeObject(principal);
                            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                                1,
                                userLogin.Username,
                                DateTime.Now,
                                DateTime.Now.AddDays(7),
                                model.RememberMe,
                                jsonPrincipal);

                            string ticketEncrypted = FormsAuthentication.Encrypt(ticket);

                            HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, ticketEncrypted);

                            Response.Cookies.Add(faCookie);

                            // Write action log
                            actionLogData = "user:" + model.UserName + ", success";
                            ActionLog.WriteLog(ActionLog.LOGIN, actionLogData, userLogin.Username, Request.ServerVariables["REMOTE_ADDR"]);

                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu!");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            // Write action log
            actionLogData = "user:" + model.UserName + ", fail";
            ActionLog.WriteLog(ActionLog.LOGIN, actionLogData, model.UserName, Request.ServerVariables["REMOTE_ADDR"]);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Write action log
                var actionLogData = "user:" + User.Identity.Name;
                ActionLog.WriteLog("logout", actionLogData, User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"]);
            }

            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}
