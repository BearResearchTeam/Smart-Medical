using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Smart_Medical.Pharmacy
{
    /// <summary>
    /// 药品查询参数DTO
    /// </summary>
    public class DrugSearchDto
    {
        /// <summary>
        /// 药品名称 (可选, 模糊查询)
        /// </summary>
        /// <example>阿莫西林</example>
        [Description("药品名称 (可选, 模糊查询)")]
        public string? DrugName { get; set; }

        /// <summary>
        /// 药品类型 (可选, 模糊查询)
        /// </summary>
        /// <example>抗生素</example>
        [Description("药品类型 (可选, 模糊查询)")]
        public string? DrugType { get; set; }

        /// <summary>
        /// 生产日期起 (可选, 查询生产日期大于等于该值)
        /// </summary>
        [Description("生产日期起 (可选, 查询生产日期大于等于该值)")]
        public DateTime? ProductionDateStart { get; set; }

        /// <summary>
        /// 生产日期止 (可选, 查询生产日期小于等于该值)
        /// </summary>
        [Description("生产日期止 (可选, 查询生产日期小于等于该值)")]
        public DateTime? ProductionDateEnd { get; set; }

        /// <summary>
        /// 最小库存 (可选, 查询库存大于等于该值)
        /// </summary>
        [Description("最小库存 (可选, 查询库存大于等于该值)")]
        public int? StockMin { get; set; }

        /// <summary>
        /// 最大库存 (可选, 查询库存小于等于该值)
        /// </summary>
        [Description("最大库存 (可选, 查询库存小于等于该值)")]
        public int? StockMax { get; set; }

        /// <summary>
        /// 当前页码 (默认为1)
        /// </summary>
        [DefaultValue(1)]
        public int pageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小 (默认为10)
        /// </summary>
        [DefaultValue(10)]
        public int pageSize { get; set; } = 5;
    }
} 