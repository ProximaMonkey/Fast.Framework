using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;


namespace Fast.Framework.Models
{

    /// <summary>
    /// 页
    /// </summary>
    [Description("页")]
    public class Pagination
    {
        /// <summary>
        /// 页码
        /// </summary>
        [Description("页码")]
        [DefaultValue(1)]
        public int Page { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        [Description("页大小")]
        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 排序依据
        /// </summary>
        [Description("排序依据")]
        public string OrderBy { get; set; }
    }
}
