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
using System.Text;
using static AuthSystem.Data.Controller.ApiVendorController;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static AuthSystem.Data.Controller.ApiUserAcessController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.Metrics;
using System.Globalization;
using Newtonsoft.Json;

namespace AuthSystem.Data.Controller
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiSupportController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ApiUserAcessController> _logger;
        public ApiSupportController(IOptions<AppSettings> appSettings, ApplicationDbContext context, ILogger<ApiUserAcessController> logger,
        JwtAuthenticationManager jwtAuthenticationManager, IWebHostEnvironment environment)
        {

            _context = context;
            _appSettings = appSettings.Value;
            _logger = logger;
            this.jwtAuthenticationManager = jwtAuthenticationManager;

        }



        [HttpGet]
        public async Task<IActionResult> GetSupportCountList()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = "";
            sql = $@"SELECT COUNT(*) AS SuppportCnt FROM tbl_SupportModel INNER JOIN tbl_StatusModel ON tbl_SupportModel.Status = tbl_StatusModel.Id WHERE 
                         (tbl_SupportModel.Status = 14)";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<SupportModel>();
            foreach (DataRow dr in dt.Rows)
            {
                var item = new SupportModel();
                item.Supportcount = int.Parse(dr["SuppportCnt"].ToString());
                result.Add(item);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetClickCountsListAll(UserFilterDateRange data)
        {
            GlobalVariables gv = new GlobalVariables();
            string sql = "";
            if (data.enddate != null)
            {
                data.enddate = (DateTime.Parse(data.enddate).AddDays(1)).ToString();
            }
            int daysLeft = (DateTime.Now - DateTime.Now.AddYears(-1)).Days;
            int day = data.day == 1 ? daysLeft : data.day;

            if (data.startdate == null && data.day == 0)
            {
                sql = $@"SELECT Business, Count(*) as count FROM tbl_audittrailModel
                         WHERE Actions LIKE '%view%'  and Module ='news' and Business <> '' GROUP BY Business order by count desc";
            }
            else if (data.startdate != null && data.day == 0)
            {
                sql = $@"SELECT Business, Count(*) as count FROM tbl_audittrailModel
                         WHERE Actions LIKE '%view%'  and Module ='news' and Business <> '' and DateCreated between '" + data.startdate + "' and '" + data.enddate + "' GROUP BY Business order by count desc";
            }
            else if (data.day != 0 && data.startdate == null)
            {
                sql = $@"SELECT Business, Count(*) as count FROM tbl_audittrailModel
                         WHERE Actions LIKE '%view%'  and Module ='news' and Business <> '' and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE())) GROUP BY Business order by count desc";
            }
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<ClicCountModel>();
            foreach (DataRow dr in dt.Rows)
            {
                var item = new ClicCountModel();
                item.Module = dr["Business"].ToString();
                item.Count = int.Parse(dr["count"].ToString());
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetMostCickStoreList()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                         FROM         tbl_audittrailModel  WHERE Actions LIKE '%View%' and module ='Shops & Services' and tbl_audittrailModel.DateCreated >= DATEADD(day,-7, GETDATE())
                         GROUP BY    Business,Actions,Module order by count desc";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<MostClickStoreModel>();
            int total = 0;
            if (dt.Rows.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    total += int.Parse(dr["count"].ToString());
                }
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new MostClickStoreModel();
                    item.Actions = dr["Actions"].ToString();
                    item.Business = dr["Business"].ToString();
                    item.Module = dr["Module"].ToString();
                    //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                    item.count = int.Parse(dr["count"].ToString());
                    double val1 = double.Parse(dr["count"].ToString());
                    double val2 = double.Parse(total.ToString());

                    double results = val1 / val2 * 100;
                    item.Total = Math.Round(results, 2);
                    result.Add(item);
                }
            }
            else
            {
                for (int x = 0; x < 4; x++)
                {
                    var item = new MostClickStoreModel();
                    item.Actions = "No Data";
                    item.Business = "No Data";
                    item.Module = "No Data";
                    item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                    item.count = 0;
                    item.Total = 0.00;
                    result.Add(item);
                }
            }


            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetMostClickedHospitalityList()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Rooms & Suites' and tbl_audittrailModel.DateCreated >= DATEADD(day,-7, GETDATE())
                        GROUP BY    Business,Actions,Module order by count desc";
            DataTable dt = db.SelectDb(sql).Tables[0];
            int total = 0;
            var result = new List<MostClickHospitalityModel>();
            if (dt.Rows.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    total += int.Parse(dr["count"].ToString());
                }
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new MostClickHospitalityModel();
                    item.Actions = dr["Actions"].ToString();
                    item.Business = dr["Business"].ToString();
                    item.Module = dr["Module"].ToString();
                    //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                    item.count = int.Parse(dr["count"].ToString());
                    double val1 = double.Parse(dr["count"].ToString());
                    double val2 = double.Parse(total.ToString());

                    double results = Math.Abs(val1 / val2 * 100);
                    item.Total = Math.Round(results, 2);
                    result.Add(item);
                }

            }
            else
            {
                for (int x = 0; x < 4; x++)
                {
                    var item = new MostClickHospitalityModel();
                    item.Actions = "No Data";
                    item.Business = "No Data";
                    item.Module = "No Data";
                    item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                    item.count = 0;
                    item.Total = 0.00;
                    result.Add(item);
                }
            }


            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMostClickedHealthList()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Health' and tbl_audittrailModel.DateCreated >= DATEADD(day,-7, GETDATE())
                        GROUP BY    Business,Actions,Module order by count desc";
            DataTable dt = db.SelectDb(sql).Tables[0];
            int total = 0;
            var result = new List<GenericMostClickModel>();
            if (dt.Rows.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    total += int.Parse(dr["count"].ToString());
                }
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new GenericMostClickModel();
                    item.Actions = dr["Actions"].ToString();
                    item.Business = dr["Business"].ToString();
                    item.Module = dr["Module"].ToString();
                    //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                    item.count = int.Parse(dr["count"].ToString());
                    double val1 = double.Parse(dr["count"].ToString());
                    double val2 = double.Parse(total.ToString());

                    double results = Math.Abs(val1 / val2 * 100);
                    item.Total = Math.Round(results, 2);
                    result.Add(item);
                }

            }
            else
            {
                for (int x = 0; x < 4; x++)
                {
                    var item = new GenericMostClickModel();
                    item.Actions = "No Data";
                    item.Business = "No Data";
                    item.Module = "No Data";
                    item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                    item.count = 0;
                    item.Total = 0.00;
                    result.Add(item);
                }
            }


            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMostClickedWellnessList()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Wellness' and tbl_audittrailModel.DateCreated >= DATEADD(day,-7, GETDATE())
                        GROUP BY    Business,Actions,Module order by count desc";
            DataTable dt = db.SelectDb(sql).Tables[0];
            int total = 0;
            var result = new List<GenericMostClickModel>();
            if (dt.Rows.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    total += int.Parse(dr["count"].ToString());
                }
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new GenericMostClickModel();
                    item.Actions = dr["Actions"].ToString();
                    item.Business = dr["Business"].ToString();
                    item.Module = dr["Module"].ToString();
                    //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                    item.count = int.Parse(dr["count"].ToString());
                    double val1 = double.Parse(dr["count"].ToString());
                    double val2 = double.Parse(total.ToString());

                    double results = Math.Abs(val1 / val2 * 100);
                    item.Total = Math.Round(results, 2);
                    result.Add(item);
                }

            }
            else
            {
                for (int x = 0; x < 4; x++)
                {
                    var item = new GenericMostClickModel();
                    item.Actions = "No Data";
                    item.Business = "No Data";
                    item.Module = "No Data";
                    item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                    item.count = 0;
                    item.Total = 0.00;
                    result.Add(item);
                }
            }


            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMostClickedAccessToCoWorkingList()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Access to co-working spaces' and tbl_audittrailModel.DateCreated >= DATEADD(day,-7, GETDATE())
                        GROUP BY    Business,Actions,Module order by count desc";
            DataTable dt = db.SelectDb(sql).Tables[0];
            int total = 0;
            var result = new List<GenericMostClickModel>();
            if (dt.Rows.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    total += int.Parse(dr["count"].ToString());
                }
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new GenericMostClickModel();
                    item.Actions = dr["Actions"].ToString();
                    item.Business = dr["Business"].ToString();
                    item.Module = dr["Module"].ToString();
                    //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                    item.count = int.Parse(dr["count"].ToString());
                    double val1 = double.Parse(dr["count"].ToString());
                    double val2 = double.Parse(total.ToString());

                    double results = Math.Abs(val1 / val2 * 100);
                    item.Total = Math.Round(results, 2);
                    result.Add(item);
                }

            }
            else
            {
                for (int x = 0; x < 4; x++)
                {
                    var item = new GenericMostClickModel();
                    item.Actions = "No Data";
                    item.Business = "No Data";
                    item.Module = "No Data";
                    item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                    item.count = 0;
                    item.Total = 0.00;
                    result.Add(item);
                }
            }


            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMostClickRestaurantList()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Food & Beverage' and tbl_audittrailModel.DateCreated >= DATEADD(day,-7, GETDATE())
                        GROUP BY    Business,Actions,Module order by count desc";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<MostClickRestoModel>();
            if (dt.Rows.Count != 0)
            {

                int total = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    total += int.Parse(dr["count"].ToString());
                }
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new MostClickRestoModel();
                    item.Actions = dr["Actions"].ToString();
                    item.Business = dr["Business"].ToString();
                    item.Module = dr["Module"].ToString();
                    //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                    item.count = int.Parse(dr["count"].ToString());
                    double val1 = double.Parse(dr["count"].ToString());
                    double val2 = double.Parse(total.ToString());

                    double results = Math.Abs(val1 / val2 * 100);
                    item.Total = Math.Round(results, 2);
                    result.Add(item);
                }


            }
            else
            {
                for (int x = 0; x < 4; x++)
                {
                    var item = new MostClickRestoModel();
                    item.Actions = "No Data";
                    item.Business = "No Data";
                    item.Module = "No Data";
                    item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                    item.count = 0;
                    item.Total = 0.00;
                    result.Add(item);
                }

            }
            return Ok(result);

        }

        [HttpGet]
        public async Task<IActionResult> GetCallToActionsList()
        {

            string sql = $@"SELECT Category.Business 'Business',COALESCE(Category.Module,'N/A') 'Category',COALESCE(Mail.Mail,0)'Email',COALESCE(Call.Call,0) 'Call',COALESCE(Book.Book,0) 'Book' from ( SELECT business,tbl_BusinessTypeModel.BusinessTypeName 'Module' from tbl_audittrailModel left join tbl_VendorModel on business = tbl_VendorModel.VendorName left join tbl_BusinessTypeModel on tbl_BusinessTypeModel.Id = tbl_VendorModel.BusinessTypeId where business != '' or tbl_BusinessTypeModel.BusinessTypeName != NULL GROUP BY Business,tbl_BusinessTypeModel.BusinessTypeName) AS Category
            LEFT JOIN (SELECT COUNT(*) as 'Mail',Business 'mailbusiness' FROM tbl_audittrailModel WHERE (Module = 'Mail')  GROUP BY Business)Mail ON Mail.mailbusiness =   Category.Business
            LEFT JOIN (SELECT COUNT(*) as 'Call',Business 'callbusiness' FROM tbl_audittrailModel WHERE (Module = 'Call')  GROUP BY Business)Call ON Call.callbusiness =   Category.Business
            LEFT JOIN (SELECT COUNT(*) as 'Book',Business 'bookbusiness' FROM tbl_audittrailModel WHERE (Module = 'Book')  GROUP BY Business)Book ON Book.bookbusiness =   Category.Business";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<CallToActionsModel>();
            foreach (DataRow dr in dt.Rows)
            {

                string call = dr["Call"].ToString() == "" ? "0" : dr["Call"].ToString();
                string book = dr["Book"].ToString() == "" ? "0" : dr["Book"].ToString();
                string cat = dr["Category"].ToString() == "" ? "" : dr["Category"].ToString(); //== "Food & Beverage" ? "Restaurant" : dr["Category"].ToString() == "Hotel" ? "Hotel" : "";
                string mail = dr["Email"].ToString() == "" ? "0" : dr["Email"].ToString();
                var item = new CallToActionsModel();
                item.Business = dr["Business"].ToString();
                item.Category = cat;
                item.EmailCount = int.Parse(mail.ToString());
                item.CallCount = int.Parse(call.ToString());
                item.BookCount = int.Parse(book.ToString());
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> PostCallToActionsList(UserFilterCatday data)
        {
            int daysLeft = (DateTime.Now - DateTime.Now.AddYears(-1)).Days;
            int day = data.day == 1 ? daysLeft : data.day;
            if (data.enddate != null)
            {
                data.enddate = (DateTime.Parse(data.enddate).AddDays(1)).ToString();
            }
            string sql = "";
            try
            {
                if (data.startdate == null && data.category == "0" && data.day == 0)
                {
                    sql = $@"SELECT Category.Business 'Business',COALESCE(Category.Module,'N/A') 'Category',COALESCE(Mail.Mail,0)'Email',COALESCE(Call.Call,0) 'Call',COALESCE(Book.Book,0) 'Book' from ( SELECT business,tbl_BusinessTypeModel.BusinessTypeName 'Module' from tbl_audittrailModel left join tbl_VendorModel on business = tbl_VendorModel.VendorName left join tbl_BusinessTypeModel on tbl_BusinessTypeModel.Id = tbl_VendorModel.BusinessTypeId where business != '' or tbl_BusinessTypeModel.BusinessTypeName != NULL GROUP BY Business,tbl_BusinessTypeModel.BusinessTypeName) AS Category
            LEFT JOIN (SELECT COUNT(*) as 'Mail',Business 'mailbusiness' FROM tbl_audittrailModel WHERE (Module = 'Mail')  GROUP BY Business)Mail ON Mail.mailbusiness =   Category.Business
            LEFT JOIN (SELECT COUNT(*) as 'Call',Business 'callbusiness' FROM tbl_audittrailModel WHERE (Module = 'Call')  GROUP BY Business)Call ON Call.callbusiness =   Category.Business
            LEFT JOIN (SELECT COUNT(*) as 'Book',Business 'bookbusiness' FROM tbl_audittrailModel WHERE (Module = 'Book')  GROUP BY Business)Book ON Book.bookbusiness =   Category.Business";
                }
                else if (data.startdate == null && data.category != "0" && data.day == 0)
                {
                    sql = $@"SELECT Category.Business 'Business',COALESCE(Category.Module,'N/A') 'Category',COALESCE(Mail.Mail,0)'Email',COALESCE(Call.Call,0) 'Call',COALESCE(Book.Book,0) 'Book' from ( SELECT business,tbl_BusinessTypeModel.BusinessTypeName 'Module' from tbl_audittrailModel left join tbl_VendorModel on business = tbl_VendorModel.VendorName left join tbl_BusinessTypeModel on tbl_BusinessTypeModel.Id = tbl_VendorModel.BusinessTypeId where business != '' or tbl_BusinessTypeModel.BusinessTypeName != NULL  GROUP BY Business,tbl_BusinessTypeModel.BusinessTypeName) AS Category
            LEFT JOIN (SELECT COUNT(*) as 'Mail',Business 'mailbusiness' FROM tbl_audittrailModel WHERE (Module = 'Mail')  GROUP BY Business)Mail ON Mail.mailbusiness =   Category.Business
            LEFT JOIN (SELECT COUNT(*) as 'Call',Business 'callbusiness' FROM tbl_audittrailModel WHERE (Module = 'Call')  GROUP BY Business)Call ON Call.callbusiness =   Category.Business
            LEFT JOIN (SELECT COUNT(*) as 'Book',Business 'bookbusiness' FROM tbl_audittrailModel WHERE (Module = 'Book')  GROUP BY Business)Book ON Book.bookbusiness =   Category.Business
			WHERE Category.Module = '" + data.category + "'";
                }
                else if (data.day != 0 && data.category == "0")
                {
                    sql = $@"SELECT Category.Business 'Business',COALESCE(Category.Module,'N/A') 'Category',COALESCE(Mail.Mail,0)'Email',COALESCE(Call.Call,0) 'Call',COALESCE(Book.Book,0) 'Book' from ( SELECT business,tbl_BusinessTypeModel.BusinessTypeName 'Module' from tbl_audittrailModel left join tbl_VendorModel on business = tbl_VendorModel.VendorName left join tbl_BusinessTypeModel on tbl_BusinessTypeModel.Id = tbl_VendorModel.BusinessTypeId where business != '' or tbl_BusinessTypeModel.BusinessTypeName != NULL GROUP BY Business,tbl_BusinessTypeModel.BusinessTypeName) AS Category
            LEFT JOIN (SELECT COUNT(*) as 'Mail',Business 'mailbusiness' FROM tbl_audittrailModel WHERE (Module = 'Mail') and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE()))  GROUP BY Business)Mail ON Mail.mailbusiness =   Category.Business" + Environment.NewLine +
            "LEFT JOIN (SELECT COUNT(*) as 'Call',Business 'callbusiness' FROM tbl_audittrailModel WHERE (Module = 'Call') and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE()))  GROUP BY Business)Call ON Call.callbusiness =   Category.Business" + Environment.NewLine +
            "LEFT JOIN (SELECT COUNT(*) as 'Book',Business 'bookbusiness' FROM tbl_audittrailModel WHERE (Module = 'Book') and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE()))  GROUP BY Business)Book ON Book.bookbusiness =   Category.Business";
                }
                else if (data.day != 0 && data.category != "0")
                {
                    sql = $@"SELECT Category.Business 'Business',COALESCE(Category.Module,'N/A') 'Category',COALESCE(Mail.Mail,0)'Email',COALESCE(Call.Call,0) 'Call',COALESCE(Book.Book,0) 'Book' from ( SELECT business,tbl_BusinessTypeModel.BusinessTypeName 'Module' from tbl_audittrailModel left join tbl_VendorModel on business = tbl_VendorModel.VendorName left join tbl_BusinessTypeModel on tbl_BusinessTypeModel.Id = tbl_VendorModel.BusinessTypeId where business != '' or tbl_BusinessTypeModel.BusinessTypeName != NULL GROUP BY Business,tbl_BusinessTypeModel.BusinessTypeName) AS Category
            LEFT JOIN (SELECT COUNT(*) as 'Mail',Business 'mailbusiness' FROM tbl_audittrailModel WHERE (Module = 'Mail') and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE()))  GROUP BY Business)Mail ON Mail.mailbusiness =   Category.Business" + Environment.NewLine +
            "LEFT JOIN (SELECT COUNT(*) as 'Call',Business 'callbusiness' FROM tbl_audittrailModel WHERE (Module = 'Call') and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE()))  GROUP BY Business)Call ON Call.callbusiness =   Category.Business" + Environment.NewLine +
            "LEFT JOIN (SELECT COUNT(*) as 'Book',Business 'bookbusiness' FROM tbl_audittrailModel WHERE (Module = 'Book') and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE()))  GROUP BY Business)Book ON Book.bookbusiness =   Category.Business WHERE Category.Module = '" + data.category + "'";
                }
                else if (data.startdate != null && data.category == "0")
                {
                    sql = $@"SELECT Category.Business 'Business',COALESCE(Category.Module,'N/A') 'Category',COALESCE(Mail.Mail,0)'Email',COALESCE(Call.Call,0) 'Call',COALESCE(Book.Book,0) 'Book' from ( SELECT business,tbl_BusinessTypeModel.BusinessTypeName 'Module' from tbl_audittrailModel left join tbl_VendorModel on business = tbl_VendorModel.VendorName left join tbl_BusinessTypeModel on tbl_BusinessTypeModel.Id = tbl_VendorModel.BusinessTypeId where business != '' or tbl_BusinessTypeModel.BusinessTypeName != NULL GROUP BY Business,tbl_BusinessTypeModel.BusinessTypeName) AS Category
            LEFT JOIN (SELECT Count(*) AS 'Mail',Call.Business from (SELECT Business,DateCreated from tbl_audittrailModel where business != '' and module = 'mail' and DateCreated between'" + data.startdate + "' and '" + data.enddate + "') AS Call  GROUP BY Business)Mail ON Mail.Business =   Category.Business" + Environment.NewLine +
            "LEFT JOIN (SELECT Count(*) AS 'Call',Call.Business from (SELECT Business,DateCreated from tbl_audittrailModel where business != '' and module = 'call' and DateCreated between'" + data.startdate + "' and '" + data.enddate + "') AS Call  GROUP BY Business)Call ON Call.Business =   Category.Business" + Environment.NewLine +
            "LEFT JOIN (SELECT Count(*) AS 'Book',Call.Business from (SELECT Business,DateCreated from tbl_audittrailModel where business != '' and module = 'book' and DateCreated between'" + data.startdate + "' and '" + data.enddate + "') AS Call  GROUP BY Business)Book ON Book.Business =   Category.Business ";
                }
                else if (data.startdate != null && data.category != "0")
                {
                    sql = $@"SELECT Category.Business 'Business',COALESCE(Category.Module,'N/A') 'Category',COALESCE(Mail.Mail,0)'Email',COALESCE(Call.Call,0) 'Call',COALESCE(Book.Book,0) 'Book' from ( SELECT business,tbl_BusinessTypeModel.BusinessTypeName 'Module' from tbl_audittrailModel left join tbl_VendorModel on business = tbl_VendorModel.VendorName left join tbl_BusinessTypeModel on tbl_BusinessTypeModel.Id = tbl_VendorModel.BusinessTypeId where business != '' or tbl_BusinessTypeModel.BusinessTypeName != NULL GROUP BY Business,tbl_BusinessTypeModel.BusinessTypeName) AS Category
            LEFT JOIN (SELECT Count(*) AS 'Mail',Call.Business from (SELECT Business,DateCreated from tbl_audittrailModel where business != '' and module = 'mail' and DateCreated between'" + data.startdate + "' and '" + data.enddate + "') AS Call  GROUP BY Business)Mail ON Mail.Business =   Category.Business" + Environment.NewLine +
             "LEFT JOIN (SELECT Count(*) AS 'Call',Call.Business from (SELECT Business,DateCreated from tbl_audittrailModel where business != '' and module = 'call' and DateCreated between'" + data.startdate + "' and '" + data.enddate + "') AS Call  GROUP BY Business)Call ON Call.Business =   Category.Business" + Environment.NewLine +
             "LEFT JOIN (SELECT Count(*) AS 'Book',Call.Business from (SELECT Business,DateCreated from tbl_audittrailModel where business != '' and module = 'book' and DateCreated between'" + data.startdate + "' and '" + data.enddate + "') AS Call  GROUP BY Business)Book ON Book.Business =   Category.Business WHERE Category.Module = '" + data.category + "'";
                }

                DataTable dt = db.SelectDb(sql).Tables[0];
                var result = new List<CallToActionsModel>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {

                        string call = dr["Call"].ToString() == "" ? "0" : dr["Call"].ToString();
                        string book = dr["Book"].ToString() == "" ? "0" : dr["Book"].ToString();
                        string cat = dr["Category"].ToString() == "" ? "" : dr["Category"].ToString();// == "Food & Beverage" ? "Restaurant" : dr["Category"].ToString() == "Hotel" ? "Hotel" : "";
                        string mail = dr["Email"].ToString() == "" ? "0" : dr["Email"].ToString();
                        var item = new CallToActionsModel();
                        item.Business = dr["Business"].ToString();
                        item.Category = cat;
                        item.EmailCount = int.Parse(mail.ToString());
                        item.CallCount = int.Parse(call.ToString());
                        item.BookCount = int.Parse(book.ToString());
                        result.Add(item);
                    }

                    return Ok(result);
                }
                else
                {
                    return BadRequest("ERROR");
                }
            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCountAllUserlist()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = "";
            sql = $@"select Count(*) as count from UsersModel where active=1";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<Usertotalcount>();

            foreach (DataRow dr in dt.Rows)
            {
                var item = new Usertotalcount();
                item.count = int.Parse(dr["count"].ToString());
                result.Add(item);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetNewRegisteredWeekly()
        {
            int total = 0;
            string sqls = $@"select Count(*) as count from UsersModel where active=1";
            DataTable dts = db.SelectDb(sqls).Tables[0];

            foreach (DataRow dr in dts.Rows)
            {
                total = int.Parse(dr["count"].ToString());
            }


            string sql = $@"SELECT count(*) as count
                         FROM  UsersModel
                         WHERE DateCreated >= DATEADD(day,-7, GETDATE()) and active= 1";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<Usertotalcount>();

            foreach (DataRow dr in dt.Rows)
            {
                var item = new Usertotalcount();
                item.count = int.Parse(dr["count"].ToString());
                double val1 = double.Parse(dr["count"].ToString());
                double val2 = double.Parse(total.ToString());

                double results = Math.Abs(val1 / val2 * 100);
                item.percentage = results;
                result.Add(item);
            }

            return Ok(result);
        }
        public class SupportDetailModelRequest
        {
            public string? status { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> GetSupportDetailsList(SupportDetailModelRequest data)
        {
            GlobalVariables gv = new GlobalVariables();
            string sql = "";
            if (data.status == null)
            {
                sql = $@"SELECT        
		                        tbl_SupportModel.Id
		                        , tbl_SupportModel.Message
		                        , tbl_SupportModel.DateCreated
		                        , tbl_SupportModel.EmployeeID
		                        , CONCAT(UsersModel.Fname, ' ', UsersModel.Lname)  AS Fullname
		                        , UsersModel.Email
		                        , tbl_StatusModel.Id AS StatusId
                                , tbl_StatusModel.Name AS Status
                        FROM            
		                        tbl_SupportModel 
		                        INNER JOIN UsersModel 
		                        ON tbl_SupportModel.EmployeeID = UsersModel.EmployeeID
		                        INNER JOIN tbl_StatusModel 
		                        ON tbl_SupportModel.Status = tbl_StatusModel.Id 
                        order by id desc";
            }
            else { 
                sql = $@"SELECT        
		                        tbl_SupportModel.Id
		                        , tbl_SupportModel.Message
		                        , tbl_SupportModel.DateCreated
		                        , tbl_SupportModel.EmployeeID
		                        , CONCAT(UsersModel.Fname, ' ', UsersModel.Lname)  AS Fullname
		                        , UsersModel.Email
		                        , tbl_StatusModel.Id AS StatusId
                                , tbl_StatusModel.Name AS Status
                        FROM            
		                        tbl_SupportModel 
		                        INNER JOIN UsersModel 
		                        ON tbl_SupportModel.EmployeeID = UsersModel.EmployeeID
		                        INNER JOIN tbl_StatusModel 
		                        ON tbl_SupportModel.Status = tbl_StatusModel.Id 
                        WHERE
		                        tbl_StatusModel.Id = '"+data.status+"'order by id desc";
            }
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<SupportDetailModel>();

            foreach (DataRow dr in dt.Rows)
            {
                var item = new SupportDetailModel();
                item.Id = int.Parse(dr["Id"].ToString());
                item.Message = dr["Message"].ToString();
                item.Fullname = dr["Fullname"].ToString();
                item.Email = dr["Email"].ToString();
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.StatusId = dr["StatusId"].ToString();
                item.Status = dr["Status"].ToString();
                item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy hh:mm:ss tt");
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetLineGraphCountList()
        {
            DateTime startDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(-6);

            DateTime endDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            List<DateTime> allDates = new List<DateTime>();
            var result = new List<UserCountLineGraphModel>();
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                //allDates.Add(date.Date);
                var dategen = date.Date.ToString("yyyy-MM-dd");
                string datecreated = "";
                int count_ = 0;
                string sql = $@"select DateCreated,Count(*) as count from UsersModel where active = 1 and DateCreated='" + dategen + "' group by DateCreated order by  DateCreated ";
                DataTable dt = db.SelectDb(sql).Tables[0];

                var item = new UserCountLineGraphModel();
                if (dt.Rows.Count == 0)
                {
                    datecreated = dategen;
                    count_ = 0;
                }
                else
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        datecreated = dr["DateCreated"].ToString();
                        count_ = int.Parse(dr["count"].ToString());
                    }
                }

                item.DateCreated = DateTime.Parse(datecreated).ToString("dd");
                item.count = count_;
                result.Add(item);


            }


            return Ok(result);
        }
        public class SuportStatus
        {
            public int Id { get; set; }
            public string Status { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> updateSupportStatus(SuportStatus data)
        {

            string sql = $@"select * from tbl_supportmodel where Id='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new FamilyMemberStatus();
            if (dt.Rows.Count > 0)
            {
                string query = $@"update tbl_supportmodel set Status = '" + data.Status + "' where Id ='" + data.Id + "'";

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
        public class SupportUpdateEmailRequest
        {
            public string Status { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> EmailSupportUpdate(SupportUpdateEmailRequest data)
        {

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ALFARDAN OYSTER PRIVILEGE CLUB", "app@alfardan.com.qa"));
            // Add all recipients at once
            message.To.Add(new MailboxAddress(data.Name, data.Email));
            message.Subject = "Status Update";
            var bodyBuilder = new BodyBuilder();



            bodyBuilder.HtmlBody = @" <head>
                                        <style>
                                            @font-face {font-family: 'Montserrat-Reg';src: url('/fonts/Montserrat/Montserrat-Regular.ttf');}
                                            @font-face {
                                            font-family: 'Montserrat-Bold';
                                            src: url('/fonts/Montserrat/Montserrat-Bold.ttf');
                                            }
                                            @font-face {
                                            font-family: 'Montserrat-SemiBold';
                                            src: url('/fonts/Montserrat/Montserrat-SemiBold.ttf');
                                            }
                                            body {
                                                margin: 0;
                                                box-sizing: border-box;
                                                justify-content: center;
                                                align-items: center;
                
                                            }
                                            .login-container {
                                                background-image: url(https://www.alfardanoysterprivilegeclub.com/build/assets/black-cover-pattern-f558a9d0.jpg);
                                                
                                                display: flex;
                                                justify-content: center;
                                                align-items: center;
                                                flex-direction: column; 
                                                background-size: cover;}
                                            .gradient-border {
                                                height: 600px;
                                                width: 700px; 
                                                justify-content: center;
                                                background-color: transparent;
                                                box-sizing: content-box;
                                                gap: 20px;
                                                flex-direction: column;
                                                padding: 100px;
                                            }
                                            .login-container img {
                                                margin: 20px auto;
                                                width: 300px;
                                                height: 110px;
                                            }
                                            h1 {
                                                text-align: left;
                                                color: #d7d2cb;
                                                font-family: 'Montserrat-SemiBold';
                                                font-size: 2rem;
                                                font-style: italic;
                                            }
                                            h3 {
                                                text-align: left;
                                                color: #d7d2cb;
                                                font-family: 'Montserrat-Reg';
                                                font-size: 1.5rem;
                                                font-style: italic;
                                            }
                                            a {
                                                text-decoration: none;
                                            }
                                            h4 {
                                                text-align: center;
                                                color: #d7d2cb;
                                                font-family: 'Montserrat-Reg';
                                                font-size: 1.2rem;
                                                font-style: italic;
                                            }
                                        </style>
                                        </head>
                                        <body>
                                            <div class='login-container'>
                                            <div class='login-logo-conctainer'>
                                                <div class='gradient-border'>
                                                <img src='https://www.alfardanoysterprivilegeclub.com/assets/img/AOPC%20Logo%20-%20White.png' alt='AOPC' width='100%'' />

                                                <h1>
                                                    Dear " + data.Name + ",<br />" +
                                                        "</h1>" +
                                                        "<h1>We wanted to provide a quick update on the issue you raised.</h1>" +
                                                        "<h1><strong>Status:</strong> " + data.Status + "</h1><br />" +
                                                        "<h1>Thank you for your patience.</h1>" +
                                                        "</div>" +
                                                    "</div>" +
                                                    "</div>" +
                                                "</body>";
            message.Body = bodyBuilder.ToMessageBody();
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("app@alfardan.com.qa", "Oyster2023!");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

            }
            return Ok();
        }
        public class SupportCountModel
        {
            public int Supportcount { get; set; }
            public int TotalCount { get; set; }

        }
        [HttpGet]
        public async Task<IActionResult> GetSupportCount()
        {
            GlobalVariables gv = new GlobalVariables();

            string sql = "";
            string sql1 = "";
            sql = $@"SELECT 
	                    sc.Count 
	                    ,SuppportCnt.*
                    FROM 
	                    tbl_TotalSupportCount sc WITH(NOLOCK)
                    CROSS JOIN ( SELECT COUNT(*) AS SuppportCnt FROM tbl_SupportModel  WHERE (tbl_SupportModel.Status = 14) ) AS SuppportCnt";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<SupportCountModel>();
            foreach (DataRow dr in dt.Rows)
            {
                var item = new SupportCountModel();
                item.TotalCount = int.Parse(dr["Count"].ToString());
                item.Supportcount = int.Parse(dr["SuppportCnt"].ToString());
                result.Add(item);

            }
            

            return Ok(result);
        }

        public class SupportCountRequest
        {
            public int TotalCount { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> updateSupportCount(SupportCountRequest data)
        {
            var result = new SupportCountRequest();
            string query = $@"update tbl_TotalSupportCount set Count = '" + data.TotalCount + "' where Id ='1'";
            db.AUIDB_WithParam(query);

            return Ok(result);
        }


        #region POST METHOD

        [HttpPost]
        public async Task<IActionResult> PostClickCountsListAll(UserFilterday data)
        {
            int daysLeft = new DateTime(DateTime.Now.Year, 12, 31).DayOfYear - DateTime.Now.DayOfYear;
            int day = data.day == 1 ? daysLeft : data.day;

            string sql = $@"SELECT Business, Count(*) as count FROM tbl_audittrailModel
                         WHERE Actions LIKE '%Clicked%'  and Module <> 'AOPC APP' and  tbl_audittrailModel.DateCreated >= DATEADD(day,-" + day + ", GETDATE()) " +
                         "GROUP BY Business order by count desc;";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<ClicCountModel>();
            if (dt.Rows.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new ClicCountModel();
                    item.Module = dr["Business"].ToString();
                    item.Count = int.Parse(dr["count"].ToString());
                    result.Add(item);
                }

            }
            for (int x = 0; x < 4; x++)
            {
                var item = new ClicCountModel();
                item.Module = "No Data";
                item.Count = 0;
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> PostMostClickRestaurantList(UserFilterDateRange data)
        {
            //int daysLeft = new DateTime(DateTime.Now.Year, 12, 31).DayOfYear - DateTime.Now.DayOfYear;
            int daysLeft = (DateTime.Now - DateTime.Now.AddYears(-1)).Days;
            int day = data.day == 1 ? daysLeft : data.day;
            if (data.enddate != null)
            {
                //data.enddate = (DateTime.Parse(data.enddate).AddDays(1)).ToString();
            }
            string sql = "";
            try
            {
                if (data.day != 0)
                {
                    sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Food & Beverage' and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE())) " +
                            "GROUP BY    Business,Actions,Module order by count desc";
                }
                else
                {
                    sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Food & Beverage' AND tbl_audittrailModel.DateCreated between '" + data.startdate + "' AND '" + data.enddate +
                            "' GROUP BY    Business,Actions,Module order by count desc";
                }
                DataTable dt = db.SelectDb(sql).Tables[0];
                var result = new List<MostClickRestoModel>();
                int total = 0;
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow dr in dt.Rows)
                    {
                        total += int.Parse(dr["count"].ToString());
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        var item = new MostClickRestoModel();
                        item.Actions = dr["Actions"].ToString();
                        item.Business = dr["Business"].ToString();
                        item.Module = dr["Module"].ToString();
                        //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                        item.count = int.Parse(dr["count"].ToString());
                        double val1 = double.Parse(dr["count"].ToString());
                        double val2 = double.Parse(total.ToString());

                        double results = Math.Abs(val1 / val2 * 100);
                        item.Total = Math.Round(results, 2);
                        result.Add(item);
                    }


                }
                else
                {
                    for (int x = 0; x < 4; x++)
                    {
                        var item = new MostClickRestoModel();
                        item.Actions = "No Data";
                        item.Business = "No Data";
                        item.Module = "No Data";
                        item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                        item.count = 0;
                        item.Total = 0.00;
                        result.Add(item);
                    }

                }
                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostMostClickWellnessList(UserFilterDateRange data)
        {
            //int daysLeft = new DateTime(DateTime.Now.Year, 12, 31).DayOfYear - DateTime.Now.DayOfYear;
            int daysLeft = (DateTime.Now - DateTime.Now.AddYears(-1)).Days;
            int day = data.day == 1 ? daysLeft : data.day;
            if (data.enddate != null)
            {
                //data.enddate = (DateTime.Parse(data.enddate).AddDays(1)).ToString();
            }
            string sql = "";
            try
            {
                if (data.day != 0)
                {
                    sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Wellness' and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE())) " +
                                    "GROUP BY    Business,Actions,Module order by count desc";
                }
                else
                {
                    sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Wellness' and  tbl_audittrailModel.DateCreated between '" + data.startdate + "' and '" + data.enddate +
                            "' GROUP BY    Business,Actions,Module order by count desc";
                }
                DataTable dt = db.SelectDb(sql).Tables[0];
                var result = new List<GenericMostClickModel>();
                int total = 0;
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow dr in dt.Rows)
                    {
                        total += int.Parse(dr["count"].ToString());
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        var item = new GenericMostClickModel();
                        item.Actions = dr["Actions"].ToString();
                        item.Business = dr["Business"].ToString();
                        item.Module = dr["Module"].ToString();
                        //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                        item.count = int.Parse(dr["count"].ToString());
                        double val1 = double.Parse(dr["count"].ToString());
                        double val2 = double.Parse(total.ToString());

                        double results = Math.Abs(val1 / val2 * 100);
                        item.Total = Math.Round(results, 2);
                        result.Add(item);
                    }


                }
                else
                {
                    for (int x = 0; x < 4; x++)
                    {
                        var item = new GenericMostClickModel();
                        item.Actions = "No Data";
                        item.Business = "No Data";
                        item.Module = "No Data";
                        item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                        item.count = 0;
                        item.Total = 0.00;
                        result.Add(item);
                    }

                }
                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostMostClickHealthList(UserFilterDateRange data)
        {
            //int daysLeft = new DateTime(DateTime.Now.Year, 12, 31).DayOfYear - DateTime.Now.DayOfYear;
            int daysLeft = (DateTime.Now - DateTime.Now.AddYears(-1)).Days;
            int day = data.day == 1 ? daysLeft : data.day;
            if (data.enddate != null)
            {
                //data.enddate = (DateTime.Parse(data.enddate).AddDays(1)).ToString();
            }
            string sql = "";
            try
            {
                if (data.day != 0)
                {
                    sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Health' and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE())) " +
                                    "GROUP BY    Business,Actions,Module order by count desc";
                }
                else
                {
                    sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Health' and  tbl_audittrailModel.DateCreated between '" + data.startdate + "' and '" + data.enddate +
                           "' GROUP BY    Business,Actions,Module order by count desc";
                }
                DataTable dt = db.SelectDb(sql).Tables[0];
                var result = new List<GenericMostClickModel>();
                int total = 0;
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow dr in dt.Rows)
                    {
                        total += int.Parse(dr["count"].ToString());
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        var item = new GenericMostClickModel();
                        item.Actions = dr["Actions"].ToString();
                        item.Business = dr["Business"].ToString();
                        item.Module = dr["Module"].ToString();
                        //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                        item.count = int.Parse(dr["count"].ToString());
                        double val1 = double.Parse(dr["count"].ToString());
                        double val2 = double.Parse(total.ToString());

                        double results = Math.Abs(val1 / val2 * 100);
                        item.Total = Math.Round(results, 2);
                        result.Add(item);
                    }


                }
                else
                {
                    for (int x = 0; x < 4; x++)
                    {
                        var item = new GenericMostClickModel();
                        item.Actions = "No Data";
                        item.Business = "No Data";
                        item.Module = "No Data";
                        item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                        item.count = 0;
                        item.Total = 0.00;
                        result.Add(item);
                    }

                }
                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostMostClickAccessToCoWorkingList(UserFilterday data)
        {
            //int daysLeft = new DateTime(DateTime.Now.Year, 12, 31).DayOfYear - DateTime.Now.DayOfYear;
            int daysLeft = (DateTime.Now - DateTime.Now.AddYears(-1)).Days;
            int day = data.day == 1 ? daysLeft : data.day;
            try
            {

                string sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Access to co-working spaces' and  tbl_audittrailModel.DateCreated >= DATEADD(day,-" + day + ", GETDATE()) " +
                        "GROUP BY    Business,Actions,Module order by count desc";
                DataTable dt = db.SelectDb(sql).Tables[0];
                var result = new List<GenericMostClickModel>();
                int total = 0;
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow dr in dt.Rows)
                    {
                        total += int.Parse(dr["count"].ToString());
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        var item = new GenericMostClickModel();
                        item.Actions = dr["Actions"].ToString();
                        item.Business = dr["Business"].ToString();
                        item.Module = dr["Module"].ToString();
                        //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                        item.count = int.Parse(dr["count"].ToString());
                        double val1 = double.Parse(dr["count"].ToString());
                        double val2 = double.Parse(total.ToString());

                        double results = Math.Abs(val1 / val2 * 100);
                        item.Total = Math.Round(results, 2);
                        result.Add(item);
                    }


                }
                else
                {
                    for (int x = 0; x < 4; x++)
                    {
                        var item = new GenericMostClickModel();
                        item.Actions = "No Data";
                        item.Business = "No Data";
                        item.Module = "No Data";
                        item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                        item.count = 0;
                        item.Total = 0.00;
                        result.Add(item);
                    }

                }
                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }
        public static IEnumerable<dynamic> MonthsBetween(
            DateTime startDate,
            DateTime endDate)
        {
            DateTime iterator;
            DateTime limit;

            if (endDate > startDate)
            {
                iterator = new DateTime(startDate.Year, startDate.Month, 1);
                limit = endDate;
            }
            else
            {
                iterator = new DateTime(endDate.Year, endDate.Month, 1);
                limit = startDate;
            }

            var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
            while (iterator <= limit)
            {

                var firstDayOfMonth = new DateTime(iterator.Year, iterator.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                yield return new
                {
                    label = dateTimeFormat.GetMonthName(iterator.Month)

                };

                iterator = iterator.AddMonths(1);
            }
        }
        [HttpPost]
        public async Task<IActionResult> PostNewRegistered(UserFilterDateRange data)
        {
            int total = 0;
            int daysLeft = (DateTime.Now - DateTime.Now.AddYears(-1)).Days;
            int day = data.day == 1 ? daysLeft : data.day;
            string datecreated = "";
            int count_ = 0;
            var result = new List<Usertotalcount>();
            try
            {
                string sqls = $@"select Count(*) as count from UsersModel where active=1";
                DataTable dts = db.SelectDb(sqls).Tables[0];

                foreach (DataRow dr in dts.Rows)
                {
                    total = int.Parse(dr["count"].ToString());
                }
                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;
                if (data.startdate != null)
                {
                    startDate = Convert.ToDateTime(data.startdate);

                    endDate = DateTime.Parse(data.enddate);
                }
                else
                {
                    startDate = DateTime.Now.AddDays(-day);

                    endDate = DateTime.Now;
                }

                var months = MonthsBetween(startDate, endDate).ToList();
                var items = new List<monthsdate>();
                var mo = JsonConvert.SerializeObject(months);
                var list = JsonConvert.DeserializeObject<List<monthsdate>>(mo);
                if (data.day == 1)
                {
                    for (int x = 0; x < list.Count; x++)
                    {
                        //var item = new monthsdate();
                        //var month = list[x].label;
                        //item.label = month;
                        //items.Add(item);
                        string sql1 = $@"SELECT  DATENAME(month,DateCreated)  AS month , count(*) as count from UsersModel where active = 1 and DATENAME(month,DateCreated) = '" + list[x].label + "'   group by   DATENAME(month,DateCreated)   ";
                        DataTable dt1 = db.SelectDb(sql1).Tables[0];


                        if (dt1.Rows.Count == 0)
                        {
                            datecreated = list[x].label;
                            count_ = 0;
                        }
                        else
                        {
                            foreach (DataRow dr in dt1.Rows)
                            {
                                datecreated = dr["month"].ToString();
                                count_ = int.Parse(dr["count"].ToString());
                            }
                        }

                        string sql = $@"SELECT count(*) as count
                         FROM  UsersModel
                         WHERE CONVERT(DATE,DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE())) and active= 1";
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        var item = new Usertotalcount();
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                double val1 = double.Parse(dr["count"].ToString());
                                double val2 = double.Parse(total.ToString());

                                double results = Math.Abs(val1 / val2 * 100);
                                item.count = int.Parse(dr["count"].ToString());
                                item.Date = datecreated;
                                item.graph_count = count_;
                                item.percentage = results;
                                result.Add(item);

                            }


                        }
                        else
                        {
                            return BadRequest("ERROR");
                        }

                    }

                }
                else
                {
                    string query = $@"select Count(*) as count from UsersModel where active=1";
                    DataTable dtble = db.SelectDb(query).Tables[0];

                    foreach (DataRow dr in dtble.Rows)
                    {
                        total = int.Parse(dr["count"].ToString());
                    }
                    List<DateTime> allDates = new List<DateTime>();

                    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        //allDates.Add(date.Date);
                        var dategen = date.Date.ToString("yyyy-MM-dd");

                        string sql1 = $@"select DateCreated,Count(*) as count from UsersModel where active = 1 and DateCreated='" + dategen + "' group by DateCreated order by  DateCreated ";
                        DataTable dt1 = db.SelectDb(sql1).Tables[0];


                        if (dt1.Rows.Count == 0)
                        {
                            datecreated = dategen;
                            count_ = 0;
                        }
                        else
                        {
                            foreach (DataRow dr in dt1.Rows)
                            {
                                datecreated = dr["DateCreated"].ToString();
                                count_ = int.Parse(dr["count"].ToString());
                            }
                        }
                        string sql = "";
                        if (data.startdate != null)
                        {
                            sql = $@"SELECT count(*) as count
                         FROM  UsersModel
                         WHERE DateCreated between '" + data.startdate + "' and '" + data.enddate + "' and active= 1";
                        }
                        else if (data.day != 0)
                        {
                            sql = $@"SELECT count(*) as count
                         FROM  UsersModel
                         WHERE CONVERT(DATE,DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE())) and active= 1";
                        }
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        var item = new Usertotalcount();
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                double val1 = double.Parse(dr["count"].ToString());
                                double val2 = double.Parse(total.ToString());

                                double results = Math.Abs(val1 / val2 * 100);

                                item.count = int.Parse(dr["count"].ToString());
                                item.Date = DateTime.Parse(datecreated).ToString("yyyy-MM-dd");
                                item.graph_count = count_;
                                item.percentage = results;
                                result.Add(item);

                            }


                        }
                        else
                        {
                            return BadRequest("ERROR");
                        }


                    }

                }


                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostMostClickedHospitalityList(UserFilterDateRange data)
        {
            //int daysLeft = new DateTime(DateTime.Now.Year, 12, 31).DayOfYear - DateTime.Now.DayOfYear;
            int daysLeft = (DateTime.Now - DateTime.Now.AddYears(-1)).Days;
            int day = data.day == 1 ? daysLeft : data.day;
            if (data.enddate != null)
            {
                //data.enddate = (DateTime.Parse(data.enddate).AddDays(1)).ToString();
            }
            string sql = "";
            try
            {
                if (data.day != 0)
                {
                    sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Rooms & Suites' and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE())) " +
                                "GROUP BY    Business,Actions,Module order by count desc";
                }
                else
                {
                    sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Rooms & Suites' and  tbl_audittrailModel.DateCreated between '" + data.startdate + "' and '" + data.enddate +
                            "' GROUP BY    Business,Actions,Module order by count desc";
                }
                DataTable dt = db.SelectDb(sql).Tables[0];
                var result = new List<MostClickHospitalityModel>();
                if (dt.Rows.Count > 0)
                {
                    int total = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        total += int.Parse(dr["count"].ToString());
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        var item = new MostClickHospitalityModel();
                        item.Actions = dr["Actions"].ToString();
                        item.Business = dr["Business"].ToString();
                        item.Module = dr["Module"].ToString();
                        //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                        item.count = int.Parse(dr["count"].ToString());
                        double val1 = double.Parse(dr["count"].ToString());
                        double val2 = double.Parse(total.ToString());

                        double results = Math.Abs(val1 / val2 * 100);
                        item.Total = Math.Round(results, 2);
                        result.Add(item);
                    }


                }
                else
                {
                    for (int x = 0; x < 4; x++)
                    {
                        var item = new MostClickHospitalityModel();
                        item.Actions = "No Data";
                        item.Business = "No Data";
                        item.Module = "No Data";
                        item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                        item.count = 0;
                        item.Total = 0.00;
                        result.Add(item);
                    }

                }
                return Ok(result);


            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostMostCickStoreList(UserFilterDateRange data)
        {
            //int daysLeft = new DateTime(DateTime.Now.Year, 12, 31).DayOfYear - DateTime.Now.DayOfYear;
            int daysLeft = (DateTime.Now - DateTime.Now.AddYears(-1)).Days;
            int day = data.day == 1 ? daysLeft : data.day;
            if (data.enddate != null)
            {
                //data.enddate = (DateTime.Parse(data.enddate).AddDays(1)).ToString();
            }
            string sql = "";
            try
            {
                if (data.day != 0)
                {
                    sql = $@"SELECT     Count(*)as count,Business,Actions,Module
                        FROM         tbl_audittrailModel  WHERE Actions LIKE '%Viewed%' and module ='Shops & Services' and  CONVERT(DATE,tbl_audittrailModel.DateCreated) >= CONVERT(DATE,DATEADD(day,-" + day + ", GETDATE())) " +
                                    "GROUP BY    Business,Actions,Module order by count desc";
                }
                else
                {
                    sql = $@"SELECT     Count(*)as count, Actions,Business,Module
                         FROM         tbl_audittrailModel  WHERE Actions LIKE '%View%' and module ='Shops & Services' and  tbl_audittrailModel.DateCreated between '" + data.startdate + "' and '" + data.enddate +
                             "' GROUP BY    Business,Actions,Module order by count desc";
                }
                DataTable dt = db.SelectDb(sql).Tables[0];
                List<MostClickStoreModel> result = new List<MostClickStoreModel>();
                List<MostClickStoreModel> result2 = new List<MostClickStoreModel>();
                int total = 0;
                double sub_total = 0;
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        total += int.Parse(dr["count"].ToString());
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        var item = new MostClickStoreModel();
                        item.Actions = dr["Actions"].ToString();
                        item.Business = dr["Business"].ToString();
                        item.Module = dr["Module"].ToString();
                        //item.DateCreated = DateTime.Parse(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                        item.count = int.Parse(dr["count"].ToString());
                        double val1 = double.Parse(dr["count"].ToString());
                        double val2 = double.Parse(total.ToString());

                        double results = val1 / val2 * 100;
                        item.Total = Math.Round(results, 2);
                        result.Add(item);

                    }
                    var total_res = Math.Abs(result.Count - 4);
                    if (result.Count != 4)
                    {
                        for (int x = 0; x < total_res; x++)
                        {
                            var item2 = new MostClickStoreModel();
                            item2.Actions = "No Data";
                            item2.Business = "No Data";
                            item2.Module = "No Data";
                            item2.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                            item2.count = 0;
                            double results = sub_total - 100;
                            item2.Total = 0.01;
                            result2.Add(item2);
                        }
                    }
                    result.AddRange(result2);
                    return Ok(result);




                }
                else
                {
                    for (int x = 0; x < 4; x++)
                    {
                        var item = new MostClickStoreModel();
                        item.Actions = "No Data";
                        item.Business = "No Data";
                        item.Module = "No Data";
                        item.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
                        item.count = 0;
                        item.Total = 0.00;
                        result.Add(item);
                    }

                }
                return Ok(result);



            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }
        #endregion
        #region Model
        public class UserFilterday
        {
            public int day { get; set; }

        }
        public class UserFilterDateRange
        {
            public int day { get; set; }
            public string? startdate { get; set; }
            public string? enddate { get; set; }

        }
        public class UserFilterCatday
        {
            public int day { get; set; }
            public string? startdate { get; set; }
            public string? enddate { get; set; }

            public string category { get; set; }

        }
        public class SupportModel
        {
            public int Supportcount { get; set; }

        }
        public class monthsdate
        {
            public string label { get; set; }

        }
        public class Usertotalcount
        {
            public int count { get; set; }
            public int graph_count { get; set; }
            public double percentage { get; set; }
            public string Date { get; set; }

        }
        public class ClicCountModel
        {
            public string Module { get; set; }
            public int Count { get; set; }

        }
        public class CallToActionsModel
        {
            public string Business { get; set; }
            public string Category { get; set; }
            public int EmailCount { get; set; }
            public int CallCount { get; set; }
            public int BookCount { get; set; }

        }
        public class MostClickStoreModel
        {
            public string Actions { get; set; }
            public string Business { get; set; }
            public string Module { get; set; }
            public string DateCreated { get; set; }
            public int count { get; set; }
            public double Total { get; set; }

        }
        public class MostClickHospitalityModel
        {
            public string Actions { get; set; }
            public string Business { get; set; }
            public string Module { get; set; }
            public string DateCreated { get; set; }
            public int count { get; set; }
            public double Total { get; set; }

        }
        public class MostClickRestoModel
        {
            public string Actions { get; set; }
            public string Business { get; set; }
            public string Module { get; set; }
            public string DateCreated { get; set; }
            public int count { get; set; }
            public double Total { get; set; }

        }

        public class GenericMostClickModel
        {
            public string Actions { get; set; }
            public string Business { get; set; }
            public string Module { get; set; }
            public string DateCreated { get; set; }
            public int count { get; set; }
            public double Total { get; set; }

        }

        public class SupportDetailModel
        {
            public int Id { get; set; }
            public string Message { get; set; }
            public string EmployeeID { get; set; }
            public string Fullname { get; set; }
            public string Email { get; set; }
            public string StatusId { get; set; }
            public string Status { get; set; }
            public string DateCreated { get; set; }

        }
        public class UserCountLineGraphModel
        {
            public string DateCreated { get; set; }
            public int count { get; set; }

        }
        #endregion
    }
}