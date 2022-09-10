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
    public class Update
    {
        /// <summary>
        /// 数据库对象
        /// </summary>
        private readonly IDbContext db;

        /// <summary>
        /// 构造方法
        /// </summary>
        public Update()
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
        /// 实体更新异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task EntityUpdateAsync()
        {
            var product = new Product()
            {
                ProductId = 1,
                ProductCode = "1001",
                ProductName = "测试商品1"
            };
            var result = await db.Update(product).ExceuteAsync();
            Console.WriteLine($"对象更新 受影响行数 {result}");
        }

        /// <summary>
        /// 实体列表更新异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task EntityListUpdateAsync()
        {
            var list = new List<Product>();
            for (int i = 0; i < 2022; i++)
            {
                list.Add(new Product()
                {
                    ProductCode = $"编号{i + 1}",
                    ProductName = $"名称{i + 1}"
                });
            }
            var result = await db.Update(list).ExceuteAsync();
            Console.WriteLine($"对象列表更新 受影响行数 {result}");
        }

        /// <summary>
        /// 匿名对象更新
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AnonymousObjAsync()
        {
            var obj = new
            {
                ProductId = 1,
                ProductCode = "1001",
                ProductName = "测试商品1"
            };
            var result = await db.Update(obj).As("product").WhereColumns("ProductId").ExceuteAsync();
            Console.WriteLine($"匿名对象更新 受影响行数 {result}");
        }

        /// <summary>
        /// 匿名对象列表更新
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AnonymousObjListAsync()
        {
            var list = new List<object>();
            for (int i = 0; i < 2022; i++)
            {
                list.Add(new
                {
                    ProductId = i + 1,
                    ProductCode = $"编号{i + 1}",
                    ProductName = $"名称{i + 1}"
                });
            }
            var result = await db.Update(list).As("Product").WhereColumns("ProductId").ExceuteAsync();
            Console.WriteLine($"匿名对象列表更新 受影响行数 {result}");
        }

        /// <summary>
        /// 字典更新异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DictionaryUpdateAsync()
        {
            var product = new Dictionary<string, object>()
            {
                { "ProductId",1},
                {"ProductCode","1001"},
                { "ProductName","测试商品1"}
            };
            var result = await db.Update<Product>(product).WhereColumns("ProductId").ExceuteAsync();
            Console.WriteLine($"字典更新 受影响行数 {result}");
        }

        /// <summary>
        /// 字典列表更新异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DictionaryListUpdateAsync()
        {
            var list = new List<Dictionary<string, object>>();
            for (int i = 0; i < 2022; i++)
            {
                list.Add(new Dictionary<string, object>()
                {
                    { "ProductId",i+1},
                    {"ProductCode",$"更新编号:{i+1}"},
                    { "ProductName",$"更新商品:{i + 1}"}
                });
            }
            var result = await db.Update<Product>(list).WhereColumns("ProductId").ExceuteAsync();
            Console.WriteLine($"字典列表更新 受影响行数 {result}");
        }

        /// <summary>
        /// 表达式更新异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task LambdaUpdateAsync()
        {
            var product = new Product()
            {
                ProductId = 1,
                ProductCode = "1001",
                ProductName = "测试商品1"
            };
            var result = await db.Update(product).Where(p => p.ProductId == 100).ExceuteAsync();
            Console.WriteLine($"表达式更新 受影响行数 {result}");
        }


    }
}
