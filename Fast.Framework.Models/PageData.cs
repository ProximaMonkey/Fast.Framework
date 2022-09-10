using System.ComponentModel;

namespace Fast.Framework.Models
{

    /// <summary>
    /// 页数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Description("页数据")]
    public class PageData<T>
    {
        /// <summary>
        /// 数据
        /// </summary>
        [Description("数据")]
        public T Data { get; set; }

        /// <summary>
        /// 计数
        /// </summary>
        [Description("计数")]
        public int Count { get; set; }
    }
}
