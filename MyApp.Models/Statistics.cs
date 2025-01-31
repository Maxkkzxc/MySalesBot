using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Models
{
    public class Statistics
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalOrders { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal AverageCheck { get; set; }
    }
}
