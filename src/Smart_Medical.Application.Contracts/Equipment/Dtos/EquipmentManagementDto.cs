using System;
using Volo.Abp.Application.Dtos;

namespace Smart_Medical.Equipment.Dtos
{
    public class EquipmentManagementDto : FullAuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? Department { get; set; }
        public string Status { get; set; }
        public bool InUse { get; set; }
        public string? Location { get; set; }
        public DateTime? LastMaintenanceTime { get; set; }
        public string? Remark { get; set; }
    }

    public class GeEquipmentParameterDtos
    {
        /// <summary>
        /// 当前页码，从 1 开始
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
} 