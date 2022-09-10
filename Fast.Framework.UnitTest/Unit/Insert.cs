

namespace Fast.Framework
{

    /// <summary>
    /// 插入
    /// </summary>
    [TestClass]
    public class Insert
    {

        /// <summary>
        /// 数据库
        /// </summary>
        private readonly IDbContext db;

        /// <summary>
        /// 构造方法
        /// </summary>
        public Insert()
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
        /// 实体对象异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task EntityAsync()
        {
            var product = new Product()
            {
                ProductCode = "1001",
                ProductName = "测试产品1"
            };
            var result = await db.Insert(product).ExceuteAsync();
            Console.WriteLine($"实体对象插入 受影响行数 {result}");
            Assert.IsTrue(result == 1);
        }

        /// <summary>
        /// 返回自增ID
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ReturnIdentityAsync()
        {
            var product = new Product()
            {
                ProductCode = "1001",
                ProductName = "测试产品1"
            };
            var result = await db.Insert(product).ExceuteReturnIdentityAsync();
            Console.WriteLine($"实体对象插入 返回自增ID {result}");
            Assert.IsTrue(result >= 0);
        }

        /// <summary>
        /// 实体对象列表异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task EntityListAsync()
        {
            var list = new List<Product>();
            for (int i = 0; i < 2100; i++)
            {
                list.Add(new Product()
                {
                    ProductCode = $"编号:{i + 1}",
                    ProductName = $"名称:{i + 1}"
                });
            }
            var result = await db.Insert(list).ExceuteAsync();
            Console.WriteLine($"实体对象列表插入 受影响行数  {result}");
            Assert.IsTrue(result == 2100);
        }

        /// <summary>
        /// 匿名对象异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AnonymousObjAsync()
        {
            var obj = new
            {
                ProductCode = "1001",
                ProductName = "测试产品1"
            };
            //注意:需要使用As方法显示指定表名称
            var result = await db.Insert(obj).As("product").ExceuteAsync();
            Console.WriteLine($"匿名对象插入 受影响行数 {result}");
            Assert.IsTrue(result == 1);
        }

        /// <summary>
        /// 匿名对象列表异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AnonymousObjListAsync()
        {
            var list = new List<object>();
            for (int i = 0; i < 2100; i++)
            {
                list.Add(new
                {
                    ProductCode = $"测试产品编号{i + 1}",
                    ProductName = $"测试产品名称{i + 1}"
                });
            }
            //注意:需要使用As方法显示指定表名称
            var result = await db.Insert(list).As("Product").ExceuteAsync();
            Console.WriteLine($"匿名对象列表插入 受影响行数 {result}");
            Assert.IsTrue(result == 2100);
        }

        /// <summary>
        /// 字典异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DictionaryAsync()
        {
            var product = new Dictionary<string, object>()
            {
                {"ProductCode","1001"},
                { "ProductName","测试产品1"}
            };
            //注意:需要显示指定泛型类型
            var result = await db.Insert<Product>(product).ExceuteAsync();
            Console.WriteLine($"字典插入 受影响行数 {result}");
            Assert.IsTrue(result == 1);
        }

        /// <summary>
        /// 字典列表异步
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DictionaryListAsync()
        {
            var list = new List<Dictionary<string, object>>();
            for (int i = 0; i < 2100; i++)
            {
                list.Add(new Dictionary<string, object>()
                {
                    {"ProductCode","1001"},
                    { "ProductName","测试产品1"}
                 });
            }
            //注意:需要显示指定泛型类型
            var result = await db.Insert<Product>(list).ExceuteAsync();
            Console.WriteLine($"字典列表插入 受影响行数 {result}");
            Assert.IsTrue(result == 2100);
        }


    }
}