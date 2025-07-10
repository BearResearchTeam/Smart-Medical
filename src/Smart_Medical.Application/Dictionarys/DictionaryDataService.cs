using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Medical.Dictionarys.DictionaryDatas;
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

namespace Smart_Medical.Dictionarys
{
    /// <summary>
    /// 字典数据
    /// </summary>
    [ApiExplorerSettings(GroupName = "字典管理")]
    public class DictionaryDataService : ApplicationService, IDictionaryDataService
    {
        // 定义一个常量作为缓存键，这是这个特定缓存项在 Redis 中的唯一标识。
        // 使用一个清晰且唯一的键很重要。
        private const string CacheKey = "SmartMedical:dicdata:All"; // 建议使用更具体的键名和前缀
        private readonly IRepository<DictionaryData, Guid> dictionaryData;
        private readonly IRepository<DictionaryType, Guid> dictionarytype;
        private readonly IRedisHelper<List<GetDictionaryDataDto>> dictdatadto;

        public DictionaryDataService(IRepository<DictionaryData, Guid> dictionaryData, IRepository<DictionaryType, Guid> dictionarytype, IRedisHelper<List<GetDictionaryDataDto>> dictdatadto)
        {
            this.dictionaryData = dictionaryData;
            this.dictionarytype = dictionarytype;
            this.dictdatadto = dictdatadto;
        }
        /// <summary>
        /// 新增字典数据
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> InsertDictionaryDataLAsync(CreateUpdateDictionaryDataDto input)
        {
            var res = ObjectMapper.Map<CreateUpdateDictionaryDataDto, DictionaryData>(input);
            res = await dictionaryData.InsertAsync(res);
            await dictdatadto.RemoveAsync(CacheKey);//删除缓存
            return ApiResult.Success(ResultCode.Success);

        }
        /// <summary>
        /// 修改字典数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult> UpdateDictionaryDataLAsync(Guid id, CreateUpdateDictionaryDataDto input)
        {
            var datalist = await dictionaryData.FindAsync(id);


            if (datalist == null)
            {
                return ApiResult.Fail("未找到对应的字典数据", ResultCode.NotFound);
            }

            // 检查同类型下是否有其他数据重名
            var exists = await dictionaryData.AnyAsync(x =>
                x.DictionaryDataName == input.DictionaryDataName &&
                x.DictionaryDataType == input.DictionaryDataType &&
                x.Id != id);

            if (exists)
            {
                return ApiResult.Fail("字典数据名称已存在，不能修改为该名称", ResultCode.NotFound);
            }

            var dto = ObjectMapper.Map(input, datalist);
            await dictionaryData.UpdateAsync(dto);
            return ApiResult.Success(ResultCode.Success);
        }
        
        /// <summary>
        /// 获取字典数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<PageResult<List<GetDictionaryDataDto>>>> GetDictionaryDataList([FromQuery] GetDictionaryDataSearchDto search)
        {
            // 利用缓存穿透，自动加载缓存
            var datalist = await dictdatadto.GetAsync(CacheKey, async () =>
            {
                var query = await dictionaryData.GetQueryableAsync();
                return ObjectMapper.Map<List<DictionaryData>, List<GetDictionaryDataDto>>(query.ToList());
            });

            // 防御性处理，确保 datalist 不为 null
            datalist ??= new List<GetDictionaryDataDto>();

            // 过滤和分页
            var datalistres = datalist.WhereIf(!string.IsNullOrEmpty(search.DictionaryDataName), x => x.DictionaryDataName.Contains(search.DictionaryDataName))
                .WhereIf(search.DictionaryDataState!=null, x => x.DictionaryDataState == search.DictionaryDataState);
            var res = datalistres.AsQueryable().PageResult(search.PageIndex, search.PageSize);
            
            var pageinfo = new PageResult<List<GetDictionaryDataDto>>
            {
                Data = res.Queryable.ToList(),
                TotleCount = res.RowCount,
                TotlePage = (int)Math.Ceiling((double)res.RowCount / search.PageSize)
            };
            return ApiResult<PageResult<List<GetDictionaryDataDto>>>.Success(pageinfo, ResultCode.Success);
        }
        /// <summary>
        /// 删除字典数据
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult> DeleteDictionaryListAsync(Guid id)
        {
            var datalist = await dictionaryData.FindAsync(id);
            await dictionaryData.DeleteAsync(datalist);
            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 获取字典数据类型联查
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResult<List<DictionaryDto>>> GetDictionaryListAsync(string Dictiontype)
        {
            var queryableData = await dictionaryData.GetQueryableAsync();
            var queryableType = await dictionarytype.GetQueryableAsync();

            var resultList = await (from a in queryableData
                                    join b in queryableType on a.DictionaryDataType equals b.DictionaryDataType
                                    // 优化2: 如果Dictiontype参数有值，添加过滤条件
                                    where !string.IsNullOrEmpty(Dictiontype) || a.DictionaryDataType == Dictiontype
                                    select new DictionaryDto
                                    {
                                        DictionaryDataState = a.DictionaryDataState,
                                        DictionaryDataName = a.DictionaryDataName,
                                        DictionaryDataType = a.DictionaryDataType,
                                        DictionarySort = b.DictionarySort,
                                        DictionaryLabel = b.DictionaryLabel,
                                        DictionaryValue = b.DictionaryValue,
                                        DictionaryTypeState = b.DictionaryTypeState
                                    }).ToListAsync(); // 优化3: 使用ToListAsync()异步执行查询

            return ApiResult<List<DictionaryDto>>.Success(resultList, ResultCode.Success);
        }
    }
}
