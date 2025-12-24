using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class VendorMaster
    {
        public int Id { get; set; }

        [ForeignKey("UserMaster")]
        public int UserId { get; set; }
        public string Name { get; set; }
        public string? ProfileImage { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<ProductMaster>? ProductMasters { get; set; }
        public UserMaster UserMaster { get; set; }


    }
}
