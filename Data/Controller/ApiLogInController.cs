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
using AuthSystem.ViewModel;
using Microsoft.Data.SqlClient;
using static AuthSystem.Data.Controller.ApiUserAcessController;
using Serilog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Web.Http.Results;
using static AuthSystem.Data.Controller.ApiVendorController;
using System.Security.Cryptography;
using System.Text;
using API.Models;
using CMS.Models;
using System.Net.NetworkInformation;

namespace AuthSystem.Data.Controller
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiLogInController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        //private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;

        public ApiLogInController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {

            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;

        }

        [HttpPost]
        public IActionResult LogIn (UserModel data)
        {
            var pass3 = Cryptography.Decrypt("cPnBpX9fadVSakJzno5TASOU3vRs0HU7Q3XaoRNY1tY=");
            Console.WriteLine(pass3);
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            //_global.Status = gv.ValidationUser(data.Username, data.Password, _context);
            bool compr_user = false;
            //var userinfo = dbContext.tbl_UsersModel.Where(a => EF.Functions.Collate(a.Username, "Latin1_General_CI_AI") == username).ToList();
            string sql = $@"SELECT        UsersModel.Id, UsersModel.Username, UsersModel.Password, UsersModel.Fname,UsersModel.Lname, UsersModel.EmployeeID,UsersModel.Active, tbl_UserTypeModel.UserType, tbl_CorporateModel.Id AS 'CorporateId', tbl_CorporateModel.CorporateName, tbl_PositionModel.Name as PositionName, UsersModel.JWToken,UsersModel.FilePath
                        FROM            UsersModel INNER JOIN
                                                 tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id INNER JOIN
                                                 tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id INNER JOIN
                                                 tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id
                        WHERE        (UsersModel.Username = '" + data.Username + "' COLLATE Latin1_General_CS_AS) AND (UsersModel.Password = '" + Cryptography.Encrypt(data.Password) + "' COLLATE Latin1_General_CS_AS) AND (UsersModel.Active = 1)";
            
            
            DataTable dt = db.SelectDb(sql).Tables[0];
            var corporate = dt.Rows[0]["CorporateId"].ToString();
            var usertype = dt.Rows[0]["UserType"].ToString();
            HttpContext.Session.SetString("BTCorporateId", corporate);
            HttpContext.Session.SetString("BTUserType", usertype);
            var CorporateId = HttpContext.Session.GetString("BTCorporateId");
            var UserType = HttpContext.Session.GetString("BTUserType");
            Console.WriteLine(CorporateId);
            Console.WriteLine(UserType);
            //_state.SomeGlobalValue = dt.Rows[0]["CorporateId"].ToString(); // Example value
            var item = new AppModel();
            if (data.Username.Length != 0 || data.Password.Length != 0)
            {
                if (dt.Rows.Count != 0)
                {
                    compr_user = String.Equals(dt.Rows[0]["Username"].ToString().Trim(), data.Username, StringComparison.Ordinal);
                }

                if (compr_user)
                {
                    string pass = Cryptography.Decrypt(dt.Rows[0]["password"].ToString().Trim());
                    if ((pass).Equals(data.Password))
                    {

                        string sql2 = $@"select tbl_CorporateModel.CorporateName,tbl_CorporateModel.Id as corporateid ,UsersModel.Username , UsersModel.Id as userid,tbl_CorporateModel.DateEnded  from UsersModel 
                                        inner join
                                        tbl_CorporateModel on UsersModel.CorporateID = tbl_CorporateModel.Id where UsersModel.Id='" + dt.Rows[0]["Id"].ToString() + "' ";
                        DataTable dt2 = db.SelectDb(sql2).Tables[0];

                     
                        DateTime date1 = Convert.ToDateTime(currentDate.ToString());
                        DateTime date2 = Convert.ToDateTime(dt2.Rows[0]["DateEnded"].ToString());
                        TimeSpan difference = date2 - date1;
                        int dayDifference = difference.Days;
                   
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
                        //gv.AudittrailLogIn("Successfully", "Log In Form", dt.Rows[0]["EmployeeID"].ToString(), 7);
                        var token = Cryptography.Encrypt(str_build.ToString());
                        string strtokenresult = token;
                        string[] charsToRemove = new string[] { "/", ",", ".", ";", "'","=" ,"+"};
                        foreach (var c in charsToRemove)
                        {
                            strtokenresult = strtokenresult.Replace(c, string.Empty);
                        }
                        if (dayDifference == 0)
                        {
                            item.Status = "Membership is already Expired";
                            item.Key = string.Concat(strtokenresult.TakeLast(15));
                            return Ok(item);
                        }
                        else
                        {
                            string query = $@"update UsersModel set JWToken='" + string.Concat(strtokenresult.TakeLast(15)) + "' where id = '" + dt.Rows[0]["id"].ToString() + "'";
                            db.AUIDB_WithParam(query);

                            item.Status = "Successfully Log In";
                            item.Key = string.Concat(strtokenresult.TakeLast(15));
                            //var refreshToken = GenerateRefreshToken();
                            //SetRefreshToken(refreshToken);
                            return Ok(item);

                        }
               
                    }
                    else
                    {

                        item.Status = "Invalid Log In";
                        item.Key = "NULL";
                        //gv.AudittrailLogIn("Bad Request", "Log In Form", dt.Rows[0]["EmployeeID"].ToString(), 8);
                        return BadRequest();
              
                    }

                }
                else
                {
                    return BadRequest("Invalid Log In");
                }
            }
            else
            {
                return BadRequest("Invalid Log In");
            }
       
       
        }
        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken();
            refreshToken.NewToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            refreshToken.TokenExpires = DateTime.Now.AddDays(7);
            refreshToken.TokenCreated = DateTime.Now;
            
            return refreshToken;
        }
        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var model = new User();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.TokenExpires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.TokenCreated.ToString());
            model.TokenCreated = newRefreshToken.TokenCreated;
            model.TokenCreated = newRefreshToken.TokenCreated;
            model.TokenCreated = newRefreshToken.TokenExpires;
        }
        public class User
        {

            public string NewToken { get; set; } = string.Empty;
            public DateTime TokenCreated { get; set; }
            public DateTime TokenExpires { get; set; }

        }
        //[HttpPost]
        //public IActionResult AdminLogIn(string username, string password)
        //{
        //    GlobalVariables gv = new GlobalVariables();
        //    _global.Status = gv.ValidationUser(username, password, _context);
        //    return Ok(_global.Status);
        //}
    }
}
