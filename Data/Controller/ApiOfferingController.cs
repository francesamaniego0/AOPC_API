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
using static AuthSystem.Data.Controller.ApiVendorController;
using static AuthSystem.Data.Controller.ApiPrivilegeController;
using MimeKit;
using static AuthSystem.Data.Controller.ApiUserAcessController;
using static System.Net.Mime.MediaTypeNames;

namespace AuthSystem.Data.Controller
{

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiOfferingController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;


        public ApiOfferingController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {
   
            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;
   
        }
      
        //[HttpGet]
        //public async Task<IActionResult> OfferingList(string? id)
        //{
        //    string sql = "";
        //    //DataTable table = db.SelectDb_SP("SP_OfferingList").Tables[0];
        //    string test = id;
        //       var result = new List<OfferingVM>();
        //    if (test == null)
        //    {
        //        //all
        //        DataTable table = db.SelectDb_SP("SP_OfferingList").Tables[0];
         
        //    foreach (DataRow dr in table.Rows)
        //    {
        //        var item = new OfferingVM();
        //        item.Id= int.Parse(dr["Id"].ToString());
        //        item.BusinessTypeName=dr["BusinessTypeName"].ToString();
        //        item.VendorName= dr["VendorName"].ToString();
        //        item.PromoReleaseText= dr["PromoReleaseText"].ToString();
        //        item.OfferingName=dr["OfferingName"].ToString();
        //        item.MembershipName=dr["MembershipName"].ToString();
        //        item.VendorID= dr["VendorID"].ToString();
        //        item.ImgUrl= dr["ImgUrl"].ToString();
        //        item.OfferingID= dr["OfferingID"].ToString();
        //        item.Status= dr["Status"].ToString();
          
        //        result.Add(item);
        //    }

        //    return Ok(result);
        //    }
        //    else
        //    {
        //        //get filter with id
        //        string sqls = $@"select Name from tbl_MembershipModel where Status = 5 ";
        //        DataTable tables = db.SelectDb(sqls).Tables[0];
           
        //        foreach (DataRow dr in tables.Rows)
        //        {
              
        //            var item = new OfferingVM();
        //            switch (dr["Name"].ToString())
        //            {
        //                case "BRONZE":
        //                    sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
        //                 tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
        //                FROM            tbl_OfferingModel INNER JOIN
        //                                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id INNER JOIN
        //                                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id INNER JOIN
        //                                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id INNER JOIN
        //                                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id
        //                WHERE        (tbl_OfferingModel.StatusID = 5) and tbl_MembershipModel.Name = 'BRONZE' ";

        //                    break;
        //                case "SILVER":
        //                    sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
        //                 tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
        //                FROM            tbl_OfferingModel INNER JOIN
        //                                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id INNER JOIN
        //                                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id INNER JOIN
        //                                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id INNER JOIN
        //                                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id
        //                WHERE        (tbl_OfferingModel.StatusID = 5) and tbl_MembershipModel.Name in ('BRONZE','SILVER')";
        //                    break;
        //                case "GOLD":
        //                    sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
        //                 tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
        //                FROM            tbl_OfferingModel INNER JOIN
        //                                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id INNER JOIN
        //                                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id INNER JOIN
        //                                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id INNER JOIN
        //                                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id
        //                WHERE        (tbl_OfferingModel.StatusID = 5) and tbl_MembershipModel.Name in ('BRONZE','SILVER','GOLD')";
        //                    break;
        //                case "PLATINUM":
        //                    sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
        //                 tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
        //                FROM            tbl_OfferingModel INNER JOIN
        //                                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id INNER JOIN
        //                                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id INNER JOIN
        //                                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id INNER JOIN
        //                                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id
        //                WHERE        (tbl_OfferingModel.StatusID = 5) and tbl_MembershipModel.Name in ('BRONZE','SILVER','GOLD','PLATINUM')";
        //                    break;
        //                case "EXCLUSIVE ":

        //                    sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
        //                 tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
        //                FROM            tbl_OfferingModel INNER JOIN
        //                                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id INNER JOIN
        //                                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id INNER JOIN
        //                                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id INNER JOIN
        //                                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id
        //                WHERE        (tbl_OfferingModel.StatusID = 5) ";

        //                    break;

        //                default:
        //                    break;
        //            }
        //            DataTable dt = db.SelectDb(sql).Tables[0];
        //            foreach (DataRow dr_ in dt.Rows)
        //            {
        //                item.Id = int.Parse(dr_["Id"].ToString());
        //                item.BusinessTypeName = dr_["BusinessTypeName"].ToString();
        //                item.VendorName = dr_["VendorName"].ToString();
        //                item.PromoReleaseText = dr_["PromoReleaseText"].ToString();
        //                item.OfferingName = dr_["OfferingName"].ToString();
        //                item.MembershipName = dr_["MembershipName"].ToString();
        //                item.VendorID = dr_["VendorID"].ToString();
        //                item.ImgUrl = dr_["ImgUrl"].ToString();
        //                item.OfferingID = dr_["OfferingID"].ToString();
        //                item.Status = dr_["Status"].ToString();

        //                result.Add(item);
        //            }
        //        }

              
              
        //    }

        //    return Ok(result);
        //}  
         [HttpGet] // change to post
        public async Task<IActionResult> OfferingList(string? id)
        {
            string sql = "";
            //DataTable table = db.SelectDb_SP("SP_OfferingList").Tables[0];
            string test = id;
               var result = new List<OfferingVM>();
            if (test == null)
            {
                //all
                DataTable table = db.SelectDb_SP("SP_OfferingList").Tables[0];
         
                foreach (DataRow dr in table.Rows)
                {
                    var item = new OfferingVM();
                    item.Id= int.Parse(dr["Id"].ToString());
                    item.BusinessTypeName=dr["BusinessTypeName"].ToString();
                    item.VendorName= dr["VendorName"].ToString();
                    item.PromoReleaseText= dr["PromoReleaseText"].ToString();
                    item.OfferingName=dr["OfferingName"].ToString();
                    item.MembershipName=dr["MembershipName"].ToString();
                    item.VendorID= dr["VendorID"].ToString();
                    item.ImgUrl= dr["ImgUrl"].ToString();
                    item.OfferingID= dr["OfferingID"].ToString();
                    item.Status= dr["Status"].ToString();
          
                    result.Add(item);
                }

                return Ok(result);
            }
            else
            {
                //get filter with id
                string sqls = $@"select Name from tbl_MembershipModel inner JOIN
tbl_CorporateModel on tbl_CorporateModel.MembershipID = tbl_MembershipModel.Id inner JOIN
UsersModel on tbl_CorporateModel.Id = UsersModel.CorporateID

where tbl_MembershipModel.Status = 5 and UsersModel.Id = '"+id+"'";
                DataTable tables = db.SelectDb(sqls).Tables[0];
           
                foreach (DataRow dr in tables.Rows)
                {
              
                  
                    switch (dr["Name"].ToString())
                    {
                        case "BRONZE":
                            sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
                         tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
                        FROM            tbl_OfferingModel left JOIN
                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id left JOIN
                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id LEFT JOIN
                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id left JOIN
                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id 
                     
                        WHERE        (tbl_OfferingModel.StatusID = 5)   and tbl_MembershipModel.Name in ('BRONZE') ";

                            break;
                        case "SILVER":
                            sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
                         tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
                        FROM            tbl_OfferingModel left JOIN
                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id left JOIN
                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id LEFT JOIN
                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id left JOIN
                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id 
                     
                        WHERE        (tbl_OfferingModel.StatusID = 5)   and tbl_MembershipModel.Name in ('BRONZE','SILVER')";
                            break;
                        case "GOLD":
                            sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
                         tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
                        FROM            tbl_OfferingModel left JOIN
                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id left JOIN
                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id LEFT JOIN
                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id left JOIN
                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id 
                     
                        WHERE        (tbl_OfferingModel.StatusID = 5)   and tbl_MembershipModel.Name in ('BRONZE','SILVER','GOLD')";
                            break;
                        case "PLATINUM":
                            sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
                         tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
                        FROM            tbl_OfferingModel left JOIN
                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id left JOIN
                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id LEFT JOIN
                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id left JOIN
                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id 
                     
                        WHERE        (tbl_OfferingModel.StatusID = 5)   and tbl_MembershipModel.Name in ('BRONZE','SILVER','GOLD','PLATINUM')";
                            break;
                        case "EXCLUSIVE":

                            sql = $@"
            SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
                                     tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.StatusID
                                    FROM            tbl_OfferingModel left JOIN
                                     tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id left JOIN
                                     tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id LEFT JOIN
                                     tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id left JOIN
                                     tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id 
                     
                        WHERE        (tbl_OfferingModel.StatusID = 5)   ";

                            break;

                        default:
                            break;
                    }
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    foreach (DataRow dr_ in dt.Rows)
                    {
                        var item = new OfferingVM();
                        item.Id = int.Parse(dr_["Id"].ToString());
                        item.BusinessTypeName = dr_["BusinessTypeName"].ToString();
                        item.VendorName = dr_["VendorName"].ToString();
                        item.PromoReleaseText = dr_["PromoReleaseText"].ToString();
                        item.OfferingName = dr_["OfferingName"].ToString();
                        item.MembershipName = dr_["MembershipName"].ToString();
                        item.VendorID = dr_["VendorID"].ToString();
                        item.ImgUrl = dr_["ImgUrl"].ToString();
                        item.OfferingID = dr_["OfferingID"].ToString();
                        item.Status = dr_["Status"].ToString();

                        result.Add(item);
                    }
                }

              
              
            }

            return Ok(result);
        }  
        [HttpGet]
        public async Task<IActionResult> CMSOfferingList()
        {
            string sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
                         tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.PromoDesc, tbl_OfferingModel.Url, tbl_OfferingModel.OfferDays, 
                         tbl_OfferingModel.StartDate, tbl_OfferingModel.EndDate, tbl_OfferingModel.FromTime, tbl_OfferingModel.ToTime, tbl_OfferingModel.DateCreated, tbl_MembershipModel.Id AS memid, tbl_BusinessTypeModel.Id AS btypeid, 
                         tbl_VendorModel.Id AS vid
FROM            tbl_OfferingModel LEFT OUTER JOIN
                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id LEFT OUTER JOIN
                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id LEFT OUTER JOIN
                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id LEFT OUTER JOIN
                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id
WHERE        (tbl_OfferingModel.StatusID = 5) order by id desc";

            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<OfferingVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new OfferingVM();
                item.Id= int.Parse(dr["Id"].ToString());
                item.OfferingName= dr["OfferingName"].ToString();
                item.OfferingID= dr["OfferingID"].ToString();
                item.ImgUrl= dr["ImgUrl"].ToString();
                item.BusinessTypeName= dr["BusinessTypeName"].ToString();
                item.VendorName= dr["VendorName"].ToString();
                item.MembershipName= dr["MembershipName"].ToString();
                item.PromoDesc= dr["PromoDesc"].ToString();
                item.PromoReleaseText= dr["PromoReleaseText"].ToString();
                item.URL= dr["URL"].ToString();
                item.Offerdays= dr["Offerdays"].ToString();
                item.StartDateTime= Convert.ToDateTime(dr["StartDate"].ToString()).ToString("MM-dd-yyyy");
                item.EndDateTime = Convert.ToDateTime(dr["EndDate"].ToString()).ToString("MM-dd-yyyy");
                item.FromTime= dr["FromTime"].ToString();
                item.ToTime= dr["ToTime"].ToString();
                item.Status= dr["Status"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                item.BusinessTypeID = dr["btypeid"].ToString();
                item.MembershipID = dr["memid"].ToString();
                item.VendorID = dr["vid"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        public class BtypeModel
        {

            public string BusinessTypeName { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> OfferingsFilteredbyVID(VendorIDVM data)
        {
            var param = new IDataParameter[]
                       {
               new SqlParameter("@VendorID",data.vendorID)
                       };
            DataTable table = db.SelectDb_SP("SP_GetOfferingFilteredbyVID", param).Tables[0];
            var result = new List<OfferingVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new OfferingVM();
                item.Id = int.Parse(dr["Id"].ToString());
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.VendorName = dr["VendorName"].ToString();
                item.PromoReleaseText = dr["PromoReleaseText"].ToString();
                item.OfferingName = dr["OfferingName"].ToString();
                item.MembershipName = dr["MembershipName"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.ImgUrl = dr["ImgUrl"].ToString();
                item.OfferingID = dr["OfferingID"].ToString();
                item.Status = dr["Status"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }

        public class offeridID
        {
            public string? OfferingID { get; set; }

        }
        [HttpPost]
        public async Task<IActionResult> OfferingsFilteredbyID(offeridID data)
        {
           
                string sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_OfferingModel.PromoReleaseText, tbl_OfferingModel.OfferingName, tbl_MembershipModel.Name AS MembershipName, 
                         tbl_VendorModel.VendorID, tbl_OfferingModel.ImgUrl, tbl_OfferingModel.Id, tbl_OfferingModel.OfferingID, tbl_StatusModel.Name AS Status, tbl_OfferingModel.Url, tbl_OfferingModel.OfferDays, tbl_OfferingModel.StartDate, 
                         tbl_OfferingModel.EndDate, tbl_OfferingModel.PromoDesc, tbl_OfferingModel.FromTime, tbl_OfferingModel.ToTime
FROM            tbl_OfferingModel INNER JOIN
                         tbl_BusinessTypeModel ON tbl_OfferingModel.BusinessTypeID = tbl_BusinessTypeModel.Id INNER JOIN
                         tbl_VendorModel ON tbl_OfferingModel.VendorID = tbl_VendorModel.Id INNER JOIN
                         tbl_MembershipModel ON tbl_OfferingModel.MembershipID = tbl_MembershipModel.Id INNER JOIN
                         tbl_StatusModel ON tbl_OfferingModel.StatusID = tbl_StatusModel.Id
WHERE        (tbl_OfferingModel.OfferingID = '" +data.OfferingID + "') and StatusID=5";
            
        

            DataTable table = db.SelectDb(sql).Tables[0];

            var item = new OfferingVM();
            if (table.Rows.Count != 0 )
            {

        
                foreach (DataRow dr in table.Rows)
                {
            
                item.Id = int.Parse(dr["Id"].ToString());
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.VendorName = dr["VendorName"].ToString();
                item.PromoReleaseText = dr["PromoReleaseText"].ToString();
                item.OfferingName = dr["OfferingName"].ToString();
                item.MembershipName = dr["MembershipName"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.ImgUrl = dr["ImgUrl"].ToString();
                item.OfferingID = dr["OfferingID"].ToString();
                item.Status = dr["Status"].ToString();
                item.URL = dr["URL"].ToString();
                item.Offerdays = dr["Offerdays"].ToString();
                item.StartDateTime = Convert.ToDateTime(dr["StartDate"].ToString()).ToString("MM/dd/yyyy");
                item.EndDateTime = Convert.ToDateTime(dr["EndDate"].ToString()).ToString("MM/dd/yyyy");
                item.PromoDesc = dr["PromoDesc"].ToString();
                item.FromTime = dr["FromTime"].ToString();
                item.ToTime = dr["ToTime"].ToString();

                }
            
            }
            else
            {
                return BadRequest();
            }

                return Ok(item);
        }
        [HttpGet]
        public async Task<IActionResult> UserListEmail()
        {
            string sql = $@"select Concat (Fname , ' ' , Lname) as Fullname, Email from UsersModel where AllowEmailNotif =  1";

            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<Userlist>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new Userlist();
                item.Fullname = dr["Fullname"].ToString();
                item.Email = dr["Email"].ToString();
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpPost] //add param userid
        public async Task<IActionResult> OfferingFilterList(BtypeModel data)
        {
            var param = new IDataParameter[]
                       {
               new SqlParameter("@BusinessTypeName",data.BusinessTypeName)
                       };
            DataTable table = db.SelectDb_SP("SP_GetOfferingFilterList", param).Tables[0];
            var result = new List<OfferingVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new OfferingVM();
                item.Id = int.Parse(dr["Id"].ToString());
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.VendorName = dr["VendorName"].ToString();
                item.PromoReleaseText = dr["PromoReleaseText"].ToString();
                item.OfferingName = dr["OfferingName"].ToString();
                item.MembershipName = dr["MembershipName"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.ImgUrl = dr["ImgUrl"].ToString();
                item.OfferingID = dr["OfferingID"].ToString();
                item.Status = dr["Status"].ToString();

                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetVendorOfferingList()
        {
            GlobalVariables gv = new GlobalVariables();

            var result = new List<VendorOfferingVM>();
            DataTable table = db.SelectDb_SP("SP_GetVendorOfferings").Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var item = new VendorOfferingVM();
                item.Id = int.Parse(dr["id"].ToString());
                item.VendorName = dr["VendorName"].ToString();
                item.Description = dr["Description"].ToString();
                item.Services = dr["Services"].ToString();
                item.WebsiteUrl = dr["WebsiteUrl"].ToString();
                item.FeatureImg = dr["FeatureImg"].ToString();
                item.Gallery = dr["Gallery"].ToString();
                item.Cno = dr["Cno"].ToString();
                item.Email = dr["Email"].ToString();
                item.VideoUrl = dr["VideoUrl"].ToString();
                item.VrUrl = dr["VrUrl"].ToString();
                item.OfferingDesc = dr["OfferingDesc"].ToString();
                item.PromoDesc = dr["PromoDesc"].ToString();
                item.Expiry = dr["Expiry"].ToString();
                item.MembershipName = dr["MembershipName"].ToString();
                item.DateEnded = Convert.ToDateTime(dr["DateEnded"].ToString()).ToString("MM/dd/yyyy");
                item.DateUsed = Convert.ToDateTime(dr["DateUsed"].ToString()).ToString("MM/dd/yyyy");
                item.PromoReleaseText = dr["PromoReleaseText"].ToString();
                item.Country = dr["Country"].ToString();
                item.City = dr["City"].ToString();
                item.PostalCode = dr["PostalCode"].ToString();
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.BusinessTypeDesc = dr["BusinessTypeDesc"].ToString();
                item.BusinessName = dr["BusinessName"].ToString();
                item.BusinessDesc = dr["BusinessDesc"].ToString();
                item.Address = dr["Address"].ToString();
                item.BusinessEmail = dr["BusinessEmail"].ToString();
                item.BusinessCno = dr["BusinessCno"].ToString();
                item.Url = dr["Url"].ToString();
                item.BusinessServ = dr["BusinessServ"].ToString();
                item.ImageUrl = dr["FeaturedImg"].ToString();
                item.BusinessGallery = dr["BusinessGallery"].ToString();
                item.FileUrl = dr["FileUrl"].ToString();
                item.Map = dr["Map"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                

                result.Add(item);
            }

            return Ok(result);
        }
        public class Registerstats
        {
            public string Status { get; set; }

        }  
        public class Userlist
        {
            public string Fullname { get; set; }
            public string Email { get; set; }

        }
        [HttpPost]
        public IActionResult SaveOffering(OfferingVM data)
        {

            string sql_ = "";
            string sql= "";

            sql = $@"select * from tbl_OfferingModel where id ='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            string FeaturedImage = "";
            string res_image = "";
            int image_ = 0;
            //if (data.Id != 0)
            //{
            //    sql_ += $@"select Top(1) OfferingID from tbl_OfferingModel where StatusID =5 and id='" + data.Id + "' order by id desc  ";
            //    DataTable table = db.SelectDb(sql_).Tables[0];
            //    string str = table.Rows[0]["OfferingID"].ToString();
            //    res_image = str;
            //}
            //else
            //{
            //    sql_ += $@"select Top(1) OfferingID from tbl_OfferingModel where StatusID =5  order by id desc  ";
            //    DataTable table = db.SelectDb(sql_).Tables[0];
            //    if (table.Rows.Count != 0)
            //    {
            //        string str = table.Rows[0]["OfferingID"].ToString();
            //        image_ = int.Parse(str.Replace("Offering-", "")) + 1;
            //        res_image = "Offering-0" + image_;
            //    }
            //}
            if (data.ImgUrl == null || data.ImgUrl == string.Empty)
            {
                FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/defaultavatar.png";
            }
            else
            {
                FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + data.ImgUrl.Replace(" ", "%20");
            }
            if (dt.Rows.Count == 0)
            {
                sql = $@"select * from tbl_OfferingModel where OfferingName='" + data.OfferingName.Replace("'","''") + "' and StatusID = 5";
                DataTable dt2 = db.SelectDb(sql).Tables[0];
                if (dt2.Rows.Count  != 0)
                {
                    result.Status = "Duplicate Offering Name";
                    return BadRequest(result);
                }
                else
                {

                    if (data.MembershipID == "ALL")
                    {
                       
                            string Insert = $@"insert into tbl_OfferingModel (VendorID,MembershipID,BusinessTypeID,OfferingName,PromoDesc,PromoReleaseText,ImgUrl,StatusID,PrivilegeID,Url,OfferDays,StartDate,EndDate,FromTime,ToTime) values 
                                   ('" + data.VendorID + "','10','" + data.BusinessTypeID + "','" + data.OfferingName.Replace("'", "''") + "','" + data.PromoDesc.Replace("'", "''") + "','" + data.PromoReleaseText + "','" + FeaturedImage + "',5,'" + data.PrivilegeID + "'" +
                                   ",'" + data.URL + "','" + data.Offerdays + "','" + data.StartDateTime + "','" + data.EndDateTime + "','" + data.FromTime + "','" + data.ToTime + "')";
                           db.AUIDB_WithParam(Insert);
                        
                    }
                    else
                    {
                        string Insert = $@"insert into tbl_OfferingModel (VendorID,MembershipID,BusinessTypeID,OfferingName,PromoDesc,PromoReleaseText,ImgUrl,StatusID,PrivilegeID,Url,OfferDays,StartDate,EndDate,FromTime,ToTime) values 
                                   ('" + data.VendorID + "','" + data.MembershipID + "','" + data.BusinessTypeID + "','" + data.OfferingName.Replace("'", "''") + "','" + data.PromoDesc.Replace("'", "''") + "','" + data.EndDateTime + "','" + FeaturedImage + "',5,'" + data.PrivilegeID + "'" +
                                    ",'" + data.URL + "','" + data.Offerdays + "','" + data.StartDateTime + "','" + data.EndDateTime + "','" + data.FromTime + "','" + data.ToTime + "')";
                        db.AUIDB_WithParam(Insert);
                  
                    }
                    result.Status = "Successfully Added";

                    return Ok(result);
                }
      

            }
            else
            {
                if (data.MembershipID == "ALL")
                {
                    string OTPInsert = $@"update tbl_OfferingModel set VendorID='" + data.VendorID + "' ,MembershipID='10' ,BusinessTypeID='" + data.BusinessTypeID + "' ,OfferingName='" + data.OfferingName.Replace("'", "''") + "' ,PromoDesc='"
                + data.PromoDesc.Replace("'", "''") + "' ,PromoReleaseText='" + data.EndDateTime + "' ,ImgUrl='" + FeaturedImage + "' ,StatusID='5' ,PrivilegeID='' ,Url='" + data.URL + "' ,OfferDays='" + data.Offerdays + "' ,StartDate='"
                + data.StartDateTime + "' ,EndDate='" + data.EndDateTime + "' ,FromTime='" + data.FromTime + "' ,ToTime='" + data.ToTime + "'  where id =" + data.Id + "";
                    db.AUIDB_WithParam(OTPInsert);
                    result.Status = "Successfully Updated";

                    return Ok(result);
                }
                else
                {
                    string OTPInsert = $@"update tbl_OfferingModel set VendorID='" + data.VendorID + "' ,MembershipID='" + data.MembershipID + "' ,BusinessTypeID='" + data.BusinessTypeID + "' ,OfferingName='" + data.OfferingName.Replace("'", "''") + "' ,PromoDesc='"
                    + data.PromoDesc.Replace("'", "''") + "' ,PromoReleaseText='" + data.EndDateTime + "' ,ImgUrl='" + FeaturedImage + "' ,StatusID='5' ,PrivilegeID='' ,Url='" + data.URL + "' ,OfferDays='" + data.Offerdays + "' ,StartDate='"
                    + data.StartDateTime + "' ,EndDate='" + data.EndDateTime + "' ,FromTime='" + data.FromTime + "' ,ToTime='" + data.ToTime + "'  where id =" + data.Id + "";
                    db.AUIDB_WithParam(OTPInsert);
                    result.Status = "Successfully Updated";

                    return Ok(result);

                }
            }


            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> ImportOffering(List<OfferingVM> list)
        {
            string result = "";
            string FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/defaultavatar.png";
            try
            {

                for (int i = 0; i < list.Count; i++)
                {
                    
                    string sql = $@"select * from tbl_OfferingModel where OfferingName='" + list[i].OfferingName.Replace("'", "''") + "' and StatusID = 5";
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    if (dt.Rows.Count == 0)
                    {
                        string Insert = $@"insert into tbl_OfferingModel (VendorID,MembershipID,BusinessTypeID,OfferingName,PromoDesc,PromoReleaseText,ImgUrl,StatusID,PrivilegeID,Url,OfferDays,StartDate,EndDate,FromTime,ToTime) values 
                                   ('" + list[i].VendorID + "','"+ list[i].MembershipID +"','" + list[i].BusinessTypeID + "','" + list[i].OfferingName.Replace("'", "''") + "','" + list[i].PromoDesc.Replace("'", "''") + "','" + list[i].PromoReleaseText + "','" + FeaturedImage + "',5,'" + list[i].PrivilegeID + "'" +
                                   ",'" + list[i].URL + "','" + list[i].Offerdays + "','" + list[i].StartDateTime + "','" + list[i].EndDateTime + "','" + list[i].FromTime + "','" + list[i].ToTime + "')";
                        db.AUIDB_WithParam(Insert);

                        _global.Status = "Successfully Saved.";
                    }
                    else
                    {

                        _global.Status = "Duplicate Entry.";
                    }

                }
                result = "Registered Successfully";


            }
            catch (Exception ex)
            {
                _global.Status = ex.GetBaseException().ToString();

            }

            return Content(_global.Status);
        }
        public class DeleteOffer
        {

            public int Id { get; set; }
        }
        [HttpPost]
        public IActionResult DeleteOfferingList(List<DeleteOffer> IdList)
        {
            //string delete = $@"delete tbl_MembershipPrivilegeModel where MembershipID='" + IdList[0].MembershipID + "'";
            //db.AUIDB_WithParam(delete);
            var result = new Registerstats();
            string imgfile = "";

            foreach (var emp in IdList)
            {
                string delete = $@"update tbl_OfferingModel set StatusID = 6 where id ='" + emp.Id + "'";
                db.AUIDB_WithParam(delete);
            }
            result.Status = "Successfully Added";

            

            return Ok(result);
        }
        public class UserEmail
        {

            public string email { get; set; }
            public string offerid { get; set; }
        }
        [HttpPost]
        public IActionResult SendEmail(List<UserEmail> IdList)
        {
            //string delete = $@"delete tbl_MembershipPrivilegeModel where MembershipID='" + IdList[0].MembershipID + "'";
            //db.AUIDB_WithParam(delete);
            var result = new Registerstats();
            string imgfile = "";

            foreach (var emp in IdList)
            {
                //var emailsend = "https://www.alfardanoysterprivilegeclub.com/change-password/" + email;
                //var message = new MimeMessage();
                //message.From.Add(new MailboxAddress("AOPC Registration", "app@alfardan.com.qa"));
                //message.To.Add(new MailboxAddress("", data.Email));
                //message.Subject = "Email Registration Link";
                //var bodyBuilder = new BodyBuilder();
                //string img = "../img/AOPCBlack.jpg";
                //bodyBuilder.HtmlBody = @"<!DOCTYPE html>
                //<html lang=""en"">
                //<head>
                //    <meta charset=""UTF-8"">
                //    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                //    <meta http-equiv=""X-UA-Compatible"" content=""ie=edge"">
                //    <title>Oyster Privilege Club</title>
                //</head>
                //<style>
                //    @font-face {
                //    font-family: 'Montserrat-Reg';
                //    src: 
                //    url('{{ config('app.url') }}/assets/fonts/Montserrat/Montserrat-Regular.ttf');
                //    }
                //    @font-face {
                //        font-family: 'Montserrat-SemiBold';
                //        src: url('{{ config('app.url') }}/assets/fonts/Montserrat/Montserrat-SemiBold.ttf');
                //    }
                //    body{
                //        display: flex;
                //        flex-direction: column;
                //        font-family: 'Montserrat-Reg';
                //    }
                //    .img-container {
                //        width: 200px;
                //        margin:0 auto;
                //    }
                //    h3{
                //        width: 400px;
                //        text-align: center;
                //        margin:20px auto;
                //    }
                //    p{
                //        width: 400px;
                //        margin:10px auto;
                //    }
                //</style>
                //<body>
                //    <div class=""img-container"">
                //        <img width=""100%"" src=""https://www.alfardanoysterprivilegeclub.com/assets/img/AOPC-low-black.png"" alt="""">
                //    </div>
                //    <h3>Reset Password</h3>
                //    <p>We received a request to reset the password for your account. If you did not initiate this request, please ignore this email.</p>
                //    <p>To reset your password, please click the following link:<a href=" + emailsend + ">" + emailsend + "</a>. This link will be valid for the next 24 hours.</p>" +
                //    "<p>If you have any issues with resetting your password or need further assistance, please contact our support team at <b>app@alfaran.com.qa</b>.</p>" +
                //"</body> " +
                //"</html>";
                //message.Body = bodyBuilder.ToMessageBody();
                //using (var client = new SmtpClient())
                //{
                //    client.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                //    client.Authenticate("app@alfardan.com.qa", "Oyster2023!");
                //    client.Send(message);
                //    client.Disconnect(true);
                //    status = "Successfully sent registration email";

                //}
                //result.Status = "Success!";
        
            }
            return Ok(result);

        }
        [HttpPost]
        public IActionResult DeleteOffering(DeleteOffer data)
        {

            string sql = $@"select * from tbl_OfferingModel where id ='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            if (dt.Rows.Count != 0)
            {

                string OTPInsert = $@"update tbl_OfferingModel set StatusID = 6 where id ='" + data.Id + "'";
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
    }
}
