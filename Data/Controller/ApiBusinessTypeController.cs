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
using static AuthSystem.Data.Controller.ApiVendorController;
using API.Models;

namespace AuthSystem.Data.Controller
{

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiBusinessTypeController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;

        //private readonly GlobalState _state;

        public ApiBusinessTypeController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {
   
            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;
        }
      
        [HttpGet]
        public async Task<IActionResult> BusinessTypeList()
        {
            //var CorporateId = _state.CorporateValue;
            var CorporateId = HttpContext.Session.GetString("BTCorporateId");
            var UserType = HttpContext.Session.GetString("BTUserType");
            string sql = "";
            //Console.WriteLine(CorporateId);
            if (UserType == "ADMIN")
            {
                sql = $@"SELECT        tbl_BusinessTypeModel.Id, tbl_BusinessTypeModel.BusinessTypeName, tbl_BusinessTypeModel.Description, tbl_BusinessTypeModel.DateCreated, tbl_BusinessTypeModel.BusinessTypeID, 
                                     tbl_BusinessTypeModel.PromoText, tbl_StatusModel.Name AS Status, tbl_BusinessTypeModel.ImgURL, tbl_BusinessTypeModel.isVIP, tbl_StatusModel.Id AS Status
            FROM            tbl_BusinessTypeModel INNER JOIN
                                     tbl_StatusModel ON tbl_BusinessTypeModel.Status = tbl_StatusModel.Id
            WHERE        (tbl_BusinessTypeModel.Status = 5)
            ORDER BY tbl_BusinessTypeModel.Id DESC";
            }
            else
            {
                if(CorporateId == null)
                {
                    return Problem("You need to login First");
                }
                sql = $@"SELECT bt.Id, bt.BusinessTypeName, bt.Description, bt.DateCreated, bt.BusinessTypeID, bt.PromoText, sm.Name AS Status, bt.ImgURL, bt.isVIP, sm.Id AS Status
                                    FROM tbl_BusinessTypeModel  bt
                                    INNER JOIN tbl_StatusModel sm
                                    ON bt.Status = sm.Id

                                    LEFT JOIN tbl_PrivilegeModel AS pm
                                    ON pm.BusinessTypeID = bt.Id
                                    LEFT JOIN tbl_CorporatePrivilegeTierModel as cptm
                                    ON cptm.PrivilegeID = pm.Id

                                    WHERE (bt.Status = 5) AND cptm.CorporateID =" + CorporateId
                         + "GROUP BY bt.Id, bt.BusinessTypeName, bt.Description, bt.DateCreated, bt.BusinessTypeID, bt.PromoText, sm.Name , bt.ImgURL, bt.isVIP, sm.Id"
                       + " ORDER BY bt.Id DESC";

            }

            //            sql = $@"SELECT        tbl_BusinessTypeModel.Id, tbl_BusinessTypeModel.BusinessTypeName, tbl_BusinessTypeModel.Description, tbl_BusinessTypeModel.DateCreated, tbl_BusinessTypeModel.BusinessTypeID, 
            //                         tbl_BusinessTypeModel.PromoText, tbl_StatusModel.Name AS Status, tbl_BusinessTypeModel.ImgURL, tbl_BusinessTypeModel.isVIP, tbl_StatusModel.Id AS Status
            //FROM            tbl_BusinessTypeModel INNER JOIN
            //                         tbl_StatusModel ON tbl_BusinessTypeModel.Status = tbl_StatusModel.Id
            //WHERE        (tbl_BusinessTypeModel.Status = 5)
            //ORDER BY tbl_BusinessTypeModel.Id DESC";

            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<BusinessTypeVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new BusinessTypeVM();
                item.Id= int.Parse(dr["Id"].ToString());
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.Description= dr["Description"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.status = dr["status"].ToString();
                item.BusinessTypeID = dr["BusinessTypeID"].ToString();
                item.PromoText = dr["PromoText"].ToString();
                item.ImgURL  = dr["ImgURL"].ToString();
                item.isVIP = dr["isVIP"].ToString();
                result.Add(item);
            }

            return Ok(result);
        }
        public class filterByCorporate
        {
            public string CorporateId { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> BusinessTypeListv3(filterByCorporate data)
        {

            string sql = "";
            if (data.CorporateId == null || data.CorporateId == "")
            {

                sql = $@"SELECT        tbl_BusinessTypeModel.Id, tbl_BusinessTypeModel.BusinessTypeName, tbl_BusinessTypeModel.Description, tbl_BusinessTypeModel.DateCreated, tbl_BusinessTypeModel.BusinessTypeID, 
                                     tbl_BusinessTypeModel.PromoText, tbl_StatusModel.Name AS Status, tbl_BusinessTypeModel.ImgURL, tbl_BusinessTypeModel.isVIP, tbl_StatusModel.Id AS Status
            FROM            tbl_BusinessTypeModel INNER JOIN
                                     tbl_StatusModel ON tbl_BusinessTypeModel.Status = tbl_StatusModel.Id
            WHERE        (tbl_BusinessTypeModel.Status = 5)
            ORDER BY tbl_BusinessTypeModel.Id DESC";
            }
            else
            {
                sql = $@"SELECT bt.Id, bt.BusinessTypeName, bt.Description, bt.DateCreated, bt.BusinessTypeID, bt.PromoText, sm.Name AS Status, bt.ImgURL, bt.isVIP, sm.Id AS Status
                                    FROM tbl_BusinessTypeModel  bt
                                    INNER JOIN tbl_StatusModel sm
                                    ON bt.Status = sm.Id

                                    LEFT JOIN tbl_PrivilegeModel AS pm
                                    ON pm.BusinessTypeID = bt.Id
                                    LEFT JOIN tbl_CorporatePrivilegeTierModel as cptm
                                    ON cptm.PrivilegeID = pm.Id

                                    WHERE (bt.Status = 5) AND cptm.CorporateID =" + data.CorporateId
                                         + "GROUP BY bt.Id, bt.BusinessTypeName, bt.Description, bt.DateCreated, bt.BusinessTypeID, bt.PromoText, sm.Name , bt.ImgURL, bt.isVIP, sm.Id"
                                       + " ORDER BY bt.Id DESC";
            }
                

            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<BusinessTypeVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new BusinessTypeVM();
                item.Id = int.Parse(dr["Id"].ToString());
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.Description = dr["Description"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.status = dr["status"].ToString();
                item.BusinessTypeID = dr["BusinessTypeID"].ToString();
                item.PromoText = dr["PromoText"].ToString();
                item.ImgURL = dr["ImgURL"].ToString();
                item.isVIP = dr["isVIP"].ToString();
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> BusinessTypeListv2()
        {
            
            string sql = "";
            
            sql = $@"SELECT        tbl_BusinessTypeModel.Id, tbl_BusinessTypeModel.BusinessTypeName, tbl_BusinessTypeModel.Description, tbl_BusinessTypeModel.DateCreated, tbl_BusinessTypeModel.BusinessTypeID, 
                                     tbl_BusinessTypeModel.PromoText, tbl_StatusModel.Name AS Status, tbl_BusinessTypeModel.ImgURL, tbl_BusinessTypeModel.isVIP, tbl_StatusModel.Id AS Status
            FROM            tbl_BusinessTypeModel INNER JOIN
                                     tbl_StatusModel ON tbl_BusinessTypeModel.Status = tbl_StatusModel.Id
            WHERE        (tbl_BusinessTypeModel.Status = 5)
            ORDER BY tbl_BusinessTypeModel.Id DESC";

            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<BusinessTypeVM>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new BusinessTypeVM();
                item.Id = int.Parse(dr["Id"].ToString());
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.Description = dr["Description"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                item.status = dr["status"].ToString();
                item.BusinessTypeID = dr["BusinessTypeID"].ToString();
                item.PromoText = dr["PromoText"].ToString();
                item.ImgURL = dr["ImgURL"].ToString();
                item.isVIP = dr["isVIP"].ToString();
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpPost]
        public async  Task<IActionResult> SaveBusinessType(BusinessTypeModel data)
        {
            try
            {
                string result = "";
                GlobalVariables gv = new GlobalVariables();
                _global.Status = gv.BusinessTypeRegister(data,  _context);
            }

            catch (Exception ex)
            {
                string status = ex.GetBaseException().ToString();
            }
             return Content(_global.Status);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBusinessType(BusinessTypeModel data)
        {
           
            string result = "";
            string query = "";
            try
            {

                if (data.BusinessTypeName.Length != 0)
                {
                    string FeaturedImage = "";
                    if (data.ImgURL == null || data.ImgURL == "")
                    {
                        FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/nophotos.JPG";
                    }
                    else
                    {
                        FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + data.ImgURL.Replace(" ", "%20");
                    }

                    if (data.Id == 0)
                    {
                        string sql = $@"select * from tbl_BusinessTypeModel where BusinessTypeName='" + data.BusinessTypeName + "' and status = 5";
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        if (dt.Rows.Count == 0)
                        {
                            query += $@"insert into tbl_BusinessTypeModel (BusinessTypeName,Description,isVIP,ImgURL,PromoText,Status) values ('"+data.BusinessTypeName
                                +"','"+ data.Description + "','"+ data.isVIP + "','"+ FeaturedImage + "','"+ data.PromoText + "',5) ";
                            db.AUIDB_WithParam(query);
                            result = "Inserted Successfully";
                            return Ok(result);

                        }
                        else
                        {
                            result = "Business Type already Exist";
                            return BadRequest(result);
                        }
                    }
                    else
                    {
                        query += $@"update  tbl_BusinessTypeModel set BusinessTypeName='" + data.BusinessTypeName + "' , Description='" + data.Description + "' , isVIP='" + data.isVIP 
                            + "' , ImgURL='"+FeaturedImage+ "' , PromoText='" + data.PromoText+"', Status='5'   where  Id='" + data.Id + "' ";
                        db.AUIDB_WithParam(query);

                        result = "Updated Successfully";
                        return BadRequest(result);
                    }


                }
                else
                {
                    result = "Error!";
                    return BadRequest(result);
                }
            }

            catch (Exception ex)
            {
                return BadRequest(result);
            }
        }
       
        public class DeleteBtype
        {
            public int Id { get; set; }
        }
        public class Registerstats
        {
            public string Status { get; set; }

        }
        [HttpPost]
        public IActionResult DeleteBusinessType(DeleteBtype data)
        {

            string sql = $@"select * from tbl_BusinessTypeModel where id ='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            if (dt.Rows.Count != 0)
            {

                //string sql1 = $@"select * from tbl_BusinessModel where TypeId ='" + data.Id + "'";
                string sql1 = $@"SELECT TOP 1 bt.id, v.BusinessTypeId, o.BusinessTypeId, p.BusinessTypeId FROM tbl_BusinessTypeModel bt WITH(NOLOCK)
                                    LEFT JOIN tbl_VendorModel v WITH(NOLOCK)
                                    ON bt.Id = v.BusinessTypeId
                                    LEFT JOIN tbl_OfferingModel o WITH(NOLOCK)
                                    ON bt.Id = o.BusinessTypeID
                                    LEFT JOIN tbl_PrivilegeModel p WITH(NOLOCK)
                                    ON bt.Id = p.BusinessTypeID
                                    WHERE 
                                    bt.id = '" + data.Id + "' AND (o.BusinessTypeId IS NOT NULL OR v.BusinessTypeId IS NOT NULL OR p.BusinessTypeId IS NOT NULL)";
                DataTable dt1 = db.SelectDb(sql1).Tables[0];
                if (dt1.Rows.Count == 0)
                {
                    string query = $@"update  tbl_BusinessTypeModel set Status='6' where  Id='" + data.Id + "'";
                    db.AUIDB_WithParam(query);
                    result.Status = "Successfully Deleted";
                    return Ok(result);
                }
                else
                {
                    result.Status = "Business Type is Already in Used!";

                    return BadRequest(result);

                }

            }
            else
            {
                result.Status = "Error";

                return BadRequest(result);

            }

        }
    }
}
