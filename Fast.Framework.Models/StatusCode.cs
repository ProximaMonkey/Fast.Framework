using System;
using System.ComponentModel;

namespace Fast.Framework.Models
{

    /// <summary>
    /// 状态码
    /// </summary>
    [Description("状态码")]
    public static class StatusCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        public const int Success = 0;

        /// <summary>
        /// 错误
        /// </summary>
        [Description("错误")]
        public const int Error = -1;

        /// <summary>
        /// 登录失效
        /// </summary>
        [Description("登录失效")]
        public const int LoginInvalid = -999;

        /// <summary>
        /// 令牌错误
        /// </summary>
        [Description("令牌错误")]
        public const int TokenError = -1000;

        /// <summary>
        /// 签名错误
        /// </summary>
        [Description("签名错误")]
        public const int SignError = -1001;

        /// <summary>
        /// 参数错误
        /// </summary>
        [Description("参数错误")]
        public const int ArgumentError = -1002;

        /// <summary>
        /// 非法请求
        /// </summary>
        [Description("非法请求")]
        public const int IllegalRequest = -1003;

    }
}
