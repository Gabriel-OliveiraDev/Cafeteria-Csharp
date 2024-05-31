using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Cafeteria.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int SupplierId {  get; set; }

        public int ProductId { get; set; }

        public decimal Value { get; set; } = decimal.Zero;
        public int Quantity { get; set; }
    }
}
