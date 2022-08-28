using System;
using System.Collections.Generic;

namespace DeliveryOrderProcessor.Models;

public class Order
{
    public string Id { get; set; }

    public string BuyerId { get; set; }
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
    public Address ShipToAddress { get; set; }

    public IEnumerable<OrderItem> OrderItems { get; set; }

    public decimal Total => GetTotal();

    public decimal GetTotal()
    {
        var total = 0m;
        foreach (var item in OrderItems)
        {
            total += item.UnitPrice * item.Units;
        }
        return total;
    }
}
