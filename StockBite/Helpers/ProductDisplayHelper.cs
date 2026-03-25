namespace StockBite.Helpers
{
    public static class ProductDisplayHelper
    {
        public static string GetImageUrl(string? productName)
        {
            var name = productName?.Trim().ToLowerInvariant() ?? "";

            return name switch
            {
                "butter" => "https://www.pngarts.com/files/3/Butter-PNG-Image-Background.png",
                "cheese" => "https://static.vecteezy.com/system/resources/thumbnails/028/201/736/small_2x/cheese-collection-various-types-of-cheese-with-herbs-close-up-photo.jpg",
                "milk" => "https://img-s-msn-com.akamaized.net/tenant/amp/entityid/AA1EVw9e.img?w=780&h=438&m=4&q=91",
                "yogurt" => "https://assets.bonappetit.com/photos/63b5fe187015aa59a8e880ac/16:9/w_1920,c_limit/010323-its-greek-yogurt-taste-test-01.jpg",
                "beef" => "https://hips.hearstapps.com/hmg-prod.s3.amazonaws.com/images/delish-roast-beef-horizontal-1540505165.jpg?crop=1xw:0.75xh;center,top&resize=1200:*",
                "chicken" => "https://flavorthemoments.com/wp-content/uploads/2024/01/how-to-cut-a-whole-chicken-hero-1.jpg",
                "mutton" => "https://thekitchencommunity.org/wp-content/uploads/2022/10/mutton-meat.jpeg",
                "corn flour" => "https://image.made-in-china.com/2f0j00ONmTQWFPMdrq/50lbs-Corn-Starch-Flour-Packaging-Bag-Brown-Kraft-Paper-Bag.jpg",
                "onions" => "https://commons.wikimedia.org/wiki/Special:FilePath/Onion.jpg?width=600",
                "potatoes" => "https://commons.wikimedia.org/wiki/Special:FilePath/Potato.jpg?width=600",
                "tomatoes" => "https://commons.wikimedia.org/wiki/Special:FilePath/Tomato_2.jpg?width=600",
                "lettuce" => "https://commons.wikimedia.org/wiki/Special:FilePath/Romaine_lettuce.jpg?width=600",
                "carrots" => "https://commons.wikimedia.org/wiki/Special:FilePath/Carrots.jpg?width=600",
                "cucumbers" => "https://commons.wikimedia.org/wiki/Special:FilePath/Cucumber_picture.jpg?width=600",
                "bell peppers" => "https://commons.wikimedia.org/wiki/Special:FilePath/Red_bell_pepper.jpg?width=600",
                "spinach" => "https://commons.wikimedia.org/wiki/Special:FilePath/Spinach_%282009%29.jpg?width=600",
                "garlic" => "https://commons.wikimedia.org/wiki/Special:FilePath/Garlic.jpg?width=600",
                "ginger" => "https://commons.wikimedia.org/wiki/Special:FilePath/Ginger_Root_%28Zingiber_officinale%29.jpg?width=600",
                "broccoli" => "https://commons.wikimedia.org/wiki/Special:FilePath/Broccoli.jpg?width=600",
                "cauliflower" => "https://commons.wikimedia.org/wiki/Special:FilePath/Cauliflower.JPG?width=600",
                "mushrooms" => "https://commons.wikimedia.org/wiki/Special:FilePath/Mushroom_%2812329086143%29.jpg?width=600",
                "zucchini" => "https://commons.wikimedia.org/wiki/Special:FilePath/Zucchini.jpg?width=600",
                "cabbage" => "https://commons.wikimedia.org/wiki/Special:FilePath/Cabbage.jpg?width=600",
                "wheat flour" => "https://static.vecteezy.com/system/resources/thumbnails/038/970/297/small_2x/ai-generated-organic-natural-whole-grain-flour-in-sacks-and-wheat-seeds-ears-of-wheat-on-an-old-wooden-floor-photo.jpg",
                "basmati rice" => "https://commons.wikimedia.org/wiki/Special:FilePath/Rice_grains.jpg?width=600",
                "jasmine rice" => "https://m.media-amazon.com/images/I/81dRpr6gfaL._AC_SX679_.jpg",
                "sunflower oil" => "https://commons.wikimedia.org/wiki/Special:FilePath/Sunflower_oil.jpg?width=600",
                "olive oil" => "https://commons.wikimedia.org/wiki/Special:FilePath/Olive_oil.jpg?width=600",
                "canola oil" => "https://tse4.mm.bing.net/th/id/OIP._pB9G95Wzh5Scxplq-qVoQHaJ4?rs=1&pid=ImgDetMain&o=7&rm=3",
                "bread" => "https://commons.wikimedia.org/wiki/Special:FilePath/Fresh_made_bread_05.jpg?width=600",
                "buns" => "https://tse1.explicit.bing.net/th/id/OIP.K04b16A2L-lYLEZB8X4qWgHaHa?rs=1&pid=ImgDetMain&o=7&rm=3",
                _ => "https://commons.wikimedia.org/wiki/Special:FilePath/Onion.jpg?width=600"
            };
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
