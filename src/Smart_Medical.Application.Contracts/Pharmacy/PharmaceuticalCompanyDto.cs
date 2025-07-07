using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities.Auditing;

namespace Smart_Medical.Pharmacy
{
    public class PharmaceuticalCompanyDto : FullAuditedAggregateRoot<Guid>
    {
    
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
    }
}
