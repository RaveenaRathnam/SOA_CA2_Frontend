namespace SOA_CA2_Frontend.Models
{
    public class CartItemModel
    {
            public int cartItem_Id { get; set; }
            public int cart_Id { get; set; }
            public int product_Id { get; set; }
            public int quantity { get; set; }

            // Additional Product Details
            public string productName { get; set; }
            public float price { get; set; }
            public string imageUrl { get; set; }
    }
}
