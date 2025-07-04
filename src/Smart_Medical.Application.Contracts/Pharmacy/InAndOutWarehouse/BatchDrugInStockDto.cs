using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Pharmacy.InAndOutWarehouse
{
    public class BatchDrugInStockDto
    {
        public List<DrugInStockDetailDto> Details { get; set; }
        public string Operator { get; set; }
        public string Remark { get; set; }
        public DateTime InStockTime { get; set; } = DateTime.Now;
    }
}
