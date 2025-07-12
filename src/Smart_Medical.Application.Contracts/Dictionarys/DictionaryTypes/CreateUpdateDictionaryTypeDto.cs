using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Dictionarys.DictionaryTypes
{
    public class CreateUpdateDictionaryTypeDto
    {
        /// <summary>
        /// 字典排序
        /// </summary>
        public int DictionarySort { get; set; }
        /// <summary>
        /// 字典标签
        /// </summary>
        public int DictionaryLabel { get; set; }
        /// <summary>
        /// 字典值
        /// </summary>
        public string DictionaryValue { get; set; }
        /// <summary>
        /// 字典数据类型
        /// </summary>
        public string DictionaryDataType { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int DictionaryTypeState { get; set; }
        /// <summary>
        /// 字典类型描述
        /// </summary>
        public string DictionaryTypeDesc { get; set; }
    }
}
