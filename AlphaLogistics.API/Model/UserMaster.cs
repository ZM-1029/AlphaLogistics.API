using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class UserMaster
    {
        public int Id { get; set; }
        [ForeignKey("RoleMaster")]
        public int RoleId { get; set; }
        public string UserName { get; set; }
        public string? ProfileImage { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsActive { get; set; }=true;
        public RoleMaster? RoleMaster { get; set; }

    }
}
