﻿using Smart_Medical.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Smart_Medical.Patient
{
    /// <summary>
    /// 预约挂号记录
    /// </summary>
    public class Appointment:FullAuditedAggregateRoot<Guid>
    {
        //public int AppointmentId { get; set; } // 挂号ID，主键  abp有默认主键 为id

        public Guid? PatientId { get; set; } = null; // 外键：患者ID

        public DateTime AppointmentDateTime { get; set; } // 预约时间 (从Schedule中获取) 哪天就诊

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked; // 挂号状态

        public decimal ActualFee { get; set; } // 实际支付费用 (可能与排班费用不同，例如有优惠)
        public string Remarks { get; set; } = string.Empty; // 备注信息

        // 可以在这里添加支付相关信息，例如：
        // public string PaymentStatus { get; set; } // 支付状态 (未支付/已支付/退款中)
        // public string TransactionId { get; set; } // 支付交易ID
    }

   



}
