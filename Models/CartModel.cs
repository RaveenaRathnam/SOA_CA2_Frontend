namespace SOA_CA2_Frontend.Models
{
    public class CartModel
    {
            public int cart_Id { get; set; }
            public int user_Id { get; set; }
            public DateTime createdAt { get; set; }
            public List<CartItemModel> items { get; set; } = new();
        
    }
}
