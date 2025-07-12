using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Smart_Medical.DoctorvVsit.DoctorAccounts
{
    public class DoctorAccountsearch:PagedAndSortedResultRequestDto
    {
        public string? EmployeeName { get; set; }
        public int? States { get; set; }
    }
}
