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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static AuthSystem.Data.Controller.ApiUserAcessController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AuthSystem.Data.Controller
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiUserAcessController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ApiUserAcessController> _logger;
        public ApiUserAcessController(IOptions<AppSettings> appSettings, ApplicationDbContext context, ILogger<ApiUserAcessController> logger,
        JwtAuthenticationManager jwtAuthenticationManager, IWebHostEnvironment environment)
        {

            _context = context;
            _appSettings = appSettings.Value;
            _logger = logger;
            this.jwtAuthenticationManager = jwtAuthenticationManager;

        }
        public class JWTModel
        {

            public string Key { get; set; }

        }
        public class StatusResult
        {
            public string Status { get; set; }

        }     
        public class ExpiryRes
        {
            public string expiryDate { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> AudtiTrails(AuditTrailModel data)
        {
            var result = new StatusResult();
            try
            {
        
                string query = "";
                 query = $@"insert into tbl_audittrailModel (Actions,Module,UserId,status,EmployeeID,ActionID,Business,DateCreated) values 
                ('" +data.Actions+"','"+data.Module+"','"+data.UserId+"','"+data.status+ "','"+data.EmployeeID+"','"+data.ActionID+"','"+data.Business+"','"+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"')";
                _logger.LogInformation("Success Insert Audit Trail");
                db.AUIDB_WithParam(query);
                result.Status = "Audit Trail Inserted";
  
                return Ok(result);
            }

            catch (Exception ex)
            {
                _logger.LogInformation("Error Insert Audit Trail");
                result.Status = "Error";
                return BadRequest(result);
            }
        
        }
        [HttpPost]
        public async Task<IActionResult> SaveJWToken(JWTokenModel data)
        {
            string status = "";
            var result = new StatusResult();
            try
            {
                string query = "";
                DateTime expirydate = DateTime.Now.AddDays(1);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(data.Email);
                string email= System.Convert.ToBase64String(plainTextBytes);
                query = $@"insert into tbl_TokenModel (Token,ExpiryDate,Status,DateCreated) values ('" + email + "','"+expirydate.ToString("yyyy-MM-dd hh:mm:ss")+"','5','"+DateTime.Now.ToString("yyyy-MM-dd")+"')";
                
                db.AUIDB_WithParam(query);
                var emailsend = "https://www.alfardanoysterprivilegeclub.com/change-password/" + email;
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("AOPC Registration", "app@alfardan.com.qa"));
                message.To.Add(new MailboxAddress("", data.Email));
                message.Subject = "Email Registration Link";
                var bodyBuilder = new BodyBuilder();
                string img = "../img/AOPCBlack.jpg";
                bodyBuilder.HtmlBody = @"<!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <meta http-equiv=""X-UA-Compatible"" content=""ie=edge"">
                    <title>Oyster Privilege Club</title>
                </head>
                <style>
                    @font-face {
                    font-family: 'Montserrat-Reg';
                    src: 
                    url('{{ config('app.url') }}/assets/fonts/Montserrat/Montserrat-Regular.ttf');
                    }
                    @font-face {
                        font-family: 'Montserrat-SemiBold';
                        src: url('{{ config('app.url') }}/assets/fonts/Montserrat/Montserrat-SemiBold.ttf');
                    }
                    body{
                        display: flex;
                        flex-direction: column;
                        font-family: 'Montserrat-Reg';
                    }
                    .img-container {
                        width: 200px;
                        margin:0 auto;
                    }
                    h3{
                        width: 400px;
                        text-align: center;
                        margin:20px auto;
                    }
                    p{
                        width: 400px;
                        margin:10px auto;
                    }
                </style>
                <body>
                    <div class=""img-container"">
                        <img width=""100%"" src=""https://www.alfardanoysterprivilegeclub.com/assets/img/AOPC-low-black.png"" alt="""">
                    </div>
                    <h3>Reset Password</h3>
                    <p>We received a request to reset the password for your account. If you did not initiate this request, please ignore this email.</p>
                    <p>To reset your password, please click the following link:<a href="+emailsend+">"+emailsend+"</a>. This link will be valid for the next 24 hours.</p>" +
                    "<p>If you have any issues with resetting your password or need further assistance, please contact our support team at <b>app@alfaran.com.qa</b>.</p>" +
                "</body> "+
                "</html>";
                message.Body = bodyBuilder.ToMessageBody();
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("app@alfardan.com.qa", "Oyster2023!");
                    client.Send(message);
                    client.Disconnect(true);
                    status = "Successfully sent registration email";

                }
                result.Status = "Success!";
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.Status = "Error";
                return BadRequest(ex.Message);
            }
        }
        public class SupportModel
        {
            public string? EmployeeID { get; set; }
            public string? Message { get; set; }
            public string? Status { get; set; }

        }     
        
        public class JWTokenModel
        {
            public string? Email { get; set; }

        }
        public class Email
        {
            public string EmailAddress { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> TrailSupport (SupportModel data)
        {
            var result = new StatusResult();
            try
            {
                string sql = $@"SELECT DISTINCT 
                         UsersModel.Id, UsersModel.Fname, UsersModel.Lname, UsersModel.Mname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, UsersModel.Type, UsersModel.Cno, UsersModel.isVIP, 
                         tbl_CorporateModel.CorporateName, tbl_PositionModel.Name
                          FROM            UsersModel INNER JOIN
                         tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id INNER JOIN
                         tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id where EmployeeID='" + data.EmployeeID + "'";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if (dt.Rows.Count != 0)
                {
                    var fullname = dt.Rows[0]["Fname"].ToString() + " " + dt.Rows[0]["Lname"].ToString();
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("ALFARDAN OYSTER PRIVILEGE CLUB", "app@alfardan.com.qa"));
                    //message.To.Add(new MailboxAddress("Ace Caspe", "ace.caspe@odecci.com"));
                    //message.To.Add(new MailboxAddress("Marito Ace", data.Email));
                    message.To.Add(new MailboxAddress("Alfardan Marketing", "mmohandas@alfardan.com.qa"));
                    message.To.Add(new MailboxAddress("Alfardan Marketing", "ccaspe@alfardan.com.qa"));
                    message.Bcc.Add(new MailboxAddress("Alfardan Support", "support@odecci.com"));
                    message.Bcc.Add(new MailboxAddress("Alfardan Marketing", "skassab@alfardan.com.qa"));
                    message.Bcc.Add(new MailboxAddress("Alfardan Marketing", "dulay@alfardan.com.qa"));
                    message.ReplyTo.Add(new MailboxAddress(fullname, dt.Rows[0]["Email"].ToString()));
                    message.Subject = "AOPC Support Concern";
                var bodyBuilder = new BodyBuilder();

                bodyBuilder.HtmlBody = @" <style>
    body {
      margin: 0;
      box-sizing: border-box;
      display: flex;
      flex-direction: column;
      font-family: ""Montserrat"";
    }
    @font-face {
      font-family: ""Montserrat"";
      src: url(""https://www.alfardanoysterprivilegeclub.com/build/assets/Montserrat-Regular-dcfe8df2.ttf"");
    }
    .header {
      width: 200px;
      height: 120px;
      overflow: hidden;
      margin: 50px auto;
    }
    .body {
      width: 500px;
      margin: 5px auto;
      font-size: 13px;
    }
    .body p {
      margin: 20px 0;
    }
    ul li {
      list-style: none;
    }
    .footer {
      width: 500px;
      margin: 20px auto;
      font-size: 13px;
    }
    .citation span {
      color: #c89328;
    }
    .body span {
      color: #c89328;
    }
  </style>
  <body>
    <div class=""header"">
      <img
        src="" https://www.alfardanoysterprivilegeclub.com/assets/img/AOPC-Black.png""
        alt=""Alfardan Oyster Privilege Club""
        width=""100%""
      />
    </div>
    <div class=""body"">
      <p class=citation>Dear <span> Team, </span></p>
      <p class=body>" +
      data.Message+
 
    " </div> <p class=footer>Regards, <br />" +
     " <br /> " +
     "<p class=footer>Employee ID: " + data.EmployeeID + " </p> " +
      "<p class=footer>Full Name: " + fullname + " </p> " +
       "<p class=footer>Contact Number: " + dt.Rows[0]["Email"].ToString() + " </p>" +
        "<p class=footer>Corporate Name: " + dt.Rows[0]["CorporateName"].ToString() + " </p>" +
         "<p class=footer>Position: " + dt.Rows[0]["Name"].ToString() + " </p>" +


    "<p class=footer > Alfardan Oyster Privilege Club App   </p>" +
     "</body>";
                    string query = "";
                    query = $@"insert into tbl_SupportModel (EmployeeID,Message,Status,DateCreated) values ('" + data.EmployeeID + "','" + data.Message + "','14','"+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"')";
                    db.AUIDB_WithParam(query);
                    result.Status = "New Support Inserted";
                    message.Body = bodyBuilder.ToMessageBody();
                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                        client.Authenticate("app@alfardan.com.qa", "Oyster2023!");
                        client.Send(message);
                        client.Disconnect(true);

                    }

                
                return Ok(result);
                }
            }
            catch (Exception ex)
            {
                result.Status = "Error";
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> EmailValidation(Email data)
        {
            var result = new StatusResult();
            try
            {
                string sql = $@"select Distinct Email from usersmodel where Email='" + data.EmailAddress + "'";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if (dt.Rows.Count == 0)
                {
                    result.Status = "Error!";
                    return BadRequest();
                }
                else
                {
                    result.Status = "Valid Email";
                    return Ok(result);
                }
             }

            catch (Exception ex)
            {
                result.Status = "Error";
                return BadRequest(result);
            }
           
        }
        [HttpPost]
        public async Task<IActionResult> GetKeyExpiry(JWTokenModel data)
        {
            var result = new ExpiryRes();
            try
            {
                string sql = $@"select  Token,expiryDate from tbl_TokenModel where Token='" + data.Email + "'";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if (dt.Rows.Count == 0)
                {
                    result.expiryDate = "Error!";
                    return BadRequest();
                }
                else
                {
                    result.expiryDate = dt.Rows[0]["ExpiryDate"].ToString();
                    return Ok(result);

                }
            }

            catch (Exception ex)
            {
                result.expiryDate = "Error";
                return BadRequest(result);
            }

        }
        public class NotifStats
        {
            public int? AllowNotif { get; set; }
            public string? Email { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> PostAllowNotif(NotifStats data)
        {
            var result = new ExpiryRes();
            try
            {
                string sql = $@"select  * from UsersModel  where   Email='" + data.Email + "'";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if (dt.Rows.Count == 0)
                {
                
                    return BadRequest("Error!");
                }
                else
                {
                    string query = $@"Update  UsersModel set AllowEmailNotif = '"+data.AllowNotif+"' where  Email='" + data.Email + "'";
                    db.AUIDB_WithParam(query);
                    return Ok("Success");
                }
            }

            catch (Exception ex)
            {
                return BadRequest("Error!");
            }

        }

      
        [HttpPost]
        public async Task<IActionResult> DeleteToken(JWTokenModel data)
        {
            var result = new StatusResult();
            try
            {
                string sql = $@"select  Token,ExpiryDate from tbl_TokenModel where Token='" + data.Email + "'";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if (dt.Rows.Count == 0)
                {
                    result.Status = "Error!";
                    return BadRequest();
                }
                else
                {
                   string query = $@"delete  tbl_TokenModel where Token = '" + dt.Rows[0]["Token"].ToString() +"'";
                    db.AUIDB_WithParam(query);
                    result.Status = "Deleted";
                    return Ok(result);

                }
            }

            catch (Exception ex)
            {
                result.Status = "Error";
                return BadRequest(result);
            }

        }
        public class QrLogsModel
        {
            public string? Id { get; set; }
            public string? EmpoyeeID { get; set; }
            public string? Longtitude { get; set; }
            public string? Latitude { get; set; }
            public string? IPAddres { get; set; }
            public string? Region { get; set; }
            public string? Country { get; set; }
            public string? City { get; set; }
            public string? AreaCode { get; set; }
            public string? ZipCode { get; set; }
            public string? ISOCode { get; set; }
            public string? MetroCode { get; set; }
            public string? TimeZone { get; set; }
            public string? PostalCode { get; set; }
            public string?  EmployeeName{ get; set; }
            

        }
        [HttpPost]
        public async Task<IActionResult> QrSendEmail(QrLogsModel data)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ALFARDAN OYSTER PRIVILEGE CLUB", "app@alfardan.com.qa"));
            //message.To.Add(new MailboxAddress("Ace Caspe", "ace.caspe@odecci.com"));
            //message.To.Add(new MailboxAddress("Marito Ace", data.Email));
            message.To.Add(new MailboxAddress("Alfardan Marketing", "app@alfardan.com.qa"));
            message.Bcc.Add(new MailboxAddress("Marito Ace", "support@odecci.com"));
            message.Bcc.Add(new MailboxAddress("Alfardan Marketing", "skassab@alfardan.com.qa"));
            message.Bcc.Add(new MailboxAddress("Alfardan Marketing", "dulay@alfardan.com.qa"));
            message.Subject = "QR Scan Logs";
            var bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = @" <style>
    body {
      margin: 0;
      box-sizing: border-box;
      display: flex;
      flex-direction: column;
      font-family: ""Montserrat"";
    }
    @font-face {
      font-family: ""Montserrat"";
      src: url(""https://www.alfardanoysterprivilegeclub.com/build/assets/Montserrat-Regular-dcfe8df2.ttf"");
    }
    .header {
      width: 200px;
      height: 120px;
      overflow: hidden;
      margin: 50px auto;
    }
    .body {
      width: 500px;
      margin: 5px auto;
      font-size: 13px;
    }
    .body p {
      margin: 20px 0;
    }
    ul li {
      list-style: none;
    }
    .footer {
      width: 500px;
      margin: 20px auto;
      font-size: 13px;
    }
    .citation span {
      color: #c89328;
    }
    .body span {
      color: #c89328;
    }
  </style>
  <body>
    <div class=""header"">
      <img
        src="" https://www.alfardanoysterprivilegeclub.com/assets/img/AOPC-Black.png""
        alt=""Alfardan Oyster Privilege Club""
        width=""100%""
      />
    </div>
    <div class=""body"">
      <p class=citation>Dear <span> Admin </span></p>
      <p class=body>
        A QR code associated with " + data.EmployeeName + " was successfully scanned from <span>" + data.City + " " + data.Region + " </span>.</p><p class=body> " +
    "The following device information was recorded at the time of the scan:</p><ul><li>IP Address: " + data.IPAddres + "</li>" +
    " <li>City: " + data.City + "</li>" +
    "<li>Region: " + data.Region + "</li>" +
   " <li>Country: " + data.Country + "</li> " +
   " <li>Postal Code: " + data.PostalCode + "</li> " +
    "<li>Zip Code: " + data.ZipCode + "</li> " +
   " <li>Area Code: " + data.AreaCode + "</li> " +
   " <li>Longitude: " + data.Longtitude + "</li> " +
   " <li>Latitude: " + data.Latitude + "</li> " +
  "  <li>Date Time of Scan: " + DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss tt") + "</li>" +
 " </ul>" +
" </div> <p class=footer>Regards, <br />" +
 " <br /> " +
 "Alfardan Oyster Privilege Club App " +
 "</p>" +
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
        [HttpPost]
        public async Task<IActionResult> QrCodeLogs(QrLogsModel data)
        {
            var result = new StatusResult();
            try
            {
        
                string query = "";
                 query = $@"insert into tbl_QrCodeLogsModel (EmployeeID,Longtitude,Latitude,IPAddres,Region,Country,City,AreaCode,ZipCode,ISOCode,MetroCode,TimeZone,PostalCode,DateCreated) values 
                        ('" +data.EmpoyeeID + "','"+data.Longtitude + "','"+data.Latitude + "','"+data.IPAddres + "','" + data.Region + "','"+data.Country + "'," +
                        "'"+data.City + "','"+data.AreaCode + "','" + data.ZipCode + "','"+data.ISOCode + "','"+data.MetroCode + "','"+data.TimeZone + "','"+data.PostalCode+"','"+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"')";
                db.AUIDB_WithParam(query);
                QrSendEmail(data);

                result.Status = "Qr Logs Inserted";
                
                return Ok(result);
            }

            catch (Exception ex)
            {
                result.Status = "Error";
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> QrAuditTrail(qrAuditTrailModel data)
        {
            var result = new StatusResult();
            try
            {
                string query = "";
                query = $@"insert into tbl_qrAuditTrailModel (Actions,Module,UserId,Locations,status,EmployeeID) values ('" + data.Actions + "','" + data.Module + "','" + data.UserId + "','" + data.Locations + "' ,'" + data.status + "','"+data.EmployeeID+"')";
                db.AUIDB_WithParam(query);
                result.Status = "Audit Trail Inserted";
                return Ok(result);
            }

            catch (Exception ex)
            {
                result.Status = "Error";
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> UserInfoFilteredbyJWToken(JWTModel data)
        {
            var param = new IDataParameter[]
              {
               new SqlParameter("@jwToken",data.Key)
              };
            DataTable table = db.SelectDb_SP("SP_UserInfoList", param).Tables[0];
            var item = new UserDetailsVM();
            if (table.Rows.Count != 0)
            {
          
                item.Id = int.Parse(table.Rows[0]["Id"].ToString());
                item.Username = table.Rows[0]["Username"].ToString();
                item.Fname = table.Rows[0]["Fname"].ToString();
                item.Lname = table.Rows[0]["Lname"].ToString();
                item.Email = table.Rows[0]["Email"].ToString();
                item.Gender = table.Rows[0]["Gender"].ToString();
                item.EmployeeID = table.Rows[0]["EmployeeID"].ToString();
                item.isVIP = table.Rows[0]["isVIP"].ToString();
                item.Corporatename = table.Rows[0]["Corporatename"].ToString();
                item.PositionName = table.Rows[0]["PositionName"].ToString();
                item.UserType = table.Rows[0]["UserType"].ToString();
                item.Status = table.Rows[0]["Status"].ToString();
                item.Cno = table.Rows[0]["Cno"].ToString();
                item.Address = table.Rows[0]["Address"].ToString();
                item.MembershipName = table.Rows[0]["MembershipName"].ToString();
                item.MemValidity = Convert.ToDateTime(table.Rows[0]["MemValidity"].ToString()).ToString("MM/dd/yyyy");
                item.CompanyAddress = table.Rows[0]["CompanyAddress"].ToString();
                item.ProfileImgPath = table.Rows[0]["ProfileImgPath"].ToString();
                item.MembershipNumber = table.Rows[0]["MembershipNumber"].ToString();
                item.CorpCno = table.Rows[0]["CorpCno"].ToString();
                item.AllowEmailNotif = table.Rows[0]["AllowEmailNotif"].ToString();
                item.MembershipCard = table.Rows[0]["MembershipCard"].ToString();
                item.VIPCard = table.Rows[0]["VIPCard"].ToString();
                item.QRFrame = table.Rows[0]["QRFrame"].ToString();
                item.VIPBadge = table.Rows[0]["VIPBadge"].ToString();
                item.TextColor = table.Rows[0]["TextColor"].ToString();





            }
            return Ok(item);
        }
        [HttpPost]
        public IActionResult AdminLogIn(string username, string password)
        {
            GlobalVariables gv = new GlobalVariables();
            _global.Status = gv.ValidationUser(username, password, _context);
            return Ok(_global.Status);
        }
        
    }
}
