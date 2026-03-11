using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
        public class VendorMaster
        {
            [Key]
            public int Id { get; set; }

            [ForeignKey("UserMaster")]
            public int UserId { get; set; }       
            public string VendorName { get; set; } 
            public string? Reason { get; set; } 
            public string ContactPerson { get; set; }

           
            public string PAN { get; set; }
            public string? VAT { get; set; }

            public string BankAccountNo { get; set; }
            public string BankName { get; set; }
            public string AccHolderName { get; set; }


            public string PrimaryAddress { get; set; }
            public string? SecondaryAddress { get; set; }


            public string? Description { get; set; }


            public bool IsApproved { get; set; } = false;
            public string CustomerType { get; set; } = "Basic"; // Basic, Regular, Premium

            public DateTime CreatedAt { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public bool IsActive { get; set; } = true;

            public int? CreatedBy { get; set; }
            public int? UpdatedBy { get; set; }
            public ICollection<ProductMaster>? ProductMasters { get; set; }
            public ICollection<DocumentMaster>? Documents { get; set; }
            public UserMaster UserMaster { get; set; }

            [ForeignKey("CreatedBy")]
            public UserMaster? CreatedByUser { get; set; }

            [ForeignKey("UpdatedBy")]
            public UserMaster? UpdatedByUser { get; set; }
    }
 }

