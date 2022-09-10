using System;
using System.IO;
using System.Data;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Transactions;
using System.Data.Common;
using Microsoft.Extensions.Options;
using Fast.Framework.Interfaces;
using Fast.Framework.Extensions;
using Fast.Framework.Utils;
using Fast.Framework.Models;
using Fast.Framework.Aop;
using System.Threading;

namespace Fast.Framework.Test
{

    public class Program
    {

        static async Task Main(string[] args)
        {
            try
            {
                //// 原生Ado
                //IAdo ado = new AdoProvider(new DbOptions()
                //{
                //    DbId = "1",
                //    DbType = Models.DbType.MySQL,
                //    ProviderName = "MySqlConnector",
                //    FactoryName = "MySqlConnector.MySqlConnectorFactory,MySqlConnector",
                //    ConnectionStrings = "server=localhost;database=Test;user=root;pwd=123456789;port=3306;min pool size=3;max pool size=100;connect timeout=30;"
                //});

                // 数据库上下文
                IDbContext db = new DbContext(new List<DbOptions>() {
                new DbOptions()
                {
                    DbId = "2",
                    DbType = Models.DbType.MySQL,
                    ProviderName = "MySqlConnector",
                    FactoryName = "MySqlConnector.MySqlConnectorFactory,MySqlConnector",
                    ConnectionStrings = "server=localhost;database=Test;user=root;pwd=123456789;port=3306;min pool size=3;max pool size=100;connect timeout=30;"
                }});

                //var list = new List<Product>();
                //for (int i = 1; i <= 10000; i++)
                //{
                //    list.Add(new Product()
                //    {
                //        ProductCode = $"测试产品编号:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        ProductName = $"测试产品名称:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        CreateTime = DateTime.Now,
                //        DeleteMark = false,
                //        Custom1 = $"测试自定义1:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom2 = $"测试自定义2:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom3 = $"测试自定义3:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom4 = $"测试自定义4:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom5 = $"测试自定义5:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom6 = $"测试自定义6:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom7 = $"测试自定义7:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom8 = $"测试自定义8:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom9 = $"测试自定义9:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom10 = $"测试自定义10:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom11 = $"测试自定义11:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //        Custom12 = $"测试自定义12:{i}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                //    });
                //}

                //var result = await db.Insert(list).ExceuteAsync();
                //Console.WriteLine($"插入数据 {result} 条");

                //for (int i = 0; i < 10; i++)
                //{
                //    var stopwatch1 = new Stopwatch();
                //    stopwatch1.Start();
                //    var data = await db.Query<Product>().ToListAsync();
                //    stopwatch1.Stop();
                //    //Console.WriteLine(Json.Serialize(data.Take(2)));
                //    Console.WriteLine($"查询总数:{data.Count} 耗时:{stopwatch1.ElapsedMilliseconds}ms {stopwatch1.ElapsedMilliseconds / 1000.00}s");
                //}


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }
}