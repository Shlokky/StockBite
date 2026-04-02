namespace StockBite.Helpers
{
    public static class ProductCatalogHelper
    {
        private static readonly Dictionary<string, string> ProductImages = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Butter"] = "https://www.pngarts.com/files/3/Butter-PNG-Image-Background.png",
            ["Cheese"] = "https://static.vecteezy.com/system/resources/thumbnails/028/201/736/small_2x/cheese-collection-various-types-of-cheese-with-herbs-close-up-photo.jpg",
            ["Milk"] = "https://img-s-msn-com.akamaized.net/tenant/amp/entityid/AA1EVw9e.img?w=780&h=438&m=4&q=91",
            ["Yogurt"] = "https://assets.bonappetit.com/photos/63b5fe187015aa59a8e880ac/16:9/w_1920,c_limit/010323-its-greek-yogurt-taste-test-01.jpg",
            ["Beef"] = "https://hips.hearstapps.com/hmg-prod.s3.amazonaws.com/images/delish-roast-beef-horizontal-1540505165.jpg?crop=1xw:0.75xh;center,top&resize=1200:*",
            ["Chicken"] = "https://flavorthemoments.com/wp-content/uploads/2024/01/how-to-cut-a-whole-chicken-hero-1.jpg",
            ["Mutton"] = "https://thekitchencommunity.org/wp-content/uploads/2022/10/mutton-meat.jpeg",
            ["Corn Flour"] = "https://image.made-in-china.com/2f0j00ONmTQWFPMdrq/50lbs-Corn-Starch-Flour-Packaging-Bag-Brown-Kraft-Paper-Bag.jpg",
            ["Onions"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Onion.jpg?width=600",
            ["Potatoes"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Potato.jpg?width=600",
            ["Tomatoes"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Tomato_2.jpg?width=600",
            ["Lettuce"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Romaine_lettuce.jpg?width=600",
            ["Carrots"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Carrots.jpg?width=600",
            ["Cucumbers"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Cucumber_picture.jpg?width=600",
            ["Bell Peppers"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Red_bell_pepper.jpg?width=600",
            ["Spinach"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Spinach_%282009%29.jpg?width=600",
            ["Garlic"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Garlic.jpg?width=600",
            ["Ginger"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Ginger_Root_%28Zingiber_officinale%29.jpg?width=600",
            ["Broccoli"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Broccoli.jpg?width=600",
            ["Cauliflower"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Cauliflower.JPG?width=600",
            ["Mushrooms"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Mushroom_%2812329086143%29.jpg?width=600",
            ["Zucchini"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Zucchini.jpg?width=600",
            ["Cabbage"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Cabbage.jpg?width=600",
            ["Wheat Flour"] = "https://static.vecteezy.com/system/resources/thumbnails/038/970/297/small_2x/ai-generated-organic-natural-whole-grain-flour-in-sacks-and-wheat-seeds-ears-of-wheat-on-an-old-wooden-floor-photo.jpg",
            ["Basmati Rice"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Rice_grains.jpg?width=600",
            ["Jasmine Rice"] = "https://m.media-amazon.com/images/I/81dRpr6gfaL._AC_SX679_.jpg",
            ["Sunflower Oil"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Sunflower_oil.jpg?width=600",
            ["Olive Oil"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Olive_oil.jpg?width=600",
            ["Canola Oil"] = "https://tse4.mm.bing.net/th/id/OIP._pB9G95Wzh5Scxplq-qVoQHaJ4?rs=1&pid=ImgDetMain&o=7&rm=3",
            ["Bread"] = "https://commons.wikimedia.org/wiki/Special:FilePath/Fresh_made_bread_05.jpg?width=600",
            ["Buns"] = "https://tse1.explicit.bing.net/th/id/OIP.K04b16A2L-lYLEZB8X4qWgHaHa?rs=1&pid=ImgDetMain&o=7&rm=3"
        };

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
            var name = productName?.Trim().ToLowerInvariant() ?? string.Empty;
            var inferredCategory = name switch
            {
                "milk" or "cheese" or "butter" or "yogurt" => "Dairy",
                "chicken" or "beef" or "mutton" => "Meat",
                "corn flour" or "wheat flour" => "Flour",
                "sunflower oil" or "olive oil" or "canola oil" => "Oil",
                "bread" or "buns" or "banana bread" => "Bakery",
                "basmati rice" or "jasmine rice" => "Grains",
                _ => "Veggies"
            };

            if (string.IsNullOrWhiteSpace(savedCategory))
            {
                return inferredCategory;
            }

            var normalizedSavedCategory = NormalizeCategory(savedCategory);

            // Older products got the default Veggies value when the Category column was added.
            // If that happened, use the product name to recover the correct original section.
            if (normalizedSavedCategory == "Veggies" && inferredCategory != "Veggies")
            {
                return inferredCategory;
            }

            return normalizedSavedCategory;
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
            var name = productName?.Trim() ?? string.Empty;

            // Keep custom local images for new products and admin uploads.
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                if (imageUrl.StartsWith("/Images/", StringComparison.OrdinalIgnoreCase))
                {
                    return imageUrl;
                }

                if (!ProductImages.ContainsKey(name))
                {
                    return imageUrl;
                }
            }

            if (ProductImages.TryGetValue(name, out var productImage))
            {
                return productImage;
            }

            return GetDefaultImageUrl(GetCategoryForProduct(productName, category));
        }
    }
}
