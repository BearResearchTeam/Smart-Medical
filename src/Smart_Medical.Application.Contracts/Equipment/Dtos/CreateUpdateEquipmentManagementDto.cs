using System;
using System.ComponentModel.DataAnnotations;

namespace Smart_Medical.Equipment.Dtos
{
    public class CreateUpdateEquipmentManagementDto
    {
        [Required(ErrorMessage = "设备名称不能为空")]
        [StringLength(64, ErrorMessage = "设备名称不能超过64个字符")]
        public string Name { get; set; }

        [StringLength(64, ErrorMessage = "设备型号不能超过64个字符")]
        public string? Model { get; set; }

        [StringLength(64, ErrorMessage = "品牌不能超过64个字符")]
        public string? Manufacturer { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [StringLength(64, ErrorMessage = "使用科室不能超过64个字符")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "设备状态不能为空")]
        [StringLength(32, ErrorMessage = "设备状态不能超过32个字符")]
        public string Status { get; set; } = "正常";

        public bool InUse { get; set; } = false;

        [StringLength(128, ErrorMessage = "位置描述不能超过128个字符")]
        public string? Location { get; set; }

        public DateTime? LastMaintenanceTime { get; set; }

        [StringLength(256, ErrorMessage = "备注长度不能超过256个字符")]
        public string? Remark { get; set; }
    }
} 