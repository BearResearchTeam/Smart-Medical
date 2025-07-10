using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.OutpatientClinic.Dtos.Parameter
{
    /// <summary>
    /// 就诊患者查询参数，支持分页和关键词搜索
    /// </summary>
    public class GetVistingParameterDtos
    {
        public int VisitStatus { get; set; } = 1;

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
    /// 就诊患者查询参数，支持分页和关键词搜索
    /// </summary>
    public class GetVistingParameterDto
    {
        public int DispensingStatus { get; set; } = 1;

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
    public class PagedResultDto<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }

        public PagedResultDto(int totalCount, List<T> items)
        {
            TotalCount = totalCount;
            Items = items;
        }
    }
}
