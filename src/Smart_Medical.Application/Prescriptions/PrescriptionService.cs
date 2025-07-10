using Microsoft.AspNetCore.Mvc;
using Smart_Medical.Pharmacy;
using Smart_Medical.Until;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace Smart_Medical.Prescriptions
{
    /// <summary>
    /// 处方服务
    /// </summary>
    [ApiExplorerSettings(GroupName = "处方管理")]
    public class PrescriptionService : ApplicationService, IPrescriptionService
    {
        private readonly IRepository<Prescription, int> pres;
        private readonly IRepository<Drug, int> drogrepository;

        public PrescriptionService(IRepository<Prescription, int> pres, IRepository<Drug, int> drogrepository)
        {
            this.pres = pres;
            this.drogrepository = drogrepository;
        }
        /// <summary>
        /// 获取处方药品信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ApiResult<List<DrugsSelectDto>>> GetDrugSelect()
        {
            var list = await drogrepository.GetQueryableAsync();
            var dto = ObjectMapper.Map<List<Drug>, List<DrugsSelectDto>>(list.ToList());
            return ApiResult<List<DrugsSelectDto>>.Success(dto, ResultCode.Success);
        }
        /// <summary>
        /// 创建新的处方模板
        /// Create a prescription
        /// </summary>
        [HttpPost]
        public async Task<ApiResult> CreateAsync(PrescriptionDto input)
        {
            var res = ObjectMapper.Map<PrescriptionDto, Prescription>(input);
            res = await pres.InsertAsync(res);
            return ApiResult.Success(ResultCode.Success);

        }
        /// <summary>
        /// 获取处方树
        /// </summary>
        /// <remarks>
        [HttpPost]
        public async Task<ApiResult<List<PrescriptionTree>>> GetPrescriptionTree(int pid)
        {
            // 一次性查出所有数据
            var allList = await pres.GetQueryableAsync();
            var tree = BuildTree(allList.ToList(), pid);
            return ApiResult<List<PrescriptionTree>>.Success(tree, ResultCode.Success);
        }
        /// <summary>
        /// 内存递归组装树
        /// </summary>
        private List<PrescriptionTree> BuildTree(List<Prescription> all, int parentId)
        {
            var children = all.Where(x => x.ParentId == parentId).ToList();
            var result = new List<PrescriptionTree>();
            foreach (var item in children)
            {
                var node = new PrescriptionTree
                {
                    value = item.Id,
                    label = item.PrescriptionName,
                    children = BuildTree(all, item.Id)
                };
                result.Add(node);
            }
            return result;
        }
        /// <summary>
        /// 获取处方树对应的药品信息列表
        /// </summary>
        /// <param name="prescriptionid"></param>
        /// <returns></returns>

        public async Task<ApiResult<List<GetPrescriptionDrugDto>>> GetPrescriptionTreeList(int? prescriptionid, string? DrugName)
        {
            var prelist = await pres.GetQueryableAsync();

            var druglist = await drogrepository.GetQueryableAsync();
            druglist = druglist.WhereIf(!string.IsNullOrEmpty(DrugName), x => x.DrugName == DrugName);
            List<GetPrescriptionDrugDto> result = new List<GetPrescriptionDrugDto>();

            // 如果 prescriptionid 为空或为0，返回所有处方下的所有药品
            var prescriptions = (prescriptionid == null || prescriptionid == 0)
                ? prelist.ToList()
                : prelist.Where(x => x.Id == prescriptionid.Value).ToList();

            if (prescriptions == null || prescriptions.Count == 0)
            {
                return ApiResult<List<GetPrescriptionDrugDto>>.Fail("未找到处方", ResultCode.NotFound);
            }

            foreach (var prescription in prescriptions)
            {
                var drugIds = (prescription.DrugIds ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out var gid) ? (int?)gid : null)
                    .Where(gid => gid != null)
                    .Select(gid => gid.Value)
                    .ToList();

                var drugs = druglist.Where(d => drugIds.Contains(d.Id)).ToList();

                result.AddRange(drugs.Select(d => new GetPrescriptionDrugDto
                {
                    Id = prescription.Id,
                    PrescriptionName = prescription.PrescriptionName,
                    DrugIds = prescription.DrugIds,
                    ParentId = prescription.ParentId,
                    DrugId=d.Id,
                    DrugName = d.DrugName,
                    DrugType = d.DrugType,
                    FeeName = d.FeeName,
                    DosageForm = d.DosageForm,
                    Specification = d.Specification,
                    PurchasePrice = d.PurchasePrice,
                    SalePrice = d.SalePrice,
                    Stock = d.Stock,
                    StockUpper = d.StockUpper,
                    StockLower = d.StockLower,
                    ProductionDate = d.ProductionDate,
                    ExpiryDate = d.ExpiryDate,
                    Effect = d.Effect,
                    Category = d.Category,
                    PharmaceuticalCompanyId = d.PharmaceuticalCompanyId,
                }));
            }

            return ApiResult<List<GetPrescriptionDrugDto>>.Success(result, ResultCode.Success);
        }
        /// <summary>
        /// 批量删除处方下的指定药品
        /// </summary>
        /// <param name="prescriptionId">处方ID</param>
        /// <param name="drugIdsToDeleteString">要删除的药品ID列表，逗号分隔的字符串形式</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> DeletePrescriptionDrugs(int prescriptionId, string? drugIdsToDeleteString)
        {
            // 1. 获取处方
            var prescription = await pres.GetAsync(prescriptionId);
            if (prescription == null)
            {
                return ApiResult.Fail("未找到处方", ResultCode.NotFound);
            }

            // 新增：解析 drugIdsToDeleteString 参数为 List<int>
            // 确保 drugIdsToDeleteString 不为 null 或空，并处理解析错误
            var drugIdsToDelete = (drugIdsToDeleteString ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id.Trim(), out var parsedId) ? (int?)parsedId : null) // Trim() 用于去除可能的空格
                .Where(id => id.HasValue) // 过滤掉无法解析为整数的项
                .Select(id => id.Value)
                .ToList();

            // 如果解析后的列表为空，则直接返回错误
            if (!drugIdsToDelete.Any())
            {
                return ApiResult.Fail("要删除的药品ID列表为空或格式不正确。", ResultCode.NotFound);
            }

            // 2. 解析当前处方的药品ID字符串，并转换为List<int>
            var currentDrugIds = (prescription.DrugIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var gid) ? gid : (int?)null)
                .Where(gid => gid.HasValue) // 确保是有效的整数
                .Select(gid => gid.Value)
                .ToList();

            // 记录原始药品数量，用于判断是否有实际删除
            var originalDrugCount = currentDrugIds.Count;

            // 3. 从当前药品ID列表中移除所有要删除的药品ID
            // 使用 HashSet 优化查找性能，如果 drugIdsToDelete 列表很大
            var drugsToDeleteSet = new HashSet<int>(drugIdsToDelete);

            // 使用 LINQ 的 Except 方法可以更简洁地实现差集操作
            var remainingDrugIds = currentDrugIds.Except(drugsToDeleteSet).ToList();

            // 4. 判断是否有实际的药品被删除
            if (remainingDrugIds.Count == originalDrugCount)
            {
                // 如果剩余药品数量和原始药品数量相同，说明没有匹配到任何要删除的药品
                return ApiResult.Fail("指定药品均不在处方中。", ResultCode.NotFound);
            }

            // 5. 更新DrugIds
            prescription.DrugIds = string.Join(",", remainingDrugIds);
            await pres.UpdateAsync(prescription);

            return ApiResult.Success(ResultCode.Success);
        }

        /// <summary>
        /// 批量修改处方下的药品信息（删除所有旧的，新增所有新的）
        /// </summary>
        /// <param name="prescriptionId">处方ID</param>
        /// <param name="newDrugIdsString">要替换为的新的药品ID列表，逗号分隔的字符串形式</param>
        /// <returns>操作结果</returns>
        public async Task<ApiResult> UpdatePrescriptionDrugs(int prescriptionId, string newDrugIdsString)
        {
            // 1. 获取处方
            var prescription = await pres.GetAsync(prescriptionId);
            if (prescription == null)
            {
                return ApiResult.Fail("未找到处方", ResultCode.NotFound);
            }

            // 2. 解析 newDrugIdsString 参数为 List<int>
            // 即使没有新药品，也允许传入空字符串或null，表示清空处方药品
            var newDrugIds = (newDrugIdsString ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id.Trim(), out var parsedId) ? (int?)parsedId : null) // Trim() 用于去除可能的空格
                .Where(id => id.HasValue) // 过滤掉无法解析为整数的项
                .Select(id => id.Value)
                .ToList();

            // 3. 获取当前处方的药品ID，用于后续判断是否有实际变更
            var currentDrugIds = (prescription.DrugIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id.Trim(), out var gid) ? (int?)gid : null)
                .Where(gid => gid.HasValue)
                .Select(gid => gid.Value)
                .ToList();

            // 4. 比较新旧药品列表，判断是否需要更新
            // 将两个列表都转换为 HashSet 以便高效比较（忽略顺序）
            var currentDrugIdsSet = new HashSet<int>(currentDrugIds);
            var newDrugIdsSet = new HashSet<int>(newDrugIds);

            // 如果两个集合的元素数量相同，并且所有元素都相同，则认为没有变化
            if (currentDrugIdsSet.SetEquals(newDrugIdsSet))
            {
                return ApiResult.Fail("药品信息未发生变更，无需更新。", ResultCode.NotFound);
            }

            // 5. 更新DrugIds为新的药品ID列表
            // 通常会按ID排序，保持一致性，即使 HashSet 是无序的
            prescription.DrugIds = string.Join(",", newDrugIds.OrderBy(id => id));
            await pres.UpdateAsync(prescription);

            return ApiResult.Success(ResultCode.Success);
        }
        /// <summary>
        /// 根据不同的处方父级id，返回不同的处方对应的信息
        /// </summary>
        [HttpGet]
        public async Task<ApiResult<List<PrescriptionListDto>>> StartPrescriptions(int pid)
        {
            var list = await pres.GetQueryableAsync();
            var res = list.Where(x => x.ParentId == pid).ToList();
            return ApiResult<List<PrescriptionListDto>>.Success(ObjectMapper.Map<List<Prescription>, List<PrescriptionListDto>>(res), ResultCode.Success);
        }
    }
}
