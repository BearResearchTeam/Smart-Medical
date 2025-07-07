using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Enums
{

    // 权限类型枚举，用于区分权限是菜单、按钮还是其他类型
    public enum PermissionType
    {
        /// <summary>
        /// 菜单权限（用于控制菜单的显示）
        /// </summary>
        Menu = 0,

        /// <summary>
        /// 按钮权限（用于控制页面内按钮的显示和操作）
        /// </summary>
        Button = 1,

        /// <summary>
        /// 页面隐藏菜单
        /// </summary>
        Operation = 2
    }
}
