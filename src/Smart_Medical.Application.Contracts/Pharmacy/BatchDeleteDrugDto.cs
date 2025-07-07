using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Smart_Medical.Pharmacy
{
    /// <summary>
    /// 批量删除药品参数DTO
    /// </summary>
    public class BatchDeleteDrugDto
    {
        /// <summary>
        /// 要删除的药品ID列表
        /// </summary>
        [Required(ErrorMessage = "药品ID列表不能为空")]
        public List<int> DrugIds { get; set; } = new List<int>();

        /// <summary>
        /// 是否强制删除（忽略库存检查）
        /// </summary>
        public bool ForceDelete { get; set; } = false;
    }
} 