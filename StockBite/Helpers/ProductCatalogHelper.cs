namespace StockBite.Helpers
{
    public static class ProductCatalogHelper
    {
        public static readonly List<string> Categories = new()
        {
            "Veggies",
            "Dairy",
            "Meat",
            "Flour",
            "Oil",
            "Bakery",
            "Grains"
        };

        public static string NormalizeCategory(string? category)
        {
            var value = category?.Trim() ?? string.Empty;
            return Categories.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase)) ?? "Veggies";
        }

        public static string GetCategoryForProduct(string? productName, string? savedCategory = null)
        {
            var normalizedSavedCategory = NormalizeCategory(savedCategory);
            if (!string.IsNullOrWhiteSpace(savedCategory))
            {
                return normalizedSavedCategory;
            }

            var name = productName?.Trim().ToLowerInvariant() ?? string.Empty;

            return name switch
            {
                "milk" or "cheese" or "butter" or "yogurt" => "Dairy",
                "chicken" or "beef" or "mutton" => "Meat",
                "corn flour" or "wheat flour" => "Flour",
                "sunflower oil" or "olive oil" or "canola oil" => "Oil",
                "bread" or "buns" or "banana bread" => "Bakery",
                "basmati rice" or "jasmine rice" => "Grains",
                _ => "Veggies"
            };
        }

        public static string GetDefaultImageUrl(string? category)
        {
            return NormalizeCategory(category) switch
            {
                "Dairy" => "/Images/Products/dairy.svg",
                "Meat" => "/Images/Products/meat.svg",
                "Flour" => "/Images/Products/flour.svg",
                "Oil" => "/Images/Products/oil.svg",
                "Bakery" => "/Images/Products/bakery.svg",
                "Grains" => "/Images/Products/grains.svg",
                _ => "/Images/Products/veggies.svg"
            };
        }

        public static string GetImageUrl(string? imageUrl, string? category, string? productName = null)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                return imageUrl;
            }

            return GetDefaultImageUrl(GetCategoryForProduct(productName, category));
        }
    }
}
