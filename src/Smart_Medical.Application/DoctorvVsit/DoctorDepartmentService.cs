using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Medical.Dictionarys.DictionaryDatas;
using Smart_Medical.DoctorvVsit.DockerDepartments;
using Smart_Medical.Until;
using Smart_Medical.Until.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace Smart_Medical.DoctorvVsit
{
    /// <summary>
    /// 科室管理
    /// </summary>
    [ApiExplorerSettings(GroupName = "科室管理")]
    public class DoctorDepartmentService : ApplicationService, IDoctorDepartmentService
    {
        // 定义一个常量作为缓存键，这是这个特定缓存项在 Redis 中的唯一标识。
        // 使用一个清晰且唯一的键很重要。
        private const string CacheKey = "SmartMedical:dept:All"; // 建议使用更具体的键名和前缀
        private readonly IRepository<DoctorDepartment, Guid> dept;
        private readonly IRedisHelper<List<GetDoctorDepartmentListDto>> deptredis;

        public DoctorDepartmentService(IRepository<DoctorDepartment, Guid> dept, IRedisHelper<List<GetDoctorDepartmentListDto>> deptredis)
        {
            this.dept = dept;
            this.deptredis = deptredis;
        }
        /// <summary>
        /// 新增科室
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> InsertDoctorDepartment(CreateUpdateDoctorDepartmentDto input)
        {
            var deptdto = ObjectMapper.Map<CreateUpdateDoctorDepartmentDto, DoctorDepartment>(input);
            deptdto = await dept.InsertAsync(deptdto);
            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 获取科室信息列表
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult<List<GetDoctorDepartmentListDto>>> GetDoctorDepartment()
        {
            var datalist = await deptredis.GetAsync(CacheKey, async () =>
            {
                var deptlist = await dept.GetQueryableAsync();
                return ObjectMapper.Map<List<DoctorDepartment>, List<GetDoctorDepartmentListDto>>(deptlist.ToList());
            });
            datalist ??= new List<GetDoctorDepartmentListDto>();
            return ApiResult<List<GetDoctorDepartmentListDto>>.Success(datalist, ResultCode.Success);
        }
        /// <summary>
        /// 获取科室列表(分页)
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<PageResult<List<GetDoctorDepartmentListDto>>>> GetDoctorDepartmentList([FromQuery] GetDoctorDepartmentSearchDto search)
        {
            var datalist = await deptredis.GetAsync(CacheKey, async () =>
            {
                var deptlist = await dept.GetQueryableAsync();
                return ObjectMapper.Map<List<DoctorDepartment>, List<GetDoctorDepartmentListDto>>(deptlist.ToList());
            });
            datalist ??= new List<GetDoctorDepartmentListDto>();
            //var list = await dept.GetQueryableAsync();
            var list = datalist.WhereIf(!string.IsNullOrEmpty(search.DepartmentName), x => x.DepartmentName.Contains(search.DepartmentName));
            var res = list.AsQueryable().PageResult(search.PageIndex, search.PageSize);
            var pageInfo = new PageResult<List<GetDoctorDepartmentListDto>>
            {
                Data = res.Queryable.ToList(),
                TotleCount = res.RowCount,
                TotlePage = (int)Math.Ceiling((double)res.RowCount / search.PageSize),
            };

            return ApiResult<PageResult<List<GetDoctorDepartmentListDto>>>.Success(pageInfo, ResultCode.Success);
        }
        /// <summary>
        /// 更新医生科室信息
        /// </summary>
        /// <param name="id">科室ID</param>
        /// <param name="input">更新科室的数据传输对象</param>
        /// <returns>API操作结果</returns>
        [HttpPut]
        public async Task<ApiResult> UpdateDoctorDepartment(Guid id, CreateUpdateDoctorDepartmentDto input)
        {
            // 1. 参数校验：检查传入的input是否为空
            if (input == null)
            {
                return ApiResult.Fail("更新信息不能为空。", ResultCode.ValidationError);
            }

            // 2. 存在性检查：查找要更新的科室是否存在
            var existingDepartment = await dept.FindAsync(id);
            if (existingDepartment == null)
            {
                return ApiResult.Fail("要更新的科室不存在。", ResultCode.NotFound);
            }

            // 3. 重复名称检查：
            //    如果新的科室名称与旧名称不同，才进行重复性检查。
            //    这样做可以避免在只修改科室其他属性（如描述）时，
            //    因为名称与自身相同而被误判为重复。
            if (!string.Equals(existingDepartment.DepartmentName, input.DepartmentName, StringComparison.OrdinalIgnoreCase))
            {
                // 在这里添加 await
                var departmentQueryable = await dept.GetQueryableAsync(); // 解决 CS1061 错误

                var departmentWithNameExists = await departmentQueryable.AnyAsync(x => string.Equals(x.DepartmentName,input.DepartmentName, StringComparison.OrdinalIgnoreCase));
                if (departmentWithNameExists)
                {
                    return ApiResult.Fail($"科室名称 '{input.DepartmentName}' 已存在，请使用其他名称。", ResultCode.ValidationError);
                }
            }

            // 4. 映射更新：使用 AutoMapper 或类似工具将 input 映射到 existingDepartment
            //    确保只更新允许修改的字段。
            ObjectMapper.Map(input, existingDepartment);

            // 5. 更新科室：执行数据库更新操作
            await dept.UpdateAsync(existingDepartment);

            // 6. 缓存清除 (可选)：
            // 如果科室列表有缓存，科室信息修改后通常需要清除科室列表缓存
            await deptredis.RemoveAsync(CacheKey); // 有独立的缓存键

            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 删除科室信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult> DeleteDoctorDepartment(Guid id)
        {
            var deptlist = await dept.FindAsync(id);
            await dept.DeleteAsync(deptlist);
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 批量删除科室信息
        /// </summary>
        /// <param name="idsString">要删除的科室ID字符串，例如："guid1,guid2,guid3"</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult> DeleteDoctorDepartment([FromQuery] string idsString) // 参数类型改为string
        {
            if (string.IsNullOrWhiteSpace(idsString))
            {
                return ApiResult.Fail("请提供要删除的科室ID字符串。", ResultCode.NotFound);
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
                return ApiResult.Fail("解析后的科室ID列表为空。", ResultCode.NotFound);
            }

            // 查找所有需要删除的科室实体

            var deptListToDelete = await dept.GetQueryableAsync();
            deptListToDelete = deptListToDelete.Where(d => ids.Contains(d.Id));
            if (!deptListToDelete.Any())
            {
                return ApiResult.Fail("没有找到匹配的科室信息。", ResultCode.NotFound);
            }

            // 批量删除 (硬删除或软删除取决于您的实体是否实现ISoftDelete)
            await dept.DeleteManyAsync(deptListToDelete);

            return ApiResult.Success(ResultCode.Success);
        }

    }
}
