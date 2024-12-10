using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthSystem.Manager;
using AuthSystem.Models;
using AuthSystem.Services;
using Microsoft.Extensions.Options;
using AuthSystem.ViewModel;
using System.Data;
using API.ViewModel;
using Microsoft.EntityFrameworkCore;
using static AuthSystem.Data.Controller.ApiPaginationController;
using MimeKit;
using MailKit.Net.Smtp;
using static AuthSystem.Data.Controller.ApiUserAcessController;

namespace API.Data.Controller
{
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiCorporateListingController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;
        DBMethods dbmet = new DBMethods();


        public ApiCorporateListingController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {

            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;

        }

        [HttpGet]
        public async Task<IActionResult> CorporateRegularUserCount()
        {
            string sql = $@"SELECT CorporateName,COUNT(*)as count from UsersModel
                         inner join tbl_CorporateModel ON tbl_CorporateModel.Id = CorporateID where Active = 1
                         group by CorporateName";

            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<CorporateListing>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new CorporateListing();
                item.Company = dr["CorporateName"].ToString();
                item.UserCount = int.Parse(dr["count"].ToString());
                result.Add(item);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> CorporateVIPUserCount()
        {
            string sql = $@"SELECT COUNT(*)as count,CorporateName from UsersModel
            inner join tbl_CorporateModel ON tbl_CorporateModel.Id = CorporateID
            where isVIP = 1 and Active = 1
            group by CorporateName";

            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<CorporateListing>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new CorporateListing();
                item.Company = dr["CorporateName"].ToString();
                item.UserCount = int.Parse(dr["count"].ToString());
                result.Add(item);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllUserCount(UsersCountFilter data)
        {
            var result = new List<UserCountListing>();
            UserCountListing item = new UserCountListing();
            int CorporateId = 0;
            int VIPCount = 0;
            int activeVIP = 0;

            string userSql = $@"select CorporateID from UsersModel where Username = '" + data.userName + "'";
            DataTable table = db.SelectDb(userSql).Tables[0];
            if(table.Rows.Count == 0)
            {
                return BadRequest("User Not Found");
            }
            else
            {
                foreach (DataRow dr in table.Rows)
                {
                    CorporateId = int.Parse(dr["CorporateID"].ToString());
                }
            }

            string sql = $@"select COUNT(*) as count from UsersModel 
                        where CorporateID = '" + CorporateId + "'";

            string registeredCount = sql + " AND Active = '1' and isVIP = 0";
            table = db.SelectDb(registeredCount).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                item.registered = int.Parse(dr["count"].ToString());
            }

            string unregisteredCount = sql + " AND Active = '2'";
            table = db.SelectDb(unregisteredCount).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                item.unregistered = int.Parse(dr["count"].ToString());
            }

            string isVIP = sql + " AND Active = '1' and isVIP = 1";
            table = db.SelectDb(isVIP).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                activeVIP = int.Parse(dr["count"].ToString());
                item.isVIP = activeVIP;
            }

            string vipCount = $@"select VipCount from tbl_CorporateModel where Id = '" + CorporateId + "'";
            table = db.SelectDb(vipCount).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                VIPCount = int.Parse(dr["VipCount"].ToString());
                item.totalVIP = VIPCount;
                item.remainingVIP = VIPCount - activeVIP;
            }

            result.Add(item);


            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UserCountWithFilter(UserListFilter data)
        {

            int pageSize = 10;
            //var model_result = (dynamic)null;
            var items = (dynamic)null;
            int totalItems = 0;
            int totalPages = 0;
            int totalVIP = 0;
            string page_size = pageSize == 0 ? "10" : pageSize.ToString();

            if (data.Corporatename.Equals("0") && data.Status.Equals("0"))
            {
                var Member = dbmet.GetUserList().ToList();
                totalItems = Member.Count;
                totalVIP = Member.Where(a => a.isVIP == "1").ToList().Count;
                totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));
                items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
            }
            else if (!data.Corporatename.Equals("0") && data.Status.Equals("0"))
            {
                var Member = dbmet.GetUserList().Where(a => a.Corporatename.ToLower() == data.Corporatename.ToLower()).ToList();
                totalItems = Member.Count;
                totalVIP = Member.Where(a => a.isVIP == "1").ToList().Count;
                totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));
                items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
            }
            else if (data.Corporatename.Equals("0") && !data.Status.Equals("0"))
            {
                var Member = dbmet.GetUserList().Where(a => a.status.ToLower() == data.Status.ToLower()).ToList();
                totalItems = Member.Count;
                totalVIP = Member.Where(a => a.isVIP == "1").ToList().Count;
                totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));
                items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
            }
            else
            {
                var Member = dbmet.GetUserList().Where(a => a.status.ToLower() == data.Status.ToLower() && a.Corporatename.ToLower() == data.Corporatename.ToLower()).ToList();
                totalItems = Member.Count;
                totalVIP = Member.Where(a => a.isVIP == "1").ToList().Count;
                totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));
                items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
            }
            
            var result = new List<PaginationCorpUserModel>();
            var item = new PaginationCorpUserModel();
            int pages = data.page == 0 ? 1 : data.page;
            item.CurrentPage = data.page == 0 ? "1" : data.page.ToString();

            int page_prev = pages - 1;
            //int t_record = int.Parse(items.Count.ToString()) / int.Parse(page_size);

            double t_records = Math.Ceiling(double.Parse(totalItems.ToString()) / double.Parse(page_size));
            int page_next = data.page >= t_records ? 0 : pages + 1;
            item.NextPage = items.Count % int.Parse(page_size) >= 0 ? page_next.ToString() : "0";
            item.PrevPage = pages == 1 ? "0" : page_prev.ToString();
            item.TotalPage = t_records.ToString();
            item.PageSize = page_size;
            item.TotalVIP = totalVIP.ToString();
            item.TotalRecord = totalItems.ToString();
            item.items = items;
            result.Add(item);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UnregisteredList(UnregisteredUserFilter data)
        {

            string sql;
            if (data.name == null)
            {
                sql = $@"select Coalesce(Fullname, Concat (Fname + ' ',Lname)) Name,Email from UsersModel where Active = '6'";
            }
            else
            {
                sql = $@"select Coalesce(Fullname, Concat (Fname + ' ',Lname)) Name,Email from UsersModel where Active = '6' and  CorporateId = '" + data.name + "'";
            }
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<UnregisteredResult>();
            foreach (DataRow dr in dt.Rows)
            {

                var item = new UnregisteredResult();
                item.Name = dr["Name"].ToString();
                item.Email = dr["Email"].ToString();
                item.Count = dt.Rows.Count;
                result.Add(item);
            }

            
            return Ok(result);
        }

        public class UsersCountFilter
        {
            public string userName { get; set; }
        }

        public class UserListFilter
        {
            public string Corporatename { get; set; }
            public string Status { get; set; }
            public string? FilterName { get; set; }
            public int page { get; set; }
        }

        public class UserCountFilter
        {
            public string? Corporatename { get; set; }
            public int page { get; set; }
        }
        public class UnregisteredUserFilter
        {
            public string? name { get; set; }
        }
        public class UnregisteredResult
        {
            public string Name { get; set; }
            public string Email { get; set; }

            public int Count { get; set; }
        }

        public class UnregisteredUserEmailRequest
        {
            public string Body { get; set; }
            public string[] Name { get; set; }
            public string[] Email { get; set; }
            //public List<UserListModel> UserList { get; set; }
        }

        public class UserListModel
        {
            public string Name { get; set; }
            public string Email { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> EmailUnregisterUserv2(UnregisteredUserEmailRequest data)
        {
            Console.Write(data.Name.Count());

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ALFARDAN OYSTER PRIVILEGE CLUB", "app@alfardan.com.qa"));
            //for (int x = 0; x < data.Name.Length; x++)
            //{
            //    message.To.Add(new MailboxAddress(data.Name[x], data.Email[x]));
            //}
            var recipients = data.Name.Zip(data.Email, (name, email) => new MailboxAddress(name, email)).ToList();

            // Add all recipients at once
            message.To.AddRange(recipients);

            message.Subject = "Email Registration Link";
            var bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = @"<style>
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
                                        height: 100vh; 
                                        width: 100vw;
                                        display: flex;
                                        justify-content: center;
                                        align-items: center;
                                        flex-direction: column; 
                                        background-size: cover;}
                                    .gradient-border {
                                        height: 600px;
                                        width: 700px; 
                                        display: flex;
                                        justify-content: center;
                                        background-color: transparent;
                                        border-width: 3px;
                                        box-sizing: content-box;
                                        border-style: solid;
                                        border-image-slice: 1;
                                        gap: 20px;
                                        border-image-source: 
                                            linear-gradient(
                                                180deg,
                                                #b07b29 17.26%,
                                                #ebcc77 31.95%,
                                                #b98732 53.29%,
                                                #ecce79 74.41%,
                                                #c69840 99.86%
                                            );
                                    flex-direction: column;
                                    }
                                    .login-container img {
                                    margin: 20px auto;
                                    width: 300px;
                                    height: 110px;
                                    }
                                    h1 {
                                        text-align: center;
                                        color: #d7d2cb;
                                        font-family: 'Montserrat-SemiBold';
                                        font-size: 2rem;
                                        font-style: italic;
                                    }
                                    h3 {
                                        text-align: center;
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
                                <body>
                                    <div class='login-container'>
                                    <div class='login-logo-conctainer'>
                                        <div class='gradient-border'>
                                        <img src='https://www.alfardanoysterprivilegeclub.com/assets/img/AOPC%20Logo%20-%20White.png' alt='AOPC' width='100%'' />

                                        <h1>
                                            WELCOME TO<br />ALFARDAN OYSTER <br />
                                            PRIVILEGE CLUB
                                        </h1>
                                        <h3>REGISTRATION FORM</h3>
                                        <a href='https://www.alfardanoysterprivilegeclub.com/user-registration'><h4> Click Here to Register in<br />Alfardan Oyster Privilege Club</h4></a>
                                        </div>
                                    </div>
                                    </div>
                                </body>";



            //bodyBuilder.HtmlBody = @" <style>
            //    body {
            //      margin: 0;
            //      box-sizing: border-box;
            //      display: flex;
            //      flex-direction: column;
            //      font-family: ""Montserrat"";
            //    }
            //    @font-face {
            //      font-family: ""Montserrat"";
            //      src: url(""https://www.alfardanoysterprivilegeclub.com/build/assets/Montserrat-Regular-dcfe8df2.ttf"");
            //    }
            //    .header {
            //      width: 200px;
            //      height: 120px;
            //      overflow: hidden;
            //      margin: 50px auto;
            //    }
            //    .body {
            //      width: 500px;
            //      margin: 5px auto;
            //      font-size: 13px;
            //    }
            //    .body p {
            //      margin: 20px 0;
            //    }
            //    ul li {
            //      list-style: none;
            //    }
            //    .footer {
            //      width: 500px;
            //      margin: 20px auto;
            //      font-size: 13px;
            //    }
            //    .citation span {
            //      color: #c89328;
            //    }
            //    .body span {
            //      color: #c89328;
            //    }
            //  </style>
            //  <body>
            //    <div class=""header"">
            //      <img
            //        src=""https://cms.alfardanoysterprivilegeclub.com/img/AOPCBlack.jpg""

            //        alt=""Alfardan Oyster Privilege Club""
            //        width=""100%""
            //      />
            //    </div>
            //    <div class=""body"">
            //      <p class=citation>Dear <span> Admin </span></p>
            //      <p class=body>
            //         " + data.Body + " </span>.</p><p class=body> " +
            //" </div> <p class=footer>Regards, <br />" +
            // " <br /> " +
            // "Alfardan Oyster Privilege Club App " +
            // "</p>" +
            // "</body>";
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
        public async Task<IActionResult> EmailUnregisterUser(UnregisteredUserEmailRequest data)
        {
            Console.Write(data.Name.Count());
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ALFARDAN OYSTER PRIVILEGE CLUB", "app@alfardan.com.qa"));
            //message.To.Add(new MailboxAddress("Ace Caspe", "ace.caspe@odecci.com"));
            //message.To.Add(new MailboxAddress("Marito Ace", data.Email));
            for (int x = 0; x < data.Name.Length; x++)
            {
                message.To.Add(new MailboxAddress(data.Name[x], data.Email[x]));
            }
            //message.Bcc.Add(new MailboxAddress("Marito Ace", "support@odecci.com"));
            //message.Bcc.Add(new MailboxAddress("Alfardan Marketing", "skassab@alfardan.com.qa"));
            //message.Bcc.Add(new MailboxAddress("Alfardan Marketing", "dulay@alfardan.com.qa"));
            message.Subject = "Test Only";
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
                    src=""https://cms.alfardanoysterprivilegeclub.com/img/AOPCBlack.jpg""

                    alt=""Alfardan Oyster Privilege Club""
                    width=""100%""
                  />
                </div>
                <div class=""body"">
                  <p class=citation>Dear <span> Admin </span></p>
                  <p class=body>
                     " + data.Body + " </span>.</p><p class=body> " +
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
        public async Task<IActionResult> CorporateUserCount(UserCountFilter data)
        {

            int pageSize = 10;
            //var model_result = (dynamic)null;
            var items = (dynamic)null;
            int totalItems = 0;
            int totalPages = 0;
            int totalVIP = 0;
            string page_size = pageSize == 0 ? "10" : pageSize.ToString();

            if (data.Corporatename == null)
            {
                var Member = dbmet.GetUserCountPerCorporate().ToList();
                totalItems = Member.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));
                items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
            }
            else
            {
                var Member = dbmet.GetUserCountPerCorporate().Where(a => a.CorporateName.ToLower().Contains(data.Corporatename.ToLower())).ToList();
                totalItems = Member.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));
                items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
            }

            var result = new List<PaginationCorpUserCountModel>();
            var item = new PaginationCorpUserCountModel();
            int pages = data.page == 0 ? 1 : data.page;
            item.CurrentPage = data.page == 0 ? "1" : data.page.ToString();

            int page_prev = pages - 1;
            //int t_record = int.Parse(items.Count.ToString()) / int.Parse(page_size);

            double t_records = Math.Ceiling(double.Parse(totalItems.ToString()) / double.Parse(page_size));
            int page_next = data.page >= t_records ? 0 : pages + 1;
            item.NextPage = items.Count % int.Parse(page_size) >= 0 ? page_next.ToString() : "0";
            item.PrevPage = pages == 1 ? "0" : page_prev.ToString();
            item.TotalPage = t_records.ToString();
            item.PageSize = page_size;
            item.TotalVIP = totalVIP.ToString();
            item.TotalRecord = totalItems.ToString();
            item.items = items;
            result.Add(item);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> CorporateUserCountAll()
        {
            //string sql = $@"select Corp.CorporateName,Coalesce(Reg.RegCount,0) 'Registered',Coalesce(UnReg.UnRegCount,0) 'Unregistered',Coalesce(VIP.VipCount,0) 'Registered VIP',Coalesce(TotVIP.Count,0) 'Total VIP Count',Coalesce(TotVIP.Count,0) - Coalesce(VIP.VipCount,0) 'Remaining VIP',TotVIP.Count 'User Count' ,Coalesce(Reg.RegCount,0)  + Coalesce(VIP.VipCount,0) 'Total User' from (select Id, CorporateName from tbl_CorporateModel group by Id,CorporateName)As Corp
            //left join (select CorporateID,Count(*) 'RegCount' from UsersModel where Active = '1' and isVIP = 0 group by CorporateID)Reg on Corp.Id = Reg.CorporateID
            //left join (select CorporateID,Count(*) 'UnRegCount' from UsersModel where Active = '6' group by CorporateID)UnReg on Corp.Id = UnReg.CorporateID
            //left join (select CorporateID,Count(*) 'VipCount' from UsersModel where Active = '1' and isVIP = 1 group by CorporateID)VIP on Corp.Id = VIP.CorporateID
            //left join (select Id,Coalesce(VipCount,0) 'Count',Count 'UserCount' from tbl_CorporateModel)TotVIP on Corp.Id = TotVIP.Id";
            string sql = $@"SELECT
	                        cm.Id
	                        ,cm.CorporateName
	                        ,sum(case when um.Active = '1' AND isVIP = 0  then 1 else 0 end) AS 'Registered'
	                        ,sum(case when um.Active = '6' then 1 else 0 end) AS 'Unregistered'
	                        ,sum(case when um.Active = '1' AND isVIP = 1 then 1 else 0 end) AS 'Registered VIP'
	                        ,cm.VipCount AS 'Total VIP Count'
	                        ,(cm.VipCount - sum(case when um.Active = '1' AND isVIP = 1 then 1 else 0 end)) AS 'Remaining VIP'
	                        ,cm.Count AS 'User Count'
	                        ,sum(case when um.Active = '1'  then 1 else 0 end) AS 'Total User'
                        FROM 
	                        tbl_CorporateModel cm WITH (NOLOCK)
                        LEFT JOIN
	                        UsersModel um WITH(NOLOCK)
	                        ON um.CorporateID = cm.Id 
                        WHERE cm.Status = '1'
                        GROUP BY cm.CorporateName, cm.VipCount, cm.Count,cm.Id";
            var result = new List<CorporateUserCountVM>();
            DataTable table = db.SelectDb(sql).Tables[0];


            foreach (DataRow dr in table.Rows)
            {
                var item = new CorporateUserCountVM();
                item.CorporateName = dr["CorporateName"].ToString();
                item.Registered = dr["Registered"].ToString();
                item.Unregistered = dr["Unregistered"].ToString();
                item.RegisteredVIP = dr["Registered VIP"].ToString();
                item.TotalVIP = dr["Total VIP Count"].ToString();
                item.RemainingVip = dr["Remaining Vip"].ToString();
                item.UserCount = dr["User Count"].ToString();
                item.TotalUser = dr["Total User"].ToString();
                result.Add(item);
            }

            return Ok(result);
        }

    }
}
