using System.Reflection;

namespace SOA_CA2_Frontend.Models
{

    public class ProductModel
    {
        public int product_Id { get; set; }
        public int category_Id { get; set; }
        public string product_Name { get; set; }
        public float price { get; set; }
        public int stock { get; set; }
        public string description { get; set; }
        public int gender { get; set; }
        public string imageUrl { get; set; }
        public string categoryName { get; set; }

        public string genderName
        {
            get
            {
                return gender switch
                {
                    0 => "Male",
                    1 => "Female",
                    2 => "Unisex",
                    _ => "Unknown"
                };
            }
        }
    }

}
