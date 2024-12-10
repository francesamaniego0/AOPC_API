using API.ViewModel;
using AuthSystem.Manager;
using AuthSystem.Models;
using AuthSystem.Services;
using AuthSystem.ViewModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Net;
using static AuthSystem.Data.Controller.ApiAuditTrailController;
using static AuthSystem.Data.Controller.ApiNotifcationController;

namespace AuthSystem.Data.Controller
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiPaginationController : ControllerBase
    {
        DbManager db = new DbManager();
        DBMethods dbmet = new DBMethods();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;

        public ApiPaginationController(JwtAuthenticationManager jwtAuthenticationManager, ApplicationDbContext context)
        {
            this.jwtAuthenticationManager = jwtAuthenticationManager;
            _context = context;
        }
        public class RegisterStats
        {
            public string Status { get; set; }

        }
        public class PaginationModel
        {
            public string? CurrentPage { get; set; }
            public string? NextPage { get; set; }
            public string? PrevPage { get; set; }
            public string? TotalPage { get; set; }
            public string? PageSize { get; set; }
            public string? TotalRecord { get; set; }
            public List<VendorVM> items { get; set; }


        }
        public class NotificationPaginateModel
        {
            public string? CurrentPage { get; set; }
            public string? NextPage { get; set; }
            public string? PrevPage { get; set; }
            public string? TotalPage { get; set; }
            public string? PageSize { get; set; }
            public string? TotalRecord { get; set; }
            public List<NotificationVM> items { get; set; }


        }
        public class AuditTrailPaginateModel
        {
            public string? CurrentPage { get; set; }
            public string? NextPage { get; set; }
            public string? PrevPage { get; set; }
            public string? TotalPage { get; set; }
            public string? PageSize { get; set; }
            public string? TotalRecord { get; set; }
            public List<Audittrailvm> items { get; set; }


        }
        public class PaginationCorpUserModel
        {
            public string? CurrentPage { get; set; }
            public string? NextPage { get; set; }
            public string? PrevPage { get; set; }
            public string? TotalPage { get; set; }
            public string? PageSize { get; set; }
            public string? TotalRecord { get; set; }
            public string? TotalVIP { get; set; }
            public List<UserVM> items { get; set; }


        }

        public class PaginationCorpUserCountModel
        {
            public string? CurrentPage { get; set; }
            public string? NextPage { get; set; }
            public string? PrevPage { get; set; }
            public string? TotalPage { get; set; }
            public string? PageSize { get; set; }
            public string? TotalRecord { get; set; }
            public string? TotalVIP { get; set; }
            public List<CorporateUserCountVM> items { get; set; }


        }
        public class paginate
        {
            public string? FilterName { get; set; }
            public int page { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> DisplayListPaginate(paginate data )
        {
          
            string module = "Vendor";
            string status = "ACTIVE";
            int pageSize = 25;
            //var model_result = (dynamic)null;
            var items = (dynamic)null;
            int totalItems = 0;
            int totalPages = 0;
            string page_size = pageSize == 0 ? "10" : pageSize.ToString();
            try
            {

                switch (module)
                {
                    case "Vendor":
                        //model_result = new PaginationModel<VendorVM>();

                        if (data.FilterName == null && status == null)
                        {

                            var Member = dbmet.GetVendorDetails().ToList();
                            totalItems = Member.Count;
                            totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));

                            items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
                        }
                        else if (data.FilterName != null && status == null)
                        {
                            var Member = dbmet.GetVendorDetails().Where(a => a.VendorName.ToUpper().Contains(data.FilterName.ToUpper())).ToList();
                            totalItems = Member.Count;
                            totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));

                            items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
                        }
                        else if (data.FilterName == null && status != null)
                        {
                            var Member = dbmet.GetVendorDetails().Where(a => a.Status == status).ToList();
                            totalItems = Member.Count;
                            totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));

                            items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
                        }
                        else
                        {

                            var Member = dbmet.GetVendorDetails().Where(a => a.VendorName.ToUpper().Contains(data.FilterName.ToUpper()) && a.Status == status).ToList();
                            totalItems = Member.Count;
                            totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(int.Parse(page_size.ToString()).ToString()));

                            items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
                        }
                        break;
                    
                    default:
                        items = null;
                        break;
                }
                var result = new List<PaginationModel>();
                var item = new PaginationModel();
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
                item.TotalRecord = totalItems.ToString();
                item.items = items;
                result.Add(item);
                return Ok(result);


            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        [HttpPost]
        public async Task<IActionResult> NotificationPaginate(paginate data)
        {

            //string module = "Vendor";
            //string status = "ACTIVE";
            int pageSize = 25;
            //var model_result = (dynamic)null;
            var items = (dynamic)null;
            int totalItems = 0;
            int totalPages = 0;
            string page_size = pageSize == 0 ? "10" : pageSize.ToString();
            try
            {

                var Member = dbmet.GetNotificationDetails().ToList();
                totalItems = Member.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));

                items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();

                var result = new List<NotificationPaginateModel>();
                var item = new NotificationPaginateModel();
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
                item.TotalRecord = totalItems.ToString();
                item.items = items;
                result.Add(item);
                return Ok(result);


            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AuditTrailPaginate(paginate data)
        {

            //string module = "Vendor";
            //string status = "ACTIVE";
            int pageSize = 25;
            //var model_result = (dynamic)null;
            var items = (dynamic)null;
            int totalItems = 0;
            int totalPages = 0;
            string page_size = pageSize == 0 ? "10" : pageSize.ToString();
            try
            {

                var Member = dbmet.GetAuditTrailList().ToList();
                totalItems = Member.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));

                items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();

                var result = new List<AuditTrailPaginateModel>();
                var item = new AuditTrailPaginateModel();
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
                item.TotalRecord = totalItems.ToString();
                item.items = items;
                result.Add(item);
                return Ok(result);


            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        public class paginateCorpUserv2
        {
            public string? CorpId { get; set; }
            public string? PosId { get; set; }
            public string? Gender { get; set; }
            public string? isVIP { get; set; }
            public string? Status { get; set; }
            public string? FilterName { get; set; }
            public int page { get; set; }
        }

        public class paginateCorpUser
        {
            public string? CorpId { get; set; }
            public string? PosId { get; set; }
            public string? FilterName { get; set; }
            public int page { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> DisplayCorporateUser(paginateCorpUser data)
        {

           
            
            string status = "ACTIVE";
            int pageSize = 10;
            //var model_result = (dynamic)null;
            var items = (dynamic)null;
            int totalItems = 0;
            int totalPages = 0;
            int totalVIP = 0;
            string page_size = pageSize == 0 ? "10" : pageSize.ToString();
            try
            {

              
               
                        //model_result = new PaginationModel<VendorVM>();

                        if (data.FilterName == null && data.PosId == "0" || data.PosId == null)
                        {

                            var Member = dbmet.GetCorporateAdminUserList().Where(a=>a.CorporateID == data.CorpId).ToList();
                            totalItems = Member.Count;
                            totalVIP = Member.Where(a=>a.isVIP == "1").ToList().Count;
                            totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));

                            items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
                        }  
                        else if(data.PosId != "0"  && data.PosId != null && data.FilterName != null)
                        {
                            var Member = dbmet.GetCorporateAdminUserList().Where(a => a.Username.ToUpper().Contains(data.FilterName.ToUpper()) && a.PositionID == data.PosId &&  a.CorporateID == data.CorpId).ToList();
                            totalItems = Member.Count;
                      totalVIP = Member.Where(a=>a.isVIP == "1").ToList().Count;
                            totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));

                            items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
                        }
                        else if(data.PosId != "0" && data.PosId != null )
                        {
                            var Member = dbmet.GetCorporateAdminUserList().Where(a =>a.PositionID == data.PosId && a.CorporateID == data.CorpId).ToList();
                            totalItems = Member.Count;
                      totalVIP = Member.Where(a=>a.isVIP == "1").ToList().Count;
                            totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));

                            items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();
                        }
                     
                        else 
                        {
                            var Member = dbmet.GetCorporateAdminUserList().Where(a => a.Username.ToUpper().Contains(data.FilterName.ToUpper()) && a.CorporateID == data.CorpId ).ToList();
                            totalItems = Member.Count;
                      totalVIP = Member.Where(a=>a.isVIP == "1").ToList().Count;
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

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DisplayRegistrationList(paginateCorpUserv2 data)
        {



            string status = "ACTIVE";
            int pageSize = 10;
            //var model_result = (dynamic)null;
            var items = (dynamic)null;
            int totalItems = 0;
            int totalVIP = 0;
            int totalPages = 0;
            string page_size = pageSize == 0 ? "10" : pageSize.ToString();
            try
            {

                var Member = dbmet.GetCorporateAdminUserListv2(data).ToList();
                totalItems = Member.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / int.Parse(page_size.ToString()));
                items = Member.Skip((data.page - 1) * int.Parse(page_size.ToString())).Take(int.Parse(page_size.ToString())).ToList();



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
                item.TotalRecord = totalItems.ToString();
                item.TotalVIP = totalVIP.ToString();
                item.items = items;
                result.Add(item);
                return Ok(result);


            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DisplayRegistrationListv2(paginateCorpUserv2 data)
        {
            try
            {

                string sql = $@"SELECT       UsersModel.Username, UsersModel.Fname, UsersModel.Lname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, tbl_PositionModel.Name AS Position, tbl_CorporateModel.CorporateName, 
                 tbl_UserTypeModel.UserType, UsersModel.Fullname, UsersModel.Id, UsersModel.DateCreated, tbl_PositionModel.Id AS PositionID, tbl_CorporateModel.Id AS CorporateID, tbl_StatusModel.Name AS status, UsersModel.isVIP, 
                 UsersModel.FilePath
                 FROM            UsersModel LEFT OUTER JOIN
                 tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id LEFT OUTER JOIN
                 tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id LEFT OUTER JOIN
                 tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id LEFT OUTER JOIN
                 tbl_StatusModel ON UsersModel.Active = tbl_StatusModel.Id
                 WHERE        (UsersModel.Active IN (1, 2, 9, 10)) AND (UsersModel.Type = 3)
                 ";

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

                sql += " order by UsersModel.Fname ASC";

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

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
        }

    }
}
