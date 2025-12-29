using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class DocumentMaster
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("VendorMaster")]
        public int VendorId { get; set; }

        public string DocumentName { get; set; }
        public string DocumentUrl { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public VendorMaster? VendorMaster { get; set; }
    }
}
