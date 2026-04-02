namespace StockBite.Helpers
{
    public static class ProductDisplayHelper
    {
        public static string GetImageUrl(string? productName)
        {
            return ProductCatalogHelper.GetImageUrl(null, null, productName);
        }

        public static string GetUnit(string? productName)
        {
            var name = productName?.Trim().ToLowerInvariant() ?? "";

            return name switch
            {
                "milk" => "bottle",
                "yogurt" => "cup",
                "butter" or "cheese" or "buns" => "pack",
                "bread" => "loaf",
                "sunflower oil" or "olive oil" or "canola oil" => "bottle",
                _ => "lb"
            };
        }

        public static string GetPriceText(decimal price, string? productName)
        {
            return $"{price:C} / {GetUnit(productName)}";
        }

        public static string GetQuantityText(int quantity, string? productName)
        {
            return $"{quantity} {GetUnit(productName)}";
        }

        public static int GetShelfLifeDays(string? productName)
        {
            var name = productName?.Trim().ToLowerInvariant() ?? "";

            return name switch
            {
                "milk" or "yogurt" => 7,
                "butter" or "cheese" => 20,
                "chicken" or "beef" or "mutton" => 5,
                "bread" or "buns" => 4,
                "onions" or "potatoes" or "garlic" or "ginger" => 20,
                "sunflower oil" or "olive oil" or "canola oil" => 90,
                "corn flour" or "wheat flour" or "basmati rice" or "jasmine rice" => 60,
                _ => 10
            };
        }

        public static int GetDailyUsage(string? productName)
        {
            var unit = GetUnit(productName);

            return unit switch
            {
                "pack" => 1,
                "cup" => 1,
                "loaf" => 1,
                "bottle" => 1,
                _ => 2
            };
        }
    }
}
