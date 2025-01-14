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
using static AuthSystem.Data.Controller.ApiVendorController;
using AuthSystem.ViewModel;

namespace AuthSystem.Data.Controller
{

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiBusinessLocController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;


        public ApiBusinessLocController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {
   
            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;
   
        }
      
        [HttpGet]
             
        public async Task<IActionResult> BusinessLocList()
        {
            string sql = $@"SELECT        tbl_BusinessLocationModel.DateCreated, tbl_BusinessLocationModel.PostalCode, tbl_BusinessLocationModel.City, tbl_BusinessLocationModel.Country, tbl_BusinessLocationModel.Id, tbl_StatusModel.Name AS Status, 
                         tbl_BusinessLocationModel.BusinessLocID
                        FROM            tbl_BusinessLocationModel INNER JOIN
                                                 tbl_StatusModel ON tbl_BusinessLocationModel.Active = tbl_StatusModel.Id
                        WHERE        (tbl_BusinessLocationModel.Active = 5)";
            var result = new List<BusinesslocationVM>();
            DataTable table = db.SelectDb(sql).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                var item = new BusinesslocationVM();
                item.Id = int.Parse(dr["Id"].ToString());
                item.PostalCode = dr["PostalCode"].ToString();
                item.City = dr["City"].ToString();
                item.Country = dr["Country"].ToString();
                item.Status = dr["Status"].ToString();
                item.BusinessLocID = dr["BusinessLocID"].ToString();
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");

                result.Add(item);
            }
            return Ok(result);
        }

        [HttpPost]
        public async  Task<IActionResult> SaveBusinessLoc(BusinessLocation data)
        {
            try
            {
                string result = "";
                GlobalVariables gv = new GlobalVariables();
                _global.Status = gv.BusinessLocationRegister(data,  _context);
            }

            catch (Exception ex)
            {
                string status = ex.GetBaseException().ToString();
            }
             return Content(_global.Status);
        }
    
        [HttpPost]
        public async Task<IActionResult> UpdateBusinessLoc(BusinessLocation data)
        {
            string result = "";
            string query = "";
            try
            {

                if (data.Country.Length != 0 || data.City.Length != 0)
                {

                    if (data.Id == 0)
                    {
                        string sql = $@"select * from tbl_BusinessLocationModel where Country='" + data.Country + "' and  City='" + data.City + "' and Active='5'";
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        if (dt.Rows.Count == 0)
                        {
                            query += $@"insert into tbl_BusinessLocationModel (Country,City,PostalCode,Active) values ('" + data.Country + "','" + data.City + "','" + data.PostalCode + "','5')";
                            db.AUIDB_WithParam(query);
                            result = "Inserted Successfully";
                            return Ok(result);

                        }
                        else
                        {
                            result = "Business Location already Exist";
                            return BadRequest(result);
                        }
                    }
                    else
                    { 
                        query += $@"update  tbl_BusinessLocationModel set Country='" + data.Country + "' , City='" + data.City + "' , PostalCode='" + data.PostalCode + "' , Active='5'  where  Id='" + data.Id + "' ";
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
        [HttpPost]
        public IActionResult DeleteBusinessLoc(DeleteVen data)
        {

            string sql = $@"select * from tbl_BusinessLocationModel where id ='" + data.Id + "' and Active='5'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            if (dt.Rows.Count != 0)
            {

                //string sql1 = $@"select * from tbl_BusinessModel where LocationId ='" + data.Id + "'";
                string sql1 = $@"SELECT TOP 1 bl.id, bl.Country, v.BusinessLocationID, b.LocationId FROM tbl_BusinessLocationModel  AS bl  WITH(NOLOCK)
                                LEFT JOIN tbl_BusinessModel AS b WITH(NOLOCK)
                                ON b.LocationId = bl.id
                                LEFT JOIN tbl_VendorModel AS v WITH(NOLOCK)
                                ON v.BusinessLocationID = CONVERT(varchar(10), bl.id)
                                WHERE 
                                bl.id = '" + data.Id + "' AND bl.Active = '5'AND (b.LocationId IS NOT NULL OR v.BusinessLocationID IS NOT NULL)";
                DataTable dt1 = db.SelectDb(sql1).Tables[0];
                if (dt1.Rows.Count == 0)
                {
                    string query = $@"update  tbl_BusinessLocationModel set Active='6' where  Id='" + data.Id + "'";
                    db.AUIDB_WithParam(query);
                    result.Status = "Successfully Deleted";
                    return Ok(result);
                }
                else
                {
                    result.Status = "Business Location is Already in Used!";

                    return BadRequest(result);

                }

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
