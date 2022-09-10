using System;
using System.Collections.Generic;

namespace Fast.Framework.Interfaces
{

    /// <summary>
    /// 删除建造者接口类
    /// </summary>
    public interface IDeleteBuilder : ISqlBuilder
    {
        /// <summary>
        /// 条件
        /// </summary>
        List<string> Where { get; }

        /// <summary>
        /// 数据库参数
        /// </summary>
        Dictionary<string, object> DbParameters { get; set; }
    }
}

