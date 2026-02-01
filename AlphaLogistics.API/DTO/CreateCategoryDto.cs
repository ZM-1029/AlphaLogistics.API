using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class CreateCategoryDto
    {
        public int? Id { get; set; }
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
