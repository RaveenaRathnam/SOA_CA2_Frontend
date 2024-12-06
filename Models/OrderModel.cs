namespace SOA_CA2_Frontend.Models
{
    public class OrderModel
    {
            public int order_Id { get; set; }
            public int user_Id { get; set; }
            public DateTime order_Date { get; set; }
            public float total_Amount { get; set; }
            public int status { get; set; }
            public DateTime createdAt { get; set; }
             public List<OrderItemModel> orderItems { get; set; } = new();

    }
}

