using Microsoft.AspNetCore.Mvc;
using Smart_Medical.Pharmacy.InAndOutWarehouse;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Smart_Medical.Pharmacy
{
    /// <summary>
    /// 药品入库
    /// </summary>
    [ApiExplorerSettings(GroupName = "药品入库管理")]
    public class DrugInStockAppService : ApplicationService, IDrugInStockAppService
    {
        private readonly IRepository<DrugInStock, Guid> _drugInStockRepository;
        private readonly IRepository<Drug, int> _drugRepository;

        public DrugInStockAppService(
            IRepository<DrugInStock, Guid> drugInStockRepository,
            IRepository<Drug, int> drugRepository)
        {
            _drugInStockRepository = drugInStockRepository;
            _drugRepository = drugRepository;
        }


        /// <summary>
        /// 批量药品入库
        /// </summary>
        /// <remarks>
        /// POST /api/app/pharmacy/drug-in-stock/batch
        /// </remarks>
        /// <param name="input">批量入库参数，包含多条明细</param>
        /// <returns>批量入库结果</returns>
        [HttpPost]
        public async Task<ApiResult<List<DrugInStockDetailDto>>> BatchStockInAsync([FromBody] BatchDrugInStockDto input)
        {
            if (input.Details == null || !input.Details.Any())
                return ApiResult<List<DrugInStockDetailDto>>.Fail("入库明细不能为空", ResultCode.ValidationError);

            var resultList = new List<DrugInStockDetailDto>();

            foreach (var detail in input.Details)
            {
                var drug = await _drugRepository.FindAsync(detail.DrugId);
                if (drug == null)
                    return ApiResult<List<DrugInStockDetailDto>>.Fail($"找不到药品ID:{detail.DrugId}", ResultCode.NotFound);

                if (detail.ProductionDate > DateTime.Now)
                    return ApiResult<List<DrugInStockDetailDto>>.Fail($"药品ID:{detail.DrugId} 生产日期不能晚于当前时间", ResultCode.ValidationError);
                if (detail.ExpiryDate <= detail.ProductionDate)
                    return ApiResult<List<DrugInStockDetailDto>>.Fail($"药品ID:{detail.DrugId} 有效期必须晚于生产日期", ResultCode.ValidationError);

                int newStock = drug.Stock + detail.Quantity;
                if (newStock > drug.StockUpper)
                    return ApiResult<List<DrugInStockDetailDto>>.Fail($"药品ID:{detail.DrugId} 入库后库存({newStock})超过上限({drug.StockUpper})", ResultCode.ValidationError);
                if (newStock < drug.StockLower)
                    return ApiResult<List<DrugInStockDetailDto>>.Fail($"药品ID:{detail.DrugId} 入库后库存({newStock})低于下限({drug.StockLower})", ResultCode.ValidationError);

                drug.Stock = newStock;
                await _drugRepository.UpdateAsync(drug);

                var drugInStock = new DrugInStock
                {
                    DrugId = detail.DrugId,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    TotalAmount = detail.UnitPrice * detail.Quantity,
                    ProductionDate = detail.ProductionDate,
                    ExpiryDate = detail.ExpiryDate,
                    BatchNumber = detail.BatchNumber,
                    Supplier = detail.Supplier,
                    Status = "已入库",
                    CreationTime = DateTime.Now
                };
                await _drugInStockRepository.InsertAsync(drugInStock);

                resultList.Add(new DrugInStockDetailDto
                {
                    DrugId = drugInStock.DrugId,
                    PharmaceuticalCompanyId = detail.PharmaceuticalCompanyId,
                    Quantity = drugInStock.Quantity,
                    UnitPrice = drugInStock.UnitPrice,
                    TotalAmount = drugInStock.TotalAmount,
                    ProductionDate = drugInStock.ProductionDate,
                    ExpiryDate = drugInStock.ExpiryDate,
                    BatchNumber = drugInStock.BatchNumber,
                    Supplier = drugInStock.Supplier,
                    Status = drugInStock.Status,
                    CreationTime = drugInStock.CreationTime
                });
            }

            return ApiResult<List<DrugInStockDetailDto>>.Success(resultList, ResultCode.Success);
        }


    }
}
