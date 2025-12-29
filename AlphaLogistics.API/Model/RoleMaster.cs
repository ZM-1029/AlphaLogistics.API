using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.Model
{
    public class RoleMaster
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!; 
        public bool IsActive { get; set; }
        public virtual ICollection<UserMaster> UserMasters { get; set; }
    }
}
