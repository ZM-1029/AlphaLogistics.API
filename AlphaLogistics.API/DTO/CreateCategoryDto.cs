using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
