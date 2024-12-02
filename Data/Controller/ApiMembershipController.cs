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
using static AuthSystem.Data.Controller.ApiCorporatePrivilegeController;

namespace AuthSystem.Data.Controller
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiMembershipController : ControllerBase
    {
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;


        public ApiMembershipController(ApplicationDbContext context)
        {
   
            _context = context;
   
        }
        public class PrivVendListItem
        {
            public string? Id { get; set; }
            public string? VendorName { get; set; }
            public string? PrivilegeID { get; set; }
            public string? vid { get; set; }
            public string? stats { get; set; }

        }
        public class VenIDs
        {
            public string Id { get; set; }
            //public string? vid { get; set; }

        }

        [HttpPost]
        public IActionResult SaveVendorePrivilegeList(List<PrivVendListItem> IdList)
        {
            string delete = $@"delete tbl_VendorPrivilegeTierModel where PrivilegeID='" + IdList[0].PrivilegeID + "'";
            db.AUIDB_WithParam(delete);
            var result = new Registerstats();
            string imgfile = "";

            foreach (var emp in IdList)
            {
                if (emp.vid != null)
                {
                    string sql = $@"SELECT      PrivilegeID, VendorID, isVIP
                                FROM           tbl_VendorPrivilegeTierModel
                        WHERE        (PrivilegeID = '" + emp.PrivilegeID + "')  and VendorID='" + emp.vid +"'";
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    if (dt.Rows.Count == 0)
                    {
                        if (emp.stats == "1")
                        {


                            string insert = $@"insert into tbl_VendorPrivilegeTierModel (PrivilegeID,VendorID) values 
                                             ('" + emp.PrivilegeID + "','" + emp.vid + "')";
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
        [HttpPost]
        public async Task<IActionResult> PrivilegeVendorList(VenIDs data)
        {

            var result = new List<PrivVendListItem>();
            var stats = "";
            var isvip = "";
            var vid = "";
            string sql2 = $@"SELECT tbl_PrivilegeModel.Id,VendorName FROM tbl_PrivilegeModel
            left join tbl_VendorModel on tbl_PrivilegeModel.BusinessTypeID = tbl_VendorModel.BusinessTypeId
            where tbl_PrivilegeModel.Active = '5' and tbl_PrivilegeModel.Id = '" + data.Id + "'";
            DataTable dt2 = db.SelectDb(sql2).Tables[0];
            foreach (DataRow dr in dt2.Rows)
            {
                var item = new PrivVendListItem();
                item.VendorName = dr["VendorName"].ToString();
                item.vid = dr["Id"].ToString();
                item.stats = "0";
                result.Add(item);
            }



            //foreach (DataRow dr in dt2.Rows)
            //{
            //    string sqls = $@"SELECT       PrivilegeID, VendorID, isVIP
            //                    FROM            tbl_VendorPrivilegeTierModel
            //                    WHERE        (PrivilegeID = '" + data.Id + "') ";
            //    DataTable dts = db.SelectDb(sqls).Tables[0];

            //    if (dts.Rows.Count == 0)
            //    {

            //        string vendor_sql = $@"SELECT      PrivilegeID, VendorID, isVIP
            //                    FROM           tbl_VendorPrivilegeTierModel
            //                    WHERE        (VendorID = '" + dr["Id"].ToString() + "')  and    (PrivilegeID = '" + data.Id + "') ";
            //        DataTable dt_ = db.SelectDb(vendor_sql).Tables[0];
            //        if (dt_.Rows.Count != 0)
            //        {
            //            stats = "1";
            //            isvip = "0";

            //        }
            //        else
            //        {
            //            stats = "0";
            //            isvip = "0";
            //        }
            //        var item = new PrivVendListItem();
            //        item.VendorName = dr["VendorName"].ToString();
            //        item.vid = dr["Id"].ToString();
            //        item.stats = stats;
            //        result.Add(item);
            //    }
            //    else
            //    {
            //        string sql = $@"SELECT      PrivilegeID, VendorID, isVIP
            //                    FROM           tbl_VendorPrivilegeTierModel
            //            WHERE        (PrivilegeID = '" + data.Id + "') and  VendorID='"+dr["Id"].ToString()+"'";
            //        DataTable dt = db.SelectDb(sql).Tables[0];

            //        if (dt.Rows.Count != 0)
            //        {
            //            stats = "1";
            //            isvip = dt.Rows[0]["isVIP"].ToString();
            //        }
            //        else
            //        {
            //            stats = "0";
            //            isvip = "0";
            //        }
            //        var item = new PrivVendListItem();
            //        item.VendorName = dr["VendorName"].ToString();
            //        item.vid = dr["Id"].ToString();
            //        item.stats = stats;
            //        result.Add(item);

            //    }


        //}
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> MembershipList()
        {
            string sql = $@"SELECT        tbl_MembershipModel.Id, tbl_MembershipModel.Name AS MembershipName, tbl_MembershipModel.Description, tbl_MembershipModel.DateUsed AS DateStarted, tbl_MembershipModel.DateEnded, 
                         tbl_MembershipModel.DateCreated, tbl_MembershipModel.MembershipID, tbl_StatusModel.Name AS Status, tbl_MembershipModel.UserCount, tbl_MembershipModel.VIPCount, tbl_MembershipModel.MembershipCard, 
                         tbl_MembershipModel.VIPCard, tbl_MembershipModel.QRFrame, tbl_MembershipModel.VIPBadge, tbl_MembershipModel.TextColor
FROM            tbl_MembershipModel INNER JOIN
                         tbl_StatusModel ON tbl_MembershipModel.Status = tbl_StatusModel.Id
WHERE        (tbl_MembershipModel.Status = 5)
ORDER BY tbl_MembershipModel.Id DESC";
            var result = new List<MembershipVM>();
            DataTable table = db.SelectDb(sql).Tables[0];
            foreach (DataRow dr in table.Rows)
            {
                string membership_path = "";
                string vipcard_path = "";
                string qrframe_path = "";
                string vipbadge_path = "";
                if (dr["MembershipCard"].ToString() == null || dr["MembershipCard"].ToString() == string.Empty) 
                {
                    membership_path = "https://alfardanoysterprivilegeclub.com/assets/img/MembershipCard/id-card.jpg";
                }
                else
                {
                    membership_path = dr["MembershipCard"].ToString();
                }

                if (dr["VIPCard"].ToString() == null || dr["VIPCard"].ToString() == "")
                {
                    vipcard_path = "https://alfardanoysterprivilegeclub.com/assets/img/MembershipCard/id-card.jpg";
                }
                else
                {
                    vipcard_path =  dr["VIPCard"].ToString();
                }
                if (dr["QRFrame"].ToString() == null || dr["QRFrame"].ToString() == string.Empty)
                {
                    qrframe_path = "https://alfardanoysterprivilegeclub.com/assets/img/MembershipCard/id-card.jpg";
                }
                else
                {
                    qrframe_path = dr["QRFrame"].ToString();
                }
                if (dr["VIPBadge"].ToString() == null || dr["VIPBadge"].ToString() == string.Empty)
                {
                    vipbadge_path = "https://alfardanoysterprivilegeclub.com/assets/img/MembershipCard/id-card.jpg";
                }
                else
                {
                    vipbadge_path = dr["VIPBadge"].ToString();
                }
                if (dr["MembershipName"].ToString() !="ALL TIER")
                {
                    var item = new MembershipVM();
                    item.Id = dr["Id"].ToString();
                    item.MembershipName = dr["MembershipName"].ToString();
                    item.Description = dr["Description"].ToString();
                    item.DateStarted = Convert.ToDateTime(dr["DateStarted"].ToString()).ToString("MM/dd/yyyy");
                    item.DateEnded = Convert.ToDateTime(dr["DateEnded"].ToString()).ToString("MM/dd/yyyy");
                    item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy");
                    item.MembershipID = dr["MembershipID"].ToString();
                    item.Status = dr["Status"].ToString();
                    item.UserCount = int.Parse(dr["UserCount"].ToString());
                    item.VIPCount = int.Parse(dr["VIPCount"].ToString());
                    item.MembershipCard = membership_path;
                    item.VIPBadge = vipbadge_path;
                    item.VIPCard = vipcard_path;
                    item.QRFrame = qrframe_path;
                    item.textCardColor = dr["TextColor"].ToString();
                    result.Add(item);
                }
                
            }
            return Ok(result);
        }

        [HttpPost]
        public async  Task<IActionResult> SaveNewMembership(MembershipModel data)
        {
            try
            {
                string result = "";
                GlobalVariables gv = new GlobalVariables();
                _global.Status = gv.MemberShipRegistration(data,_context);
            }

            catch (Exception ex)
            {
                string status = ex.GetBaseException().ToString();
            }
             return Content(_global.Status);
        }
        public class MembershipID
        {
            public int Id { get; set; }

        }
        [HttpPost]
        public async  Task<IActionResult> MembershipFilterbyID(MembershipID data)
        {
            var result = new List<MembershipModelVM>();
            try
            {
                string sql = $@"SELECT        Id, Name AS MembershipName, Description, MembershipID, UserCount, VIPCount, DateCreated
                                FROM            tbl_MembershipModel
                                WHERE        (Id = '"+data.Id+"')";

                DataTable table = db.SelectDb(sql).Tables[0];
              
                if (table.Rows.Count != 0)
                {
           
                    foreach (DataRow dr in table.Rows)
                    {
                        var item = new MembershipModelVM();
                        item.Id = int.Parse(dr["Id"].ToString());
                        item.MembershipName = dr["MembershipName"].ToString();
                        item.Description = dr["Description"].ToString();
                        item.MembershipID = dr["MembershipID"].ToString();
                        item.UserCount = dr["UserCount"].ToString();
                        item.VIPCount = dr["VIPCount"].ToString();
                        item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy") ;
                        result.Add(item);

                    }
                }
            }

            catch (Exception ex)
            {
                string status = ex.GetBaseException().ToString();
            }
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMembershipInfo(MembershipModel data)
        {
            try
            {
                string result = "";
                GlobalVariables gv = new GlobalVariables();
                _global.Status = gv.MembershipUpdateInfo(data ,_context);
            }

            catch (Exception ex)
            {
                string status = ex.GetBaseException().ToString();
            }
            return Content(_global.Status);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteMembershipInfo(int id)
        {
            try
            {
                var result = await _context.tbl_MembershipModel.FindAsync(id);
                _context.tbl_MembershipModel.Remove(result);
                await _context.SaveChangesAsync();
                _global.Status = "Successfully Deleted.";

            }
            catch (Exception ex)
            {
                _global.Status = ex.GetBaseException().ToString();
            }

            return Content(_global.Status);
        }
        public class Registerstats
        {
            public string Status { get; set; }

        }
        [HttpPost]
        public IActionResult SaveMembershipTier(MembershipVM data)
        {


            string sql = "";
            sql = $@"select * from tbl_MembershipModel where id ='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            string membership_path = "";
            string vipcard_path = "";
            string qrframe_path = "";
            string vipbadge_path = "";
            if (data.MembershipCard == null)
            {
                membership_path = "https://alfardanoysterprivilegeclub.com/assets/img/MembershipCard/id-card.jpg";
            }
            else
            {
                membership_path = "https://www.alfardanoysterprivilegeclub.com/assets/img/MembershipCard/" + data.MembershipCard.Replace(" ", "%20");
            }

            if (data.VIPCard == null)
            {
                vipcard_path = "https://alfardanoysterprivilegeclub.com/assets/img/MembershipCard/id-card.jpg";
            }
            else
            {
                vipcard_path = "https://www.alfardanoysterprivilegeclub.com/assets/img/VIPCard/" + data.VIPCard.Replace(" ", "%20");
            }
            if (data.QRFrame == null)
            {
                qrframe_path = "https://alfardanoysterprivilegeclub.com/assets/img/MembershipCard/id-card.jpg";
            }
            else
            {
                qrframe_path = "https://www.alfardanoysterprivilegeclub.com/assets/img/QRFrame/" + data.QRFrame.Replace(" ", "%20");
            }
            if (data.VIPBadge == null)
            {
                vipbadge_path = "https://alfardanoysterprivilegeclub.com/assets/img/MembershipCard/id-card.jpg";
            }
            else
            {
                vipbadge_path = "https://www.alfardanoysterprivilegeclub.com/assets/img/VIPBadge/" + data.VIPBadge.Replace(" ", "%20");
            }
            Console.Write(dt.Rows.Count + " Test1");
            if (dt.Rows.Count == 0)
            {
                sql = $@"select * from tbl_MembershipModel where Name ='" + data.MembershipName + "'  and Status=5";
                DataTable dt2 = db.SelectDb(sql).Tables[0];
                if (dt2.Rows.Count == 0)
                {


           
                    string Insert = $@"insert into tbl_MembershipModel (Name,Description,DateUsed,DateEnded,UserCount,VIPCount,MembershipCard,VIPCard,QRFrame,VIPBadge,Status,TextColor) values 
                                   ('" + data.MembershipName + "'," +
                                   "'" + data.Description + "'," +
                                   "'" + data.DateStarted + "'," +
                                   "'" + data.DateEnded + "'," +
                                   "'" + data.UserCount + "'," +
                                   "'" + data.VIPCount + "'," +
                                   "'" + membership_path+ "'," +
                                   "'" + vipcard_path + "'," +
                                   "'" + qrframe_path + "'," +
                                   "'" + vipbadge_path + "'," +
                                   "5,"+
                                   "'"+data.textCardColor+"') ";
                    db.AUIDB_WithParam(Insert);
                    result.Status = "Successfully Added";

                    return Ok(result);
                }
                else
                {

                    result.Status = "Duplicate Entry";

                    return BadRequest(result);
                }
                   

            }
            else
            {

                

                string Update = $@"update tbl_MembershipModel
                                set Name='" + data.MembershipName + "', " +
                                "Description='" + data.Description + "' , " +
                                "DateUsed='" + data.DateStarted + "', " +
                                "MembershipCard='" + membership_path + "', " +
                                "VIPCard='" + vipcard_path + "', " +
                                "QRFrame='" + qrframe_path + "', " +
                                "VIPBadge='" + vipbadge_path + "', " +
                                "DateEnded='" + data.DateEnded +"'," +
                                "UserCount='" + data.UserCount + "',"  +
                                "TextColor ='" + data.textCardColor + "', " +
                                "VIPCount='" + data.VIPCount + "' " +
                                "where id='"+data.Id+"'";
                db.AUIDB_WithParam(Update);
                result.Status = "Successfully Updated";

                return Ok(result);
            }


            return Ok(result);
        }
        public class DeleteMem
        {

            public int Id { get; set; }
        }
        [HttpPost]
        public IActionResult DeleteMemship(DeleteMem data)
        {

            string sql = $@"select * from tbl_MembershipModel where id ='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            string imgfile = "";
            if (dt.Rows.Count != 0)
            {
                string sql_user = $@"select * from UsersModel inner JOIN
                        tbl_CorporateModel on UsersModel.CorporateID = tbl_CorporateModel.Id
                        where tbl_CorporateModel.MembershipID = '" + data.Id + "' and UsersModel.Active <> 6   ";
                DataTable dt_user = db.SelectDb(sql_user).Tables[0];
                if (dt_user.Rows.Count == 0)

                {

                    string OTPInsert = $@"update tbl_MembershipModel set Status = 6 where id ='" + data.Id + "'";
                    db.AUIDB_WithParam(OTPInsert);
                    result.Status = "Succesfully deleted";

                    return Ok(result);
                }
                else
                {
                    result.Status = "Membership is being used and cannot be deleted!";
                    return Ok(result);
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
