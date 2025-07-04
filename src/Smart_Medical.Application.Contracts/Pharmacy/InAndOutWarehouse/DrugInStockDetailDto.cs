using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Pharmacy.InAndOutWarehouse
{
    public class DrugInStockDetailDto
    {
        public int DrugId { get; set; }
        public Guid PharmaceuticalCompanyId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BatchNumber { get; set; }
        public string Supplier { get; set; }
        public string Status { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
    }
}
