using Microsoft.AspNetCore.Mvc;
using Smart_Medical.Pharmacy;
using Smart_Medical.Until;
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
        /// 创建处方
        /// Create a prescription
        /// </summary>
        /// <remarks>
        /// POST /api/app/prescriptions/prescription
        /// 创建新的处方模板。
        /// </remarks>
        /// <param name="input">处方参数</param>
        /// <returns>操作结果</returns>
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
        /// Get prescription tree
        /// </summary>
        /// <remarks>
        /// POST /api/app/prescriptions/prescription-tree
        /// 获取所有处方的树形结构。
        /// </remarks>
        /// <param name="pid">父级ID</param>
        /// <returns>处方树</returns>
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

        //[HttpPost]
        //public async Task<ApiResult<List<PrescriptionTree>>> GetPrescriptionTreeList(int pid)
        //{
            
        //}
        /// <summary>
        /// 根据不同的处方父级id，返回不同的处方对应的信息
        /// Get prescriptions by parent id
        /// </summary>
        /// <remarks>
        /// GET /api/app/prescriptions/start-prescriptions?pid={pid}
        /// 根据父级ID获取对应的处方信息。
        /// </remarks>
        /// <param name="pid">父级ID</param>
        /// <returns>处方列表</returns>
        [HttpGet]
        public async Task<ApiResult<List<PrescriptionDto>>> StartPrescriptions(int pid)
        {
            var list = await pres.GetQueryableAsync();
            var res = list.Where(x => x.ParentId == pid).ToList();
            return ApiResult<List<PrescriptionDto>>.Success(ObjectMapper.Map<List<Prescription>, List<PrescriptionDto>>(res), ResultCode.Success);
        }
    }
}
