using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Pharmacy.InAndOutWarehouse
{
    public class DrugInStockCompanyFullDto
    {
        // 药品入库字段
        public Guid InStockId { get; set; }
        public Guid DrugId { get; set; }
        public Guid PharmaceuticalCompanyId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BatchNumber { get; set; }
        public string Supplier { get; set; }
        public string Status { get; set; }
        public DateTime CreationTime { get; set; }

        // 药品管理字段
        public string DrugName { get; set; }
        public string DrugCode { get; set; }
        public string Pinyin { get; set; }
        public string Specification { get; set; }
        public string Unit { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public int Stock { get; set; }
        public int StockUpper { get; set; }
        public int StockLower { get; set; }
        public string Effect { get; set; }
        public string DrugCategory { get; set; }
        public DateTime DrugProductionDate { get; set; }
        public DateTime DrugExpiryDate { get; set; }
        public string Effect2 { get; set; }
        public string DrugRemark { get; set; }

        // 制药公司字段
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public string LicenseNumber { get; set; }
        public string CompanyRemark { get; set; }
    }
}
