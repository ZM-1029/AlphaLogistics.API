using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class ProductImages
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("ProductMaster")]
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public ProductMaster? ProductMaster { get; set; }

    }
}
