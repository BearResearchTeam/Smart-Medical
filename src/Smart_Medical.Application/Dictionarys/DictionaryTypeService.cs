using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Smart_Medical.Dictionarys.DictionaryTypes;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Smart_Medical.Dictionarys
{
    /// <summary>
    /// 字典类型/字典项
    /// </summary>
    [ApiExplorerSettings(GroupName = "字典管理")]
    public class DictionaryTypeService : ApplicationService, IDictionaryTypeService
    {
        // 定义一个常量作为缓存键，这是这个特定缓存项在 Redis 中的唯一标识。
        // 使用一个清晰且唯一的键很重要。
        private const string CacheKey = "SmartMedical:DictionaryTypes:All"; // 建议使用更具体的键名和前缀

        private readonly IRepository<DictionaryType, Guid> dictionarytype;
        private readonly IDistributedCache<CreateUpdateDictionaryTypeDto> dicttype;
        private readonly IDistributedCache<List<GetDictionaryTypeDto>> dictypedto;

        public DictionaryTypeService(IRepository<DictionaryType, Guid> dictionarytype, IDistributedCache<CreateUpdateDictionaryTypeDto> dicttype, IDistributedCache<List<GetDictionaryTypeDto>> dictypedto)
        {
            this.dictionarytype = dictionarytype;
            this.dicttype = dicttype;
            this.dictypedto = dictypedto;
        }


        private async Task LoadDictionaryTypeDto()
        {
            var typelist = await dictionarytype.GetQueryableAsync();
            var dtoList = ObjectMapper.Map<List<DictionaryType>, List<GetDictionaryTypeDto>>(typelist.ToList());
            await dictypedto.SetAsync(CacheKey, dtoList, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
                // 示例：缓存一天
                // 如果需要滑动过期，也可以添加：
                SlidingExpiration = TimeSpan.FromHours(6) // 例如，6小时内不访问则过期
            });
        }
        /// <summary>
        /// 移除所有字典类型缓存。在数据增删改后调用。
        /// </summary>
        private async Task RemoveAllDictionaryTypesCacheAsync()
        {
            await dictypedto.RemoveAsync(CacheKey);//所有字典类型缓存已失效
        }

        /// <summary>
        /// 新增字典数据类型
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> InsertDictionaryTypeLAsync(CreateUpdateDictionaryTypeDto input)
        {


            var res = ObjectMapper.Map<CreateUpdateDictionaryTypeDto, DictionaryType>(input);
            res = await dictionarytype.InsertAsync(res);
            await RemoveAllDictionaryTypesCacheAsync(); // 新增后清除缓存
            return ApiResult.Success(ResultCode.Success);

        }
        /// <summary>
        /// 修改字典数据类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult> UpdateDictionaryTypeLAsync(Guid id, CreateUpdateDictionaryTypeDto input)
        {
            if (input == null)
            {
                return ApiResult.Fail("字典数据类型信息无效", ResultCode.NotFound);
            }

            var typelist = await dictionarytype.FindAsync(id);

            if (typelist == null)
            {
                return ApiResult.Fail("未找到对应的字典类型", ResultCode.NotFound);
            }

            var dto = ObjectMapper.Map(input, typelist);
            await dictionarytype.UpdateAsync(dto);
            await RemoveAllDictionaryTypesCacheAsync(); // 修改后清除缓存
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 获取字典数据类型
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<PageResult<List<GetDictionaryTypeDto>>>> GetDictionaryTypeList(string datetype, [FromQuery] GetDictionaryTypeSearchDto search)
        {
            var typelist = await dictypedto.GetAsync(CacheKey);
            if (typelist == null || !typelist.Any())// 检查是否为null或空列表
            {
                await LoadDictionaryTypeDto(); // 字典类型缓存中没有数据（或已过期)就调用内部方法加载数据到缓存
                typelist = await dictypedto.GetAsync(CacheKey);//// 再次从缓存获取，确保拿到最新加载的数据
            }
            //// 确保即使缓存加载失败，allTypesFromCache 也不会是 null，避免后续操作出错
            typelist ??= new List<GetDictionaryTypeDto>();
            var filteredTypes = typelist.WhereIf(!string.IsNullOrEmpty(search.DictionaryLabel), x => x.DictionaryValue.Contains(search.DictionaryLabel) || x.DictionaryLabel.Contains(search.DictionaryLabel)).Where(x => x.DictionaryDataType == datetype);
            var res = filteredTypes.AsQueryable().PageResult(search.PageIndex, search.PageSize);
            //var dto = ObjectMapper.Map<List<DictionaryType>, List<GetDictionaryTypeDto>>(res.Queryable.ToList());
            var pageinfo = new PageResult<List<GetDictionaryTypeDto>>
            {
                Data = res.Queryable.ToList(),
                TotleCount = res.RowCount,
                TotlePage = (int)Math.Ceiling((double)res.RowCount / search.PageSize)
            };
            return ApiResult<PageResult<List<GetDictionaryTypeDto>>>.Success(pageinfo, ResultCode.Success);
        }

        /// <summary>
        /// 删除字典数据类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult> DeleteDictionaryType(Guid id)
        {

            var typelist = await dictionarytype.FindAsync(id);
            if (typelist == null)
            {
                return ApiResult.Fail("未找到对应的字典类型信息", ResultCode.NotFound);
            }
            await dictionarytype.DeleteAsync(typelist);
            await RemoveAllDictionaryTypesCacheAsync(); // 删除后清除缓存
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 批量删除字典数据类型
        /// </summary>
        /// <param name="ids">逗号分隔的id字符串</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult> DeleteDictionaryType(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return ApiResult.Fail("参数不能为空", ResultCode.NotFound);
            }

            // 将传入的逗号分隔字符串按逗号拆分为字符串数组，去除空项
            var idArray = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                // 去除每个分割后字符串的首尾空白字符
                .Select(x => x.Trim())
                // 只保留能成功转换为 Guid 的字符串
                .Where(x => Guid.TryParse(x, out _))
                // 将字符串转换为 Guid 类型
                .Select(Guid.Parse)
                // 转换为 List<Guid>
                .ToList();

            if (idArray.Count == 0)
            {
                return ApiResult.Fail("参数格式不正确", ResultCode.NotFound);
            }

            try
            {
                foreach (var id in idArray)
                {
                    var entity = await dictionarytype.FindAsync(id);
                    if (entity != null)
                    {
                        await dictionarytype.DeleteAsync(entity);
                    }
                }
                var res = new DistributedCacheEntryOptions();
                await RemoveAllDictionaryTypesCacheAsync(); // 批量删除后清除缓存
                return ApiResult.Success(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return ApiResult.Fail($"批量删除失败: {ex.Message}", ResultCode.NotFound);
            }
        }
    }
}
