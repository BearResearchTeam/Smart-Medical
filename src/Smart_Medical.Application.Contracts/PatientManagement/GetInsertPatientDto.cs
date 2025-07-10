using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.PatientManagement
{
    /// <summary>
    /// 操作患者信息dto
    /// </summary>
    public class GetInsertPatientDto
    {
        /// <summary>
        /// 就诊ID（主键）
        /// </summary>
        //[Required(ErrorMessage = "就诊ID不能为空")]
        //[StringLength(20, ErrorMessage = "就诊ID长度不能超过20个字符")]
        public string VisitId { get; set; } = string.Empty;

        /// <summary>
        /// 患者姓名（必填）
        /// </summary>
        [Required(ErrorMessage = "患者姓名不能为空")]
        [StringLength(50, ErrorMessage = "患者姓名长度不能超过50个字符")]
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// 性别【1】男【2】女
        /// </summary>
        public int Gender { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        [Range(0, 150, ErrorMessage = "年龄必须在0-150之间")]
        public int? Age { get; set; }

        /// <summary>
        /// 年龄单位（年/月/日）
        /// </summary>
        [StringLength(10, ErrorMessage = "年龄单位长度不能超过10个字符")]
        public string AgeUnit { get; set; } = string.Empty;

        /// <summary>
        /// 联系方式
        /// </summary>
        [StringLength(20, ErrorMessage = "联系方式长度不能超过20个字符")]
        [Phone(ErrorMessage = "请输入有效的电话号码")]
        public string ContactPhone { get; set; } = string.Empty;

        /// <summary>
        /// 身份证号
        /// </summary>
        [StringLength(18, ErrorMessage = "身份证号长度必须为18位", MinimumLength = 18)]
        public string IdNumber { get; set; } = string.Empty;

        /// <summary>
        /// 就诊 类型（初诊/复诊，必填）
        /// </summary>
        [Required(ErrorMessage = "就诊类型不能为空")]
        [StringLength(20, ErrorMessage = "就诊类型长度不能超过20个字符")]
        public string VisitType { get; set; } = "初诊";

        /// <summary>
        /// 是否为传染病 默认没有
        /// </summary>
        public bool IsInfectiousDisease { get; set; } = false;

        /// <summary>
        /// 发病时间
        /// </summary>
        public DateTime? DiseaseOnsetTime { get; set; }

        /// <summary>
        /// 急救时间
        /// </summary>
        public DateTime? EmergencyTime { get; set; }

        /// <summary>
        /// 就诊状态（待就诊/已就诊/已取消）
        /// </summary>
        [StringLength(20, ErrorMessage = "就诊状态长度不能超过20个字符")]
        public string VisitStatus { get; set; } = "待就诊";

        /// <summary>
        /// 就诊日期
        /// </summary>
        public DateTime VisitDate { get; set; } = DateTime.Now;

    }

    /// <summary>
    /// 更新患者信息
    /// </summary>
    public class UpdPatientDto
    {
        public Guid ID { get; set; }

        /// <summary>
        /// 就诊ID（主键）
        /// </summary>
        //[Required(ErrorMessage = "就诊ID不能为空")]
        //[StringLength(20, ErrorMessage = "就诊ID长度不能超过20个字符")]
        public string VisitId { get; set; } = string.Empty;

        /// <summary>
        /// 患者姓名（必填）
        /// </summary>
        [Required(ErrorMessage = "患者姓名不能为空")]
        [StringLength(50, ErrorMessage = "患者姓名长度不能超过50个字符")]
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// 性别【1】男【2】女
        /// </summary>
        public int Gender { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        [Range(0, 150, ErrorMessage = "年龄必须在0-150之间")]
        public int? Age { get; set; }

        /// <summary>
        /// 年龄单位（年/月/日）
        /// </summary>
        [StringLength(10, ErrorMessage = "年龄单位长度不能超过10个字符")]
        public string AgeUnit { get; set; } = string.Empty;

        /// <summary>
        /// 联系方式
        /// </summary>
        [StringLength(20, ErrorMessage = "联系方式长度不能超过20个字符")]
        [Phone(ErrorMessage = "请输入有效的电话号码")]
        public string ContactPhone { get; set; } = string.Empty;

        /// <summary>
        /// 身份证号
        /// </summary>
        [StringLength(18, ErrorMessage = "身份证号长度必须为18位", MinimumLength = 18)]
        public string IdNumber { get; set; } = string.Empty;

        /// <summary>
        /// 就诊 类型（初诊/复诊，必填）
        /// </summary>
        [Required(ErrorMessage = "就诊类型不能为空")]
        [StringLength(20, ErrorMessage = "就诊类型长度不能超过20个字符")]
        public string VisitType { get; set; } = "初诊";

        /// <summary>
        /// 是否为传染病 默认没有
        /// </summary>
        public bool IsInfectiousDisease { get; set; } = false;

        /// <summary>
        /// 发病时间
        /// </summary>
        public DateTime? DiseaseOnsetTime { get; set; }

        /// <summary>
        /// 急救时间
        /// </summary>
        public DateTime? EmergencyTime { get; set; }

        /// <summary>
        /// 就诊状态（待就诊/已就诊/已取消）
        /// </summary>
        [StringLength(20, ErrorMessage = "就诊状态长度不能超过20个字符")]
        public string VisitStatus { get; set; } = "待就诊";

        /// <summary>
        /// 就诊日期
        /// </summary>
        public DateTime VisitDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 就诊患者查询参数，支持分页和关键词搜索
    /// </summary>
    public class GetVistingParameterDtos
    {

        /// <summary>
        /// 统一关键词查询（身份证号、姓名、电话）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 当前页码，从 1 开始
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 10;
    }


    /// <summary>
    /// 分页结果 DTO 通用封装（如果你项目里已经有，可以直接用）
    /// </summary>
    public class PageResultDto<T>
    {
        public int TotleCount { get; set; }
        public int TotlePage { get; set; }
        public List<T> Items { get; set; }

        public PageResultDto(int totalCount, List<T> items)
        {
            TotleCount  = 10;
            TotlePage = 10;
            Items = items;
        }
    }
}
