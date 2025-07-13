using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Smart_Medical.DoctorvVsit
{
    /// <summary>
    /// 医生审核表
    /// </summary>
    public class DoctorAudit:FullAuditedEntity<Guid>
    {
        /// <summary>
        /// 审核人
        /// </summary>
        public string AuditName { get; set; }
       
        /// <summary>
        /// 医生id
        /// </summary>
        public Guid DoctorId { get; set; }
        /// <summary>
        /// 审核状态 0待审核 1审核通过 2 驳回
        /// </summary>
        public int AuditState { get; set; } = 0;
        /// <summary>
        /// 描述
        /// </summary>
        public string AuditDesc { get; set; }

    }
}
