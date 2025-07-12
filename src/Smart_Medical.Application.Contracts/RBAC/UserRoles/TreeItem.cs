using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.RBAC.UserRoles
{
    public class TreeItem
    {
        public string value { get; set; }
        public string label { get; set; }
        public List<TreeItem> children { get; set; }

    }
}
