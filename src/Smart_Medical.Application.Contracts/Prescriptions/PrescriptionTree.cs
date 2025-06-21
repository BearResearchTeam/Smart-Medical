﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Prescriptions
{
    public class PrescriptionTree
    {
        //保存节点编号
        public int value { get; set; }
        //保存节点名称
        public string label { get; set; }
        //保存子节点内容
        public List<PrescriptionTree> children { get; set; }
    }
}
