namespace Catalog.API.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation property mapping
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
