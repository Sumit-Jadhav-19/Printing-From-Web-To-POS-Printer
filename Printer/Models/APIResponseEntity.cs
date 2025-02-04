using System.Collections.Generic;

namespace Printer.Models
{
    public class APIResponseEntity
    {
        public int statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
    }
    public class Menu
    {
        public string MenuName { get; set; }
        public string Size { get; set; }
        public string Quantity { get; set; }
        public decimal Price { get; set; }
        public string TotalPrice { get; set; }
    }

    public class OrderData
    {
        public long OrderId { get; set; }
        public string Hall { get; set; }
        public string Table { get; set; }
        public List<Menu> Menus { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }

    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public List<OrderData> Data { get; set; }
    }

}
