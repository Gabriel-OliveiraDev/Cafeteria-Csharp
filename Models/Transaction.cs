using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Cafeteria.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public Client Client { get; set; }
        public Supplier Supplier {  get; set; }

        public Product Product { get; set; }

        public decimal Value { get; set; } = decimal.Zero;
        public int Quantity { get; set; }
    }
}
