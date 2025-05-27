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
using System.Text;
using AuthSystem.ViewModel;
using static AuthSystem.Data.Controller.ApiUserAcessController;
using static AuthSystem.Data.Controller.ApiRegisterController;
using static AuthSystem.Data.Controller.ApiPaginationController;

namespace AuthSystem.Data.Controller
{

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize("ApiKey")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiCorporateController : ControllerBase
    {
        GlobalVariables gv = new GlobalVariables();
        DbManager db = new DbManager();
        private readonly AppSettings _appSettings;
        private ApplicationDbContext _context;
        private ApiGlobalModel _global = new ApiGlobalModel();
        private readonly JwtAuthenticationManager jwtAuthenticationManager;


        public ApiCorporateController(IOptions<AppSettings> appSettings, ApplicationDbContext context, JwtAuthenticationManager jwtAuthenticationManager)
        {
   
            _context = context;
            _appSettings = appSettings.Value;
            this.jwtAuthenticationManager = jwtAuthenticationManager;
   
        }

        //[HttpGet]
        //public async Task<IActionResult> CorporateList()
        //{
        //    var list = (from a in _context.tbl_CorporatePrivilegesModel
        //                join b in _context.tbl_MembershipModel
        //                on a.MembershipID equals b.Id
        //                join c in _context.tbl_CorporateModel
        //                on a.CorporateID equals c.Id
        //                join e in _context.tbl_UsersModel2
        //               on c.Id equals e.CorporateID
        //                select new
        //                {
        //                    Id = a.Id,
        //                    Membership = b.Name,
        //                    CorporateNmae = c.CorporateName,
        //                    Desc = a.Description,
        //                    Address = c.Address,
        //                    Cno = c.CNo,
        //                    Email = c.EmailAddress,
        //                    Size = a.Size,
        //                    Count = a.Count,
        //                    DateIssued = Convert.ToDateTime(a.DateIssued).ToString("MM/dd/yyyy hh:mm:ss"),
        //                    DateExpired = Convert.ToDateTime(a.DateExpired).ToString("MM/dd/yyyy hh:mm:ss"),
        //                    DateCreated = Convert.ToDateTime(a.DateCreated).ToString("MM/dd/yyyy hh:mm:ss"),
        //                    Fullname = e.Fullname

        //                }
        //                ).ToList();
        //    return Ok(list);
        //}

        [HttpPost]
        public async Task<IActionResult> Corporatelist(paginateCorpUserv2 data)
        {
            GlobalVariables gv = new GlobalVariables();
            string sql = $@"SELECT        UsersModel.Username, UsersModel.Fname, UsersModel.Lname, UsersModel.Email, UsersModel.Gender, UsersModel.EmployeeID, tbl_PositionModel.Name AS Position, tbl_CorporateModel.CorporateName, 
                  tbl_UserTypeModel.UserType, UsersModel.Fullname, UsersModel.Id, UsersModel.DateCreated, tbl_PositionModel.Id AS PositionID, tbl_CorporateModel.Id AS CorporateID, tbl_StatusModel.Name AS status, UsersModel.isVIP, 
                  UsersModel.FilePath
                 FROM            UsersModel INNER JOIN
                 tbl_CorporateModel ON UsersModel.CorporateID = tbl_CorporateModel.Id LEFT OUTER JOIN
                 tbl_PositionModel ON UsersModel.PositionID = tbl_PositionModel.Id LEFT OUTER JOIN
                 tbl_UserTypeModel ON UsersModel.Type = tbl_UserTypeModel.Id LEFT OUTER JOIN
                 tbl_StatusModel ON UsersModel.Active = tbl_StatusModel.Id
                 WHERE        (UsersModel.Active IN (1, 2, 9, 10)) AND (UsersModel.Type = 2)";
            //order by UsersModel.Id Desc";
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
            sql += " order by UsersModel.Id desc";
            //if (data.CorpId != null)
            //{
            //    sql += " AND tbl_CorporateModel.Id = " + data.CorpId;
            //}
            //if (data.PosId != null)
            //{
            //    sql += " AND tbl_PositionModel.Id = " + data.PosId;
            //}
            //if (data.Gender != null)
            //{
            //    sql += " AND UsersModel.Gender = '" + data.Gender + "'";
            //}
            //if (data.isVIP != null)
            //{
            //    sql += " AND UsersModel.isVIP = " + data.isVIP;
            //}
            //if (data.Status != null)
            //{
            //    sql += " AND tbl_StatusModel.Name = '" + data.Status + "'";
            //}
            //if (data.FilterName != null)
            //{
            //    sql += " AND (UsersModel.Fname like '%" + data.FilterName + "%' or UsersModel.Lname like '%" + data.FilterName + "%')";
            //}

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
                item.FilePath = dr["FilePath"].ToString();
                item.isVIP = dr["isVIP"].ToString();
                result.Add(item);
            }

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> CompanyList()
        {
            string sql = $@"SELECT DISTINCT 
                         tbl_CorporateModel.CorporateName, tbl_CorporateModel.Address, tbl_CorporateModel.CNo, tbl_CorporateModel.EmailAddress, tbl_CorporateModel.CompanyID, tbl_MembershipModel.Name AS Tier, 
                         tbl_MembershipModel.Description, tbl_CorporateModel.MembershipID AS memid, tbl_StatusModel.Name AS Status, tbl_CorporateModel.Id, tbl_CorporateModel.DateCreated,
                         tbl_CorporateModel.VipCount AS VIPCount, tbl_CorporateModel.Count AS UserCount, tbl_CorporateModel.DateUsed, tbl_CorporateModel.DateEnded
FROM            tbl_MembershipModel LEFT OUTER JOIN
                         tbl_CorporateModel ON tbl_MembershipModel.Id = tbl_CorporateModel.MembershipID LEFT OUTER JOIN
                         tbl_StatusModel ON tbl_CorporateModel.Status = tbl_StatusModel.Id 
WHERE        (tbl_CorporateModel.Status = 1) order by tbl_CorporateModel.Id desc";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new List<CorporateVM>();
            foreach(DataRow dr in dt.Rows)
            {
                var dateu = dr["DateUsed"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : dr["DateUsed"].ToString();
                var datee = dr["DateEnded"].ToString() == "" ? DateTime.Now.ToString("yyyy-MM-dd") : dr["DateEnded"].ToString();
                var item = new CorporateVM();
                item.Id = int.Parse(dr["Id"].ToString()) ;
                item.DateCreated = Convert.ToDateTime(dr["DateCreated"].ToString()).ToString("MM/dd/yyyy") ;
                item.CNo = dr["CNo"].ToString();
                item.EmailAddress = dr["EmailAddress"].ToString();
                item.CorporateName = dr["CorporateName"].ToString();
                item.Address = dr["Address"].ToString();
                item.CompanyID = dr["CompanyID"].ToString();
                item.Tier = dr["Tier"].ToString();
                item.UserCount = dr["UserCount"].ToString();
                item.VIPCount = dr["VIPCount"].ToString();
                item.Description = dr["Description"].ToString();
                item.Status = dr["Status"].ToString();
                item.memid = dr["memid"].ToString();
                item.DateUsed = Convert.ToDateTime(dateu).ToString("yyyy-MM-dd");
                item.DateEnded = Convert.ToDateTime(datee).ToString("yyyy-MM-dd");
                result.Add(item);

            }
            return Ok(result);
        } 
        public class membershiptier
        {

            public string Id { get; set; }
            public string MembershipName { get; set; }
        } 
        public class corporateid
        {
            public string Id { get; set; }
        }
        public class CorporateStatus
        {

            public string status { get; set; }
        }
   
        public class membershipprivilegedata
        {
            public int MembershipID { get; set; }
            public int Count { get; set; }
            public int VipCount { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> MembershipCorporate(corporateid data)
        {
            var result = new membershiptier();
            var res = new CorporateStatus();
            try
            {

                string sql = $@"SELECT  tbl_CorporateModel.Address, tbl_CorporateModel.CorporateName, tbl_CorporateModel.CNo, tbl_CorporateModel.EmailAddress, tbl_CorporateModel.DateCreated, tbl_CorporateModel.Status, 
                         tbl_CorporateModel.CompanyID, tbl_MembershipModel.Name as MembershipName, tbl_MembershipModel.Id as memid
                         FROM            tbl_CorporateModel INNER JOIN
                         tbl_MembershipModel ON tbl_CorporateModel.MembershipID = tbl_MembershipModel.Id where      (tbl_CorporateModel.Id = '" +data.Id+"')";
                DataTable dt = db.SelectDb(sql).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    result.Id = dt.Rows[0]["memid"].ToString() ;
                    result.MembershipName = dt.Rows[0]["MembershipName"].ToString();
                }
                    

                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest("ERROR");
            }
            return Ok(result);
        }
        [HttpPost]
        public async  Task<IActionResult> SaveCorporate(CorporateVM data)
        {
            try
            {
                string result = "";
                GlobalVariables gv = new GlobalVariables();
                _global.Status = gv.CorporateRegister(data,  _context);
            }

            catch (Exception ex)
            {
                string status = ex.GetBaseException().ToString();
            }
             return Content(_global.Status);
        }
        //public class CorporateModel
        //{

        //    public int Id { get; set; }
        //    public int Count { get; set; }
        //    public int VipCount { get; set; }

        //    public string CorporateName { get; set; }
        //    public string? Address { get; set; }
        //    public string? CNo { get; set; }
        //    public string? EmailAddress { get; set; }
        //    public string? Description { get; set; }
        //    public string? MembershipID { get; set; }
        //    public int? Status { get; set; }

        //}
        [HttpPost]
        public async Task<IActionResult> UpdateCorporate(CorporateModel data)
        {
            string result = "";
            try
            {
                string query = "";

                if (data.CorporateName.Length != 0 || data.CNo.Length != 0 || data.EmailAddress.Length != null )
                {
                    if (data.Id == 0)
                    {

                  

                        query += $@"insert into tbl_CorporateModel ( CorporateName, Address, CNo, EmailAddress, Status, MembershipID,Count,VipCount,DateUsed,DateEnded) values
                                ('" + data.CorporateName + "','" + data.Address + "','" + data.CNo + "','" + data.EmailAddress + "','1','" + data.MembershipID + "' ,'" + data.Count + "' ,'" + data.VipCount + "' ,'" + data.DateUsed + "','" + data.DateEnded + "')";
                        db.AUIDB_WithParam(query);


                        string sql = $@"SELECT [Id]
                                      ,[PrivilegeID]
                                      ,[MembershipID]
                                      ,[Count]
                                      ,[VipCount]
                                  FROM [dbo].[tbl_MembershipPrivilegeModel] where MembershipID='"+ data.MembershipID + "'";
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        //string sql_corpid = $@"SELECT top(1) [Id]
                        //              ,[PrivilegeID]
                        //              ,[MembershipID]
                        //              ,[Count]
                        //              ,[VipCount]
                        //          FROM [dbo].[tbl_MembershipPrivilegeModel] order by id desc";
                        //DataTable dt_corpid = db.SelectDb(sql_corpid).Tables[0];
                        //string delete = $@"delete tbl_CorporatePrivilegeTierModel where CorporateID='" + dt_corpid.Rows[0]["Id"].ToString() + "'";



                        string sql_corpid = $@"SELECT TOP (1) [Id],[CorporateName] FROM [AOPCDB].[dbo].[tbl_CorporateModel] order by id desc";
                        DataTable dt_corpid = db.SelectDb(sql_corpid).Tables[0];
                        string delete = $@"delete tbl_CorporatePrivilegeTierModel where CorporateID='" + dt_corpid.Rows[0]["Id"].ToString() + "'";
                        db.AUIDB_WithParam(delete);
                        foreach (DataRow dr in dt.Rows)
                        {
                            string insert = $@"insert into tbl_CorporatePrivilegeTierModel (PrivilegeID,CorporateID,Count,VipCount) values 
                                             ('" + dr["PrivilegeID"].ToString() + "','" + dt_corpid.Rows[0]["Id"].ToString() + "','" + data.Count + "','" + data.VipCount + "')";
                            db.AUIDB_WithParam(insert);
                        }

                     
                        result = "Registered Successfully";
                        
                    }
                    else
                    {
                        //string delete = $@"delete tbl_UserMembershipModel where MembershipID='" + data.MembershipID + "'";
                        //db.AUIDB_WithParam(delete);
                        string sql = $@"select tbl_CorporateModel.CorporateName,tbl_CorporateModel.Id as corporateid ,UsersModel.Username , UsersModel.Id as userid from UsersModel 
                                        inner join
                                        tbl_CorporateModel on UsersModel.CorporateID = tbl_CorporateModel.Id where tbl_CorporateModel.Id='" + data.Id + "' ";
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        foreach (DataRow dr in dt.Rows)
                        {
                            string delete = $@"delete tbl_UserPrivilegeModel where UserID='" + dr["userid"].ToString() + "'";
                            db.AUIDB_WithParam(delete);

                            string sql_insertuserprivilege = $@"SELECT  tbl_CorporatePrivilegeTierModel.Id, tbl_CorporatePrivilegeTierModel.PrivilegeID, tbl_CorporatePrivilegeTierModel.CorporateID, tbl_CorporatePrivilegeTierModel.Count, tbl_CorporatePrivilegeTierModel.VipCount, 
                                                            tbl_CorporateModel.DateEnded
                                                            FROM            tbl_CorporatePrivilegeTierModel INNER JOIN
                                                            tbl_CorporateModel ON tbl_CorporatePrivilegeTierModel.CorporateID = tbl_CorporateModel.Id where CorporateID ='" +data.Id+"'";
                            DataTable dt_ = db.SelectDb(sql_insertuserprivilege).Tables[0];
                            foreach (DataRow dr_ in dt_.Rows)
                            {

                                string insertuserprivilege = $@"insert into tbl_UserPrivilegeModel (PrivilegeId,UserID,Validity) values 
                                ('" + dr_["PrivilegeID"].ToString() + "','" + dr["userid"].ToString() + "','" + Convert.ToDateTime(dt_.Rows[0]["DateEnded"].ToString()).ToString("yyyy-MM-dd") + "')";
                                db.AUIDB_WithParam(insertuserprivilege);
                            }
                                string updateuserprivilege = "";
                            updateuserprivilege = $@"
                                                        UPDATE [dbo].[tbl_UserMembershipModel]
                                                           SET [MembershipID] = '" + data.MembershipID + "', [Validity] ='" + data.DateEnded + "' WHERE [UserID]='" + dr["userid"].ToString() + "'";


                            db.AUIDB_WithParam(updateuserprivilege);

                        }


                        string sqls = $@"SELECT [Id]
                                      ,[PrivilegeID]
                                      ,[MembershipID]
                                      ,[Count]
                                      ,[VipCount]
                                  FROM [dbo].[tbl_MembershipPrivilegeModel] where MembershipID='" + data.MembershipID + "'";
                        DataTable dts = db.SelectDb(sqls).Tables[0];
            
                        string deletes = $@"delete tbl_CorporatePrivilegeTierModel where CorporateID='" + data.Id + "'";
                        db.AUIDB_WithParam(deletes);
                        foreach (DataRow drs in dts.Rows)
                        {
                            string insert = $@"insert into tbl_CorporatePrivilegeTierModel (PrivilegeID,CorporateID,Count,VipCount) values 
                                             ('" + drs["PrivilegeID"].ToString() + "','" + data.Id + "','" + data.Count + "','" + data.VipCount + "')";
                            db.AUIDB_WithParam(insert);
                        }
                        query += $@"update  tbl_CorporateModel set CorporateName='" + data.CorporateName + "',Address='" + data.Address + "' " +
                               ",CNo='" + data.CNo + "' , EmailAddress='" + data.EmailAddress + "' , MembershipID='" + data.MembershipID + "', Count='"+data.Count+"', VipCount='"+data.VipCount+"', DateUsed='"+Convert.ToDateTime(data.DateUsed).ToString("yyyy-MM-dd HH:mm:ss")+"', DateEnded='"+ Convert.ToDateTime(data.DateEnded).ToString("yyyy-MM-dd HH:mm:ss") + "' where  Id='" + data.Id + "' ";
                        db.AUIDB_WithParam(query);
                        result = "Updated Successfully";
                    }




                }
                else
                {
                    result = "Error in Registration";
                }
                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> SaveUserMembership(CorporateModel data)
        {
            string result = "";
            try
            {
                string query = "";

                if (data.CorporateName.Length != 0 ||data.CNo.Length != 0 || data.EmailAddress.Length != null)
                {
                    if (data.Id == 0)
                    {

                        query += $@"insert into tbl_CorporateModel ( CorporateName, Address, CNo, EmailAddress, Status, MembershipID,DateUsed,DateEnded) values
                                ('" + data.CorporateName + "','" + data.Address + "','" + data.CNo + "','" + data.EmailAddress + "','1','" + data.MembershipID + "' ,'"+data.DateUsed+"','"+ data.DateEnded + "')";
                        db.AUIDB_WithParam(query);
                        result = "Registered Successfully";

                    }
                    else
                    {
                        string delete = $@"delete tbl_UserMembershipModel where MembershipID='" +data.Id+ "'";
                        string sql = $@"select tbl_CorporateModel.CorporateName,tbl_CorporateModel.Id as corporateid ,UsersModel.Username , UsersModel.Id as userid from UsersModel 
                                        inner join
                                        tbl_CorporateModel on UsersModel.CorporateID = tbl_CorporateModel.Id where tbl_CorporateModel.Id='" + data.Id + "' ";
                        DataTable dt = db.SelectDb(sql).Tables[0];
                        foreach(DataRow dr in dt.Rows)
                        {
                            string updateuserprivilege = $@"
                                                        UPDATE [dbo].[tbl_UserMembershipModel]
                                                           SET [MembershipID] = '"+data.MembershipID+"', [Validity] ='"+ data.DateEnded + "' WHERE [UserID]='"+ dr["userid"].ToString() + "'";
                            db.AUIDB_WithParam(updateuserprivilege);
                        }
                        query += $@"update  tbl_CorporateModel set CorporateName='" + data.CorporateName + "',Address='" + data.Address + "'" +
                               ",CNo='" + data.CNo + "' , EmailAddress='" + data.EmailAddress + "' , MembershipID='" + data.MembershipID + "' where  Id='" + data.Id + "' ";
                        db.AUIDB_WithParam(query);
                        result = "Updated Successfully";
                    }




                }
                else
                {
                    result = "Error in Registration";
                }
                return Ok(result);
            }

            catch (Exception ex)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCorproate(DeleteUser data)
        {

            string sql = $@"select * from tbl_CorporateModel where Id='" + data.Id + "'";
            DataTable dt = db.SelectDb(sql).Tables[0];
            var result = new Registerstats();
            if (dt.Rows.Count > 0)
            {
                string sql1 = $@"select * from UsersModel where CorporateID ='" + data.Id + "' and  Active IN (1,2,9,10)";
                DataTable dt1 = db.SelectDb(sql1).Tables[0];
                if (dt1.Rows.Count == 0)
                {
                    string query = $@"update  tbl_CorporateModel set Status='6' where  Id='" + data.Id + "'";
                    db.AUIDB_WithParam(query);
                    result.Status = "Successfully Deleted";
                    return Ok(result);
                }
                else
                {
                    result.Status = "Corporate is Already in Used!";

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
        [HttpPost]
        public async Task<IActionResult> ImportCorporate(List<CorporateModel> list)
        {
            string result = "";
            try
            {

                for (int i = 0; i < list.Count; i++)
                {
                    string sql = $@"select * from tbl_CorporateModel where CorporateName='" + list[i].CorporateName + "' and Status in(1,2)";
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    if (dt.Rows.Count == 0)
                    {
                       
                        string query = $@"insert into tbl_CorporateModel ( CorporateName, Address, CNo, EmailAddress, Status, MembershipID) values
                    ('" + list[i].CorporateName + "','" + list[i].Address + "','" + list[i].CNo + "','" + list[i].EmailAddress + "','1','" + list[i].MembershipID + "')";
                        db.AUIDB_WithParam(query);
     
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

        [HttpPost]
        public async Task<IActionResult> UpdateMembershipPrivilege(membershipprivilegedata data)
        {
            string result = "";
            try
            {

                    string sql = $@"select * from tbl_MembershipPrivilegeModel where MembershipID = '"+data.MembershipID + "'";
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    if (dt.Rows.Count != 0)
                    {

                        string query = $@"update tbl_MembershipPrivilegeModel set VipCount='"+data.VipCount+"' , Count= '"+data.Count+"' where MembershipID = '"+data.MembershipID+"'";
                        db.AUIDB_WithParam(query);

                        return Ok("Success");
                    }
                    else
                    {

                    return BadRequest("There is no Privilege assigned in this Tier ");
                    }

                


            }
            catch (Exception ex)
            {
                _global.Status = ex.GetBaseException().ToString();

            }

            return Ok(db);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteCorporate(int id)
        {
            try
            {
                var result = await _context.tbl_BusinessTypeModel.FindAsync(id);
                _context.tbl_BusinessTypeModel.Remove(result);
                await _context.SaveChangesAsync();
                _global.Status = "Successfully Deleted.";

            }
            catch (Exception ex)
            {
                _global.Status = ex.GetBaseException().ToString();
            }

            return Content(_global.Status);
        }

        [HttpPost]
        public async Task<IActionResult> FinalCorporateRegistration(CorporateModel data)
        {
            try
            {
                var model = new CorporateModel()
                {
                    CorporateName = data.CorporateName,
                    Address = data.Address,
                    CNo = data.CNo,
                    EmailAddress = data.EmailAddress,
                    Status = 1,

                };

                if (data.Id != 0)
                {
                 
                    string sql = $@"SELECT        CorporateName, Address, CNo, EmailAddress, Status, CompanyID
                                FROM            tbl_CorporateModel
                                WHERE        (CorporateName = '" + data.CorporateName + "') AND (Address = '" + data.Address + "')  AND (EmailAddress = '" + data.EmailAddress + "') AND (Status = 2)";
                    DataTable dt = db.SelectDb(sql).Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        model.Id = data.Id;
                        _context.tbl_CorporateModel.Update(model);
                        _context.SaveChanges();

                        return Ok("Successfully Registered");
                    }
                    else
                    {

                        return BadRequest("Invalid Registration");
                    }
                }
                else
                {
                    return BadRequest("Invalid Registration");
                }

            }

            catch (Exception ex)
            {
                string status = ex.GetBaseException().ToString();
            }

            return Ok("Successfully Registered");
        }
    }
}
