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
    public class Delete
    {
        /// <summary>
        /// 数据库对象
        /// </summary>
        private readonly IDbContext db;

        /// <summary>
        /// 构造方法
        /// </summary>
        public Delete()
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

        /// <summary>
        /// 实体对象删除
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task EntityDeleteAsync()
        {
            var product = new Product()
            {
                ProductId = 1,
                ProductCode = "1001",
                ProductName = "测试商品1"
            };
            var result = await db.Delete(product).ExceuteAsync();
            Console.WriteLine($"实体删除 受影响行数 {result}");
        }

        /// <summary>
        /// 无条件删除
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task NotWhereDeleteAsync()
        {
            var result = await db.Delete<Product>().ExceuteAsync();
            Console.WriteLine($"无条件删除 受影响行数 {result}");
        }

        /// <summary>
        /// 条件删除
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task WhereDeleteAsync()
        {
            var result = await db.Delete<Product>().Where(w => w.ProductId == 1).ExceuteAsync();
            Console.WriteLine($"条件删除 受影响行数 {result}");
        }

        /// <summary>
        /// 特殊删除
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SpecialDeleteAsync()
        {
            var result = await db.Delete<object>().As("Product").WhereColumn("ProductId", 1001).ExceuteAsync();
            Console.WriteLine($"无实体删除 受影响行数 {result}");
        }

    }
}
