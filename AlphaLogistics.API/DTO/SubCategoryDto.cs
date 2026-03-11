namespace AlphaLogistics.API.DTO
{
    public class SubCategoryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        /// <summary>Number of products in this subcategory.</summary>
        public int ProductCount { get; set; }
    }
}
