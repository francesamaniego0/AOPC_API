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
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;
using AuthSystem.ViewModel;
using static AuthSystem.Data.Controller.ApiRegisterController;
using System.Web.Http.Results;
using MimeKit;
using MailKit.Net.Smtp;
namespace AuthSystem.Data.Controller
{
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiAuditTrailController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;
        private readonly IWebHostEnvironment _environment;

        public ApiAuditTrailController(IOptions<AppSettings> appSettings, ApplicationDbContext context, 
        JwtAuthenticationManager jwtAuthenticationManager, IWebHostEnvironment environment)
        {

            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;

        }
     
        [HttpGet]
        public async Task<IActionResult> AudittrailList()
        {
            GlobalVariables gv = new GlobalVariables();
            string sql = $@"SELECT        tbl_audittrailModel.Id, tbl_audittrailModel.Actions, tbl_audittrailModel.Module, tbl_audittrailModel.DateCreated, tbl_StatusModel.Name AS status, 
 case when UsersModel.EmployeeID is null then 'Alfardan-Admin' else UsersModel.EmployeeID end as EmployeeID , 
 case when UsersModel.Fname is null then 'Alfardan' else UsersModel.Fname end Fname, 
case when UsersModel.Lname is null then 'Administrator' else UsersModel.Lname end Lname, 
case when tbl_PositionModel.Name is null then 'System Administrator' else tbl_PositionModel.Name end AS PositionName, 
case when tbl_CorporateModel.CorporateName is null then 'Alfardan Oyster Privilege Club' else  tbl_CorporateModel.CorporateName end CorporateName, 
case when tbl_UserTypeModel.UserType is null then 'ADMIN' else tbl_UserTypeModel.UserType end UserType
                         FROM            tbl_audittrailModel LEFT OUTER JOIN
                         tbl_StatusModel ON tbl_audittrailModel.status = tbl_StatusModel.Id LEFT OUTER JOIN
                         UsersModel ON tbl_audittrailModel.EmployeeID = UsersModel.EmployeeID LEFT OUTER JOIN
                         tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id LEFT OUTER JOIN
                         tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id LEFT OUTER JOIN
                         tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id order by tbl_audittrailModel.DateCreated desc";
            var result = new List<Audittrailvm>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new Audittrailvm();
                item.Id = int.Parse(dr["id"].ToString());
                item.Actions = dr["Actions"].ToString();
                item.Module = dr["Module"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy hh:mm:ss tt");
                item.status = dr["status"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.FullName = dr["Fname"].ToString() + " " + dr["Lname"].ToString();
                item.PositionName = dr["PositionName"].ToString();
                item.CorporateName = dr["CorporateName"].ToString();
                item.UserType = dr["UserType"].ToString();
                result.Add(item);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> QRTrailList()
        {
            GlobalVariables gv = new GlobalVariables();
            string sql = $@"SELECT     tbl_QrCodeLogsModel.*,Concat( UsersModel.Fname,' ' ,UsersModel.Lname) as Fullname
                         FROM            tbl_QrCodeLogsModel INNER JOIN
                         UsersModel ON tbl_QrCodeLogsModel.EmployeeID = UsersModel.EmployeeID order by id desc";
            var result = new List<QrTrailVM>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new QrTrailVM();
                item.Id = int.Parse(dr["Id"].ToString());
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Longtitude = dr["Longtitude"].ToString();
                item.Latitude = dr["Latitude"].ToString();
                item.IPAddres = dr["IPAddres"].ToString();
                item.Region = dr["Region"].ToString();
                item.Country = dr["Country"].ToString();
                item.City = dr["City"].ToString();
                item.AreaCode = dr["AreaCode"].ToString();
                item.ZipCode = dr["ZipCode"].ToString();
                item.ISOCode = dr["ISOCode"].ToString();
                item.MetroCode = dr["MetroCode"].ToString();
                item.TimeZone = dr["TimeZone"].ToString();
                item.Fullname = dr["Fullname"].ToString();
                item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy hh:mm:ss tt");

                result.Add(item);
            }

            return Ok(result);
        }
        #region Model
        public class JWTModel
        {

            public string Key { get; set; }

        }
        public class StatusResult
        {
            public string Status { get; set; }

        }
        public class Audittrailvm
        {

            public int Id { get; set; }
            public string Actions { get; set; }
            public string Module { get; set; }
            public string DateCreated { get; set; }
            public string status { get; set; }
            public string EmployeeID { get; set; }
            public string FullName { get; set; }
            public string Fname { get; set; }
            public string Lname { get; set; }
            public string PositionName { get; set; }
            public string CorporateName { get; set; }
            public string UserType { get; set; }
        }  
        public class QrTrailVM
        {

            public int Id { get; set; }
            public string EmployeeID { get; set; }
            public string Longtitude { get; set; }
            public string Latitude { get; set; }
            public string IPAddres { get; set; }
            public string Region { get; set; }
            public string Country { get; set; }
            public string City { get; set; }
            public string AreaCode { get; set; }
            public string ZipCode { get; set; }
            public string ISOCode { get; set; }
            public string MetroCode { get; set; }
            public string TimeZone { get; set; }
            public string DateCreated { get; set; }
            public string PostalCode { get; set; }
            public string Fullname { get; set; }
        }
        #endregion

    }
}
