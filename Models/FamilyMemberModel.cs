using System.Text.Json.Serialization;

namespace API.Models
{
    public class FamilyMemberModel
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Relationship { get; set; }
        public int FamilyUserId { get; set; }
        public string ApplicationStatus { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
