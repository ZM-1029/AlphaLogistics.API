using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class SubCategoryMaster
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("CategoryMaster")]
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public CategoryMaster? CategoryMaster { get; set; }
    }
}
