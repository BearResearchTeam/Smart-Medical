using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Smart_Medical.Prescriptions
{
    /// <summary>
    /// 处方信息
    /// </summary>
    public class Prescription : FullAuditedAggregateRoot<int>
    {

        /// <summary>
        /// 处方名称
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PrescriptionName { get; set; }

        /// <summary>
        /// 药品信息id  此处方下所有药品的ID集合
        /// </summary>
        public string? DrugIds { get; set; }=string.Empty;

        /// <summary>
        /// 父级Id   1中药和2西药
        /// </summary>
        [Required]
        public int ParentId { get; set; } 


    }
}
