﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace Fast.Framework.Web.Test
{

    /// <summary>
    /// Body缓存中间件 
    /// </summary>
    public class BodyCacheMiddleware
    {

        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger<BodyCacheMiddleware> logger;

        /// <summary>
        /// 请求委托
        /// </summary>
        private readonly RequestDelegate next;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="next">请求委托</param>
        public BodyCacheMiddleware(ILogger<BodyCacheMiddleware> logger, RequestDelegate next)
        {
            this.logger = logger;
            this.next = next;
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="httpContext">http上下文</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request?.ContentLength > 0 && httpContext.Request.ContentType != null)
            {
                if (httpContext.Request.ContentType != null && httpContext.Request.ContentType.StartsWith("application/json"))
                {
                    httpContext.Request.EnableBuffering();
                    var bodyString = await new StreamReader(httpContext.Request.Body, Encoding.UTF8).ReadToEndAsync();
                    httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                    httpContext.Request.Headers["BodyString_Cache"] = bodyString;
                }
            }
            await next(httpContext);
        }
    }
}
