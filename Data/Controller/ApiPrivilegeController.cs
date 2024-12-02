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
using System.Collections;
using static AuthSystem.Data.Controller.ApiAdminAcessController;

namespace AuthSystem.Data.Controller
{

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiPrivilegeController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;


        public ApiPrivilegeController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {
   
            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;
   
        }
      
        [HttpGet]
        public async Task<IActionResult> PrivilegeCardList()
        {
            DataTable table = db.SelectDb_SP("SP_PrivilegeCardList").Tables[0];
            var result = new List<PrivilegeVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new PrivilegeVM();
                item.Id= int.Parse(dr["Id"].ToString());
                item.Title=dr["Title"].ToString();
                item.ImgUrl= dr["ImgUrl"].ToString();
                item.VendorID= dr["VendorID"].ToString();
                item.VendorName=dr["VendorName"].ToString();
                item.Validity = Convert.ToDateTime(dr["Validity"].ToString()).ToString("MM/dd/yyyy");
                item.FeatureImg= dr["FeatureImg"].ToString();
                item.VendorID= dr["VendorID"].ToString();
                item.Status= dr["Status"].ToString();
          
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> PrivilegeList()
        {
            string sql = $@"SELECT        tbl_PrivilegeModel.Id, tbl_PrivilegeModel.Title, tbl_PrivilegeModel.Description, tbl_PrivilegeModel.Validity, tbl_PrivilegeModel.NoExpiry, tbl_PrivilegeModel.DateCreated, tbl_PrivilegeModel.ImgUrl, 
                         tbl_PrivilegeModel.PrivilegeID, tbl_PrivilegeModel.isVIP, 
                         REPLACE(REPLACE(REPLACE(tbl_PrivilegeModel.TMC, '?	', CHAR(10)+CHAR(12)+'• '),'?', CHAR(10)+CHAR(12)+'  •'),CHAR(10),CHAR(10)+CHAR(12)) as TMC,
						 REPLACE(REPLACE(REPLACE(tbl_PrivilegeModel.Mechanics, '?	', CHAR(10)+CHAR(12)+'• '),'?', CHAR(10)+CHAR(12)+'  •'),CHAR(10),CHAR(10)+CHAR(12)) as Mechanics,
                         tbl_StatusModel.Name AS Status, tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, 
                         tbl_PrivilegeModel.BusinessTypeID, tbl_VendorModel.Id AS VendorID
FROM            tbl_PrivilegeModel INNER JOIN
                         tbl_StatusModel ON tbl_PrivilegeModel.Active = tbl_StatusModel.Id LEFT OUTER JOIN
                         tbl_BusinessTypeModel ON tbl_PrivilegeModel.BusinessTypeID = tbl_BusinessTypeModel.Id LEFT OUTER JOIN
                         tbl_VendorModel ON tbl_PrivilegeModel.VendorID = tbl_VendorModel.Id
WHERE        (tbl_PrivilegeModel.Active = 5)
ORDER BY tbl_PrivilegeModel.Id DESC";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<PrivilegeVM>();
            foreach (DataRow dr in dt.Rows)
            {

                string mecha = dr["Mechanics"].ToString();
                string tmc = dr["TMC"].ToString();
                if (!string.IsNullOrEmpty(mecha))
                {
                    string newString = mecha.Substring(0, mecha.Length - 1);

                }
                //if (!string.IsNullOrEmpty(tmc))
                //{
                //    string newString = tmc.Substring(0, tmc.Length - 1);

                //}
                var item = new PrivilegeVM();
                item.Id = int.Parse(dr["Id"].ToString());
                item.Title = dr["Title"].ToString();
                item.Description = dr["Description"].ToString();
                item.Validity =Convert.ToDateTime( dr["Validity"].ToString()).ToString("MM-dd-yyyy");
                item.noExpiry = int.Parse(dr["noExpiry"].ToString());
                item.ImgUrl = dr["ImgUrl"].ToString().Replace(" ","");
                item.PrivilegeID = dr["PrivilegeID"].ToString();
                item.isVIP = int.Parse(dr["isVIP"].ToString());
                item.TMC = dr["TMC"].ToString();
                item.Status = dr["Status"].ToString();
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.VendorName = dr["VendorName"].ToString();
                item.DateCreated =Convert.ToDateTime( dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                item.Mechanics = dr["Mechanics"].ToString();
                item.BusinessTypeID = dr["BusinessTypeID"].ToString();
                item.VendorID = dr["VendorID"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        public class PrivMemListItem
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string PrivilegeID { get; set; }
            public string MembershipID { get; set; }
            public string MembershipName { get; set; }
            public string UserCount { get; set; }
            public string VIPCount { get; set; }
            public string Status { get; set; }

        }
        public class privID
        {
            public string Id { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> PrivilegeMembershipList(privID data)
        {

            var result = new List<PrivMemListItem>();
            var stats = "";
            string sql2 = $@"SELECT        Title, Id AS PrivilegeID FROM   tbl_PrivilegeModel WHERE   (Active = 5)";
            DataTable dt2 = db.SelectDb(sql2).Tables[0];

            foreach (DataRow dr in dt2.Rows)
            {
                string sql = $@"SELECT        TOP (200) PrivilegeID, MembershipID, Count, VipCount
                                FROM            tbl_MembershipPrivilegeModel
                                WHERE        (MembershipID = '" + data.Id + "') AND (PrivilegeID = '" + dr["PrivilegeID"].ToString() + "')";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if (dt.Rows.Count != 0)
                {
                    stats = "1";
                }
                else
                {
                    stats = "0";
                }
                var item = new PrivMemListItem();
                item.Title = dr["Title"].ToString();
                item.PrivilegeID = dr["PrivilegeID"].ToString();
                item.MembershipID = data.Id;
                item.VIPCount = "0";
                item.UserCount = "0";
                item.Status = stats;
                result.Add(item);

            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> PrivilegeCardbyUser(UserIDVM data)
        {
            var param = new IDataParameter[]
            {
            new SqlParameter("@EmployeeID",data.EmployeeID)
            };
            DataTable table = db.SelectDb_SP("SP_GetAllPrivilegeCardbyUser", param).Tables[0];
            var result = new List<PrivilegeUserVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new PrivilegeUserVM();
                
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Title = dr["Title"].ToString();
                item.Validity = Convert.ToDateTime(dr["Validity"].ToString()).ToString("MM/dd/yyyy");
                item.PrivilegeImg = dr["PrivilegeImg"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                item.PrivilegeID = dr["PrivilegeID"].ToString();
                item.PrivilegeID = dr["PrivilegeID"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        static string RemoveLastSpecialChars(string inputStr)
        {
            int lastSpecialIndex = -1;

            for (int i = inputStr.Length - 1; i >= 0; i--)
            {
                if (!char.IsLetterOrDigit(inputStr[i]))
                {
                    lastSpecialIndex = i;
                    break;
                }
            }

            if (lastSpecialIndex != -1)
            {
                // Remove all occurrences of the last special character
                inputStr = inputStr.Remove(lastSpecialIndex, 1);
                return RemoveLastSpecialChars(inputStr);
            }

            return inputStr;
        }
        [HttpPost]
        public async Task<IActionResult> GetPrivilegeByPID(PrivilegeIDVM data)
        {
            var param = new IDataParameter[]
            {
            new SqlParameter("@PrivilegeID",data.PrivilegeID)
            };
            DataTable table = db.SelectDb_SP("SP_GetPrivilegeByPID", param).Tables[0];
            var item = new PrivilegeFIDVM();
            foreach (DataRow dr in table.Rows)
            {

                //string mecha = RemoveLastSpecialChars(dr["Mechanics"].ToString());
                //string tmc = RemoveLastSpecialChars(dr["TMC"].ToString());
                //if (!string.IsNullOrEmpty(mecha))
                //{
                //    string newString = mecha.Substring(0, mecha.Length - 1);
              
                //}
                //if (!string.IsNullOrEmpty(tmc))
                //{
                //    string newString = tmc.Substring(0, tmc.Length - 1);

                //}
                //int ctr = 0;
                //var res = "";
                //var ress = "";
       
                //var gal = dr["TMC"].ToString();
                //var mec = dr["Mechanics"].ToString();
                //string[] gallist = gal.Split("^");
                //string[] meclist = mec.Split("^");
                //foreach (string author in gallist)
                //{
             
                //    if (author != ""  && author != "||")
                //    {

                //        res += author.Replace("||","");
                     
                //        ctr++;

                //    }

                //}
                //foreach (string mechanics in meclist)
                //{

                //    if (mechanics != "")
                //    {

                //        ress += mechanics.Replace("||", "");

                //        ctr++;

                //    }

                //}
                var tmc = dr["PrivilegeID"].ToString();
                item.Title = dr["Title"].ToString();
                item.ImgUrl = dr["ImgUrl"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.PrivilegeID = dr["PrivilegeID"].ToString();
                item.Mechanics = dr["Mechanics"].ToString();
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.Description = dr["Description"].ToString();
                item.TMC = dr["TMC"].ToString();
                item.VendorName = dr["VendorName"].ToString();
                item.Validity = Convert.ToDateTime(dr["Validity"].ToString()).ToString("MM/dd/yyyy");


            }

            return Ok(item);
        }
        [HttpPost]
        public IActionResult SavePrivilege(PrivilegeVM data)
        {



            string sql = $@"select * from tbl_PrivilegeModel where id ='"+data.Id+"'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            if (dt.Rows.Count == 0)
            {
                if (data.ImgUrl== null || data.ImgUrl == "")
                {
                    imgfile = "https://www.alfardanoysterprivilegeclub.com/assets/img/defaultavatar.png";
                }
                else
                {
                    imgfile = "https://www.alfardanoysterprivilegeclub.com/assets/img/" +data.ImgUrl.Replace(" ", "%20");
                }
                string OTPInsert = $@"insert into tbl_PrivilegeModel (Title,Description,Mechanics,Validity,NoExpiry,ImgUrl,VendorID,isVIP,BusinessTypeID,TMC,Active) values 
                                     ('"+data.Title.Replace("'","''")+"','"+data.Description+"',N'"+data.Mechanics.Replace("'", "''") + "','"+data.Validity + "','"+data.noExpiry + "','"+ imgfile + "','"+data.VendorID + "','"+data.isVIP + "','"+data.BusinessTypeID + "',N'"+data.TMC.Replace("'", "''") + "','"+data.Active+"')";
                db.AUIDB_WithParam(OTPInsert);
                result.Status = "Successfully Added";

                return Ok(result);

            }
            else
            {

                if (data.ImgUrl == null || data.ImgUrl == "")
                {
                    imgfile = "https://www.alfardanoysterprivilegeclub.com/assets/img/defaultavatar.png";
                }
                else
                {
                    imgfile = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + data.ImgUrl.Replace(" ", "%20");
                }
                string OTPInsert = $@"update tbl_PrivilegeModel set Title='"+data.Title.Replace("'", "''") + "', Description='"+data.Description+"' , Mechanics=N'"+data.Mechanics.Replace("'", "''") + "', Validity='"+data.Validity+"', NoExpiry='"+data.noExpiry+"', ImgUrl='"+ imgfile + "', " +
                    "VendorID='"+data.VendorID+"', isVIP='"+data.isVIP+"', BusinessTypeID='"+data.BusinessTypeID+"', TMC=N'"+data.TMC.Replace("'", "''") + "' where id =" + data.Id + "";
                db.AUIDB_WithParam(OTPInsert);

                result.Status = "Successfully Updated";

                return Ok(result);
            }


            return Ok(result);
        }
        public class PrivMem
        {
            public string? privilegeID { get; set; }
            public string? usercount { get; set; }
            public string? vipcount { get; set; }
            public string? MembershipID { get; set; }
            public string? status { get; set; }
            public string? stats { get; set; }
        }
            [HttpPost]
        public IActionResult SavePrivilegeList(List<PrivMem> IdList)
        {
            string delete = $@"delete tbl_MembershipPrivilegeModel where MembershipID='" + IdList[0].MembershipID + "'";
            db.AUIDB_WithParam(delete);

            var result = new Registerstats();
            string imgfile = "";
           
            foreach (var emp in IdList)
            {
                if (emp.privilegeID != null)
                {
                    string sql = $@"SELECT        tbl_MembershipPrivilegeModel.Id, tbl_MembershipPrivilegeModel.PrivilegeID, tbl_MembershipPrivilegeModel.MembershipID, tbl_MembershipPrivilegeModel.Count, tbl_MembershipPrivilegeModel.VipCount, 
                             tbl_PrivilegeModel.Title
                             FROM            tbl_MembershipPrivilegeModel INNER JOIN
                             tbl_PrivilegeModel ON tbl_MembershipPrivilegeModel.PrivilegeID = tbl_PrivilegeModel.Id where    (tbl_MembershipPrivilegeModel.PrivilegeID = '" + emp.privilegeID + "')  and  (tbl_MembershipPrivilegeModel.MembershipID = '" + emp.MembershipID + "') ";
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    if (dt.Rows.Count == 0)
                    {
                        if (emp.status == "1")
                        {
                            //string delete = $@"delete tbl_MembershipPrivilegeModel where MembershipID='"+ data.MembershipID + "'";
                            //db.AUIDB_WithParam(delete);

                            string insert = $@"insert into tbl_MembershipPrivilegeModel (PrivilegeID,MembershipID,Count,VipCount) values 
                                             ('" + emp.privilegeID + "','" + emp.MembershipID + "','" + emp.usercount + "','" + emp.vipcount + "')";
                            db.AUIDB_WithParam(insert);

                        }

                    }
                    else
                    {


                    }
                }
                result.Status = "Successfully Added";

            }
          
            return Ok(result);
        }
        public class DeletePriv
        {

            public int Id { get; set; }
        }
        [HttpPost]
        public IActionResult DeletePrivilege(DeletePriv data)
        {

            string sql = $@"select * from tbl_PrivilegeModel where id ='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            if (dt.Rows.Count != 0)
            {
                
                string OTPInsert = $@"update tbl_PrivilegeModel set Active = 6 where id ='"+data.Id+"'";
                db.AUIDB_WithParam(OTPInsert);
                result.Status = "Succesfully deleted";

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
        public async Task<IActionResult> PrivilegeByUserAndBType(UserIDVM data)
        {
            string sql = $@"select * from UsersModel where EmployeeID ='" + data.EmployeeID + "' and isVIP = 1";
            var param = new IDataParameter[]
            {
            new SqlParameter("@EmployeeID",data.EmployeeID),
            new SqlParameter("@BusinessTypeName",data.BusinessType)
            };
            DataTable dt = db.SelectDb(sql).Tables[0];
            DataTable table = new DataTable();
            if (dt.Rows.Count != 0)
            {
                table = db.SelectDb_SP("Get_PrivilegeisVIP", param).Tables[0];
                var result = new List<PrivilegeUserVM>();
                foreach (DataRow dr in table.Rows)
                {
                    var item = new PrivilegeUserVM();

                    item.EmployeeID = dr["EmployeeID"].ToString();
                    item.Title = dr["Title"].ToString();
                    item.Validity = Convert.ToDateTime(dr["Validity"].ToString()).ToString("MM/dd/yyyy");
                    item.PrivilegeImg = dr["PrivilegeImg"].ToString();
                    item.VendorLogo = dr["VendorLogo"].ToString();
                    item.PrivilegeID = dr["PrivilegeID"].ToString();
                    item.VendorID = dr["VendorID"].ToString();
                    item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                    item.Mechanics = dr["BusinessTypeName"].ToString();
                    item.TMC = dr["TMC"].ToString();
                    item.Description = dr["BusinessTypeName"].ToString();

                    result.Add(item);
                }


                return Ok(result);
            }
            else
            {
                table = db.SelectDb_SP("SP_GetPFByUserAndBType", param).Tables[0];
                var result = new List<PrivilegeUserVM>();
                foreach (DataRow dr in table.Rows)
                {
                    var item = new PrivilegeUserVM();

                    item.EmployeeID = dr["EmployeeID"].ToString();
                    item.Title = dr["Title"].ToString();
                    item.Validity = Convert.ToDateTime(dr["Validity"].ToString()).ToString("MM/dd/yyyy");
                    item.PrivilegeImg = dr["PrivilegeImg"].ToString();
                    item.VendorLogo = dr["VendorLogo"].ToString();
                    item.PrivilegeID = dr["PrivilegeID"].ToString();
                    item.VendorID = dr["VendorID"].ToString();
                    item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                    item.Mechanics = dr["BusinessTypeName"].ToString();
                    item.TMC = dr["TMC"].ToString();
                    item.Description = dr["BusinessTypeName"].ToString();

                    result.Add(item);
                }

                return Ok(result);

            }


        }
        [HttpPost]
        public async Task<IActionResult> PrivilegeFilterIsVIPCMS(UserIDVM data)
        {
            string sql = "";
            string query = $@"select * from usersmodel where EmployeeID='" + data.EmployeeID + "' and isVIP = 1";
            DataTable dt = db.SelectDb(query).Tables[0];
            if (dt.Rows.Count != 0)
            {

                sql += $@"
            SELECT        UsersModel.Username, UsersModel.EmployeeID, tbl_PrivilegeModel.Title,tbl_CorporateModel.DateEnded as Validity, tbl_VendorModel.VendorID, tbl_BusinessTypeModel.BusinessTypeName, tbl_PrivilegeModel.PrivilegeID, 
                         tbl_PrivilegeModel.ImgUrl AS PrivilegeImg, tbl_VendorModel.FileUrl AS VendorLogo, tbl_PrivilegeModel.Description, tbl_PrivilegeModel.Mechanics, tbl_PrivilegeModel.TMC, tbl_BusinessTypeModel.Id
FROM            tbl_CorporatePrivilegeTierModel INNER JOIN
                         UsersModel ON tbl_CorporatePrivilegeTierModel.CorporateID = UsersModel.CorporateID INNER JOIN
						 tbl_CorporateModel on tbl_CorporateModel.id = UsersModel.CorporateID inner join
                         tbl_PrivilegeModel ON tbl_CorporatePrivilegeTierModel.PrivilegeId = tbl_PrivilegeModel.Id LEFT OUTER JOIN
                         tbl_VendorPrivilegeTierModel on tbl_PrivilegeModel.Id = tbl_VendorPrivilegeTierModel.PrivilegeID left join
                         tbl_VendorModel ON tbl_VendorPrivilegeTierModel.VendorID = tbl_VendorModel.Id LEFT OUTER JOIN
                         tbl_BusinessModel ON tbl_VendorModel.BusinessLocationID = tbl_BusinessModel.Id LEFT OUTER JOIN
                         tbl_BusinessTypeModel ON tbl_PrivilegeModel.BusinessTypeID = tbl_BusinessTypeModel.Id
WHERE       (UsersModel.EmployeeID = '" + data.EmployeeID + "') and tbl_PrivilegeModel.Active = 5 and  UsersModel.Active = 1";

            }
            else
            {
                sql += $@"    SELECT        UsersModel.Username, UsersModel.EmployeeID, tbl_PrivilegeModel.Title,tbl_CorporateModel.DateEnded as Validity, tbl_VendorModel.VendorID, tbl_BusinessTypeModel.BusinessTypeName, tbl_PrivilegeModel.PrivilegeID, 
                         tbl_PrivilegeModel.ImgUrl AS PrivilegeImg, tbl_VendorModel.FileUrl AS VendorLogo, tbl_PrivilegeModel.Description, tbl_PrivilegeModel.Mechanics, tbl_PrivilegeModel.TMC, tbl_BusinessTypeModel.Id
FROM            tbl_CorporatePrivilegeTierModel INNER JOIN
                         UsersModel ON tbl_CorporatePrivilegeTierModel.CorporateID = UsersModel.CorporateID INNER JOIN
						 tbl_CorporateModel on tbl_CorporateModel.id = UsersModel.CorporateID inner join
                         tbl_PrivilegeModel ON tbl_CorporatePrivilegeTierModel.PrivilegeId = tbl_PrivilegeModel.Id LEFT OUTER JOIN
                         tbl_VendorPrivilegeTierModel on tbl_PrivilegeModel.Id = tbl_VendorPrivilegeTierModel.PrivilegeID left join
                         tbl_VendorModel ON tbl_VendorPrivilegeTierModel.VendorID = tbl_VendorModel.Id LEFT OUTER JOIN
                         tbl_BusinessModel ON tbl_VendorModel.BusinessLocationID = tbl_BusinessModel.Id LEFT OUTER JOIN
                         tbl_BusinessTypeModel ON tbl_PrivilegeModel.BusinessTypeID = tbl_BusinessTypeModel.Id
WHERE       (UsersModel.EmployeeID = '" +data.EmployeeID + "') and tbl_CorporatePrivilegeTierModel.isVIP is null and tbl_PrivilegeModel.Active = 5 and  UsersModel.Active = 1 ";

            }
            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<PrivilegeUserVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new PrivilegeUserVM();

                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Title = dr["Title"].ToString();
                item.Validity = Convert.ToDateTime(dr["Validity"].ToString()).ToString("MM/dd/yyyy");
                item.PrivilegeImg = dr["PrivilegeImg"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                item.PrivilegeID = dr["PrivilegeID"].ToString();

                result.Add(item);
            }
           
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> PrivilegeFilterIsVIP(UserIDVM data)
        {
            string sql = "";
            string query = $@"select * from usersmodel where EmployeeID='" + data.EmployeeID + "' and isVIP = 1";
            DataTable dt = db.SelectDb(query).Tables[0];
            if (dt.Rows.Count != 0)
            {

                sql += $@"
            SELECT        UsersModel.Username, UsersModel.EmployeeID, tbl_PrivilegeModel.Title,tbl_CorporateModel.DateEnded as Validity, tbl_VendorModel.VendorID, tbl_BusinessTypeModel.BusinessTypeName, tbl_PrivilegeModel.PrivilegeID, 
                         tbl_PrivilegeModel.ImgUrl AS PrivilegeImg, tbl_VendorModel.FileUrl AS VendorLogo, tbl_PrivilegeModel.Description, tbl_PrivilegeModel.Mechanics, tbl_PrivilegeModel.TMC, tbl_BusinessTypeModel.Id
FROM            tbl_CorporatePrivilegeTierModel INNER JOIN
                         UsersModel ON tbl_CorporatePrivilegeTierModel.CorporateID = UsersModel.CorporateID INNER JOIN
						 tbl_CorporateModel on tbl_CorporateModel.id = UsersModel.CorporateID inner join
                         tbl_PrivilegeModel ON tbl_CorporatePrivilegeTierModel.PrivilegeId = tbl_PrivilegeModel.Id LEFT OUTER JOIN
                         tbl_VendorModel ON tbl_PrivilegeModel.VendorID = tbl_VendorModel.Id LEFT OUTER JOIN
                         tbl_BusinessModel ON tbl_VendorModel.BusinessLocationID = tbl_BusinessModel.Id LEFT OUTER JOIN
                         tbl_BusinessTypeModel ON tbl_PrivilegeModel.BusinessTypeID = tbl_BusinessTypeModel.Id
WHERE       (UsersModel.EmployeeID = '" + data.EmployeeID + "') and tbl_PrivilegeModel.Active = 5 and  UsersModel.Active = 1";

            }
            else
            {
                sql += $@"    SELECT        UsersModel.Username, UsersModel.EmployeeID, tbl_PrivilegeModel.Title,tbl_CorporateModel.DateEnded as Validity, tbl_VendorModel.VendorID, tbl_BusinessTypeModel.BusinessTypeName, tbl_PrivilegeModel.PrivilegeID, 
                         tbl_PrivilegeModel.ImgUrl AS PrivilegeImg, tbl_VendorModel.FileUrl AS VendorLogo, tbl_PrivilegeModel.Description, tbl_PrivilegeModel.Mechanics, tbl_PrivilegeModel.TMC, tbl_BusinessTypeModel.Id
FROM            tbl_CorporatePrivilegeTierModel INNER JOIN
                         UsersModel ON tbl_CorporatePrivilegeTierModel.CorporateID = UsersModel.CorporateID INNER JOIN
						 tbl_CorporateModel on tbl_CorporateModel.id = UsersModel.CorporateID inner join
                         tbl_PrivilegeModel ON tbl_CorporatePrivilegeTierModel.PrivilegeId = tbl_PrivilegeModel.Id LEFT OUTER JOIN
                         tbl_VendorModel ON tbl_PrivilegeModel.VendorID = tbl_VendorModel.Id LEFT OUTER JOIN
                         tbl_BusinessModel ON tbl_VendorModel.BusinessLocationID = tbl_BusinessModel.Id LEFT OUTER JOIN
                         tbl_BusinessTypeModel ON tbl_PrivilegeModel.BusinessTypeID = tbl_BusinessTypeModel.Id
WHERE       (UsersModel.EmployeeID = '" + data.EmployeeID + "') and tbl_CorporatePrivilegeTierModel.isVIP is null and tbl_PrivilegeModel.Active = 5 and  UsersModel.Active = 1 ";

            }
            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<PrivilegeUserVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new PrivilegeUserVM();

                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Title = dr["Title"].ToString();
                item.Validity = Convert.ToDateTime(dr["Validity"].ToString()).ToString("MM/dd/yyyy");
                item.PrivilegeImg = dr["PrivilegeImg"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                item.PrivilegeID = dr["PrivilegeID"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> GetPrivilegeFbyUandBtype(UserIDVM data)
        {
            var param = new IDataParameter[]
            {
            new SqlParameter("@EmployeeID",data.EmployeeID),
            new SqlParameter("@BusinessTypeID",data.BusinessType)
            };
       DataTable table = new DataTable();
            if (table.Rows.Count != 0)
            {
                table = db.SelectDb_SP("Get_PrivilegeisVIP", param).Tables[0];


            }
            else
            {
                table = db.SelectDb_SP("SP_GetPFByUserAndBType", param).Tables[0];
            }
            var result = new List<PrivilegeUserVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new PrivilegeUserVM();
                
                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Title = dr["Title"].ToString();
                item.Validity = Convert.ToDateTime(dr["Validity"].ToString()).ToString("MM/dd/yyyy");
                item.PrivilegeImg = dr["PrivilegeImg"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                item.PrivilegeID = dr["PrivilegeID"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.BusinessTypeID = dr["BusinessTypeID"].ToString();
                item.Mechanics = dr["Mechanics"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> GetMechanicsPrivilegeListbyEIDandBType(UserIDVM data)
        {
            var param = new IDataParameter[]
            {
            new SqlParameter("@EmployeeID",data.EmployeeID),
            new SqlParameter("@BusinessTypeID",data.BusinessType)
            };
            DataTable table = db.SelectDb_SP("SP_GetPrivilegeFbyUandBtype", param).Tables[0];
            var result = new List<PrivilegeUserVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new PrivilegeUserVM();

                item.EmployeeID = dr["EmployeeID"].ToString();
                item.Title = dr["Title"].ToString();
                item.Validity = Convert.ToDateTime(dr["Validity"].ToString()).ToString("MM/dd/yyyy");
                item.PrivilegeImg = dr["PrivilegeImg"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                item.PrivilegeID = dr["PrivilegeID"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.BusinessTypeID = dr["BusinessTypeID"].ToString();

                var datas = dr["Mechanics"].ToString().Split('^');
                var arraycount = datas.Count();
                string sublist_ = "";
                item.Mechanics = datas[0];
                for (int x = 1; x < datas.ToList().Count; x++)
                {

                    item.Submechanics += datas[x] + "^";
                }


                result.Add(item);
            }

            return Ok(result);
        }
        public class ArrayListtable
        {
            public string? arraylist { get; set; }
            public string? sublist { get; set; }

        }
        public class SubArrayListtable
        {
            public string? arraylist { get; set; }
            public string? sublist { get; set; }

        }
        public class PrivilegeUserVM
        {
            
            public string EmployeeID { get; set; }
            public string BusinessTypeID { get; set; }
            public string Title { get; set; }
            public string Validity { get; set; }
            public string PrivilegeImg { get; set; }
            public string VendorID { get; set; }
            public string BusinessTypeName { get; set; }
            public string VendorLogo { get; set; }
            public string PrivilegeID { get; set; }
            public string Mechanics { get; set; }
            public string FileUrl { get; set; }
            public string Submechanics { get; set; }
            public string Description { get; set; }
            public string TMC { get; set; }


        }
    }
}
