namespace AlphaLogistics.API.DTO
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SubCategoryDto>? SubCategories { get; set; }
        /// <summary>Total number of products in this category (across all its subcategories).</summary>
        public int ProductCount { get; set; }
    }
}
