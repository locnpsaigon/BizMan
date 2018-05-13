using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.DAL
{
    public class ActionLog
    {
        public static string LOGIN = "Đăng nhập";
        public static string LOGOUT = "Đăng xuất";

        public static string VIEW_EXPENSE_LIST = "Xem danh mục chi phí";
        public static string ADD_EXPENSE_INFO = "Thêm chi phí";
        public static string EDIT_EXPENSE_INFO = "Cập nhật chi phí";
        public static string DELETE_EXPENSE_INFO = "Xóa chi phí";
        public static string EXPORT_EXCEL_EXPENSES_LIST = "Xuất excel danh sách khoản chi";

        public static string VIEW_ROLE_LIST = "Xem danh mục vai trò";
        public static string ADD_ROLE_INFO = "Thêm vai trò";
        public static string EDIT_ROLE_INFO = "Cập nhật vai trò";
        public static string DELETE_ROLE_INFO = "Xóa vai trò";

        public static string VIEW_USER_LIST = "Xem danh mục tài khoản";
        public static string ADD_USER_INFO = "Thêm tài khoản";
        public static string EDIT_USER_INFO = "Cập nhật tài khoản";
        public static string DELETE_USER_INFO = "Xóa tài khoản";
        public static string CHANGE_PASSWORD = "Đổi mật khẩu";

        public static string VIEW_SAND_LIST = "Xem danh mục cát";
        public static string ADD_SAND_INFO = "Thêm loại cát";
        public static string EDIT_SAND_INFO = "Cập nhật loại cát";
        public static string DELETE_SAND_INFO = "Xóa loại cát";

        public static string VIEW_BOAT_LIST = "Xem danh mục ghe";
        public static string ADD_BOAT_INFO = "Thêm ghe";
        public static string EDIT_BOAT_INFO = "Cập nhật ghe";
        public static string DELETE_BOAT_INFO = "Xóa ghe";

        public static string VIEW_BARGE_LIST = "Xem danh mục sà lan";
        public static string ADD_BARGE_INFO = "Thêm sà lan";
        public static string EDIT_BARGE_INFO = "Cập nhật sà lan";
        public static string DELETE_BARGE_INFO = "Xóa sà lan";

        public static string VIEW_CUSTOMER_LIST = "Xem danh mục khách hàng";
        public static string ADD_CUSTOMER_INFO = "Thêm khách hàng";
        public static string EDIT_CUSTOMER_INFO = "Cập nhật khách hàng";
        public static string DELETE_CUSTOMER_INFO = "Xóa khách hàng";

        public static string VIEW_ORDER_LIST = "Xem danh sách chuyến";
        public static string ADD_ORDER_INFO = "Thêm chuyến";
        public static string EDIT_ORDER_INFO = "Cập nhật chuyến";
        public static string DELETE_ORDER_INFO = "Xóa chuyến";
        public static string EXPORT_EXCEL_ORDER_REPORT = "Xuất excel số liệu chuyến";

        public static string VIEW_BOAT_REPORT = "Xem số liệu ghe";
        public static string EXPORT_EXCEL_BOAT_REPORT = "Xuất excel số liệu ghe";

        public static string VIEW_FINANCE_REPORT = "Xem báo cáo tài chính";
        public static string EXPORT_EXCEL_FINANCE_REPORT = "Xuất excel báo cáo tài chính";

        [Key]
        public int ActionLogId { get; set; }

        public DateTime ActionDate{ get; set; }

        public string ActionName { get; set; }

        public string ActionData { get; set; }

        public string UserName { get; set; }

        public string IP { get; set; }

        public static void WriteLog(string actionName, string actionData, string userName, string ip) {

            try
            {
                using (DataContext db = new DataContext())
                {
                    ActionLog log = new ActionLog();
                    log.ActionDate = DateTime.Now;
                    log.ActionName = actionName;
                    log.ActionData = actionData;
                    log.UserName = userName;
                    log.IP = ip;

                    db.ActionLogs.Add(log);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
    }
}