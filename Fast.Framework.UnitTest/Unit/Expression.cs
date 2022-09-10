using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Framework.UnitTest.Unit
{

    /// <summary>
    /// 表达式
    /// </summary>
    [TestClass]
    public class Expression
    {

        /// <summary>
        /// 选项
        /// </summary>
        private readonly ResolveSqlOptions options = new ResolveSqlOptions() { DbType = Models.DbType.MySQL };


        /// <summary>
        /// 布尔测试1
        /// </summary>
        [TestMethod]
        public void BoolTest1()
        {
            Expression<Func<Product, bool>> ex = p => true;
            var result = ex.ResolveSql(options);
            Console.WriteLine(result.SqlString);
            Assert.AreEqual(result.SqlString, "1 = 1");
        }

        /// <summary>
        /// 布尔测试2
        /// </summary>
        [TestMethod]
        public void BoolTest2()
        {
            Expression<Func<Product, bool>> ex = p => true || p.DeleteMark || !p.DeleteMark || p.DeleteMark == true || p.DeleteMark == false || false;
            var result = ex.ResolveSql(options);
            Console.WriteLine(result.SqlString);
            Assert.AreEqual(result.SqlString, "( ( ( ( ( 1 = 1 OR p.`DeleteMark` = 1 ) OR p.`DeleteMark` = 0 ) OR ( p.`DeleteMark` = 1 ) ) OR ( p.`DeleteMark` = 0 ) ) OR 0 = 1 )");
        }

        [TestMethod]
        public void Conditional1()
        {
            Expression<Func<Product, object>> ex = p => p.ProductId == 1 ? "测试1" : "测试2";
            var result = ex.ResolveSql(options);
            Console.WriteLine(result.SqlString);
            Assert.AreEqual(result.SqlString, "CASE WHEN ( p.`ProductId` = 1 ) THEN '测试1' ELSE '测试2' END");
        }
    }
}
