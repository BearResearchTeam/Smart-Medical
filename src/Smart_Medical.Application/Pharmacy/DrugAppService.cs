using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;

namespace Smart_Medical.Pharmacy
{
    /// <summary>
    /// 药品管理
    /// </summary>
    [ApiExplorerSettings(GroupName = "药品管理")]
    public class DrugAppService : ApplicationService, IDrugAppService
    {
        public IRepository<Drug, int> Repository { get; }
        public IRepository<MedicalHistory> GetRepository { get; }
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public DrugAppService(IRepository<Drug, int> repository, IRepository<MedicalHistory> GetRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            Repository = repository;
            this.GetRepository = GetRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }
        /// <summary>
        /// 根据Id获取药品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ApiResult<DrugDto>> GetAsync(int id)
        {
            try
            {
                var drug = await Repository.FindAsync(id);
                if (drug == null)
                {
                    throw new Exception("药品不存在，无法删除！");
                }
                var result = ObjectMapper.Map<Drug, DrugDto>(drug);
                return ApiResult<DrugDto>.Success(result, ResultCode.Success);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 分页查询药品
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<PageResult<List<DrugDto>>>> GetListAsync([FromQuery] DrugSearchDto search)
        {
            var list = await Repository.GetQueryableAsync();
            var lists = await GetRepository.GetQueryableAsync();
            // 按药品名称模糊查询
            if (!string.IsNullOrWhiteSpace(search.DrugName))
            {
                list = list.Where(x => x.DrugName.Contains(search.DrugName));
            }

            // 按药品类型模糊查询
            if (!string.IsNullOrWhiteSpace(search.DrugType))
            {
                list = list.Where(x => x.DrugType.Contains(search.DrugType));
            }

            // 按生产日期起筛选
            if (search.ProductionDateStart.HasValue)
            {
                list = list.Where(x => x.ProductionDate >= search.ProductionDateStart.Value);
            }

            // 按生产日期止筛选
            if (search.ProductionDateEnd.HasValue)
            {
                list = list.Where(x => x.ProductionDate <= search.ProductionDateEnd.Value);
            }

            // 按最小库存筛选
            if (search.StockMin.HasValue)
            {
                list = list.Where(x => x.Stock >= search.StockMin.Value);
            }

            // 按最大库存筛选
            if (search.StockMax.HasValue)
            {
                list = list.Where(x => x.Stock <= search.StockMax.Value);
            }
            // 联表查公司名称
            var query = from drug in list
                        join company in lists
                            on drug.PharmaceuticalCompanyId equals company.Id
                        select new DrugDto
                        {
                            DrugID = drug.Id,
                            DrugName = drug.DrugName,
                            DrugType = drug.DrugType,
                            PharmaceuticalCompanyId = company.Id,
                            PharmaceuticalCompanyName = company != null ? company.CompanyName : null,

                            FeeName = drug.FeeName,
                            DosageForm = drug.DosageForm,
                            Specification = drug.Specification,
                            PurchasePrice = drug.PurchasePrice,
                            SalePrice = drug.SalePrice,
                            Stock = drug.Stock,
                            StockUpper = drug.StockUpper,
                            StockLower = drug.StockLower,
                            ProductionDate = drug.ProductionDate,
                            ExpiryDate = drug.ExpiryDate,
                            Effect = drug.Effect,
                            Category = drug.Category
                        };
            // 分页处理
            var res = query.PageResult(search.pageIndex, search.pageSize);
            var dto = res.Queryable.ToList();
            var pageInfo = new PageResult<List<DrugDto>>
            {
                Data = dto,
                TotleCount = res.RowCount,
                TotlePage = (int)Math.Ceiling((double)res.RowCount / search.pageSize)
            };
            return ApiResult<PageResult<List<DrugDto>>>.Success(pageInfo, ResultCode.Success);
        }

        /// <summary>
        /// 添加药品
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> CreateAsync(CreateUpdateDrugDto input)
        {
            var exists = await Repository.AnyAsync(d => d.DrugName == input.DrugName);

            if (exists)
                throw new UserFriendlyException($"药品名称 '{input.DrugName}' 已存在！");

            if (input.StockLower > input.StockUpper)
                throw new UserFriendlyException("库存下限不能大于库存上限！");
            if (input.Stock < input.StockLower || input.Stock > input.StockUpper)
                throw new UserFriendlyException("库存必须在上下限之间！");

            if (input.PurchasePrice < 0 || input.SalePrice < 0)
                throw new UserFriendlyException("价格不能为负数！");
            if (input.PurchasePrice > input.SalePrice)
                throw new UserFriendlyException("进价不能大于售价！");

            if (input.ProductionDate > input.ExpiryDate)
                throw new UserFriendlyException("生产日期不能晚于有效期！");

            var drug = ObjectMapper.Map<CreateUpdateDrugDto, Drug>(input);
            await Repository.InsertAsync(drug);

            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 修改药品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ApiResult> UpdateAsync(int id, CreateUpdateDrugDto input)
        {
            var drug = await Repository.GetAsync(id);

            var nameExists = await Repository.AnyAsync(d => d.DrugName == input.DrugName && d.Id != id);
            if (nameExists)
                throw new UserFriendlyException($"药品名称 '{input.DrugName}' 已存在！");

            if (input.StockLower > input.StockUpper)
                throw new UserFriendlyException("库存下限不能大于库存上限！");
            if (input.Stock < input.StockLower || input.Stock > input.StockUpper)
                throw new UserFriendlyException("库存必须在上下限之间！");
            if (input.PurchasePrice < 0 || input.SalePrice < 0)
                throw new UserFriendlyException("价格不能为负数！");
            if (input.PurchasePrice > input.SalePrice)
                throw new UserFriendlyException("进价不能大于售价！");
            if (input.ProductionDate > input.ExpiryDate)
                throw new UserFriendlyException("生产日期不能晚于有效期！");

            ObjectMapper.Map(input, drug);

            await Repository.UpdateAsync(drug);

            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 删除药品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<ApiResult> DeleteAsync(int id)
        {
            await Repository.DeleteAsync(id);
            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 批量删除药品
        /// </summary>
        /// <param name="input">批量删除参数</param>
        /// <returns></returns>
        [HttpPost("batch-delete")]
        public async Task<ApiResult> BatchDeleteAsync(BatchDeleteDrugDto input)
        {
            // 注入 IUnitOfWorkManager _unitOfWorkManager
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
            {
                try
                {
                    if (input.DrugIds == null || !input.DrugIds.Any())
                        return ApiResult.Fail("请选择要删除的药品", ResultCode.ValidationError);

                    // 查询要删除的药品
                    var drugsToDelete = await Repository.GetListAsync(d => input.DrugIds.Contains(d.Id));
                    if (!drugsToDelete.Any())
                        return ApiResult.Fail("未找到要删除的药品", ResultCode.NotFound);

                    // 检查库存
                    if (!input.ForceDelete)
                    {
                        var drugsWithStock = drugsToDelete.Where(d => d.Stock > 0).ToList();
                        if (drugsWithStock.Any())
                        {
                            var drugNames = string.Join(", ", drugsWithStock.Select(d => d.DrugName));
                            return ApiResult.Fail($"以下药品还有库存，无法删除：{drugNames}。如需强制删除，请设置ForceDelete为true", ResultCode.ValidationError);
                        }
                    }

                    // 检查有效期
                    var validDrugs = drugsToDelete.Where(d => d.ExpiryDate > DateTime.Now).ToList();
                    if (validDrugs.Any() && !input.ForceDelete)
                    {
                        var drugNames = string.Join(", ", validDrugs.Select(d => d.DrugName));
                        return ApiResult.Fail($"以下药品还在有效期内，无法删除：{drugNames}。如需强制删除，请设置ForceDelete为true", ResultCode.ValidationError);
                    }

                    // 执行批量删除
                    await Repository.DeleteManyAsync(drugsToDelete);

                    // 提交事务
                    await uow.CompleteAsync();

                    return ApiResult.Success(ResultCode.Success);
                }
                catch (Exception ex)
                {
                    // 事务自动回滚
                    return ApiResult.Fail($"批量删除失败: {ex.Message}", ResultCode.Error);
                }
            }
        }

        /// <summary>
        /// 药品数据导出
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public FileResult ExportDrugExcel()
        {
            // 1. 获取药品数据（包含公司名称）
            var drugList = (from drug in Repository.GetListAsync().Result
                            join company in GetRepository.GetListAsync().Result
                            on drug.PharmaceuticalCompanyId equals company.Id into gj
                            from company in gj.DefaultIfEmpty()
                            select new DrugDto
                            {
                                DrugID = drug.Id,
                                DrugName = drug.DrugName,
                                DrugType = drug.DrugType,
                                PharmaceuticalCompanyName = company != null ? company.CompanyName : string.Empty
                            }).ToList();

            // 2. 创建Excel对象
            IWorkbook workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("药品信息");

            // 3. 创建表头
            var row0 = sheet.CreateRow(0);
            row0.CreateCell(0).SetCellValue("药品ID");
            row0.CreateCell(1).SetCellValue("药品名称");
            row0.CreateCell(2).SetCellValue("类别");
    
            row0.CreateCell(11).SetCellValue("药品功效");
            row0.CreateCell(12).SetCellValue("生产厂家");

            // 4. 填充数据
            int indexnum = 1;
            foreach (var item in drugList)
            {
                var row = sheet.CreateRow(indexnum);
                row.CreateCell(0).SetCellValue(item.DrugID);
                row.CreateCell(1).SetCellValue(item.DrugName);
                row.CreateCell(2).SetCellValue(item.DrugType);
           
                row.CreateCell(11).SetCellValue(item.Effect);
                row.CreateCell(12).SetCellValue(item.PharmaceuticalCompanyName);
                indexnum++;
            }

            // 5. 导出为字节流
            byte[] s;
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                s = ms.ToArray();
            }

            // 6. 返回文件
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return new FileContentResult(s, contentType)
            {
                FileDownloadName = "药品信息.xlsx"
            };
        }

    }
}

