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

        public PrescriptionService(IRepository<Prescription, int> pres,IRepository<Drug, int> drogrepository)
        {
            this.pres = pres;
            this.drogrepository = drogrepository;
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
            //var prescription = await pres.InsertAsync(input);
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

        public async Task<ApiResult<List<GetPrescriptionDrugDto>>> GetPrescriptionTreeList(int? prescriptionid)
        {
            var prelist = await pres.GetQueryableAsync();
            var druglist = await drogrepository.GetQueryableAsync();

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
                    PrescriptionName = prescription.PrescriptionName,
                    DrugIds = prescription.DrugIds,
                    ParentId = prescription.ParentId,
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
        /// 根据不同的处方父级id，返回不同的处方对应的信息
        /// </summary>
        [HttpGet]
        public async Task<ApiResult<List<PrescriptionDto>>> StartPrescriptions(int pid)
        {
            var list = await pres.GetQueryableAsync();
            var res = list.Where(x => x.ParentId == pid).ToList();
            return ApiResult<List<PrescriptionDto>>.Success(ObjectMapper.Map<List<Prescription>, List<PrescriptionDto>>(res), ResultCode.Success);
        }
    }
}
