using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Medical.DoctorvVsit.DoctorAccounts;
using Smart_Medical.RBAC;
using Smart_Medical.Until;
using Smart_Medical.Until.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Smart_Medical.DoctorvVsit
{
    /// <summary>
    /// 医生管理
    /// </summary>
    [ApiExplorerSettings(GroupName = "医生管理")]
    public class DoctorAccountSerivce : ApplicationService, IDoctorAccountSerivce
    {
        // 定义一个常量作为缓存键，这是这个特定缓存项在 Redis 中的唯一标识。
        // 使用一个清晰且唯一的键。
        private const string CacheKey = "SmartMedical:doctor:All"; // 建议使用更具体的键名和前缀
        private readonly IRepository<DoctorAccount, Guid> doctors;
        private readonly IRepository<DoctorDepartment, Guid> dept;
        private readonly IRepository<User, Guid> users;
        private readonly IRedisHelper<List<DoctorAccountListDto>> doctorredis;
        private readonly IRepository<DoctorAudit, Guid> audits;

        public DoctorAccountSerivce(IRepository<DoctorAccount, Guid> doctors, IRepository<DoctorDepartment, Guid> dept, IRepository<User, Guid> users, IRedisHelper<List<DoctorAccountListDto>> doctorredis, IRepository<DoctorAudit, Guid> audits)
        {
            this.doctors = doctors;
            this.dept = dept;
            this.users = users;
            this.doctorredis = doctorredis;
            this.audits = audits;
        }

        /// <summary>
        /// 添加医生注册申请
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ApiResult> InsertDoctorAccount(CreateUpdateDoctorAccountDto input)
        {
            if (input == null)
            {
                //123123
                return ApiResult.Fail("信息错误", ResultCode.NotFound);
            }

            // 检查工号或账户标识是否已存在
            var existingDoctors = await doctors.GetQueryableAsync();
            if (existingDoctors.Any(x => x.EmployeeId == input.EmployeeId || x.AccountId == input.AccountId))
            {
                return ApiResult.Fail("工号或账户标识已存在", ResultCode.ValidationError);
            }

            // 获取并设置部门名称
            var deptlist = await dept.GetQueryableAsync();
            input.DepartmentName = deptlist.FirstOrDefault(x => x.Id == input.DepartmentId)?.DepartmentName;

            // --- EmployeeId 自增逻辑开始 ---
            // 获取当前最大的 EmployeeId
            var allDoctors = await doctors.GetQueryableAsync();
            // 1. 在数据库层面进行初步筛选：以"D"开头且长度为7
            var potentialEmployeeIds = await allDoctors
                .Where(x => x.EmployeeId.StartsWith("D") && x.EmployeeId.Length == 7)
                .Select(x => x.EmployeeId) // 只选择 EmployeeId 字段，减少数据传输
                .ToListAsync(); // 将结果加载到内存中

            // 2. 在内存中进行 int.TryParse 筛选和排序
            var lastEmployeeId = potentialEmployeeIds
                .Where(id => int.TryParse(id.Substring(1), out _)) // 在内存中可以使用 _ 或 tempId
                .OrderByDescending(id => id)
                .FirstOrDefault();

            int newIdNumber = 1;
            if (!string.IsNullOrEmpty(lastEmployeeId))
            {
                // 尝试解析数字部分并自增
                if (int.TryParse(lastEmployeeId.Substring(1), out int parsedId))
                {
                    newIdNumber = parsedId + 1;
                }
            }
            // 格式化新的 EmployeeId
            input.EmployeeId = "D" + newIdNumber.ToString("D6");
            // --- EmployeeId 自增逻辑结束 ---

            var dto = ObjectMapper.Map<CreateUpdateDoctorAccountDto, DoctorAccount>(input);
            await doctors.InsertAsync(dto);
            await doctorredis.RemoveAsync(CacheKey);

            var Auditdto = new DoctorAccountAuditDto()
            {
                AuditName = dto.AuditName,
                DoctorId = dto.Id,
                AuditState = dto.IsActive,
                AuditDesc = ""
            };
            var Auditdtores=ObjectMapper.Map<DoctorAccountAuditDto, DoctorAudit>(Auditdto);
            await audits.InsertAsync(Auditdtores);
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 获取医生账户列表(分页)
        /// </summary>
        /// <param name="seach"></param>
        /// <returns></returns>
        public async Task<ApiResult<PageResult<List<DoctorAccountListDto>>>> GetDoctorAccountList(DoctorAccountsearch seach)
        {
            var datalist = await doctorredis.GetAsync(CacheKey, async () =>
            {
                var list = await doctors.GetQueryableAsync();
                return ObjectMapper.Map<List<DoctorAccount>, List<DoctorAccountListDto>>(list.ToList());

            });
            // 防御性处理，确保 datalist 不为 null
            datalist ??= new List<DoctorAccountListDto>();

            var list = datalist.WhereIf(!string.IsNullOrEmpty(seach.EmployeeName), x => x.EmployeeName.Contains(seach.EmployeeName));
            //var deptlist = await dept.GetQueryableAsync();
            //var dto = from d in list
            //          join dp in deptlist on d.DepartmentId equals dp.Id
            //          select new DoctorAccountListDto
            //          {
            //              Id = d.Id,
            //              DepartmentId = d.DepartmentId,
            //              IsActive = d.IsActive,
            //              AccountId = d.AccountId,
            //              EmployeeId = d.EmployeeId,
            //              EmployeeName = d.EmployeeName,
            //              InstitutionName = d.InstitutionName,
            //              DepartmentName = dp.DepartmentName
            //          };
            // 统计总数 (在应用分页之前)
            var totalCount = list.Count();

            list = list.OrderBy(x => x.EmployeeId).Skip(seach.SkipCount).Take(seach.MaxResultCount);
            var totalPage = (int)Math.Ceiling((double)totalCount / seach.MaxResultCount);
            var pagedList = new PageResult<List<DoctorAccountListDto>>
            {
                TotlePage = totalPage,
                TotleCount = totalCount,
                Data = list.ToList(),
            };
            return ApiResult<PageResult<List<DoctorAccountListDto>>>.Success(pagedList, ResultCode.Success);
        }

        /// <summary>
        /// 获取医生账户审批列表(分页)
        /// </summary>
        /// <param name="seach"></param>
        /// <returns></returns>
        public async Task<ApiResult<PageResult<List<GetDoctorAuditDto>>>> GetDoctorAccountAuditList(DoctorAccountsearch seach)
        {
            var doctorlist = await doctors.GetQueryableAsync();
            doctorlist = doctorlist.WhereIf(seach.States!=null, x => x.IsActive==seach.States);
            var list = doctorlist.WhereIf(!string.IsNullOrEmpty(seach.EmployeeName), x => x.EmployeeName.Contains(seach.EmployeeName));

            var doctorauditlist = await audits.GetQueryableAsync();
            var dto = from d in list
                      join dp in doctorauditlist on d.Id equals dp.DoctorId
                      select new GetDoctorAuditDto
                      {
                          Id = d.Id,
                          DepartmentId = d.DepartmentId,
                          IsActive = d.IsActive,
                          AccountId = d.AccountId,
                          EmployeeId = d.EmployeeId,
                          EmployeeName = d.EmployeeName,
                          InstitutionName = d.InstitutionName,
                          Doctorimgs = d.Doctorimgs,
                          EmployeePhone=d.EmployeePhone,
                          Sex=d.Sex,
                          DepartmentName=d.DepartmentName,
                          DoctorGoodat=d.DoctorGoodat,
                          Desc=d.Desc,
                          Certificate=d.Certificate,
                          DoctorId=d.Id,
                          AuditName=d.AuditName,
                          AuditState=dp.AuditState,
                          AuditDesc=dp.AuditDesc
                      };
            // 统计总数 (在应用分页之前)
            var totalCount = dto.Count();

            dto = dto.OrderBy(x => x.EmployeeId).Skip(seach.SkipCount).Take(seach.MaxResultCount);
            var totalPage = (int)Math.Ceiling((double)totalCount / seach.MaxResultCount);
            var pagedList = new PageResult<List<GetDoctorAuditDto>>
            {
                TotlePage = totalPage,
                TotleCount = totalCount,
                Data = dto.ToList(),
            };
            return ApiResult<PageResult<List<GetDoctorAuditDto>>>.Success(pagedList, ResultCode.Success);
        }
        /// <summary>
        /// 根据医生账户id获取详情
        /// </summary>
        /// <param name="idsString"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<List<DoctorAccountListDto>>> DetailDoctorAccount(Guid id)
        {

            var list = await doctors.GetQueryableAsync();
            list = list.Where(x => x.Id == id);
            var dto = ObjectMapper.Map<List<DoctorAccount>, List<DoctorAccountListDto>>(list.ToList());
            return ApiResult<List<DoctorAccountListDto>>.Success(dto, ResultCode.Success);
        }
        /// <summary>
        /// 审批医生账户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<ApiResult> EditDoctorAccount(Guid id, CreateUpdateDoctorAccountDto input)
        {
            if (input == null)
            {
                return ApiResult.Fail("信息错误", ResultCode.NotFound);
            }
            var entity = await doctors.GetAsync(id);
            var deptlist = await dept.GetQueryableAsync();

            deptlist = deptlist.Where(x => x.Id == input.DepartmentId);
            input.DepartmentName = deptlist.FirstOrDefault()?.DepartmentName;

            ObjectMapper.Map(input, entity);
            await doctors.UpdateAsync(entity);
            var dto=new DoctorAccountAuditDto()
            {
                AuditName= entity.AuditName,
                DoctorId= entity.Id,
                AuditState= entity.IsActive,
                AuditDesc= input.AuditDesc,
            };
            var auditlist=await audits.GetAsync(x=>x.DoctorId== entity.Id);
           var auditdto= ObjectMapper.Map(dto, auditlist);
            await audits.UpdateAsync(auditlist);
            await doctorredis.RemoveAsync(CacheKey);

            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 删除医生账户
        /// </summary>
        /// <param name="idsString"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        [HttpDelete]
        public async Task<ApiResult> DeleteDoctorAccount([FromQuery] string idsString)
        {


            if (string.IsNullOrWhiteSpace(idsString))
            {
                return ApiResult.Fail("请提供要删除的医生账户ID字符串。", ResultCode.NotFound);
            }
            // 将逗号分隔的字符串解析为 List<Guid>
            var ids = idsString.Split(',')
                               .Where(s => !string.IsNullOrWhiteSpace(s))
                               .Select(s =>
                               {
                                   if (Guid.TryParse(s.Trim(), out Guid id))
                                   {
                                       return id;
                                   }
                                   throw new FormatException($"无效的GUID格式: {s}"); // 如果有无效GUID，可以抛出异常
                               })
                               .ToList();
            if (!ids.Any())
            {
                return ApiResult.Fail("解析后的医生账户ID列表为空。", ResultCode.NotFound);
            }
            var entity = await doctors.GetQueryableAsync();
            entity = entity.Where(x => ids.Contains(x.Id));
            if (!entity.Any())
            {
                return ApiResult.Fail("未找到要删除的医生账户", ResultCode.NotFound);
            }
            // 批量删除医生账户
            await doctors.DeleteManyAsync(entity);
            await doctorredis.RemoveAsync(CacheKey);
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 查询全部科室
        /// </summary>
        public async Task<ApiResult<List<DoctorDepartDto>>> GetAllDepartmentsAsync()
        {
            try
            {
                var departments = await dept.GetListAsync();
                var result = departments.Select(d => new DoctorDepartDto
                {
                    Id = d.Id,
                    DepartmentName = d.DepartmentName
                }).ToList();
                return ApiResult<List<DoctorDepartDto>>.Success(result, ResultCode.Success);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 根据科室 ID 查询医生列表
        /// </summary>
        /// <param name="departmentId">科室 ID</param>
        public async Task<ApiResult<List<DoctorListDto>>> GetDoctorsByDepartmentIdAsync(Guid departmentId)
        {
            try
            {
                var doctorList = await doctors.GetListAsync(d => d.DepartmentId == departmentId);
                var result = doctorList.Select(d => new DoctorListDto
                {
                    Id = d.Id,
                    EmployeeName = d.EmployeeName
                }).ToList();
                return ApiResult<List<DoctorListDto>>.Success(result, ResultCode.Success);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
