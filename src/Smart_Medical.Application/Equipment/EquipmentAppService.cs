using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Medical.Equipment.Dtos;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Smart_Medical.Equipment
{
    [ApiExplorerSettings(GroupName = "设备管理")]
    public class EquipmentAppService : ApplicationService, IEquipmentAppService
    {
        private readonly IRepository<EquipmentManagement, Guid> _repository;

        public EquipmentAppService(IRepository<EquipmentManagement, Guid> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取单个设备信息
        /// </summary>
        /// <param name="id">设备ID</param>
        /// <returns>设备详细信息</returns>
        public async Task<ApiResult<EquipmentManagementDto>> GetAsync(Guid id)
        {
            // 1. 根据ID查询设备实体
            var entity = await _repository.GetAsync(id);
            // 2. 实体转DTO
            var dto = ObjectMapper.Map<EquipmentManagement, EquipmentManagementDto>(entity);
            // 3. 返回成功结果
            return ApiResult<EquipmentManagementDto>.Success(dto, ResultCode.Success);
        }

        

        /// <summary>
        /// 分页获取设备列表
        /// </summary>
        /// <param name="input">分页和排序参数</param>
        /// <returns>设备分页列表</returns>
        public async Task<ApiResult<PagedResultDto<EquipmentManagementDto>>> GetListAsync(GeEquipmentParameterDtos input)
        {
            try
            {
                // 1. 获取设备查询对象
                var query = await _repository.GetQueryableAsync();
                // 2. 统计总数
                var totalCount = await AsyncExecuter.CountAsync(query);

                var items = await AsyncExecuter.ToListAsync(
                    query.Page
                        (input.PageIndex,input.PageSize)
                    );

                
                // 4. 实体转DTO列表
                var result = ObjectMapper.Map<List<EquipmentManagement>, List<EquipmentManagementDto>>(items);
                // 5. 返回分页结果
                return ApiResult<PagedResultDto<EquipmentManagementDto>>.Success(new PagedResultDto<EquipmentManagementDto>(totalCount, result), ResultCode.Success);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 设备登记（新增设备信息）
        /// </summary>
        /// <param name="input">设备登记信息</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult<ApiResult>> CreateAsync(CreateUpdateEquipmentManagementDto input)
        {
            // 1. 参数校验（可根据需要扩展）
            // 2. 映射DTO到实体
            var entity = ObjectMapper.Map<CreateUpdateEquipmentManagementDto, EquipmentManagement>(input);
            // 3. 插入数据库
            await _repository.InsertAsync(entity);
            // 4. 返回成功结果
            return ApiResult<ApiResult>.Success(ApiResult.Success(ResultCode.Success), ResultCode.Success);
        }

        /// <summary>
        /// 更新设备信息
        /// </summary>
        /// <param name="id">设备ID</param>
        /// <param name="input">设备更新信息</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> UpdateAsync(Guid id, CreateUpdateEquipmentManagementDto input)
        {
            // 1. 查询原始设备实体
            var entity = await _repository.GetAsync(id);
            // 2. 映射更新内容到实体
            ObjectMapper.Map(input, entity);
            // 3. 更新数据库
            await _repository.UpdateAsync(entity);
            // 4. 返回成功结果
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 删除设备信息
        /// </summary>
        /// <param name="id">设备ID</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> DeleteAsync(Guid id)
        {
            // 1. 删除数据库中的设备实体
            await _repository.DeleteAsync(id);
            // 2. 返回成功结果
            return ApiResult.Success(ResultCode.Success);
        }
    }
} 