using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthSystem.ViewModel
{
    public class UserDetailsVM
    {
        public int Id { get; set; }

        public string Username { get; set; }


        public string Fname { get; set; }

  
        public string Lname { get; set; }


        public string Email { get; set; }
 
        public string Gender { get; set; }

        public string isVIP { get; set; }

        public string PositionName { get; set; }

        public string Corporatename { get; set; }

        public string UserType { get; set; }
        public string Status { get; set; }
        public string Cno { get; set; }
        public string Address { get; set; }
        public string MembershipName { get; set; }
        public string MemValidity { get; set; }
        public string CompanyAddress { get; set; }
        public string ProfileImgPath { get; set; }
        public string EmployeeID { get; set; }
        public string MembershipNumber { get; set; }
        public string CorpCno { get; set; }
        public string AllowEmailNotif { get; set; }

        public string? MembershipCard { get; set; }
        public string? VIPCard { get; set; }
        public string? QRFrame { get; set; }
        public string? VIPBadge { get; set; }
        public string? TextColor { get; set; }

    }
}
