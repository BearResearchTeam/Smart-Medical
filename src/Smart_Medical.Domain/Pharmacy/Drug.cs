using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Smart_Medical.Pharmacy
{
    public class Drug : FullAuditedAggregateRoot<int>
    {

       

        /// <summary>
        /// 药品名称s
        /// </summary>
        [Required]
        [StringLength(128)]
        public string DrugName { get; set; }

        /// <summary>
        ///  药品类型
        /// </summary>
        [Required]
        [StringLength(32)]
        public string DrugType { get; set; }

        /// <summary>
        /// 费用名称
        /// </summary>
        [Required]
        [StringLength(32)]
        public string FeeName { get; set; }



        /// <summary>
        ///  剂型
        /// </summary>
        [Required]
        [StringLength(32)]
        public string DosageForm { get; set; }

        /// <summary>
        /// 规格
        /// </summary>
        [Required]
        [StringLength(64)]
        public string Specification { get; set; }

        /// <summary>
        ///  进价
        /// </summary>
        [Required]
        public decimal PurchasePrice { get; set; }

        /// <summary>
        ///  售价
        /// </summary>
        [Required]
        public decimal SalePrice { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        [Required]
        public int Stock { get; set; }

        /// <summary>
        /// 库存上限
        /// </summary>
        [Required]
        public int StockUpper { get; set; }

        /// <summary>
        /// 库存下限
        /// </summary>
        [Required]
        public int StockLower { get; set; }

        /// <summary>
        ///  生产日期
        /// </summary>
        [Required]
        public DateTime ProductionDate { get; set; }

        /// <summary>
        /// 有效日期
        /// </summary>
        [Required]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// 药功效
        /// </summary>
        [Required]
        [StringLength(256)]
        public string Effect { get; set; }

        /// <summary>
        ///  中西药类型
        /// </summary>
        [Required]
        public DrugCategory Category { get; set; }

        /// <summary>
        ///  供应商ID
        /// </summary>
        public Guid? PharmaceuticalCompanyId { get; set; }


    }
}
