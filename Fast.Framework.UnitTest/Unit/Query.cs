using Fast.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework
{

    /// <summary>
    /// 删除
    /// </summary>
    [TestClass]
    public class Query
    {
        /// <summary>
        /// 数据库对象
        /// </summary>
        private readonly IDbContext db;

        /// <summary>
        /// 构造方法
        /// </summary>
        public Query()
        {
            db = new DbContext(new List<DbOptions>() { new DbOptions()
                {
                    DbId = "1",
                    DbType = DbType.MySQL,
                    ProviderName = "MySqlConnector",
                    FactoryName = "MySqlConnector.MySqlConnectorFactory,MySqlConnector",
                    ConnectionStrings = "server=localhost;database=Test;user=root;pwd=123456789;port=3306;min pool size=3;max pool size=100;connect timeout=30;"
                } });
        }

    }
}
