using AuthSystem.Manager;
using AuthSystem.Models;
using AuthSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Reflection;
using System.IO;
using AuthSystem.Data;
using AuthSystem.Data.Class;
using Newtonsoft.Json;
using AuthSystem.ViewModel;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Web.Http.Results;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Components.Forms;
using static AuthSystem.Data.Controller.ApiAuditTrailController;
using MimeKit;
using static API.Data.Controller.ApiCorporateListingController;
using MailKit.Net.Smtp;

namespace AuthSystem.Data.Controller
{

    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiNotifcationController : ControllerBase
    {
        GlobalVariables gv = new GlobalVariables();
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;
        DBMethods dbmet = new DBMethods();


        public ApiNotifcationController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {
   
            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;
   
        }

       // for CMS
        [HttpPost]
        public async Task<IActionResult> InsertNotifications(NotificationInsertModel data)
        {
            var result = new Registerstats();
            try
            {
                string sql0 = "";
                string itemid = "";
                string modulename = data.Module == "Vendor" ? "Vendor" : "Offering";
                if(modulename== "Offering")
                {
                    sql0 = $@"SELECT TOP (1) OfferingID FROM tbl_OfferingModel order by id desc";
                    DataTable dt2 = db.SelectDb(sql0).Tables[0];
                    itemid = dt2.Rows[0]["OfferingID"].ToString();

                }
                else
                {
                    sql0 = $@"SELECT TOP (1) VendorID FROM tbl_VendorModel order by id desc";
                    DataTable dt2 = db.SelectDb(sql0).Tables[0];
                    itemid = dt2.Rows[0]["VendorID"].ToString();
                }
             


                string sql1 = $@"SELECT EmployeeID  FROM UsersModel WHERE active=1";
                    DataTable table = db.SelectDb(sql1).Tables[0];
                foreach (DataRow dr in table.Rows)
                {
                    string sql = $@"SELECT  * from UsersModel where EmployeeID='" + dr["EmployeeID"].ToString() + "' WHERE active=1";
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    if (dt.Rows.Count != 0)
                    {
                        string Insert = $@"insert into tbl_NotificationModel (EmployeeID,Details,isRead,Module,ItemID,EmailStatus,DateCreated) values
                        ('" + dr["EmployeeID"].ToString() + "','" + data.Details + "','" + data.isRead + "','" + modulename + "','" + itemid + "','" + data.EmailStatus + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "') ";
                        db.AUIDB_WithParam(Insert);
                    }
                
                    else
                    {
                        result.Status = "Error";
                        return BadRequest(result);

                    }
            }
                    result.Status = "New Notifications Added";
                    return Ok(result);
              
            }

            catch (Exception ex)
            {
                return BadRequest(result);
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetNotifEmpId(NotifId data)
        {
            string sql = "";
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string yesterday = DateTime.Now.AddDays(-1).ToString();
            if (data.day != 0)
            {
                if (data.day == 1)
                {
                    sql = $@"SELECT        Id, EmployeeID, Details, isRead,DateCreated,Module,ItemID,EmailStatus
                            FROM            tbl_NotificationModel
                            WHERE        (EmployeeID = '" + data.EmployeeID + "') and CONVERT(DATE,DateCreated) = CONVERT(DATE,'" + today + "')  and tbl_NotificationModel.isRead = 0  order by id desc";
                }
                else if (data.day == -1)
                {
                    sql = $@"SELECT        Id, EmployeeID, Details, isRead,DateCreated,Module,ItemID,EmailStatus
                            FROM            tbl_NotificationModel
                            WHERE        (EmployeeID = '" + data.EmployeeID + "') and CONVERT(DATE,DateCreated) = CONVERT(DATE,'" + yesterday + "')  and tbl_NotificationModel.isRead = 0  order by id desc";
                }
            }
            else if (data.startdate != null)
            {
                sql = $@"SELECT        Id, EmployeeID, Details, isRead,DateCreated,Module,ItemID,EmailStatus
                            FROM            tbl_NotificationModel
                            WHERE        (EmployeeID = '" + data.EmployeeID + "') and CONVERT(DATE,DateCreated) between CONVERT(DATE,'" + data.startdate + "')  and CONVERT(DATE,'" + data.enddate + "') and tbl_NotificationModel.isRead = 0  order by id desc";
            }
            else
            {
                sql = $@"SELECT        Id, EmployeeID, Details, isRead,DateCreated,Module,ItemID,EmailStatus
                            FROM            tbl_NotificationModel
                            WHERE        (EmployeeID = '" + data.EmployeeID + "')   and tbl_NotificationModel.isRead = 0  order by id desc";
            }
            var result = new List<NotificationModel>();
            DataTable table = db.SelectDb(sql).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                var item = new NotificationModel();
                item.Id = dr["Id"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Details = dr["Details"].ToString();
                item.isRead = dr["isRead"].ToString();
                item.DateCreated = dr["DateCreated"].ToString();
                item.Module = dr["Module"].ToString();
                item.ItemID = dr["ItemID"].ToString();
                item.EmailStatus = dr["EmailStatus"].ToString();
                result.Add(item);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> MarkReadAll_Notification(NotifId data)
        {
            string delete = "";
            string sql = $@"SELECT Id from  tbl_NotificationModel  EmployeeID='" + data.EmployeeID + "'";
            DataTable table = db.SelectDb(sql).Tables[0];
            if (table.Rows.Count != 0)
            {
                foreach (DataRow dt in table.Rows)
                {
                    delete += $@" update tbl_NotificationModel set isRead ='1'  where EmployeeID='" + data.EmployeeID + "'";
                }
                 
                return Ok(db.AUIDB_WithParam(delete) + " Marked Read All");
            }
            else
            {
                return BadRequest("Error/Deleted Data");
            }
               
            }
        [HttpPost]
        public async Task<IActionResult> UpdateNotification(NotifIdUpdate data)
        {
        
            string sql = $@"SELECT * from  tbl_NotificationModel where id='" + data.id + "' and EmployeeID='" + data.EmployeeID + "'";
            DataTable table = db.SelectDb(sql).Tables[0];
            if (table.Rows.Count != 0)
            {
                string Insert = $@" update tbl_NotificationModel set isRead ='1'  where id='" + data.id + "' and EmployeeID='" + data.EmployeeID + "'";
                
                return Ok(db.AUIDB_WithParam(Insert) +" Read");
            }
            else
            {
                return BadRequest("Error/Deleted Data");
            }

        }
        [HttpGet]
        public async Task<IActionResult> NotificationList()
        {
            GlobalVariables gv = new GlobalVariables();
            string sql = $@"SELECT        tbl_NotificationModel.Details, tbl_NotificationModel.isRead, tbl_NotificationModel.DateCreated,Concat(UsersModel.Fname,' ', UsersModel.Lname) as Fullname, tbl_NotificationModel.Id, tbl_NotificationModel.EmployeeID, tbl_NotificationModel.Module, tbl_NotificationModel.ItemID, 
                         tbl_NotificationModel.EmailStatus
                         FROM            tbl_NotificationModel INNER JOIN
                         UsersModel ON tbl_NotificationModel.EmployeeID = UsersModel.EmployeeID order by id desc ";
            var result = new List<NotificationVM>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                string read = dr["isRead"].ToString() == "1" ? "Read" :"Unread";

                var item = new NotificationVM();
                item.Id = dr["Id"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Fullname = dr["Fullname"].ToString();
                item.Details = dr["Details"].ToString();
                item.Module = dr["Module"].ToString();
                item.ItemID = dr["ItemID"].ToString();
                item.EmailStatus = dr["EmailStatus"].ToString();
                item.isRead = read;
                item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");

                result.Add(item);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SendNotificationPerUserCorporate(CorporateNotificationEmailRequest data)
        {
            int isRead = 0;

            List<CorporateNotificationData> item = new List<CorporateNotificationData>();
            for (int x = 0; x < data.CorporateList.Length; x++)
            {
                item = dbmet.GetCompanyUserDetails(data.CorporateList[x]);
                
            }
            //Console.WriteLine(item);
            //Console.WriteLine("Hello");
            //for (int x = 0; x < item.Count; x++)
            //{
            //    string Insert = $@"insert into tbl_NotificationModel (EmployeeID,Details,isRead,Module,ItemID,EmailStatus,DateCreated) values
            //            ('" + item[x].EmployeeID + "','" + data.Body + "','" + isRead + "','" + "Company" + "','" + item[x].CompanyID + "','" + 15 + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "') ";
            //    db.AUIDB_WithParam(Insert);

            //}
            foreach (var a in item)
            {
                string Insert = $@"insert into tbl_NotificationModel (EmployeeID,Details,isRead,Module,ItemID,EmailStatus,DateCreated) values
                        ('" + a.EmployeeID + "','" + data.Body + "','" + isRead + "','" + "Company" + "','" + a.CompanyID + "','" + 15 + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "') ";
                db.AUIDB_WithParam(Insert);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendNotificationForActiveUser()
        {
            int isRead = 0;
            string body = "Dear Valued Member, \r\nWe would like to inform you that, due to exceptionally high guest occupancy at The St. Regis Doha, external access to the pool and beach facilities will be restricted on November 21st, 22nd, and 23rd.\r\n \r\nThank you for your understanding and we apologize for any inconvenience caused.";
            List<CorporateNotificationData> item = new List<CorporateNotificationData>();
            item = dbmet.GetAllActiveusers();
            foreach (var a in item)
                {
                string Insert = $@"insert into tbl_NotificationModel (EmployeeID,Details,isRead,Module,ItemID,EmailStatus,DateCreated) values
                ('" + a.EmployeeID + "','" + body + "','" + isRead + "','" + " " + "','" + " " + "','" + 15 + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "') ";
                db.AUIDB_WithParam(Insert);
                }
            
            return Ok();
        }

        #region Model

        public class CorporateNotificationEmailRequest
        {
            public string Body { get; set; }
            public string[] CorporateList { get; set; }

        }
        public class CorporateNotificationData
        {
            public string EmployeeID { get; set; }
            public string CompanyID { get; set; }

        }
        public class CorporateListModel
        {
            public string CorporateName { get; set; }
        }
        public class NotificationModel
        {

            public string? Id { get; set; }
            public string? EmployeeID { get; set; }
            public string? Details { get; set; }
            public string? isRead { get; set; }
            public string? DateCreated { get; set; }
            public string? Module { get; set; }
            public string? ItemID { get; set; }
            public string? EmailStatus { get; set; }

        }   
        public class NotificationVM
        {

            public string? Id { get; set; }
            public string? EmployeeID { get; set; }
            public string? Details { get; set; }
            public string? Fullname { get; set; }
            public string? isRead { get; set; }
            public string? DateCreated { get; set; }
            public string? Module { get; set; }
            public string? ItemID { get; set; }
            public string? EmailStatus { get; set; }

        }

        public class NotificationInsertModel
        {

            public string? Id { get; set; }
            public string? EmployeeID { get; set; }
            public string? Details { get; set; }
            public string? Module { get; set; }
            public string? ItemID { get; set; }
            public int? isRead { get; set; }
            public int? EmailStatus { get; set; }


        }
        public class NotifId
        {
            public string? EmployeeID { get; set; }
            public int day { get; set; }
            public string? startdate { get; set; }
            public string? enddate { get; set; }

        }
        public class NotifIdUpdate
        {
            public int? id { get; set; }
            public string? EmployeeID { get; set; }

        }
        public class DeleteUser
        {

            public int Id { get; set; }
        }
        public class UserID
        {

            public int Id { get; set; }
        }
        public class Registerstats
        {
            public string Status { get; set; }

        }
        #endregion
    }
}
