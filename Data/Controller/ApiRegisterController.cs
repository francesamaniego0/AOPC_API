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
using static AuthSystem.Data.Controller.ApiVendorController;
using static AuthSystem.Data.Controller.ApiRegisterController;
using System.Security.Cryptography;
using System.Net;
using API.Models;
using CMS.Models;
using Org.BouncyCastle.Asn1.Mozilla;
using static AuthSystem.Data.Controller.ApiPaginationController;
using MimeKit;
using MailKit.Net.Smtp;
using static API.Data.Controller.ApiCorporateListingController;
using static AuthSystem.Data.Controller.ApiSupportController;

namespace AuthSystem.Data.Controller
{

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiRegisterController : ControllerBase
    {
        GlobalVariables gv = new GlobalVariables();
        DBMethods dbmet = new DBMethods();
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;


        public ApiRegisterController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {
   
            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;
   
        }
        public class Emails
        {
            public string? Email { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> UserInfoList(Emails data)
        {
            var results= Cryptography.Decrypt("wU8BuRVCukbBjmyAn17X6A==");
            GlobalVariables gv = new GlobalVariables();

            var result = new List<UserVM>();
            string sql = $@"SELECT        UsersModel.Username, UsersModel.Fname, UsersModel.Lname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, tbl_PositionModel.Name AS Position, tbl_CorporateModel.CorporateName, 
                         tbl_UserTypeModel.UserType, UsersModel.Fullname, UsersModel.Id, UsersModel.DateCreated, tbl_PositionModel.Id AS PositionID, tbl_CorporateModel.Id AS CorporateID, tbl_StatusModel.Name AS status, 
                         UsersModel.AllowEmailNotif
                        FROM            UsersModel INNER JOIN
                                                 tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id INNER JOIN
                                                 tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id INNER JOIN
                                                 tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id INNER JOIN
                                                 tbl_StatusModel ON UsersModel.Active = tbl_StatusModel.Id
                        WHERE        (UsersModel.Active IN (1, 2, 9)) and Email='"+data.Email+"'";
            DataTable table = db.SelectDb(sql).Tables[0];
            var item = new UserVM();
            foreach (DataRow dr in table.Rows)
            {
               
                item.Id = int.Parse(dr["id"].ToString());
                item.Fullname = dr["Fname"].ToString()+" " + dr["Lname"].ToString();
                item.Username = dr["Username"].ToString();
                item.Fname = dr["Fname"].ToString();
                item.Lname = dr["Lname"].ToString();
                item.Email = dr["Email"].ToString();
                item.Gender = dr["Gender"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Position = dr["Position"].ToString();
                item.Corporatename = dr["Corporatename"].ToString();
                item.UserType = dr["UserType"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.CorporateID = dr["CorporateID"].ToString();
                item.PositionID = dr["PositionID"].ToString();
                item.status = dr["status"].ToString();
                item.AllowNotif = dr["AllowEmailNotif"].ToString();

            }

            return Ok(item);
        }
        //[HttpPost]
        //public async Task<IActionResult> Corporatelist(paginateCorpUserv2 data)
        //{
        //    string status = "ACTIVE";
        //    int pageSize = 10;
        //    //var model_result = (dynamic)null;
        //    var items = (dynamic)null;
        //    int totalItems = 0;
        //    int totalVIP = 0;
        //    int totalPages = 0;
        //    string page_size = pageSize == 0 ? "10" : pageSize.ToString();
        //    try
        //    {

        //        var Member = dbmet.GetCorporateList(data).ToList();
        //        totalItems = Member.Count;
        //        totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));
        //        items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();



        //        var result = new List<PaginationCorpUserModel>();
        //        var item = new PaginationCorpUserModel();
        //        int pages = data.page == 0 ? 1 : data.page;
        //        item.CurrentPage = data.page == 0 ? "1" : data.page.ToString();

        //        int page_prev = pages - 1;
        //        //int t_record = int.Parse(items.Count.ToString()) / int.Parse(page_size);

        //        double t_records = Math.Ceiling(double.Parse(totalItems.ToString()) / double.Parse(page_size));
        //        int page_next = data.page >= t_records ? 0 : pages + 1;
        //        item.NextPage = items.Count % int.Parse(page_size) >= 0 ? page_next.ToString() : "0";
        //        item.PrevPage = pages == 1 ? "0" : page_prev.ToString();
        //        item.TotalPage = t_records.ToString();
        //        item.PageSize = page_size;
        //        item.TotalRecord = totalItems.ToString();
        //        item.TotalVIP = totalVIP.ToString();
        //        item.items = items;
        //        result.Add(item);
        //        return Ok(result);


        //    }

        //    catch (Exception ex)
        //    {
        //        return BadRequest("ERROR");
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> Corporatelist(paginateCorpUserv2 data)
        {
            GlobalVariables gv = new GlobalVariables();
            string sql = $@"SELECT        UsersModel.Username, UsersModel.Fname, UsersModel.Lname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, tbl_PositionModel.Name AS Position, tbl_CorporateModel.CorporateName, 
                 tbl_UserTypeModel.UserType, UsersModel.Fullname, UsersModel.Id, UsersModel.DateCreated, tbl_PositionModel.Id AS PositionID, tbl_CorporateModel.Id AS CorporateID, tbl_StatusModel.Name AS status, UsersModel.isVIP, 
                 UsersModel.FilePath
                FROM            UsersModel INNER JOIN
                tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id LEFT OUTER JOIN
                tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id LEFT OUTER JOIN
                tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id LEFT OUTER JOIN
                tbl_StatusModel ON UsersModel.Active = tbl_StatusModel.Id
                WHERE        (UsersModel.Active IN (1, 2, 9, 10)) AND (UsersModel.Type = 2)";

                //order by UsersModel.Id Desc";
                if (data.CorpId != null)
                {
                    sql += " AND CorporateID = " + data.CorpId;
                }
                if (data.PosId != null)
                {
                    sql += " AND tbl_PositionModel.Id = " + data.PosId;
                }
                if (data.Gender != null)
                {
                    sql += " AND UsersModel.Gender = '" + data.Gender + "'";
                }
                if (data.isVIP != null)
                {
                    sql += " AND UsersModel.isVIP = " + data.isVIP;
                }
                if (data.Status != null)
                {
                    sql += " AND tbl_StatusModel.Name = '" + data.Status + "'";
                }
                if (data.FilterName != null)
                {
                    sql += " AND (UsersModel.Fname like '%" + data.FilterName + "%' or UsersModel.Lname like '%" + data.FilterName + "%' or tbl_CorporateModel.CorporateName like '%" + data.FilterName + "%')";
                }
                sql += " order by UsersModel.Id desc";

            var result = new List<UserVM>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new UserVM();
                item.Id = int.Parse(dr["id"].ToString());
                item.Fullname = dr["Fname"].ToString() + " " + dr["Lname"].ToString();
                item.Username = dr["Username"].ToString();
                item.Fname = dr["Fname"].ToString();
                item.Lname = dr["Lname"].ToString();
                item.Email = dr["Email"].ToString();
                item.Gender = dr["Gender"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Position = dr["Position"].ToString();
                item.Corporatename = dr["Corporatename"].ToString();
                item.UserType = dr["UserType"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.CorporateID = dr["CorporateID"].ToString();
                item.PositionID = dr["PositionID"].ToString();
                item.status = dr["status"].ToString();
                item.FilePath = dr["FilePath"].ToString();
                item.FilePath = dr["FilePath"].ToString();
                item.isVIP = dr["isVIP"].ToString();
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> UserAllist()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = $@"SELECT        UsersModel.Username, UsersModel.Fname, UsersModel.Lname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, tbl_PositionModel.Name AS Position, tbl_CorporateModel.CorporateName, 
                         tbl_UserTypeModel.UserType, UsersModel.Fullname, UsersModel.Id, UsersModel.DateCreated, tbl_PositionModel.Id AS PositionID, tbl_CorporateModel.Id AS CorporateID, tbl_StatusModel.Name AS status, UsersModel.isVIP, 
                         UsersModel.FilePath
FROM            UsersModel LEFT OUTER JOIN
                         tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id LEFT OUTER JOIN
                         tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id LEFT OUTER JOIN
                         tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id LEFT OUTER JOIN
                         tbl_StatusModel ON UsersModel.Active = tbl_StatusModel.Id
WHERE        (UsersModel.Active IN (1, 2, 9,10)) AND (UsersModel.Type = 3) order by UsersModel.Id desc";
            var result = new List<UserVM>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new UserVM();
                item.Id = int.Parse(dr["id"].ToString());
                item.Fullname = dr["Fname"].ToString() + " " + dr["Lname"].ToString();
                item.Username = dr["Username"].ToString();
                item.Fname = dr["Fname"].ToString();
                item.Lname = dr["Lname"].ToString();
                item.Email = dr["Email"].ToString();
                item.Gender = dr["Gender"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Position = dr["Position"].ToString();
                item.Corporatename = dr["Corporatename"].ToString();
                item.UserType = dr["UserType"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.CorporateID = dr["CorporateID"].ToString();
                item.PositionID = dr["PositionID"].ToString();
                item.status = dr["status"].ToString();
                item.FilePath = dr["FilePath"].ToString();
                item.isVIP = dr["isVIP"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        public class CorporateID
        {
            public string ID { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> CorporateAdminUserList(CorporateID data)
        {
         

            string sql = $@"SELECT        UsersModel.Username, UsersModel.Fname, UsersModel.Lname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, tbl_PositionModel.Name AS Position, tbl_CorporateModel.CorporateName, 
                         tbl_UserTypeModel.UserType, UsersModel.Fullname, UsersModel.Id, UsersModel.DateCreated, tbl_PositionModel.Id AS PositionID, tbl_CorporateModel.Id AS CorporateID, tbl_StatusModel.Name AS status, UsersModel.isVIP, 
                         UsersModel.FilePath
                         FROM            UsersModel LEFT OUTER JOIN
                         tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id LEFT OUTER JOIN
                         tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id LEFT OUTER JOIN
                         tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id LEFT OUTER JOIN
                         tbl_StatusModel ON UsersModel.Active = tbl_StatusModel.Id
                         WHERE        (UsersModel.Active IN (1, 2, 9, 10)) AND (UsersModel.Type = 3) AND (UsersModel.CorporateID = '"+data.ID+"') " +
                         "order by UsersModel.Id desc";
            var result = new List<UserVM>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new UserVM();
                item.Id = int.Parse(dr["id"].ToString());
                item.Fullname = dr["Fname"].ToString() + " " + dr["Lname"].ToString();
                item.Username = dr["Username"].ToString();
                item.Fname = dr["Fname"].ToString();
                item.Lname = dr["Lname"].ToString();
                item.Email = dr["Email"].ToString();
                item.Gender = dr["Gender"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Position = dr["Position"].ToString();
                item.Corporatename = dr["Corporatename"].ToString();
                item.UserType = dr["UserType"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.CorporateID = dr["CorporateID"].ToString();
                item.PositionID = dr["PositionID"].ToString();
                item.status = dr["status"].ToString();
                item.FilePath = dr["FilePath"].ToString();
                item.isVIP = dr["isVIP"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        public class CorporateModelId
        {
            public string Id { get; set; }
            public string CorpId { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> CorporateAdminUserFilderById(CorporateModelId data)
        {


            string sql = $@"SELECT        UsersModel.Username, UsersModel.Fname, UsersModel.Lname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, tbl_PositionModel.Name AS Position, tbl_CorporateModel.CorporateName, 
                         tbl_UserTypeModel.UserType, UsersModel.Fullname, UsersModel.Id, UsersModel.DateCreated, tbl_PositionModel.Id AS PositionID, tbl_CorporateModel.Id AS CorporateID, tbl_StatusModel.Name AS status, UsersModel.isVIP, 
                         UsersModel.FilePath,tbl_CorporateModel.MembershipID,UsersModel.AllowEmailNotif
                         FROM            UsersModel LEFT OUTER JOIN
                         tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id LEFT OUTER JOIN
                         tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id LEFT OUTER JOIN
                         tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id LEFT OUTER JOIN
                         tbl_StatusModel ON UsersModel.Active = tbl_StatusModel.Id
                         WHERE        (UsersModel.Active IN (1, 2, 9, 10)) AND (UsersModel.Type = 3) and  UsersModel.Id ='" +data.Id+"' AND (UsersModel.CorporateID = '" + data.CorpId + "') " +
                         "order by UsersModel.Id desc";
            var result = new List<UserVM>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new UserVM();
                item.Id = int.Parse(dr["id"].ToString());
                item.Fullname = dr["Fname"].ToString() + " " + dr["Lname"].ToString();
                item.Username = dr["Username"].ToString();
                item.Fname = dr["Fname"].ToString();
                item.Lname = dr["Lname"].ToString();
                item.Email = dr["Email"].ToString();
                item.Gender = dr["Gender"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Position = dr["Position"].ToString();
                item.Corporatename = dr["Corporatename"].ToString();
                item.UserType = dr["UserType"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.CorporateID = dr["CorporateID"].ToString();
                item.PositionID = dr["PositionID"].ToString();
                item.status = dr["status"].ToString();
                item.FilePath = dr["FilePath"].ToString();
                item.isVIP = dr["isVIP"].ToString();
                item.MembershipID = dr["MembershipID"].ToString();
                item.AllowNotif = dr["AllowEmailNotif"].ToString() == "" ? "0" : dr["AllowEmailNotif"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CorporateAdminUserFilterById(CorporateModelId data)
        {


            string sql = $@"SELECT        UsersModel.Username, UsersModel.Fname, UsersModel.Lname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, tbl_PositionModel.Name AS Position, tbl_CorporateModel.CorporateName, 
                         tbl_UserTypeModel.UserType, UsersModel.Fullname, UsersModel.Id, UsersModel.DateCreated, tbl_PositionModel.Id AS PositionID, tbl_CorporateModel.Id AS CorporateID, tbl_StatusModel.Name AS status, UsersModel.isVIP, 
                         UsersModel.FilePath,tbl_CorporateModel.MembershipID,UsersModel.AllowEmailNotif
                         FROM            UsersModel LEFT OUTER JOIN
                         tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id LEFT OUTER JOIN
                         tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id LEFT OUTER JOIN
                         tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id LEFT OUTER JOIN
                         tbl_StatusModel ON UsersModel.Active = tbl_StatusModel.Id
                         WHERE        (UsersModel.Active IN (1, 2, 9, 10)) AND (UsersModel.Type = 2) and  UsersModel.Id ='" + data.Id + "' AND (UsersModel.CorporateID = '" + data.CorpId + "') " +
                         "order by UsersModel.Id desc";
            var result = new List<UserVM>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new UserVM();
                item.Id = int.Parse(dr["id"].ToString());
                item.Fullname = dr["Fname"].ToString() + " " + dr["Lname"].ToString();
                item.Username = dr["Username"].ToString();
                item.Fname = dr["Fname"].ToString();
                item.Lname = dr["Lname"].ToString();
                item.Email = dr["Email"].ToString();
                item.Gender = dr["Gender"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Position = dr["Position"].ToString();
                item.Corporatename = dr["Corporatename"].ToString();
                item.UserType = dr["UserType"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.CorporateID = dr["CorporateID"].ToString();
                item.PositionID = dr["PositionID"].ToString();
                item.status = dr["status"].ToString();
                item.FilePath = dr["FilePath"].ToString();
                item.isVIP = dr["isVIP"].ToString();
                item.MembershipID = dr["MembershipID"].ToString();
                item.AllowNotif = dr["AllowEmailNotif"].ToString() == "" ? "0" : dr["AllowEmailNotif"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> AdminList()
        {
            GlobalVariables gv = new GlobalVariables();

   
            string sql = $@"SELECT        UsersModel.Username, UsersModel.Fname, UsersModel.Lname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, tbl_PositionModel.Name AS Position, tbl_CorporateModel.CorporateName, 
                         tbl_UserTypeModel.UserType, UsersModel.Fullname, UsersModel.Id, UsersModel.DateCreated, tbl_PositionModel.Id AS PositionID, tbl_CorporateModel.Id AS CorporateID, tbl_StatusModel.Name AS status, UsersModel.isVIP,    UsersModel.FilePath
                        FROM            UsersModel INNER JOIN
                                                 tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id INNER JOIN
                                                 tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id INNER JOIN
                                                 tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id INNER JOIN
                                                 tbl_StatusModel ON UsersModel.Active = tbl_StatusModel.Id
                        WHERE        (UsersModel.Active IN (1, 2, 9,10)) and Type=1 order by UsersModel.Id desc";
            var result = new List<UserVM>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new UserVM();
                item.Id = int.Parse(dr["id"].ToString());
                item.Fullname = dr["Fname"].ToString() + " " + dr["Lname"].ToString();
                item.Username = dr["Username"].ToString();
                item.Fname = dr["Fname"].ToString();
                item.Lname = dr["Lname"].ToString();
                item.Email = dr["Email"].ToString();
                item.Gender = dr["Gender"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Position = dr["Position"].ToString();
                item.Corporatename = dr["Corporatename"].ToString();
                item.UserType = dr["UserType"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.CorporateID = dr["CorporateID"].ToString();
                item.PositionID = dr["PositionID"].ToString();
                item.status = dr["status"].ToString();
                item.FilePath = dr["FilePath"].ToString();
                item.isVIP = dr["isVIP"].ToString();
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> UserInfoFilteredbyJWToken()
        {
            GlobalVariables gv = new GlobalVariables();

            var result = new List<UserVM>();
            DataTable table = db.SelectDb_SP("SP_UserInfo").Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new UserVM();
                item.Id = int.Parse(dr["id"].ToString());
                item.Fullname = dr["Fname"].ToString() + " " + dr["Lname"].ToString();
                item.Username = dr["Username"].ToString();
                item.Fname = dr["Fname"].ToString();
                item.Lname = dr["Lname"].ToString();
                item.Email = dr["Email"].ToString();
                item.Gender = dr["Gender"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Position = dr["Position"].ToString();
                item.Corporatename = dr["Corporatename"].ToString();
                item.UserType = dr["UserType"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.CorporateID = dr["CorporateID"].ToString();
                item.PositionID = dr["PositionID"].ToString();
          

                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> PositionList()
        {
            string sql = $@"SELECT        tbl_StatusModel.Name AS Status, tbl_PositionModel.Id, tbl_PositionModel.Name AS PositionName, tbl_PositionModel.Description, tbl_PositionModel.DateCreated, tbl_PositionModel.PositionID
                            FROM            tbl_PositionModel INNER JOIN
                                                     tbl_StatusModel ON tbl_PositionModel.Status = tbl_StatusModel.Id
                            WHERE        (tbl_PositionModel.Status = 5)
                            ORDER BY tbl_PositionModel.Name asc";
            var result = new List<PositionModel>();
            DataTable table = db.SelectDb(sql).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                var item = new PositionModel();
                item.Id = int.Parse(dr["Id"].ToString());
                item.PositionName = dr["PositionName"].ToString();
                item.PositionID = dr["PositionID"].ToString();
                item.Description = dr["Description"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("yyyy-MM-dd");
                item.Status = dr["Status"].ToString();
                result.Add(item);

            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> UserTypeList()
        {
            string sql = $@"SELECT    tbl_UserTypeModel.Id, tbl_UserTypeModel.UserType, tbl_UserTypeModel.Description, tbl_UserTypeModel.DateCreated, tbl_StatusModel.Name as Status
                    FROM            tbl_UserTypeModel INNER JOIN
                                             tbl_StatusModel ON tbl_UserTypeModel.Status = tbl_StatusModel.Id
                    WHERE        (tbl_UserTypeModel.Status = 5)";
            var result = new List<UserTypeVM>();
            DataTable table = db.SelectDb(sql).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                var item = new UserTypeVM();
                item.Id = dr["Id"].ToString();
                item.UserType = dr["UserType"].ToString();
                item.Description = dr["Description"].ToString();
                item.Status = dr["Status"].ToString();
                result.Add(item);

            }

                return Ok(result);
        }
        //[HttpPost]
        //public async  Task<IActionResult> SaveUserInfo(UsersModel data)
        //{
        //    try
        //    {
        //        string result = "";
        //        GlobalVariables gv = new GlobalVariables();
        //        _global.Token = _global.GenerateToken(data.Username, _appSettings.Key.ToString());
        //        UsersModel item  = new UsersModel();

        //        _global.Status = gv.EmployeeRegistration(data, _global.Token, _context);
        //    }

        //    catch (Exception ex)
        //    {
        //        string status = ex.GetBaseException().ToString();
        //    }
        //     return Content(_global.Status);
        //}
        [HttpPost]
        public async Task<IActionResult> SendOTP(RegistrationOTPModel data)
        {
            try
            {
                var model = new RegistrationOTPModel()
                {
                    Email = data.Email,
                    OTP = data.OTP,
                    Status = 10,

                };
                _context.tbl_RegistrationOTPModel.Add(model);
                _context.SaveChanges();
                _global.Status = "OTP SENT.";
            }

            catch (Exception ex)
            {
                string status = ex.GetBaseException().ToString();
            }
            return Ok(_global.Status);
        }
        [HttpPost]
        public async Task<IActionResult> VerifyOTP(RegistrationOTPModel data)
        {
            var result = new Registerstats();
            try
            {
                string query = "";
          
                string sql = $@"select * from tbl_registrationOTPModel where OTP = '"+data.OTP+ "' and  email ='"+data.Email+"' AND Status in (9,10)";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if(dt.Rows.Count > 0)
                {
                    query += $@"update tbl_RegistrationOTPModel set status = 11 where  email ='" + data.Email + "' and OTP = '" + data.OTP+"'";
                    //-- if email exist - update OTP column--<soon
                    query += $@"update  UsersModel set  Active=1  where Active = 10 and LOWER(Email) ='" + data.Email + "' ";
                    db.AUIDB_WithParam(query);

                    result.Status = "OTP Matched!";
                    return Ok(result);
                }
                else
                {
                   query = $@"update tbl_RegistrationOTPModel set status = 10 where  email ='" + data.Email + "'";
                    db.AUIDB_WithParam(query);
                    result.Status = "OTP UnMatched!";
                    return BadRequest(result);
                }
            }

            catch (Exception ex)
            {
                result.Status = "OTP UnMatched!";
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ResendOTP(RegistrationOTPModel data)
        {
            var result = new OTP();
            try
            {
                string query = "";

                string sql = $@"select * from tbl_registrationOTPModel where email ='" + data.Email + "' AND Status=10";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    query += $@"update tbl_RegistrationOTPModel set otp = '"+data.OTP+"' where  email ='" + data.Email + "' ";
                    db.AUIDB_WithParam(query);

                    result.otp = data.OTP;
                    return Ok(result);
                }
                else
                {
                    result.otp = "Error";
                    return BadRequest(result);
                }
            }

            catch (Exception ex)
            {
                result.otp = "Error";
                return BadRequest(result);
            }
            return Ok(result);
        }
  
        [HttpPost]
        public IActionResult FinalUserRegistration(UserModel data)
        {
            
           

                string sql = $@"SELECT        UsersModel.Id, UsersModel.Username, UsersModel.Password, UsersModel.Fullname, UsersModel.Active, tbl_UserTypeModel.UserType, tbl_CorporateModel.CorporateName, tbl_PositionModel.Name, UsersModel.JWToken
                        FROM            UsersModel INNER JOIN
                                                 tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id INNER JOIN
                                                 tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id INNER JOIN
                                                 tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id
                        WHERE     (UsersModel.Active in(9,10, 2) and LOWER(Email) ='" + data.Email.ToLower()+"' and LOWER(Fname) ='"+data.Fname.ToLower()+"' and LOWER(Lname) ='"+data.Lname.ToLower()+"')";
                DataTable dt = db.SelectDb(sql).Tables[0];
                var result = new Registerstats();
                if (dt.Rows.Count > 0)
                 {
                    string EncryptPword = Cryptography.Encrypt(data.Password);

                    string query = $@"update  UsersModel set Username='"+data.Username+"',Password='"+EncryptPword+"', Fname='"+data.Fname+"',Lname='"+data.Lname+"',cno='"+data.Cno+"', Active=10 , Address ='"+data.Address+"' where  Id='" + dt.Rows[0]["Id"].ToString() +"' ";
                    db.AUIDB_WithParam(query);
                string message = "Welcome to the Alfardan Oyster Privilege Club Application to confirm your registration. Here’s your one-time password " + data.OTP + ".. Please do not share.";
                //string message = "Welcome to Alfardan Oyster Privilege Application to confirm your registration here's your one time password " + data.OTP + ". Please do not share.";
                string username = "Carlo26378";
                string password = "d35HV7kqQ8Hsf24";
                string sid = "Oyster Club";
                string type = "N";

                var url = "https://api.smscountry.com/SMSCwebservice_bulk.aspx?User="+ username + "&passwd="+password+"&mobilenumber="+data.Cno+"&message="+message+"&sid="+sid+"&mtype="+type+"";
                //    string response = url;

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //optional
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream stream = response.GetResponseStream();
                ////gv.AudittrailLogIn("Successfully Registered", "User Registration Form",data.Id.ToString(),7);
                string OTPInsert = $@"insert into tbl_RegistrationOTPModel (email,OTP,status) values ('"+data.Email+"','"+data.OTP+"','10')";
                    db.AUIDB_WithParam(OTPInsert);
                    result.Status = "Waiting for Verification";

                    return Ok(result);
                    
                }
                else
                {
                //gv.AudittrailLogIn("Failed Registration", "User Registration Form", data.Id.ToString(), 8);

                result.Status = "Invalid Registration";
              
                return BadRequest(result);
            
                }
            

            return Ok(result);
        }     
        [HttpPost]
        public IActionResult FinalUserRegistration2(UsersModel data)
        {
            
           

                string sql = $@"SELECT        UsersModel.Id, UsersModel.Username, UsersModel.Password, UsersModel.Fullname, UsersModel.Active, tbl_UserTypeModel.UserType, tbl_CorporateModel.CorporateName, tbl_PositionModel.Name, UsersModel.JWToken
                        FROM            UsersModel INNER JOIN
                                                 tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id INNER JOIN
                                                 tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id INNER JOIN
                                                 tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id
                        WHERE     (UsersModel.Active in(9,10, 2) and LOWER(Email) ='" + data.Email.ToLower()+"' and LOWER(Fname) ='"+data.Fname.ToLower()+"' and LOWER(Lname) ='"+data.Lname.ToLower()+"')";
                DataTable dt = db.SelectDb(sql).Tables[0];
                var result = new Registerstats();
                if (dt.Rows.Count > 0)
                 {
                    string EncryptPword = Cryptography.Encrypt(data.Password);

                    string query = $@"update  UsersModel set Username='"+data.Username+"',Password='"+EncryptPword+"', Fname='"+data.Fname+"',Lname='"+data.Lname+"',cno='"+data.Cno+"', Active=10 , Address ='"+data.Address+"' where  Id='" + dt.Rows[0]["Id"].ToString() +"' ";
                    db.AUIDB_WithParam(query);
                int otp = 0;
                StringBuilder builder = new StringBuilder();
                Random rnd = new Random();
                for (int j = 0; j < 6; j++)
                {
                    otp = rnd.Next(10);
                    builder.Append(otp);//returns random integers < 10
                }
                string OTPInsert = $@"insert into tbl_RegistrationOTPModel (email,OTP,status) values ('"+data.Email+"','"+ builder + "','10')";
                    db.AUIDB_WithParam(OTPInsert);
                    result.Status = "Waiting for Verification";

                    return Ok(result);
                    
                }
                else
                {
                //gv.AudittrailLogIn("Failed Registration", "User Registration Form", data.Id.ToString(), 8);

                result.Status = "Invalid Registration";
              
                return BadRequest(result);
            
                }
            

            return Ok(result);
        }
        public class UpdatePassword
        {
            public string? EmployeeID { get; set; }
            public string? Fname { get; set; }
            public string? Lname { get; set; }
            public string  Address { get; set; }
            public string? Email { get; set; }
            public string  Cno { get; set; }

        }
        [HttpPost]
        public IActionResult UpdateUserInformation(UpdatePassword data)
        {


            string sql = $@"select * from usersmodel where EmployeeID='"+data.EmployeeID+"' ";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            if (dt.Rows.Count > 0)
            {
                string query = $@"update  UsersModel set Fname='" + data.Fname + "',Lname='" + data.Lname + "'" +
                    ",cno='" + data.Cno + "', Address ='" + data.Address + "' , Email='"+data.Email+"' " +
                    "where  EmployeeID='"+data.EmployeeID+"'";
                db.AUIDB_WithParam(query);
                result.Status = "User Information Updated";

                return Ok(result);

            }
            else
            {
                result.Status = "Invalid User Information";

                return BadRequest(result);

            }


            return Ok(result);
        }
        public class ProfileImge
        {
            public string FilePath { get; set; }
            public string EmployeeID { get; set; }

        }
        [HttpPost]
        public IActionResult UpdateProfileImg(ProfileImge data)
        {


            string sql = $@"select * from usersmodel where EmployeeID='" + data.EmployeeID + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var uploadimage = "https://www.alfardanoysterprivilegeclub.com/assets/img/"+data.FilePath;
            var result = new Registerstats();
            if (dt.Rows.Count > 0)
            {
                string query = $@"update  UsersModel set FilePath='" + uploadimage + "' where  EmployeeID='" + data.EmployeeID + "'";
                db.AUIDB_WithParam(query);
                result.Status = "User Profile Updated";
                return Ok(result);

            }
            else
            {
                result.Status = "Error";

                return BadRequest(result);

            }


            return Ok(result);
        }
        public class Registerstats
        {
            public string Status { get; set; }

        }
        public class OTP
        {
            public string otp { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> Import(List<UserModel> list)
        {
            string result = "";
            string query = "";
            try
            {

                for (int i = 0; i < list.Count; i++)
                {
                    string sql = $@"select * from usersmodel where EmployeeID='" + list[i].EmployeeID + "' and Active in(1,2) and CorporateID='" + list[i].CorporateID +"'";
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    if (dt.Rows.Count == 0)
                    {
                        StringBuilder str_build = new StringBuilder();
                        Random random = new Random();
                        int length = 8;
                        char letter;

                        for (int x = 0; x < length; x++)
                        {
                            double flt = random.NextDouble();
                            int shift = Convert.ToInt32(Math.Floor(25 * flt));
                            letter = Convert.ToChar(shift + 2);
                            str_build.Append(letter);
                        }

                        var token = Cryptography.Encrypt(str_build.ToString());
                        string strtokenresult = token;
                        string[] charsToRemove = new string[] { "/", ",", ".", ";", "'", "=", "+" };
                        foreach (var c in charsToRemove)
                        {
                            strtokenresult = strtokenresult.Replace(c, string.Empty);
                        }
                        string filepath = "";
                        if (list[i].FilePath == null || list[i].FilePath == "")
                        {
                            filepath = "https://www.alfardanoysterprivilegeclub.com/assets/img/defaultavatar.png";
                        }
                        else
                        {
                            filepath = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + list[i].FilePath.Replace(" ", "%20");
                        }
                        string EncryptPword = Cryptography.Encrypt(list[i].Password);
                        list[i].Fname = list[i].Fname.Replace("'", "''");
                        list[i].Lname = list[i].Lname.Replace("'", "''");
                        var fullname = list[i].Fname + " " + list[i].Lname;
                       query += $@"insert into UsersModel (Username,Password,Fullname,Fname,Lname,Email,Gender,CorporateID,PositionID,JWToken,FilePath,Active,Cno,isVIP,Address,Type,EmployeeID,DateCreated) values
                         ('" + list[i].Username + "','','" + fullname + "','" + list[i].Fname + "','" + list[i].Lname + "','" + list[i].Email + "','" + list[i].Gender + "','" + list[i].CorporateID + "','" + list[i].PositionID + "','" + string.Concat(strtokenresult.TakeLast(15)) + "','" + filepath + "','2','" + list[i].Cno + "','" + list[i].isVIP + "','N/A','" + list[i].Type + "','" + list[i].EmployeeID + "','"+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"')";
                      
                        gv.AudittrailLogIn("User Registration Form", "Successfully Import User Pre-Registration " + list[i].EmployeeID.ToString(),  list[i].EmployeeID.ToString(), 7);
               
                        _global.Status = "Successfully Saved.";
                    }
                    else
                    {
                        gv.AudittrailLogIn("User Registration Form","Duplicate Entry. " + list[i].EmployeeID.ToString(), list[i].EmployeeID.ToString(), 8);
                        _global.Status = "' "+ list[i].Username+ " '"+" has Duplicate Entry.";
                    }

                }
                db.AUIDB_WithParam(query);
                result = "Registered Successfully";


            }
            catch (Exception ex)
            {
                _global.Status = ex.GetBaseException().ToString();

            }

            return Content(_global.Status);
        }
        [HttpPost]
        public IActionResult SavePosition(PositionModel data)
        {


            string result = "";
            string query = "";
            try
            {

                if (data.PositionName.Length != 0 || data.Description.Length != 0)
                {
                
                    if (data.Id == 0)
                    {
                        string sql = $@"select * from tbl_PositionModel where Name='" + data.PositionName + "'";
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        if (dt.Rows.Count == 0)
                        {
                            query += $@"insert into tbl_PositionModel (Name,Description,Status,DateCreated) values ('" +data.PositionName+"','"+data.Description+"','5','"+DateTime.Now.ToString("yyyy-MM-dd")+"')"  ;
                            db.AUIDB_WithParam(query);
                            result = "Inserted Successfully";
                            return Ok(result);

                        }
                        else
                        {
                            result = "Error! Position already exists.";
                            return BadRequest(result);
                        }
                    }
                    else
                    {
                        query += $@"update  tbl_PositionModel set Name='" + data.PositionName + "' , Description='" + data.Description + "' , Status='5'  where  Id='" + data.Id + "' ";
                        db.AUIDB_WithParam(query);

                        result = "Updated Successfully";
                        return BadRequest(result);
                    }


                }
                else
                {
                    result = "Error in Registration";
                    return BadRequest(result);
                }
                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        public class DeletePos
        {

            public int Id { get; set; }
        }
        [HttpPost]
        public IActionResult DeletePosition(DeletePos data)
        {

            string sql = $@"select * from tbl_PositionModel where id ='" + data.Id + "' ";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            if (dt.Rows.Count != 0)
            {
                string sql1 = $@"select * from UsersModel where PositionID ='" + data.Id + "' and Status <> 6";
                DataTable dt1 = db.SelectDb(sql1).Tables[0];
                if (dt1.Rows.Count == 0)
                {
                    string OTPInsert = $@"update tbl_PositionModel set Status = 6 where id ='" + data.Id + "'";
                    db.AUIDB_WithParam(OTPInsert);
                    result.Status = "Succesfully deleted";

                    return Ok(result);
                }
                else
                {
                    result.Status = "Position is Already in Used!";

                    return BadRequest(result);

                }
       

            }
            else
            {
                result.Status = "Error";

                return BadRequest(result);

            }


            return Ok(result);
        }
        public class StatusResult
        {
            public string Status { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> UpdateUserInfo(UserModel data)
        {
            string result="";
            try
            {

                if (data.EmployeeID.Length != 0 || data.Fname.Length != 0 || data.Lname.Length != 0 || data.Email.Length != null || data.EmployeeID.Length != null ||
                    data.Email.Length != 0)
                {


                    StringBuilder str_build = new StringBuilder();
                    Random random = new Random();
                    int length = 8;
                    char letter;

                    for (int i = 0; i < length; i++)
                    {
                        double flt = random.NextDouble();
                        int shift = Convert.ToInt32(Math.Floor(25 * flt));
                        letter = Convert.ToChar(shift + 2);
                        str_build.Append(letter);
                    }

                    var token = Cryptography.Encrypt(str_build.ToString());
                    string strtokenresult = token;
                    string[] charsToRemove = new string[] { "/", ",", ".", ";", "'", "=", "+" };
                    foreach (var c in charsToRemove)
                    {
                        strtokenresult = strtokenresult.Replace(c, string.Empty);
                    }
                    string query = "";
                    string fullname = data.Fname + " " + data.Lname;
                    string filepath = "";
                    //if (data.FilePath == null)
                    //{
                    //    filepath = "https://www.alfardanoysterprivilegeclub.com/assets/img/defaultavatar.png";
                    //}
                    //else
                    //{
                    //    filepath = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + data.FilePath;
                    //}

                    if (data.FilePath == null)
                    {
                        filepath = "https://www.alfardanoysterprivilegeclub.com/assets/img/defaultavatar.png";
                    }
                    else
                    {
                        filepath = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + data.FilePath.Replace(" ", "%20"); ;
                        //FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + res_image +".jpg";
                    }
                    if (data.Id == 0)
                    {
                        string sql1 = $@"select * from usersmodel where Username ='" + data.Username + "' and Active <> 6 ";
                        DataTable dt1 = db.SelectDb(sql1).Tables[0];
                        string sql = $@"select * from usersmodel where EmployeeID='" + data.EmployeeID + "' and Active <> 6  and  CorporateID ='"+data.CorporateID+ "' and Email='"+data.Email+"' and Username ='"+data.Username+"'";
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        string sqlUserCount = $@"SELECT 
	                                                COUNT(CASE WHEN um.active = 1 THEN um.id END) AS userCount,
	                                                cm.count
                                                FROM tbl_CorporateModel cm
                                                left join UsersModel um
                                                on cm.id = um.CorporateID
 
                                                where cm.Id = '" + data.CorporateID + "' group by cm.count";
                        DataTable dtUserCount = db.SelectDb(sqlUserCount).Tables[0];
                        var userCorpCount = 0;
                        var userCurrentCorpCount = 0;
                        foreach (DataRow dr in dtUserCount.Rows)
                        {
                            userCorpCount = int.Parse(dr["count"].ToString());
                            userCurrentCorpCount = int.Parse(dr["userCount"].ToString());
                        }
                        if (dt1.Rows.Count != 0)
                        {
                            result = "User Information Already Used!";
                        }
                        else if (userCurrentCorpCount >= userCorpCount)
                        {
                            result = "User registration limit reached, no additional users can be registered at this time!";
                        }
                        else if (dt.Rows.Count == 0)
                        {

                            query += $@"insert into UsersModel (Username,Fullname,Fname,Lname,Email,Gender,CorporateID,PositionID,JWToken,FilePath,Active,Cno,isVIP,Address,Type,EmployeeID,DateCreated) values
                                     ('" + data.Username + "','" + fullname + "','" + data.Fname + "','" + data.Lname + "','" + data.Email + "','" + data.Gender + "','" + data.CorporateID + "','" + data.PositionID + "','" + string.Concat(strtokenresult.TakeLast(15)) + "','" + filepath + "','" + data.Active + "','" + data.Cno + "','" + data.isVIP + "','N/A','" + data.Type + "','" + data.EmployeeID + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "')";
                            db.AUIDB_WithParam(query);

                            string getlastinserted = $@"select Top(1) * from UsersModel order by id desc";
                            DataTable dt2 = db.SelectDb(getlastinserted).Tables[0];
                            if (dt2.Rows.Count > 0)
                            {
                                string getid = dt2.Rows[0]["Id"].ToString();

                                string sqlmembership = $@"SELECT     tbl_CorporateModel.MembershipID, tbl_MembershipModel.Name, tbl_MembershipModel.Id AS MembershipID, tbl_CorporateModel.CorporateName, tbl_CorporateModel.Id AS CorporateID, 
                                                  tbl_MembershipModel.DateEnded
                                                  FROM            tbl_CorporateModel INNER JOIN
                                                  tbl_MembershipModel ON tbl_CorporateModel.MembershipID = tbl_MembershipModel.Id
                                                WHERE        (tbl_CorporateModel.Id = '" + data.CorporateID + "')";
                                DataTable dt3 = db.SelectDb(sqlmembership).Tables[0];
                                if (dt3.Rows.Count > 0)
                                {
                                    {

                                        string sqlprivilege = $@"select * from tbl_MembershipPrivilegeModel where MembershipID = '" + dt3.Rows[0]["MembershipID"].ToString() + "'";
                                        DataTable dt4 = db.SelectDb(sqlprivilege).Tables[0];
                                        if (dt3.Rows.Count > 0)
                                        {
                                            foreach (DataRow dr4 in dt4.Rows)
                                            {
                                                //item.Id = int.Parse(dr["id"].ToString())
                                                string insertuserprivilege = $@"insert into tbl_UserPrivilegeModel (PrivilegeId,UserID,Validity) values ('" + dr4["PrivilegeID"].ToString() + "','" + getid + "','" + dt3.Rows[0]["DateEnded"].ToString() + "')";
                                                db.AUIDB_WithParam(insertuserprivilege);
                                            }

                                            string insertusermembership = $@"insert into tbl_UserMembershipModel (UserID,MembershipID,Validity) values ('" + getid + "','" + dt3.Rows[0]["MembershipID"].ToString() + "','" + dt3.Rows[0]["DateEnded"].ToString() + "')";
                                            db.AUIDB_WithParam(insertusermembership);
                                            //dt.Rows[0]["MembershipName"].ToString();
                                        }
                                    }
                                    gv.AudittrailLogIn("User Registration Form", "Registered New User "+ data.EmployeeID, data.EmployeeID, 7);
                               
                                    result = "Registered Successfully";
                                }
                            }
                            else
                            {
                                string username = $@"select from UsersModel where Username ='" + data.Username + "' and Status <> 6";
                                DataTable username_dt = db.SelectDb(username).Tables[0];
                                string email = $@"select from UsersModel where Email ='" + data.Email + "' and Status <> 6";
                                DataTable dt_email = db.SelectDb(email).Tables[0]; 
                                string empid = $@"select from UsersModel where Email ='" + data.Email + "' and Status <> 6";
                                DataTable dt_empid = db.SelectDb(empid).Tables[0];
                                if (username_dt.Rows.Count !=0)
                                {
                                    result = "Username is Already Used and Active!";
                                }
                                else if (dt_email.Rows.Count != 0)
                                {
                                    result = "Email is Already Used and Active!";
                                }
                                else if(dt_empid.Rows.Count != 0)
                                {
                                    result = "EmployeeID is Already Used and Active!";
                                }

             
                            }
                        }
                    }
                    else
                    {
                        string password = "";
                        if (data.Type == 1)
                        {
                            password = Cryptography.Encrypt(data.Password);
                            query += $@"update  UsersModel set Password='" + password + "', Fname='" + data.Fname + "',Lname='" + data.Lname + "',Username='" + data.Username + "'" +
                        ",cno='" + data.Cno + "' , Email='" + data.Email + "' , CorporateID='" + data.CorporateID + "' ,isVIP='" + data.isVIP + "', PositionID='" + data.PositionID + "'" +
                        ", Type='" + data.Type + "'  , Gender='" + data.Gender + "', FilePath='" + filepath + "' , EmployeeID='" + data.EmployeeID + "' " +
                        "where  Id='" + data.Id + "' ";
                            db.AUIDB_WithParam(query);

                            gv.AudittrailLogIn("User Registration Form", "Registered Updated User Information " + data.EmployeeID, data.EmployeeID, 7);
                        }
                        else
                        {
                            password = "";
                            query += $@"update  UsersModel set Fname='" + data.Fname + "',Lname='" + data.Lname + "',Username='" + data.Username + "'" +
                              ",cno='" + data.Cno + "' , Email='" + data.Email + "' , CorporateID='" + data.CorporateID + "' ,isVIP='" + data.isVIP + "', PositionID='" + data.PositionID + "'" +
                              ", Type='" + data.Type + "'  , Gender='" + data.Gender + "', FilePath='" + filepath + "' , EmployeeID='" + data.EmployeeID + "' " +
                              "where  Id='" + data.Id + "' ";
                            db.AUIDB_WithParam(query);

                            gv.AudittrailLogIn("User Registration Form", "Registered Updated User Information " + data.EmployeeID, data.EmployeeID, 7);
                        }
                       


                        result = "Updated Successfully";
                    }


                    }
                else
                {
                    result = "Error in Registration";
                }
                return Ok(result);
                
            }

            catch (Exception ex)
            {
                return BadRequest(result);
            }
       
        }
        public class CoporateVIP
        {
            public int CorporateID { get; set; }
            public string? Fullname { get; set; }
            public string? AdminEmail { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> EmailRemainingVIP(CoporateVIP data)
        {
            //Console.Write(data.Name.Count());
            string sql = "";
            string status = "";
            //sql = $@"select Count(*) as count from UsersModel where active=1";
            int remainingVip = 0;
            string corp = "";
            sql = $@"SELECT
	                cm.CorporateName AS 'CorporateName'
	                ,(cm.VipCount - sum(case when um.Active = '1' AND isVIP = 1 then 1 else 0 end)) AS 'RemainingVIPCount'
                    FROM 
	                    tbl_CorporateModel cm WITH (NOLOCK)
                    LEFT JOIN
	                    UsersModel um WITH(NOLOCK)
	                    ON um.CorporateID = cm.Id 
                    WHERE cm.Status = '1' and cm.id = '" +data.CorporateID + "'GROUP BY cm.CorporateName, cm.VipCount, cm.Count,cm.Id";
            DataTable dt = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                remainingVip = int.Parse(dr["RemainingVIPCount"].ToString());
                corp = dr["CorporateName"].ToString();
            }
            Console.WriteLine();
            if(remainingVip == 0)
            {
                string body = "Remaining VIP";
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("ALFARDAN OYSTER PRIVILEGE CLUB", "app@alfardan.com.qa"));
                //for (int x = 0; x < data.Name.Length; x++)
                //{
                //    message.To.Add(new MailboxAddress(data.Name[x], data.Email[x]));
                //}
                message.To.Add(new MailboxAddress(data.Fullname, data.AdminEmail));
                message.Subject = "Remaining VIP";
                var bodyBuilder = new BodyBuilder();

                    bodyBuilder.HtmlBody = @" <body>
                                                    <div class='container-holder' style='font-size:16px;font-family:Helvetica,sans-serif;margin:0;padding:100px 0;line-height:1.3;background-image:url(https://www.alfardanoysterprivilegeclub.com/build/assets/black-cover-pattern-f558a9d0.jpg);background-repeat:no-repeat;background-size:cover;display: flex;justify-content:center;align-items:center;'>
                                                        <div class='container' style='font-size:16px;font-family:Helvetica,sans-serif;background-color:white;margin: 30%;border-radius:15px;padding:24px;box-sizing:border-box;'>
                                                        <div class='logo-holder' style='justify-content: center;'>
                                                                <img style='margin-left: 25%' src='https://cms.alfardanoysterprivilegeclub.com/img/AOPCBlack.jpg' alt='Alfardan Oyster Privilege Club' width='50%' />
                                                                </div>
                                                                    </br>
                                                                <p style='font-family: Helvetica, sans-serif; font-size: 16px; font-weight: normal; margin: 0; margin-bottom: 16px;'>Dear "+data.Fullname+",</p>"
                                                                + "<p style='font-family: Helvetica, sans-serif; font-size: 16px; font-weight: normal; margin: 0; margin-bottom: 16px;'>I hope this message finds you well.</br></br>"
                                                                    + "Please be informed that the allocated VIP count for your company has been fully utilized. As a result, no additional VIP registrations can be processed at this time.</p>"
                                                              + "<p style='font-family: Helvetica, sans-serif; font-size: 16px; font-weight: normal; margin: 0; margin-bottom: 16px;'>If you have any issues or need further assistance, please contact our support team at <a href='mailto:afpmarketing@alfardan.com.qa'>afpmarketing@alfardan.com.qa</a>.</p> "
                                                              + "<p style='font-family: Helvetica, sans-serif; font-size: 16px; font-weight: normal; margin: 0; margin-bottom: 16px;'>Thank you!</p> "
                                                         
                                                        + "</div> "
                                                  + "</div> "
                                                + "</body> "; 
                message.Body = bodyBuilder.ToMessageBody();
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("app@alfardan.com.qa", "0!S+Er-@Pp");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                status = "error";
            }
            else
            {
                status = "success";
            }

            return Ok(status);
        }
        public class ChangePW
        {

            public string Email { get; set; }
            public string Password { get; set; }

        }
        public class DeleteUser
        {

            public int Id { get; set; }
        }  
        public class DeleteUserbEMp
        {

            public string EmployeeID { get; set; }
        }
        public class UserID
        {

            public int Id { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePW data)
        {
            var result = new Registerstats();
            try
            {
                string sql = $@"select * from UsersModel where Email ='" + data.Email + "' AND Active=1";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    string EncryptPword = Cryptography.Encrypt(data.Password);
                    string query = $@"update  UsersModel set Password='" + EncryptPword + "' where Active = 1 and Email ='" + data.Email + "'";
                    db.AUIDB_WithParam(query);
                    result.Status = "Password Successfuly Updated";
                    return Ok(result);
                }
                else
                {
                    result.Status = "Error!";
                    return BadRequest(result);

                }
            }

            catch (Exception ex)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUserStatus(UserID data)
        {

            string sql = $@"select * from usersmodel where Id='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            if (dt.Rows.Count > 0)
            {
                string query = $@"update  UsersModel set Active='9' where  Id='" + data.Id + "'";
                db.AUIDB_WithParam(query);
                result.Status = "Successfully Updated";
                return Ok(result);

            }
            else
            {
                result.Status = "Error";

                return BadRequest(result);

            }


            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUserInfo(DeleteUser data)
        {

            string sql = $@"select * from usersmodel where Id='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            if (dt.Rows.Count > 0)
            {
                string query = $@"update  UsersModel set Active='6' where  Id='"+ data.Id + "'";
                db.AUIDB_WithParam(query);
                result.Status = "Successfully Deleted";
                return Ok(result);

            }
            else
            {
                result.Status = "Error";

                return BadRequest(result);

            }


            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUserInfobyEmpId(DeleteUserbEMp data)
        {

            string sql = $@"select * from usersmodel where EmpoyeeID='" + data.EmployeeID + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            if (dt.Rows.Count > 0)
            {
                string query = $@"update  UsersModel set Active='6' where  EmpoyeeID='" + data.EmployeeID + "'";
                db.AUIDB_WithParam(query);
                result.Status = "Successfully Deleted";
                return Ok(result);

            }
            else
            {
                result.Status = "Error";

                return BadRequest(result);

            }


            return Ok(result);
        }

        [HttpPost]
        public string UploadImage([FromForm] IFormFile file, [FromForm] string empid)
        {
            try
            {

                string sql = $@"select * from usersmodel where EmployeeID='" + empid + "'";
                DataTable dt = db.SelectDb(sql).Tables[0];
                var result = new Registerstats();
                if (dt.Rows.Count > 0)
                {
                    //var filePath = "C:\\Files\\";
                    var filePath = "C:\\inetpub\\AOPCAPP\\public\\assets\\img\\";
                    // getting file original name
                    string FileName = file.FileName;
                    string getextension = Path.GetExtension(FileName);
                    string MyUserDetailsIWantToAdd = dt.Rows[0]["EmployeeID"].ToString() + getextension;
                    string files = Path.Combine(filePath, FileName);

                    //var stream = new FileStream(files, FileMode.Create);
                

                    // getting full path inside wwwroot/images
                    //var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/", FileName);
                    var imagePath = Path.Combine(filePath, MyUserDetailsIWantToAdd);
                    //var imagePath = Path.Combine("C:\\inetpub\\AOPCAPP\\public\\assets\\img\\", uniqueFileName);

                    // copying file
                    //file.CopyTo(new FileStream(imagePath, FileMode.Create));
                    using (FileStream streams = new FileStream(Path.Combine(filePath, MyUserDetailsIWantToAdd), FileMode.Create))
                    {
                        file.CopyTo(streams);
                    }
                    string filepath = "";
                    if (file == null)
                    {
                        filepath = "https://www.alfardanoysterprivilegeclub.com/assets/img/defaultavatar.png";
                    }
                    else
                    {
                        filepath = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + MyUserDetailsIWantToAdd.Replace(" ", "%20"); ;
                    }
                    string query = $@"update  UsersModel set FilePath='" + filepath + "' where  EmployeeID='" + empid + "'";
                    db.AUIDB_WithParam(query);
                    return "File Uploaded Successfully";
                }
                else
                {
                    return "Error!";
                }
   

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public class FamilyMemberModelreq
        {
            public int Id { get; set; }
            public string Fullname { get; set; }
            public string Relationship { get; set; }
            public int FamilyUserId { get; set; }
            public string ApplicationStatus { get; set; }
            public int Status { get; set; }
            public string? DateCreated { get; set; }
        }
        [HttpPost]
        public async Task<ActionResult<FamilyMemberModel>> SaveFamilyMember(FamilyMemberModelreq famMember)
        {
            //famMember.DateCreated = DateTime.Today;
            //DateTime dateTime = DateTime.Parse(date);
            //
            var item = new FamilyMemberModel();
            item.Fullname = famMember.Fullname;
            item.Relationship = famMember.Relationship;
            item.FamilyUserId = famMember.FamilyUserId;
            item.ApplicationStatus = famMember.ApplicationStatus;
            item.Status = famMember.Status;
            item.DateCreated = DateTime.Parse(famMember.DateCreated);

            if (_context.tbl_FamilyMember == null)
            {
                return Problem("Entity set '_context.tbl_FamilyMember'  is null.");
            }
            bool hasDuplicateOnSave = (_context.tbl_FamilyMember?.Any(familyMember => familyMember.Fullname == famMember.Fullname && familyMember.FamilyUserId == famMember.FamilyUserId)).GetValueOrDefault();


            if (hasDuplicateOnSave)
            {
                return Conflict("Entity already exists");
            }

            try
            {
                _context.tbl_FamilyMember.Add(item);
                await _context.SaveChangesAsync();

                return CreatedAtAction("SaveFamilyMember", new { id = item.Id }, item);
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
        }
        [HttpPost]
       // public async Task<ActionResult<IEnumerable<FamilyMemberModel>>> ListFamilyMember(FamMemberRequest searchFilter)
        public async Task<ActionResult<IEnumerable<FamilyMemberModel>>> ListFamilyMemberV2(FamMemberRequestV2 searchFilter) 
        {
            try
            {
                List<FamilyMemberModel> famMemberList = await buildFamMemberSearchQueryV2(searchFilter).ToListAsync();
                //var result = buildBirthTypesPagedModel(searchFilter, famMemberList);
                //var result = List<FamilyMemberModel>();
                return Ok(famMemberList);
            }
            catch (Exception ex)
            {
                return Problem(ex.GetBaseException().ToString());
            }
        }
        public class FamMemberRequestV2
        {
            public int FamilyUserId { get; set; }
            //public int page { get; set; }
            //public int pageSize { get; set; }
        }
        private IQueryable<FamilyMemberModel> buildFamMemberSearchQueryV2(FamMemberRequestV2 searchFilter)
        {
            IQueryable<FamilyMemberModel> query = _context.tbl_FamilyMember.Where(fam => fam.Status == 1);
            if (searchFilter.FamilyUserId != 0)
                query = query.Where(fam => fam.FamilyUserId.Equals(searchFilter.FamilyUserId));

            return query;
        }



        [HttpPost]
        public async Task<ActionResult<IEnumerable<FamilyMemberpagedModel>>> ListFamilyMember(FamMemberRequest searchFilter)
        {

            try
            {
                //List<FamilyMemberModel> famMemberList = await buildFamMemberSearchQuery(searchFilter).ToListAsync();
                //var result = buildBirthTypesPagedModel(searchFilter, famMemberList);
                //return Ok(result);
                List<FamilyMemberModel> famMemberList = await buildFamMemberSearchQuery(searchFilter).ToListAsync();
                //var result = buildBirthTypesPagedModel(searchFilter, famMemberList);
                //var result = List<FamilyMemberModel>();
                return Ok(famMemberList);
            }
            catch (Exception ex)
            {
                return Problem(ex.GetBaseException().ToString());
            }
        }

        public class FamMemberRequest
        {
            public int id { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
        }
        private IQueryable<FamilyMemberModel> buildFamMemberSearchQuery(FamMemberRequest searchFilter)
        {
            //Sir CJ
            IQueryable<FamilyMemberModel> query = _context.tbl_FamilyMember.Where(fam => fam.Status == 1);
            //France Simple Adjusment (Remove Status filter)
            //IQueryable<FamilyMemberModel> query = _context.tbl_FamilyMember;
            if (searchFilter.id != 0)
                query = query.Where(fam => fam.FamilyUserId.Equals(searchFilter.id));

            return query;
        }

        private List<FamilyMemberpagedModel> buildBirthTypesPagedModel(FamMemberRequest searchFilter, List<FamilyMemberModel> FamMember)
        {
            int pagesize = searchFilter.pageSize == 0 ? 10 : searchFilter.pageSize;
            int page = searchFilter.page == 0 ? 1 : searchFilter.page;
            var items = (dynamic)null;
            int totalItems = 0;
            int totalPages = 0;

            totalItems = FamMember.Count;
            totalPages = (int)Math.Ceiling((double)totalItems / pagesize);
            items = FamMember.Skip((page - 1) * pagesize).Take(pagesize).ToList();

            var result = new List<FamilyMemberpagedModel>();
            var item = new FamilyMemberpagedModel();

            int pages = searchFilter.page == 0 ? 1 : searchFilter.page;
            item.CurrentPage = searchFilter.page == 0 ? "1" : searchFilter.page.ToString();
            int page_prev = pages - 1;

            double t_records = Math.Ceiling(Convert.ToDouble(totalItems) / Convert.ToDouble(pagesize));
            int page_next = searchFilter.page >= t_records ? 0 : pages + 1;
            item.NextPage = items.Count % pagesize >= 0 ? page_next.ToString() : "0";
            item.PrevPage = pages == 1 ? "0" : page_prev.ToString();
            item.TotalPage = t_records.ToString();
            item.PageSize = pagesize.ToString();
            item.TotalRecord = totalItems.ToString();
            item.data = FamMember;
            result.Add(item);

            return result;
        }

        //private List<FamilyMemberpagedModel> buildBirthTypesPagedModel(FamMemberRequest searchFilter, List<FamilyMemberModel> FamMember)
        //{
        //int pagesize = searchFilter.pageSize == 0 ? 10 : searchFilter.pageSize;
        //int page = searchFilter.page == 0 ? 1 : searchFilter.page;
        //var items = (dynamic)null;
        //int totalItems = 0;
        //int totalPages = 0;

        //totalItems = FamMember.Count;
        //totalPages = (int)Math.Ceiling((double)totalItems / pagesize);
        //items = FamMember.Skip((page - 1) * pagesize).Take(pagesize).ToList();

        //var result = new List<FamilyMemberpagedModel>();
        //var item = new FamilyMemberpagedModel();

        //int pages = searchFilter.page == 0 ? 1 : searchFilter.page;
        //item.CurrentPage = searchFilter.page == 0 ? "1" : searchFilter.page.ToString();
        //int page_prev = pages - 1;

        //double t_records = Math.Ceiling(Convert.ToDouble(totalItems) / Convert.ToDouble(pagesize));
        //int page_next = searchFilter.page >= t_records ? 0 : pages + 1;
        //item.NextPage = items.Count % pagesize >= 0 ? page_next.ToString() : "0";
        //item.PrevPage = pages == 1 ? "0" : page_prev.ToString();
        //item.TotalPage = t_records.ToString();
        //item.PageSize = pagesize.ToString();
        //item.TotalRecord = totalItems.ToString();
        //    item.data = FamMember;
        //    result.Add(item);

        //    return result;
        //}
        [HttpPost]
        public async Task<IActionResult> deleteFamilyMember(DeletionModel deletionModel)
        {

            if (_context.tbl_FamilyMember == null)
            {
                return Problem("Entity set '_context.tbl_FamilyMember' is null!");
            }

            var famMember = await _context.tbl_FamilyMember.FindAsync(deletionModel.id);
            if (famMember == null || famMember.Status == 0)
            {
                return Conflict("No records matched!");
            }

            try
            {
                famMember.Status = 0;
                //famMember.DateDeleted = DateTime.Now;
                _context.Entry(famMember).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok("Deletion Successful!");
            }
            catch (Exception ex)
            {
                return Problem(ex.GetBaseException().ToString());
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> update(int id, FamilyMemberModelreq famMember)
        {
            if (_context.tbl_FamilyMember == null)
            {
                return Problem("Entity set '_context.tbl_FamilyMember' is null!");
            }

            var family = _context.tbl_FamilyMember.AsNoTracking().Where(fam => fam.Status == 1 && fam.Id == id).FirstOrDefault();

            if (family == null)
            {
                return Conflict("No records matched!");
            }

            if (id != family.Id)
            {
                return Conflict("Ids mismatched!");
            }

            bool hasDuplicateOnUpdate = (_context.tbl_FamilyMember?.Any(fam => fam.Status == 1 && fam.Fullname == famMember.Fullname && fam.FamilyUserId == famMember.FamilyUserId && fam.Id != id)).GetValueOrDefault();

            // check for duplication
            if (hasDuplicateOnUpdate)
            {
                return Conflict("Entity already exists");
            }

            string sql = $@"SELECT DateCreated FROM tbl_FamilyMember WHERE ID ='"+id+"'";
            DataTable table = db.SelectDb(sql).Tables[0];
            string dateCreatedexisting = "";
            foreach (DataRow dr in table.Rows)
            {
                 dateCreatedexisting = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("yyyy-MM-dd");
            }
            var item = new FamilyMemberModel();
            item.Id = id;
            item.Fullname = famMember.Fullname;
            item.Relationship = famMember.Relationship;
            item.FamilyUserId = famMember.FamilyUserId;
            item.ApplicationStatus = famMember.ApplicationStatus;
            item.Status = famMember.Status;
            item.DateCreated = DateTime.Parse(dateCreatedexisting);
            try
            {
                _context.Entry(item).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok("Update Successful!");
            }
            catch (Exception ex)
            {

                return Problem(ex.GetBaseException().ToString());
            }
        }
        public class FamilyMemberStatus
        {

            public int Id { get; set; }
            public string Status { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> updateFamilyMemberStatus(FamilyMemberStatus data)
        {

            string sql = $@"select * from tbl_familymember where Id='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new FamilyMemberStatus();
            if (dt.Rows.Count > 0)
            {
                string query = $@"update tbl_familymember set ApplicationStatus = '" + data.Status + "' where Id ='" + data.Id + "'";

                db.AUIDB_WithParam(query);
                result.Status = "Successfully Updated";
                return Ok(result);

            }
            else
            {
                result.Status = "Error";

                return BadRequest(result);

            }


            return Ok(result);
        }
        public class DeletionModel
        {
            public int id { get; set; }
        }
    }
}
