using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Smart_Medical.Pharmacy
{
    /// <summary>
    /// 药品信息DTO
    /// </summary>
    public class DrugDto 
    {
        /// <summary>
        /// 
        /// </summary>
        public int DrugID { get; set; }
        /// <summary>
        /// 药品名称
        /// </summary>
        public string DrugName { get; set; }

        /// <summary>
        /// 药品类型
        /// </summary>
        public string DrugType { get; set; }

        /// <summary>
        /// 费用名称
        /// </summary>
        public string FeeName { get; set; }

        /// <summary>
        /// 剂型
        /// </summary>
        public string DosageForm { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        public string Specification { get; set; }

        /// <summary>
        /// 进价（元）
        /// </summary>
        public decimal PurchasePrice { get; set; }

        /// <summary>
        /// 售价（元）
        /// </summary>
        public decimal SalePrice { get; set; }

        /// <summary>
        /// 当前库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 库存上限
        /// </summary>
        public int StockUpper { get; set; }

        /// <summary>
        /// 库存下限
        /// </summary>
        public int StockLower { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime ProductionDate { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// 药品功效
        /// </summary>
        public string Effect { get; set; }

        /// <summary>
        /// 药品类别（中药/西药等）
        /// </summary>
        public DrugCategory Category { get; set; }

        /// <summary>
        ///  供应商ID
        /// </summary>
        public Guid? PharmaceuticalCompanyId { get; set; }
        /// <summary>
        /// 公司名称////
        /// </summary>
        [Required]
        [StringLength(128)]
        public string PharmaceuticalCompanyName { get; set; }
    }
}
