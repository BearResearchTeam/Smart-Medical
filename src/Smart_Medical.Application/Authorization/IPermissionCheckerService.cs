using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smart_Medical.Authorization
{
    /// <summary>
    /// 权限校验服务接口，负责判断用户是否拥有指定权限及获取用户所有权限
    /// </summary>
    public interface IPermissionCheckerService
    {
        /// <summary>
        /// 判断指定用户是否拥有某个权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="permissionCode">权限编码</param>
        /// <returns>如果拥有该权限返回true，否则返回false</returns>
        Task<bool> IsGrantedAsync(Guid userId, string permissionCode);

        /// <summary>
        /// 获取指定用户拥有的所有权限编码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>该用户拥有的权限编码列表</returns>
        Task<List<string>> GetGrantedPermissionsAsync(Guid userId);
    }
}
