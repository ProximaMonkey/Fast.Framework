using System.ComponentModel;

namespace Fast.Framework.Models
{

    /// <summary>
    /// Api结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Description("Api结果")]
    public class ApiResult<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        [Description("状态码 0 请求成功 其它都为失败")]
        public int Code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [Description("消息")]
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [Description("数据 详细参考每个接口说明")]
        public T Data { get; set; }

        /// <summary>
        /// 计数
        /// </summary>
        [Description("计数")]
        public int? Count { get; set; }
    }
}
