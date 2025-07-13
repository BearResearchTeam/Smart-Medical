using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Medical.Dictionarys
{
    public class DictionaryDto
    {
        /// <summary>
        /// 字典数据名称
        /// </summary>
        public string DictionaryDataName { get; set; }
        /// <summary>
        /// 字典数据类型
        /// </summary>
        public string DictionaryDataType { get; set; }
        /// <summary>
        /// 字典数据状态
        /// </summary>
        public int DictionaryDataState { get; set; }
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
        /// 状态
        /// </summary>
        public int DictionaryTypeState { get; set; }
    }
}
