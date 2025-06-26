using System;
using System.ComponentModel.DataAnnotations;

namespace Smart_Medical.Pharmacy.InAndOutWarehouse
{
    /// <summary>
    /// 药品入库参数
    /// </summary>
    public class CreateUpdateDrugInStockDto
    {
        /// <summary>
        /// 药品ID
        /// </summary>
        public int DrugId { get; set; }

        /// <summary>
        /// 制药公司ID
        /// </summary>
        public Guid PharmaceuticalCompanyId { get; set; }

        /// <summary>
        /// 入库数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 入库单价（元）
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 入库总金额（元）
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime ProductionDate { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string BatchNumber { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public string Supplier { get; set; }

        /// <summary>
        /// 入库状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.Now;
    }
} 