namespace SOA_CA2_Frontend.Models
{
    public class OrderItemModel
    {
            public int orderItem_Id { get; set; }
            public int order_Id { get; set; }
            public int product_Id { get; set; }
            public int quantity { get; set; }
            public float price { get; set; }
            public string imageUrl { get; set; }
    }
}
