using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Smart_Medical.Equipment
{
    /// <summary>
    /// 医疗设备实体类
    /// 用于记录医院资产设备的基本信息、状态、使用情况等
    /// </summary>
    public class EquipmentManagement : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 设备名称
        /// 如：心电图仪、B超机、CT扫描仪
        /// </summary>
        [Required(ErrorMessage = "设备名称不能为空")]
        [StringLength(64, ErrorMessage = "设备名称不能超过64个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 设备型号
        /// 如：GE E9、Philips ClearVue550
        /// </summary>
        [StringLength(64, ErrorMessage = "设备型号不能超过64个字符")]
        public string? Model { get; set; }

        /// <summary>
        /// 品牌/厂商
        /// 如：飞利浦、迈瑞、GE
        /// </summary>
        [StringLength(64, ErrorMessage = "品牌不能超过64个字符")]
        public string? Manufacturer { get; set; }

        /// <summary>
        /// 购置日期
        /// 用于判断是否过保、统计折旧
        /// </summary>
        public DateTime? PurchaseDate { get; set; }

        /// <summary>
        /// 使用科室
        /// 如：放射科、急诊科、ICU
        /// </summary>
        [StringLength(64, ErrorMessage = "使用科室不能超过64个字符")]
        public string? Department { get; set; }

        /// <summary>
        /// 当前状态
        /// 如：正常、维修中、报废、闲置
        /// </summary>
        [Required(ErrorMessage = "设备状态不能为空")]
        [StringLength(32, ErrorMessage = "设备状态不能超过32个字符")]
        public string Status { get; set; } = "正常";

        /// <summary>
        /// 是否在使用中
        /// true 表示正在使用，false 表示未被占用
        /// </summary>
        public bool InUse { get; set; } = false;

        /// <summary>
        /// 存放位置
        /// 详细房间或位置描述
        /// </summary>
        [StringLength(128, ErrorMessage = "位置描述不能超过128个字符")]
        public string? Location { get; set; }

        /// <summary>
        /// 最后一次维护时间
        /// 用于判断是否需要保养
        /// </summary>
        public DateTime? LastMaintenanceTime { get; set; }

        /// <summary>
        /// 备注信息
        /// 可以写使用注意事项、报废说明等
        /// </summary>
        [StringLength(256, ErrorMessage = "备注长度不能超过256个字符")]
        public string? Remark { get; set; }
    }
}
