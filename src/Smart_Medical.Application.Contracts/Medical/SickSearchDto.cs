using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Medical
{
    using System;

    namespace Smart_Medical.Medical
    {
        /// <summary>
        /// 病历分页查询条件
        /// </summary>
        public class SickSearchDto
        {
            /// <summary>
            /// 患者姓名（可选，模糊查询）
            /// </summary>
            public string? PatientName { get; set; }

            /// <summary>
            /// 住院号（可选，模糊查询）
            /// </summary>
            public string? InpatientNumber { get; set; }

            /// <summary>
            /// 当前页码（默认1）
            /// </summary>
            public int PageIndex { get; set; } = 1;

            /// <summary>
            /// 每页条数（默认5）
            /// </summary>
            public int PageSize { get; set; } = 5;
        }
    }
}
