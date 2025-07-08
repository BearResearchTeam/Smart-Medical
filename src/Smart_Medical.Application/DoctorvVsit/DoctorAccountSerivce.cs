using Microsoft.AspNetCore.Mvc;
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

namespace Smart_Medical.DoctorvVsit
{
    /// <summary>
    /// 医生管理
    /// </summary>
    [ApiExplorerSettings(GroupName = "医生管理")]
    public class DoctorAccountSerivce:ApplicationService,IDoctorAccountSerivce
    {
        // 定义一个常量作为缓存键，这是这个特定缓存项在 Redis 中的唯一标识。
        // 使用一个清晰且唯一的键很重要。
        private const string CacheKey = "SmartMedical:doctor:All"; // 建议使用更具体的键名和前缀
        private readonly IRepository<DoctorAccount,Guid> doctors;
        private readonly IRepository<DoctorDepartment, Guid> dept;
        private readonly IRepository<User, Guid> users;
        private readonly IRedisHelper<List<DoctorAccountListDto>> doctorredis;

        public DoctorAccountSerivce(IRepository<DoctorAccount, Guid> doctors, IRepository<DoctorDepartment, Guid> dept,IRepository<User, Guid> users,IRedisHelper<List<DoctorAccountListDto>> doctorredis)
        {
            this.doctors = doctors;
            this.dept = dept;
            this.users = users;
            this.doctorredis = doctorredis;
        }

        /// <summary>
        /// 添加医生账户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ApiResult> InsertDoctorAccount(CreateUpdateDoctorAccountDto input)
        { 
            if (input == null) 
            {
                return ApiResult.Fail("信息错误", ResultCode.NotFound);
            }
            var list=await doctors.GetQueryableAsync();
            list=list.Where(x=>x.EmployeeId==input.EmployeeId||x.AccountId==input.AccountId);
            if (list.Any())
            {
                return ApiResult.Fail("工号或账户标识已存在", ResultCode.ValidationError);
            }
            var deptlist = await dept.GetQueryableAsync();
            deptlist = deptlist.Where(x => x.Id == input.DepartmentId);
            input.DepartmentName=deptlist.FirstOrDefault()?.DepartmentName;
            //var res=list.OrderByDescending(x => x.EmployeeId).FirstOrDefault();

            //input.EmployeeId = "D" + (int.Parse(res.Replace<string>("D", "")) + 1).ToString("D6");
            var dto=ObjectMapper.Map<CreateUpdateDoctorAccountDto, DoctorAccount>(input);
            await doctors.InsertAsync(dto);
           await doctorredis.RemoveAsync(CacheKey);
            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 获取医生账户列表(分页)
        /// </summary>
        /// <param name="seach"></param>
        /// <returns></returns>
        public async Task<ApiResult<PageResult<List<DoctorAccountListDto>>>> GetDoctorAccountList(DoctorAccountsearch seach)
        {
            var datalist=await doctorredis.GetAsync(CacheKey, async () =>
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

            list = list.OrderBy(x => x.Id).Skip(seach.SkipCount).Take(seach.MaxResultCount);
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
        /// 修改医生账户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ApiResult> EditDoctorAccount(Guid id,CreateUpdateDoctorAccountDto input)
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
        public async Task<ApiResult> DeleteDoctorAccount([FromQuery]string idsString)
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
                var doctorList = await doctors.GetListAsync(d => d.DepartmentId == departmentId && d.IsActive);
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
