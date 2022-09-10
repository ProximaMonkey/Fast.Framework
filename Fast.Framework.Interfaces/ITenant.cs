using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.Interfaces
{

    /// <summary>
    /// 租户接口类
    /// </summary>
    public interface ITenant
    {

        /// <summary>
        /// 改变数据库
        /// </summary>
        /// <param name="dbId">数据库ID</param>
        /// <returns></returns>
        void ChangeDb(string dbId);
    }
}
