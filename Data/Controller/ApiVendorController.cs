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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Data.SqlClient;
using static AuthSystem.Data.Controller.ApiUserAcessController;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static AuthSystem.Data.Controller.ApiBusinessController;

namespace AuthSystem.Data.Controller
{

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiVendorController : ControllerBase
    {
        DbManager db = new DbManager();
        DBMethods dbmet = new DBMethods();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;


        public ApiVendorController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {
   
            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;
   
        }
       
        [HttpPost]
        public async Task<IActionResult> VendorGallery(Bid data)
        {
            int ctr = 0;
            string sql = $@"SELECT        Id, Gallery
                    FROM            tbl_VendorModel
                    WHERE        (Status = 5) AND (Id = '" + data.id + "')";
            var result = new List<BusinessArray>();
            DataTable table = db.SelectDb(sql).Tables[0];

            foreach (DataRow dr in table.Rows)
            {
                var gal = dr["Gallery"].ToString();
                string[] gallist = gal.Split(";");
                foreach (string author in gallist)
                {
                    var item = new BusinessArray();
                    if (author != "")
                    {
                        item.Id = ctr.ToString();
                        item.Gallery = author;
                        result.Add(item);
                        ctr++;

                    }

                }
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> VendorList()
        {
            //            string sql = $@"SELECT   tbl_VendorModel.Id,tbl_VendorModel.VendorName, tbl_VendorModel.Description, tbl_VendorModel.Services, tbl_VendorModel.WebsiteUrl, tbl_VendorModel.FeatureImg, tbl_VendorModel.Gallery, tbl_VendorModel.Cno, tbl_VendorModel.Email, 
            //                         tbl_VendorModel.VideoUrl, tbl_VendorModel.VrUrl, tbl_VendorModel.DateCreated, tbl_VendorModel.VendorID, tbl_VendorModel.FileUrl, tbl_VendorModel.Map, tbl_VendorModel.VendorLogo, 
            //                         tbl_BusinessTypeModel.BusinessTypeName, tbl_BusinessLocationModel.Country, tbl_BusinessLocationModel.City, tbl_BusinessLocationModel.PostalCode, tbl_VendorModel.Address, tbl_VendorModel.Id, 
            //                         tbl_BusinessModel.Address AS location, tbl_BusinessModel.BusinessName, tbl_StatusModel.Name AS Status, tbl_VendorModel.BusinessLocationID , tbl_VendorModel.BusinessTypeId
            //FROM            tbl_VendorModel INNER JOIN
            //                         tbl_BusinessTypeModel ON tbl_VendorModel.BusinessTypeId = tbl_BusinessTypeModel.Id LEFT OUTER JOIN
            //                         tbl_BusinessModel ON tbl_BusinessTypeModel.Id = tbl_BusinessModel.TypeId LEFT OUTER JOIN
            //                         tbl_BusinessLocationModel ON tbl_VendorModel.BusinessLocationID = tbl_BusinessLocationModel.Id LEFT OUTER JOIN
            //                         tbl_StatusModel ON tbl_VendorModel.Status = tbl_StatusModel.Id
            //WHERE        (tbl_VendorModel.Status = 5)
            //ORDER BY tbl_VendorModel.Id DESC";
            string sql = $@"SELECT   tbl_VendorModel.Id,tbl_VendorModel.VendorName, tbl_VendorModel.Description, tbl_VendorModel.Services, tbl_VendorModel.WebsiteUrl, tbl_VendorModel.FeatureImg, tbl_VendorModel.Gallery, tbl_VendorModel.Cno, tbl_VendorModel.Email, 
                         tbl_VendorModel.VideoUrl, tbl_VendorModel.VrUrl, tbl_VendorModel.DateCreated, tbl_VendorModel.VendorID, tbl_VendorModel.FileUrl, tbl_VendorModel.Map, tbl_VendorModel.VendorLogo, 
                         tbl_BusinessTypeModel.BusinessTypeName, tbl_BusinessLocationModel.Country, tbl_BusinessLocationModel.City, tbl_BusinessLocationModel.PostalCode, tbl_VendorModel.Address, tbl_VendorModel.Id, 
                         tbl_BusinessLocationModel.City AS location,  tbl_StatusModel.Name AS Status, tbl_VendorModel.BusinessLocationID , tbl_VendorModel.BusinessTypeId
FROM           tbl_VendorModel INNER JOIN
                         tbl_BusinessTypeModel ON tbl_VendorModel.BusinessTypeId = tbl_BusinessTypeModel.Id LEFT OUTER JOIN
                         tbl_BusinessLocationModel ON tbl_VendorModel.BusinessLocationID = tbl_BusinessLocationModel.Id LEFT OUTER JOIN
                         tbl_StatusModel ON tbl_VendorModel.Status = tbl_StatusModel.Id  
WHERE        (tbl_VendorModel.Status = 5)
ORDER BY tbl_VendorModel.Id DESC";
            var result = new List<VendorVM>();
            DataTable table = db.SelectDb(sql).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                var item = new VendorVM();
                item.Id = dr["Id"].ToString();
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
                item.VendorID = dr["VendorID"].ToString();
                item.FileUrl = dr["FileUrl"].ToString();
                item.Map = dr["Map"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.Country = dr["Country"].ToString();
                item.City = dr["City"].ToString();
                item.PostalCode = dr["PostalCode"].ToString();
                //item.BusinessName = dr["BusinessName"].ToString();
                item.Address = dr["Address"].ToString();
                item.Id = dr["Id"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                item.Status = dr["Status"].ToString();
                item.Vendorlocation = dr["location"].ToString();
                item.BID = dr["BusinessLocationID"].ToString();
                item.BtypeID = dr["BusinessTypeId"].ToString();

                result.Add(item);
            }
            return Ok(result);
        }
        public class VendorIdModel
        {
            public string Id { get; set; }
        }
        public class VendorNameModel
        {
            public string VendorName { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> VendorFilterbYId(VendorIdModel data)
        {
            string sql = $@"SELECT   tbl_VendorModel.Id,tbl_VendorModel.VendorName, tbl_VendorModel.Description, tbl_VendorModel.Services, tbl_VendorModel.WebsiteUrl, tbl_VendorModel.FeatureImg, tbl_VendorModel.Gallery, tbl_VendorModel.Cno, tbl_VendorModel.Email, 
                         tbl_VendorModel.VideoUrl, tbl_VendorModel.VrUrl, tbl_VendorModel.DateCreated, tbl_VendorModel.VendorID, tbl_VendorModel.FileUrl, tbl_VendorModel.Map, tbl_VendorModel.VendorLogo, 
                         tbl_BusinessTypeModel.BusinessTypeName, tbl_BusinessLocationModel.Country, tbl_BusinessLocationModel.City, tbl_BusinessLocationModel.PostalCode, tbl_VendorModel.Address, tbl_VendorModel.Id, 
                         tbl_BusinessModel.Address AS location, tbl_BusinessModel.BusinessName, tbl_StatusModel.Name AS Status, tbl_VendorModel.BusinessLocationID , tbl_VendorModel.BusinessTypeId
FROM            tbl_VendorModel INNER JOIN
                         tbl_BusinessTypeModel ON tbl_VendorModel.BusinessTypeId = tbl_BusinessTypeModel.Id LEFT OUTER JOIN
                         tbl_BusinessModel ON tbl_BusinessTypeModel.Id = tbl_BusinessModel.TypeId LEFT OUTER JOIN
                         tbl_BusinessLocationModel ON tbl_VendorModel.BusinessLocationID = tbl_BusinessLocationModel.Id LEFT OUTER JOIN
                         tbl_StatusModel ON tbl_VendorModel.Status = tbl_StatusModel.Id
WHERE        (tbl_VendorModel.Status = 5) and tbl_VendorModel.Id='"+data.Id+"' ORDER BY tbl_VendorModel.Id DESC";
            var result = new List<VendorVM>();
            DataTable table = db.SelectDb(sql).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                var item = new VendorVM();
                item.Id = dr["Id"].ToString();
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
                item.VendorID = dr["VendorID"].ToString();
                item.FileUrl = dr["FileUrl"].ToString();
                item.Map = dr["Map"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.Country = dr["Country"].ToString();
                item.City = dr["City"].ToString();
                item.PostalCode = dr["PostalCode"].ToString();
                item.BusinessName = dr["BusinessName"].ToString();
                item.Address = dr["Address"].ToString();
                item.Id = dr["Id"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM-dd-yyyy");
                item.Status = dr["Status"].ToString();
                item.Vendorlocation = dr["location"].ToString();
                item.BID = dr["BusinessLocationID"].ToString();
                item.BtypeID = dr["BusinessTypeId"].ToString();

                result.Add(item);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> VendorSearch(VendorNameModel data)
        {
            var result = dbmet.GetVendorDetails().Where(a => a.VendorName.ToUpper().Contains(data.VendorName.ToUpper())).ToList(); 
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> VendorListFilterByVID(VendorIDVM data)
        {
            string sql = "";
            var vendorlocation = _context.tbl_VendorModel.Where(a=>a.Address == null  && a.VendorID == data.vendorID).ToList();
            if(vendorlocation.Count() == 0)
            {
                sql = $@"SELECT        tbl_VendorModel.WebsiteUrl, tbl_VendorModel.FeatureImg, tbl_VendorModel.VendorName, tbl_VendorModel.Description, tbl_VendorModel.Cno, tbl_VendorModel.Gallery, tbl_VendorModel.Email, tbl_VendorModel.Id, 
                         tbl_VendorModel.VideoUrl, tbl_VendorModel.VrUrl, tbl_VendorModel.VendorID, tbl_VendorModel.FileUrl, tbl_VendorModel.Services, tbl_VendorModel.Map, tbl_VendorModel.VendorLogo, tbl_VendorModel.Address AS location, 
                         tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.Status
FROM            tbl_VendorModel INNER JOIN
                         tbl_BusinessTypeModel ON tbl_VendorModel.BusinessTypeId = tbl_BusinessTypeModel.Id
                        WHERE        (tbl_VendorModel.Status = 5) AND     (VendorID = '" + data.vendorID + "')";
            }
            else
            {
                sql = $@"SELECT        tbl_VendorModel.WebsiteUrl, tbl_VendorModel.FeatureImg, tbl_VendorModel.VendorName, tbl_VendorModel.Description, tbl_VendorModel.Cno, tbl_VendorModel.Gallery, tbl_VendorModel.Email, tbl_VendorModel.Id, 
                         tbl_VendorModel.VideoUrl, tbl_VendorModel.VrUrl, tbl_VendorModel.VendorID, tbl_VendorModel.FileUrl, tbl_VendorModel.Services, tbl_VendorModel.Map, tbl_VendorModel.VendorLogo, tbl_VendorModel.Address, 
                         tbl_BusinessModel.Address AS location, tbl_BusinessTypeModel.BusinessTypeName
FROM            tbl_VendorModel INNER JOIN
                         tbl_BusinessModel ON tbl_VendorModel.BusinessLocationID = tbl_BusinessModel.Id INNER JOIN
                         tbl_BusinessTypeModel ON tbl_VendorModel.BusinessTypeId = tbl_BusinessTypeModel.Id
                         WHERE                  (Status = 5) AND     (VendorID = '" + data.vendorID + "')";
            }

            var item = new VendorDetail3();
            DataTable table = db.SelectDb(sql).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                item.Id = dr["Id"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.WebsiteUrl = dr["WebsiteUrl"].ToString();
                item.FeatureImg = dr["FeatureImg"].ToString();
                item.VendorName = dr["VendorName"].ToString();
                item.FeatureImg = dr["FeatureImg"].ToString();
                item.Description = dr["Description"].ToString();
                item.location = dr["location"].ToString();
                item.Cno = dr["Cno"].ToString();
                item.Gallery = dr["Gallery"].ToString();
                item.Email = dr["Email"].ToString();
                item.VideoUrl = dr["VideoUrl"].ToString();
                item.VrUrl = dr["VrUrl"].ToString();
                item.FileUrl = dr["FileUrl"].ToString();
                item.Services = dr["Services"].ToString();
                item.Map = dr["Map"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();


            }
            return Ok(item);
        }
        [HttpPost]
        public async Task<IActionResult> VendorListFilterByBtypeName(btypeName data)
        {
            string sql = "";
            if(data.BusinessTypeName == null)
            {
                sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_VendorModel.Description, tbl_VendorModel.Address, tbl_VendorModel.FeatureImg, tbl_VendorModel.VendorID, tbl_VendorModel.VendorLogo
                         FROM            tbl_VendorModel INNER JOIN
                         tbl_BusinessTypeModel ON tbl_VendorModel.BusinessTypeId = tbl_BusinessTypeModel.Id
                        WHERE    tbl_VendorModel.Status = 5";
            }
            else
            {
                sql = $@"SELECT        tbl_BusinessTypeModel.BusinessTypeName, tbl_VendorModel.VendorName, tbl_VendorModel.Description, tbl_VendorModel.Address, tbl_VendorModel.FeatureImg, tbl_VendorModel.VendorID, tbl_VendorModel.VendorLogo
                         FROM            tbl_VendorModel INNER JOIN
                         tbl_BusinessTypeModel ON tbl_VendorModel.BusinessTypeId = tbl_BusinessTypeModel.Id
                        WHERE        (tbl_BusinessTypeModel.BusinessTypeName = '" + data.BusinessTypeName + "')  AND tbl_VendorModel.Status = 5";
            }
        
            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<VendorDetail2>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new VendorDetail2();
                item.BusinessTypeName = dr["BusinessTypeName"].ToString();
                item.VendorName = dr["VendorName"].ToString();
                item.Description = dr["Description"].ToString();
                item.Address = dr["Address"].ToString();
                item.FeatureImg = dr["FeatureImg"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                result.Add(item);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> DiscoverFilterbyBtypeAndBID(Discover data)
        {
            var param = new IDataParameter[]
              {
               new SqlParameter("@BusinessTypeName",data.BusinessTypeName),
               new SqlParameter("@BusinessID",data.BusinessID)
              };
            DataTable table = db.SelectDb_SP("SP_DiscoverFilterbyBtypeAndBID", param).Tables[0];
            var result = new List<DiscoverDetails>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new DiscoverDetails();
                item.VendorName = dr["VendorName"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.VendorLogo = dr["VendorLogo"].ToString();
                result.Add(item);
            }
            return Ok(result);

        }
        [HttpPost]
        public async Task<IActionResult> DiscoverFilterbyBtype(Discover data)
        {
            string sql = "";

            sql = $@"SELECT        tbl_VendorModel.VendorName, tbl_VendorModel.VendorID,tbl_VendorModel.FeatureImg 
                                    FROM            tbl_VendorModel LEFT OUTER JOIN
                                                             tbl_BusinessTypeModel ON tbl_VendorModel.BusinessTypeId = tbl_BusinessTypeModel.Id LEFT OUTER JOIN
                                                             tbl_BusinessModel ON tbl_VendorModel.BusinessLocationID = tbl_BusinessModel.Id
                                    WHERE        (tbl_BusinessTypeModel.BusinessTypeName ='" +data.BusinessTypeName+ "') and  tbl_VendorModel.Status =5";


            DataTable table = db.SelectDb(sql).Tables[0];
            var result = new List<DiscoverDetails>();
            foreach (DataRow dr in table.Rows)
            {
                var item = new DiscoverDetails();
                item.VendorName = dr["VendorName"].ToString();
                item.VendorID = dr["VendorID"].ToString();
                item.FeatureImg = dr["FeatureImg"].ToString();
                result.Add(item);
            }
            return Ok(result);

        }
        [HttpPost]
        public IActionResult SaveVendor(VendorModel data)
        {


            string result = "";
            string description = "";
            string services = "";
            string query = "";
            try
            {

                if (data.VendorName.Length != 0 || data.Description.Length != 0 )
                {
                    string FeaturedImage = "";
                    string Logo = "";
                    if (data.FeatureImg == null)
                    {
                        FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/defaultavatar.png";
                    }
                    else
                    {
                        FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + data.FeatureImg.Replace("'", "''").Replace(" ", "%20"); ;
                    }
                    if (data.VendorLogo == null)
                    {
                        Logo = "https://www.alfardanoysterprivilegeclub.com/assets/img/nophotos.JPG";
                    }
                    else
                    {
                        Logo = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + data.VendorLogo.Replace("'", "''").Replace(" ", "%20"); ;
                    }
                    string gallery = "";
                    if (data.Gallery == null || data.Gallery == "")
                    {
                        gallery = "";

                    }
                    else
                    {
                        gallery = data.Gallery.Replace("'", "''").Replace(" ", "%20");
                    }
                    description = data.Description.Replace("'", "''");
                    services = data.Services == null? "" : data.Services.Replace("'", "''");
                    if (data.Id == 0)
                    {
                        string sql = $@"select * from tbl_VendorModel where VendorName='" + data.VendorName.Replace("'", "''") + "' and Status=5";
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        if (dt.Rows.Count == 0)
                        {
                            //query += $@"insert into tbl_VendorModel (VendorName,BusinessTypeId,Description,Services,WebsiteUrl,FeatureImg,Gallery,Cno,Email,VideoUrl,VrUrl,BusinessLocationID,Status,FileUrl,Map,VendorLogo,Address,DateCreated) values
                            //         ('"+data.VendorName.Replace("'", "''") + "','"+data.BusinessTypeId + "','"+ description + "','"+ services + "','"+data.WebsiteUrl + "','"+ FeaturedImage + "','"+data.Gallery + "','"+data.Cno + "','"+data.Email + "'" +
                            //         ",'"+data.VideoUrl + "','"+data.VrUrl + "','"+data.BusinessLocationID + "',5,'"+data.FileUrl + "','"+data.Map + "','"+ Logo + "','"+data.Address.Replace("'", "''") + "','"+ DateTime.Now.ToString() + "')";
                            //db.AUIDB_WithParam(query);

                            query += $@"
                                    INSERT INTO [dbo].[tbl_VendorModel]
                                               ([VendorName]
                                               ,[BusinessTypeId]
                                               ,[Description]
                                               ,[Services]
                                               ,[WebsiteUrl]
                                               ,[FeatureImg]
                                               ,[Gallery]
                                               ,[Cno]
                                               ,[Email]
                                               ,[VideoUrl]
                                               ,[VrUrl]
                                               ,[BusinessLocationID]
                                               ,[DateCreated]
                                               ,[Status]
                                               ,[FileUrl]
                                               ,[Map]
                                               ,[VendorLogo]
                                               ,[Address])
                                         VALUES
                                               ('" + data.VendorName.Replace("'", "''") + "'" +
                                                ",'" + data.BusinessTypeId + "'," +
                                                "'" + description + "'," +
                                                "'" + services + "'," +
                                                "'" + data.WebsiteUrl + "'," +
                                                "'" + FeaturedImage + "'," +
                                                "'" + data.Gallery + "'," +
                                                "'" + data.Cno + "'," +
                                                "'" + data.Email + "'," +
                                                "'" + data.VideoUrl + "'," +
                                                "'" + data.VrUrl + "'," +
                                                "'" + data.BusinessLocationID + "'," +
                                                "'" + DateTime.Now.ToString("yyyy-MM-dd") + "'," +
                                                "'5'," +
                                                "'" + data.FileUrl + "'," +
                                                "'" + data.Map + "'," +
                                                "'" + Logo + "'," +
                                                "'" + data.Address.Replace("'", "''") + "')";
                            result = db.AUIDB_WithParam(query) + " Added";
                            return Ok("Inserted Successfully");

                        }
                        else
                        {
                            result = "Vendor Name already Exist";
                            return BadRequest(result);
                        }
                    }
                    else
                    {
                         query = $@"update  tbl_VendorModel set VendorName='" + 
                                data.VendorName.Replace("'", "''") +
                                "' , BusinessTypeId='" +
                                data.BusinessTypeId + 
                                "' , Description='" +
                                description +
                                "' , Services='" +
                                services + 
                                "' , WebsiteUrl='" +
                                data.WebsiteUrl + 
                                "' , FeatureImg='" +
                                FeaturedImage + 
                                "' , Gallery='" +
                                gallery + 
                                "' , Cno='" + data.Cno +
                                "', Email='" +
                                data.Email +
                                "', VideoUrl='"+
                                data.VideoUrl +
                                "' , VrUrl='" +
                                data.VrUrl +
                                "'  , BusinessLocationID='" +
                                data.BusinessLocationID +
                                "' , Status='5' , FileUrl='" +
                                data.FileUrl+ 
                                "' , Map='" + 
                                data.Map + 
                                "' , VendorLogo='" +
                                Logo + "' , Address='" +
                                data.Address.Replace("'", "''") + 
                                "'  where  Id='" + 
                                data.Id + "' ";
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
            }

            catch (Exception ex)
            {
                return BadRequest(result);
            }
      
        }
        public class DeleteVen
        {

            public int Id { get; set; }
        }
        public class Registerstats
        {
            public string Status { get; set; }

        }
        [HttpPost]
        public IActionResult DeleteVendor(DeleteVen data)
        {

            string sql = $@"select * from tbl_VendorModel where id ='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            if (dt.Rows.Count != 0)
            {

                string OTPInsert = $@"update tbl_VendorModel set Status = 6 where id ='" + data.Id + "'";
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
        public async Task<IActionResult> Import(List<VendorModel> list)
        {
            string result = "";
            try
            {

                for (int i = 0; i < list.Count; i++)
                {
                    string sql = $@"select * from tbl_VendorModel where VendorName='" + list[i].VendorName + "' and Status=5";
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    if (dt.Rows.Count == 0)
                    {
                        string FeaturedImage = "";
                        string Logo = "";
                        if (list[i].FeatureImg == null || list[i].FeatureImg == "")
                        {
                            FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/nophotos.JPG";
                        }
                        else
                        {
                            FeaturedImage = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + list[i].FeatureImg.Replace(" ", "%20"); ;
                        }
                        if (list[i].VendorLogo == null || list[i].VendorLogo == "")
                        {
                            Logo = "https://www.alfardanoysterprivilegeclub.com/assets/img/nophotos.JPG";
                        }
                        else
                        {
                            Logo = "https://www.alfardanoysterprivilegeclub.com/assets/img/" + list[i].VendorLogo.Replace(" ", "%20"); ;
                        }


                        string query = $@"insert into tbl_VendorModel (VendorName,BusinessTypeId,Description,Services,WebsiteUrl,FeatureImg,Gallery,Cno,Email,VideoUrl,VrUrl,BusinessLocationID,Status,FileUrl,Map,VendorLogo,Address) values
                                     ('" + list[i].VendorName + "','" + list[i].BusinessTypeId + "','" + list[i].Description + "','" + list[i].Services + "','" + list[i].WebsiteUrl + "','" + FeaturedImage + "','" + list[i].Gallery + "','" + list[i].Cno + "','" + list[i].Email + "'" +
                                       ",'" + list[i].VideoUrl + "','" + list[i].VrUrl + "','" + list[i].BusinessLocationID + "',5,'" + list[i].FileUrl + "','" + list[i].Map + "','" + Logo + "','" + list[i].Address + "')";
                        db.AUIDB_WithParam(query);
                        result = "Inserted Successfully";
                    }
                    else
                    {
                        _global.Status = "Duplicate Entry.";
                    }

                }



            }
            catch (Exception ex)
            {
                _global.Status = ex.GetBaseException().ToString();

            }

            return Content(_global.Status);
        }
        public class VendorDetails
        {
            public string? Id { get; set; }
            public string? vendorID { get; set; }
            public string? WebsiteUrl { get; set; }
            public string? FeatureImg { get; set; }
            public string? VendorName { get; set; }
            public string? Description { get; set; }
            public string? location { get; set; }
            public string? Cno { get; set; }
            public string? Gallery { get; set; }
            public string? Email { get; set; }
            public string? VideoUrl { get; set; }
            public string? VrUrl { get; set; }
            public string? FileUrl { get; set; }
            public string? Services { get; set; }
            public string? Map { get; set; }
            public string? Address { get; set; }
            public string? VendorLogo { get; set; }
            public string? BusinessTypeName { get; set; }
            public string? BusinessName { get; set; }
            public string? VendorID { get; set; }

        }
        public class VendorDetail2
        {
           
            public string? BusinessTypeName { get; set; }
            public string? VendorName { get; set; }
            public string? Description { get; set; }
            public string? Address { get; set; }
            public string? VendorID { get; set; }
            public string? FeatureImg { get; set; }
            public string? VendorLogo { get; set; }

        } 
        public class VendorDetail3
        {
           
            public string? WebsiteUrl { get; set; }
            public string? BusinessTypeName { get; set; }
            public string? FeatureImg { get; set; }
            public string? VendorName { get; set; }
            public string? Description { get; set; }
            public string? Cno { get; set; }
            public string? Gallery { get; set; }
            public string? Email { get; set; }
            public string? Id { get; set; }
            public string? VideoUrl { get; set; }
            public string? VrUrl { get; set; }
            public string? VendorID { get; set; }
            public string? FileUrl { get; set; }
            public string? Services { get; set; }
            public string? Map { get; set; }
            public string? VendorLogo { get; set; }
            public string? Address { get; set; }
            public string? location { get; set; }

        }
        public class DiscoverDetails
        {
            public string? VendorName { get; set; }
            public string? FeatureImg { get; set; }
            public string? VendorLogo { get; set; }
            public string? VendorID { get; set; }

        }
        public class Discover
        {

            public string? BusinessTypeName { get; set; }
            public string? BusinessID { get; set; }

        }
        public class btypeName
        {
            public string? BusinessTypeName { get; set; }

        }


    }
}
