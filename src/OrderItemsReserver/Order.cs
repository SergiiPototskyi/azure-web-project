using System.Collections.Generic;

namespace OrderItemsReserver
{
    public class Order
    {
        public int Id { get; set; }

        public List<OrderItem> Items { get; set; }
    }

    public class OrderItem
    {
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public int CatalogItemId { get; set; }

        public int BasketId { get; set; }

        public int Id { get; set; }
    }
}
