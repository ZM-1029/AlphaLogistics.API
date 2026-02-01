using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class CreateSubCategoryDto
    {

        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
