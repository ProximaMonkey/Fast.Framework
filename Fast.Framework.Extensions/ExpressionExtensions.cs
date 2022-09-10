using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Fast.Framework.Interfaces;
using Fast.Framework.Models;


namespace Fast.Framework.Extensions
{

    /// <summary>
    /// 表达式扩展类
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// 解析Sql缓存
        /// </summary>
        private static readonly ConcurrentDictionary<string, Lazy<ResolveSqlResult>> resolveSqlCache;

        /// <summary>
        /// 表达式类型映射
        /// </summary>
        private static readonly Dictionary<ExpressionType, string> expressionTypeMapping;

        /// <summary>
        /// 访问映射
        /// </summary>
        private static readonly Dictionary<DbType, Dictionary<string, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>>>> methodMapping;

        /// <summary>
        /// 构造方法
        /// </summary>
        static ExpressionExtensions()
        {
            resolveSqlCache = new ConcurrentDictionary<string, Lazy<ResolveSqlResult>>();
            expressionTypeMapping = new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.Add,"+" },
                { ExpressionType.Subtract,"-" },
                { ExpressionType.Multiply,"*" },
                { ExpressionType.Divide,"/" },
                { ExpressionType.Assign,"AS" },
                { ExpressionType.And,"AND" },
                { ExpressionType.AndAlso,"AND" },
                { ExpressionType.OrElse,"OR" },
                { ExpressionType.Or,"OR" },
                { ExpressionType.Equal,"=" },
                { ExpressionType.NotEqual,"<>" },
                { ExpressionType.GreaterThan,">" },
                { ExpressionType.LessThan,"<" },
                { ExpressionType.GreaterThanOrEqual,">=" },
                { ExpressionType.LessThanOrEqual,"<=" }
            };
            methodMapping = new Dictionary<DbType, Dictionary<string, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>>>>();

            var sqlserverFunc = new Dictionary<string, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>>>();
            var mysqlFunc = new Dictionary<string, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>>>();
            var oracleFunc = new Dictionary<string, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>>>();
            var pgsqlFunc = new Dictionary<string, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>>>();
            var sqliteFunc = new Dictionary<string, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>>>();

            #region SqlServer 函数

            #region 类型转换
            sqlserverFunc.Add("ToString", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 0)
                {
                    visit.Visit(method.Arguments[0]);
                }
                else
                {
                    visit.Visit(method.Object);
                }
                sqlStack.Push(",");
                sqlStack.Push("CONVERT( VARCHAR(255)");
            });

            sqlserverFunc.Add("ToDateTime", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                sqlStack.Push("CONVERT( DATETIME");
            });

            sqlserverFunc.Add("ToDecimal", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                sqlStack.Push("CONVERT( DECIMAL(10,6)");
            });

            sqlserverFunc.Add("ToDouble", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                sqlStack.Push("CONVERT( NUMERIC(10,6)");
            });

            sqlserverFunc.Add("ToSingle", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                sqlStack.Push("CONVERT( FLOAT");
            });

            sqlserverFunc.Add("ToInt32", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                sqlStack.Push("CONVERT( INT");
            });
            sqlserverFunc.Add("ToInt64", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                sqlStack.Push("CONVERT( BIGINT");
            });
            sqlserverFunc.Add("ToBoolean", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                sqlStack.Push("CONVERT( BIT");
            });
            sqlserverFunc.Add("ToChar", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                sqlStack.Push("CONVERT( CHAR(2)");
            });
            #endregion

            #region 聚合
            sqlserverFunc.Add("Max", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MAX");
            });

            sqlserverFunc.Add("Min", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MIN");
            });

            sqlserverFunc.Add("Count", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("COUNT");
            });

            sqlserverFunc.Add("Sum", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("SUM");
            });

            sqlserverFunc.Add("Avg", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("AVG");
            });
            #endregion

            #region 数学
            sqlserverFunc.Add("Abs", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ABS");
            });

            sqlserverFunc.Add("Round", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ROUND");
            });
            #endregion

            #region 字符串
            sqlserverFunc.Add("StartsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            sqlserverFunc.Add("EndsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            sqlserverFunc.Add("Contains", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            sqlserverFunc.Add("Substring", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("SUBSTRING");
            });

            sqlserverFunc.Add("Replace", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("REPLACE");
            });

            sqlserverFunc.Add("Len", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("LEN");
            });

            sqlserverFunc.Add("TrimStart", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LTRIM");
            });

            sqlserverFunc.Add("TrimEnd", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("RTRIM ");
            });

            sqlserverFunc.Add("ToUpper", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("UPPER");
            });

            sqlserverFunc.Add("ToLower", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LOWER");
            });

            #endregion

            #region 日期
            sqlserverFunc.Add("DateDiff", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[2]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                var constantExpression = method.Arguments[0] as ConstantExpression;
                sqlStack.Push(Convert.ToString(constantExpression.Value));
                sqlStack.Push("DATEDIFF( ");
            });

            sqlserverFunc.Add("AddYears", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("DATEADD( YEAR,");
            });

            sqlserverFunc.Add("AddMonths", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("DATEADD( MONTH,");
            });

            sqlserverFunc.Add("AddDays", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("DATEADD( DAY,");
            });

            sqlserverFunc.Add("AddHours", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("DATEADD( HOUR,");
            });

            sqlserverFunc.Add("AddMinutes", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("DATEADD( MINUTE,");
            });

            sqlserverFunc.Add("AddSeconds", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("DATEADD( SECOND,");
            });

            sqlserverFunc.Add("AddMilliseconds", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("DATEADD( MILLISECOND,");
            });

            sqlserverFunc.Add("Year", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("YEAR");
            });

            sqlserverFunc.Add("Month", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MONTH");
            });

            sqlserverFunc.Add("Day", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("DAY");
            });

            #endregion

            #region 查询
            sqlserverFunc.Add("In", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" IN ");
                visit.Visit(method.Arguments[0]);
            });

            sqlserverFunc.Add("NotIn", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" NOT IN ");
                visit.Visit(method.Arguments[0]);
            });
            #endregion

            #region 其它
            sqlserverFunc.Add("Equals", (visit, method, sqlStack) =>
            {
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" = ");
                visit.Visit(method.Object);
            });

            sqlserverFunc.Add("IsNull", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ISNULL ");
            });
            #endregion

            #endregion

            #region MySql 函数

            #region 类型转换
            mysqlFunc.Add("ToString", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" AS CHAR(510) )");
                if (method.Arguments.Count > 0)
                {
                    visit.Visit(method.Arguments[0]);
                }
                else
                {
                    visit.Visit(method.Object);
                }
                sqlStack.Push("CAST( ");
            });

            mysqlFunc.Add("ToDateTime", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" AS DATETIME )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("CAST( ");
            });

            mysqlFunc.Add("ToDecimal", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" AS DECIMAL(10,6) )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("CAST( ");
            });

            mysqlFunc.Add("ToDouble", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" AS DECIMAL(10,2) )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("CAST( ");
            });

            mysqlFunc.Add("ToInt32", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" AS DECIMAL(10) )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("CAST( ");
            });

            mysqlFunc.Add("ToInt64", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" AS DECIMAL(19) )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("CAST( ");
            });

            mysqlFunc.Add("ToBoolean", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" AS UNSIGNED )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("CAST( ");
                sqlStack.Push(" )");
            });

            mysqlFunc.Add("ToChar", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" AS CHAR(2) )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("CAST( ");
            });
            #endregion

            #region 聚合
            mysqlFunc.Add("Max", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MAX");
            });

            mysqlFunc.Add("Min", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MIN");
            });

            mysqlFunc.Add("Count", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("COUNT");
            });

            mysqlFunc.Add("Sum", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("SUM");
            });

            mysqlFunc.Add("Avg", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("AVG");
            });
            #endregion

            #region 数学
            mysqlFunc.Add("Abs", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ABS");
            });

            mysqlFunc.Add("Round", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ROUND");
            });
            #endregion

            #region 字符串
            mysqlFunc.Add("StartsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            mysqlFunc.Add("EndsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            mysqlFunc.Add("Contains", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            mysqlFunc.Add("Substring", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("SUBSTRING");
            });

            mysqlFunc.Add("Replace", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("REPLACE");
            });

            mysqlFunc.Add("Length", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("LENGTH");
            });

            mysqlFunc.Add("Trim", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("TRIM");
            });

            mysqlFunc.Add("TrimStart", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LTRIM");
            });

            mysqlFunc.Add("TrimEnd", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("RTRIM");
            });

            mysqlFunc.Add("ToUpper", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("UPPER");
            });

            mysqlFunc.Add("ToLower", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LOWER");
            });

            #endregion

            #region 日期
            mysqlFunc.Add("DateDiff", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[2]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("DATEDIFF( ");
            });

            sqlserverFunc.Add("TimestampDiff", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[2]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                var constantExpression = method.Arguments[0] as ConstantExpression;
                sqlStack.Push(Convert.ToString(constantExpression.Value));
                sqlStack.Push("TIMESTAMPDIFF( ");
            });

            mysqlFunc.Add("AddYears", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" YEAR )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("DATE_ADD( ");
            });

            mysqlFunc.Add("AddMonths", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" MONTH )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("DATE_ADD( ");
            });

            mysqlFunc.Add("AddDays", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" DAY )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("DATE_ADD( ");
            });

            mysqlFunc.Add("AddHours", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" HOUR )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("DATE_ADD( ");
            });

            mysqlFunc.Add("AddMinutes", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" MINUTE )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("DATE_ADD( ");
            });

            mysqlFunc.Add("AddSeconds", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" SECOND )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("DATE_ADD( ");
            });

            mysqlFunc.Add("AddMilliseconds", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("DATEADD( MINUTE_SECOND,");
            });

            mysqlFunc.Add("Year", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("YEAR");
            });

            mysqlFunc.Add("Month", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MONTH");
            });

            mysqlFunc.Add("Day", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("DAY");
            });

            #endregion

            #region 查询
            mysqlFunc.Add("In", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" IN ");
                visit.Visit(method.Arguments[0]);
            });

            mysqlFunc.Add("NotIn", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" NOT IN ");
                visit.Visit(method.Arguments[0]);
            });
            #endregion

            #region 其它
            mysqlFunc.Add("Equals", (visit, method, sqlStack) =>
            {
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" = ");
                visit.Visit(method.Object);
            });

            mysqlFunc.Add("IfNull", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("IFNULL");
            });
            #endregion

            #endregion

            #region Oracle 函数

            #region 聚合
            oracleFunc.Add("Max", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MAX");
            });

            oracleFunc.Add("Min", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MIN");
            });

            oracleFunc.Add("Count", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("COUNT");
            });

            oracleFunc.Add("Sum", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("SUM");
            });

            oracleFunc.Add("Avg", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("AVG");
            });
            #endregion

            #region 数学
            oracleFunc.Add("Abs", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ABS");
            });

            oracleFunc.Add("Round", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ROUND");
            });
            #endregion

            #region 字符串
            oracleFunc.Add("StartsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            oracleFunc.Add("EndsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            oracleFunc.Add("Contains", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            oracleFunc.Add("Substring", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("SUBSTR");
            });

            oracleFunc.Add("Replace", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("REPLACE");
            });

            oracleFunc.Add("Length", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("LENGTH");
            });

            oracleFunc.Add("TrimStart", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LTRIM");
            });

            oracleFunc.Add("TrimEnd", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("RTRIM");
            });

            oracleFunc.Add("ToUpper", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("UPPER");
            });

            oracleFunc.Add("ToLower", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LOWER");
            });

            #endregion

            #region 日期

            oracleFunc.Add("AddYears", (visit, method, sqlStack) =>
            {

            });

            oracleFunc.Add("AddMonths", (visit, method, sqlStack) =>
            {

            });

            oracleFunc.Add("AddDays", (visit, method, sqlStack) =>
            {

            });

            oracleFunc.Add("AddHours", (visit, method, sqlStack) =>
            {

            });

            oracleFunc.Add("AddMinutes", (visit, method, sqlStack) =>
            {

            });

            oracleFunc.Add("AddSeconds", (visit, method, sqlStack) =>
            {

            });

            oracleFunc.Add("AddMilliseconds", (visit, method, sqlStack) =>
            {

            });

            oracleFunc.Add("Year", (visit, method, sqlStack) =>
            {

            });

            oracleFunc.Add("Month", (visit, method, sqlStack) =>
            {

            });

            oracleFunc.Add("Day", (visit, method, sqlStack) =>
            {

            });

            #endregion

            #region 查询
            oracleFunc.Add("In", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" IN ");
                visit.Visit(method.Arguments[0]);
            });

            oracleFunc.Add("NotIn", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" NOT IN ");
                visit.Visit(method.Arguments[0]);
            });
            #endregion

            #region 其它
            oracleFunc.Add("Equals", (visit, method, sqlStack) =>
            {
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" = ");
                visit.Visit(method.Object);
            });

            oracleFunc.Add("Nvl", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("NVL ");
            });
            #endregion

            #endregion

            #region PostgreSQL 函数

            #region 聚合
            pgsqlFunc.Add("Max", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MAX");
            });

            pgsqlFunc.Add("Min", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MIN");
            });

            pgsqlFunc.Add("Count", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("COUNT");
            });

            pgsqlFunc.Add("Sum", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("SUM");
            });

            pgsqlFunc.Add("Avg", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("AVG");
            });
            #endregion

            #region 数学
            pgsqlFunc.Add("Abs", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ABS");
            });

            pgsqlFunc.Add("Round", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ROUND");
            });
            #endregion

            #region 字符串
            pgsqlFunc.Add("StartsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            pgsqlFunc.Add("EndsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            pgsqlFunc.Add("Contains", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            pgsqlFunc.Add("Substring", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("SUBSTRING");
            });

            pgsqlFunc.Add("Replace", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("REPLACE");
            });

            pgsqlFunc.Add("Length", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("LENGTH");
            });

            pgsqlFunc.Add("Trim", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("TRIM");
            });

            pgsqlFunc.Add("TrimStart", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LTRIM");
            });

            pgsqlFunc.Add("TrimEnd", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("RTRIM");
            });

            pgsqlFunc.Add("ToUpper", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("UPPER");
            });

            pgsqlFunc.Add("ToLower", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LOWER");
            });

            #endregion

            #region 日期

            pgsqlFunc.Add("AddYears", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" YEAR' )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'");
                sqlStack.Push("|| INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
            });

            pgsqlFunc.Add("AddMonths", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" MONTH' )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'");
                sqlStack.Push("|| INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
            });

            pgsqlFunc.Add("AddDays", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" DAY' )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'");
                sqlStack.Push("|| INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
            });

            pgsqlFunc.Add("AddHours", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" HOUR' )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'");
                sqlStack.Push("|| INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
            });

            pgsqlFunc.Add("AddMinutes", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" MINUTE' )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'");
                sqlStack.Push("|| INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
            });

            pgsqlFunc.Add("AddSeconds", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" SECOND' )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'");
                sqlStack.Push("|| INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
            });

            pgsqlFunc.Add("AddMilliseconds", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" MILLISECOND' )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'");
                sqlStack.Push("|| INTERVAL ");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
            });

            #endregion

            #region 查询
            pgsqlFunc.Add("In", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" IN ");
                visit.Visit(method.Arguments[0]);
            });

            pgsqlFunc.Add("NotIn", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" NOT IN ");
                visit.Visit(method.Arguments[0]);
            });
            #endregion

            #region 其它
            pgsqlFunc.Add("Equals", (visit, method, sqlStack) =>
            {
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" = ");
                visit.Visit(method.Object);
            });
            #endregion

            #endregion

            #region Sqlite 函数

            #region 聚合
            sqliteFunc.Add("Max", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MAX");
            });

            sqliteFunc.Add("Min", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("MIN");
            });

            sqliteFunc.Add("Count", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("COUNT");
            });

            sqliteFunc.Add("Sum", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("SUM");
            });

            sqliteFunc.Add("Avg", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("AVG");
            });
            #endregion

            #region 数学
            sqliteFunc.Add("Abs", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ABS");
            });

            sqliteFunc.Add("Round", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("ROUND");
            });
            #endregion

            #region 字符串
            sqliteFunc.Add("StartsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            sqliteFunc.Add("EndsWith", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            sqliteFunc.Add("Contains", (visit, method, sqlStack) =>
            {
                visit.SetTemplate("%{0}%");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" LIKE ");
                visit.Visit(method.Object);
            });

            sqliteFunc.Add("Substring", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                if (method.Arguments.Count > 1)
                {
                    visit.Visit(method.Arguments[1]);
                    sqlStack.Push(",");
                }
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("SUBSTRING");
            });

            sqliteFunc.Add("Replace", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("REPLACE");
            });

            sqliteFunc.Add("Length", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("LENGTH");
            });

            sqliteFunc.Add("Trim", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("TRIM");
            });

            sqliteFunc.Add("TrimStart", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LTRIM");
            });

            sqliteFunc.Add("TrimEnd", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("RTRIM");
            });

            sqliteFunc.Add("ToUpper", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("UPPER");
            });

            sqliteFunc.Add("ToLower", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("LOWER");
            });

            #endregion

            #region 日期

            sqliteFunc.Add("AddYears", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                sqlStack.Push(" YEAR'");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",'");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("DATETIME");
            });

            sqliteFunc.Add("AddMonths", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                sqlStack.Push(" MONTH'");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",'");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("DATETIME");
            });

            sqliteFunc.Add("AddDays", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                sqlStack.Push(" DAY'");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",'");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("DATETIME");
            });

            sqliteFunc.Add("AddHours", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                sqlStack.Push(" HOUR'");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",'");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("DATETIME");
            });

            sqliteFunc.Add("AddMinutes", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                sqlStack.Push(" MINUTE'");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",'");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("DATETIME");
            });

            sqliteFunc.Add("AddSeconds", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                sqlStack.Push(" SECOND'");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(",'");
                visit.Visit(method.Object);
                sqlStack.Push("( ");
                sqlStack.Push("DATETIME");
            });

            sqliteFunc.Add("Year", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'%Y',");
                sqlStack.Push("( ");
                sqlStack.Push("STRFTIME");
            });

            sqliteFunc.Add("Month", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'%m',");
                sqlStack.Push("( ");
                sqlStack.Push("STRFTIME");
            });

            sqliteFunc.Add("Day", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("'%j',");
                sqlStack.Push("( ");
                sqlStack.Push("STRFTIME");
            });

            #endregion

            #region 查询
            sqliteFunc.Add("In", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" IN ");
                visit.Visit(method.Arguments[0]);
            });

            sqliteFunc.Add("NotIn", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push("( ");
                sqlStack.Push(" NOT IN ");
                visit.Visit(method.Arguments[0]);
            });
            #endregion

            #region 其它
            sqliteFunc.Add("Equals", (visit, method, sqlStack) =>
            {
                visit.Visit(method.Arguments[0]);
                sqlStack.Push(" = ");
                visit.Visit(method.Object);
            });

            sqliteFunc.Add("IfNull", (visit, method, sqlStack) =>
            {
                sqlStack.Push(" )");
                visit.Visit(method.Arguments[1]);
                sqlStack.Push(",");
                visit.Visit(method.Arguments[0]);
                sqlStack.Push("( ");
                sqlStack.Push("IFNULL");
            });
            #endregion

            #endregion

            methodMapping.Add(DbType.SQLServer, sqlserverFunc);
            methodMapping.Add(DbType.MySQL, mysqlFunc);
            methodMapping.Add(DbType.Oracle, oracleFunc);
            methodMapping.Add(DbType.PostgreSQL, pgsqlFunc);
            methodMapping.Add(DbType.SQLite, sqliteFunc);
        }

        /// <summary>
        /// 添加Sql函数
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="action">委托</param>
        public static void AddSqlFunc(this DbType dbType, string methodName, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>> action)
        {
            if (!methodMapping.ContainsKey(dbType))
            {
                methodMapping.Add(dbType, new Dictionary<string, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>>>());//初始化类型
            }
            dbType.MethodMapping().Add(methodName, action);
        }

        /// <summary>
        /// 表达式类型映射
        /// </summary>
        /// <param name="expressionType">表达式类型</param>
        /// <returns></returns>
        public static string ExpressionTypeMapping(this ExpressionType expressionType)
        {
            return expressionTypeMapping[expressionType];
        }

        /// <summary>
        /// 方法映射
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <returns></returns>
        public static Dictionary<string, Action<ExpressionResolveSql, MethodCallExpression, Stack<string>>> MethodMapping(this DbType dbType)
        {
            return methodMapping[dbType];
        }

        /// <summary>
        /// 解析Sql
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public static ResolveSqlResult ResolveSql(this Expression expression, ResolveSqlOptions options)
        {
            var cacheKey = expression.ToString();
            var isAdd = false;// 是否添加标识
            var result = resolveSqlCache.GetOrAdd(cacheKey, key => new Lazy<ResolveSqlResult>(() =>
            {
                var expressionResolve = new ExpressionResolveSql(options);
                expressionResolve.Visit(expression);
                var result = expressionResolve.Build();
                isAdd = true;
                return result;
            })).Value;
            if (isAdd || result.Cache.MemberInfoCache.Count == 0)
            {
                return result;
            }
            // 动态更新参数逻辑
            var newResult = new ResolveSqlResult();
            newResult.SqlString = result.Cache.Data;

            var visitCompilerVar = new VisitCompilerVar();
            visitCompilerVar.Visit(expression);
            foreach (var item in result.Cache.MemberInfoCache)
            {
                object value = null;
                if (visitCompilerVar.CompilerVars.ContainsKey(item.Key))
                {
                    value = visitCompilerVar.CompilerVars[item.Key];//获取目标对象
                    value = item.Value.MemberInfos.GetValue(value, out var key);//获取成员变量值
                }
                if (value is IQuery)
                {
                    var subQuery = value as IQuery;
                    newResult.DbParameters.Append(subQuery.QueryBuilder.DbParameters);
                    newResult.SqlString = result.SqlString.Replace(item.Value.ParameterName, subQuery.QueryBuilder.ToSql());
                }
                else if (value is IList)
                {
                    var list = value as IList;
                    var names = item.Value.ParameterName.Split(",");

                    var parameterName = Guid.NewGuid().ToString().Replace("-", "");
                    var parNames = new List<string>();
                    for (int i = 0; i <= list.Count - 1; i++)
                    {
                        var newName = $"{parameterName}_{i}";
                        parNames.Add(newName);
                        newResult.DbParameters.Add(newName, list[i]);
                    }

                    //替换新的参数名称
                    var oldNames = string.Join(",", item.Value.ParameterName.Split(",").Select(s => $"{options.DbType.MappingParameterSymbol()}{s}"));
                    var newNames = string.Join(",", parNames.Select(s => $"{options.DbType.MappingParameterSymbol()}{s}"));
                    newResult.SqlString = result.SqlString.Replace(oldNames, newNames);
                }
                else
                {
                    if (item.Value.TargetType.Equals(typeof(DateTime)))
                    {
                        value = DateTime.Now;
                    }
                    newResult.DbParameters.Add(item.Value.ParameterName, string.IsNullOrWhiteSpace(item.Value.Template) ? value : string.Format(item.Value.Template, value));
                }
            }
            return newResult;
        }

    }

    #region 解析核心实现

    /// <summary>
    /// 访问编译器变量
    /// </summary>
    public class VisitCompilerVar : ExpressionVisitor
    {

        /// <summary>
        /// 编译器变量
        /// </summary>
        public Dictionary<string, object> CompilerVars { get; }

        /// <summary>
        /// 数组索引
        /// </summary>
        private readonly Stack<int> arrayIndexs;

        /// <summary>
        /// 成员信息
        /// </summary>
        private readonly Stack<MemberInfo> memberInfos;

        /// <summary>
        /// 构造方法
        /// </summary>
        public VisitCompilerVar()
        {
            CompilerVars = new Dictionary<string, object>();
            arrayIndexs = new Stack<int>();
            memberInfos = new Stack<MemberInfo>();
        }

        /// <summary>
        /// 访问表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda(node);
        }

        /// <summary>
        /// 访问二元表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            #region 解析数组索引访问
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                var index = Convert.ToInt32((node.Right as ConstantExpression).Value);
                arrayIndexs.Push(index);
                return Visit(node.Left);
            }
            #endregion

            Visit(node.Right);

            Visit(node.Left);

            return node;
        }

        /// <summary>
        /// 访问成员表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null)
            {
                if (node.Expression.NodeType == ExpressionType.MemberAccess || node.Expression.NodeType == ExpressionType.Constant)
                {
                    memberInfos.Push(node.Member);
                }
            }
            return base.VisitMember(node);
        }

        /// <summary>
        /// 访问常量表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (memberInfos.Count > 0)
            {
                var caceKey = Convert.ToString(node.Value);
                foreach (var item in memberInfos)
                {
                    caceKey += $".{item.Name}_{string.Join("_", arrayIndexs)}";
                }
                if (!CompilerVars.ContainsKey(caceKey))
                {
                    CompilerVars.Add(caceKey, node.Value);
                }
                memberInfos.Clear();
                arrayIndexs.Clear();
            }
            return node;
        }
    }

    /// <summary>
    /// 表达式解析Sql
    /// </summary>
    public class ExpressionResolveSql : ExpressionVisitor
    {

        /// <summary>
        /// 子查询Sql
        /// </summary>
        private readonly Dictionary<string, string> subQuerySql;

        /// <summary>
        /// 缓存
        /// </summary>
        private ExpressionCache<string> cache;

        /// <summary>
        /// 选项
        /// </summary>
        private readonly ResolveSqlOptions options;

        /// <summary>
        /// Sql堆
        /// </summary>
        private Stack<string> sqlStack;

        /// <summary>
        /// 数据库参数
        /// </summary>
        private Dictionary<string, object> dbParameters;

        /// <summary>
        /// 成员信息
        /// </summary>
        private Stack<MemberInfoEx> memberInfos;

        /// <summary>
        /// 访问二元表达式
        /// </summary>
        private bool visitBinary;

        /// <summary>
        /// 数组索引
        /// </summary>
        private Stack<int> arrayIndexs;

        /// <summary>
        /// 模板
        /// </summary>
        private string template;

        /// <summary>
        /// 构造方法
        /// </summary>
        public ExpressionResolveSql(ResolveSqlOptions options)
        {
            this.options = options;
            subQuerySql = new Dictionary<string, string>();
            cache = new ExpressionCache<string>();
            sqlStack = new Stack<string>();
            dbParameters = new Dictionary<string, object>();
            memberInfos = new Stack<MemberInfoEx>();
            arrayIndexs = new Stack<int>();
        }

        /// <summary>
        /// 设置模板
        /// </summary>
        /// <param name="template">模板</param>
        public void SetTemplate(string template)
        {
            this.template = template;
        }

        /// <summary>
        /// 访问
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("node")]
        public override Expression Visit(Expression node)
        {
            //Console.WriteLine($"当前访问 {node.NodeType} 类型表达式");
            return base.Visit(node);
        }

        /// <summary>
        /// 访问Lambda表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.Visit(node.Body);
        }

        /// <summary>
        /// 访问参数表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (!options.IgnoreParameter)
            {
                if (!string.IsNullOrWhiteSpace(node.Name))
                {
                    sqlStack.Push($"{node.Name}.");
                }
            }
            return base.VisitParameter(node);
        }

        /// <summary>
        /// 访问一元表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (options.ResolveSqlType == ResolveSqlType.Where && node.Type.Equals(typeof(bool)))
            {
                sqlStack.Push(options.DbType == DbType.PostgreSQL ? " = FALSE" : " = 0");
            }
            return base.VisitUnary(node);
        }

        /// <summary>
        /// 访问二元表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            visitBinary = true;

            #region 解析数组索引访问
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                var index = Convert.ToInt32((node.Right as ConstantExpression).Value);
                arrayIndexs.Push(index);
                return Visit(node.Left);
            }
            #endregion

            sqlStack.Push(" )");

            #region Boolean 类型特殊处理
            if (options.ResolveSqlType == ResolveSqlType.Where && node.Right is not BinaryExpression && node.Right.Type.Equals(typeof(bool)) && node.Right.NodeType != ExpressionType.Not
    && node.Right.NodeType != ExpressionType.Call && node.NodeType != ExpressionType.Equal && node.NodeType != ExpressionType.NotEqual)
            {
                if (options.DbType == DbType.PostgreSQL)
                {
                    sqlStack.Push(" = TRUE");
                }
                else
                {
                    sqlStack.Push(" = 1");
                }
            }
            #endregion

            Visit(node.Right);

            var op = node.NodeType.ExpressionTypeMapping();

            sqlStack.Push($" {op} ");

            #region Boolean 类型特殊处理
            if (options.ResolveSqlType == ResolveSqlType.Where && node.Left is not BinaryExpression && node.Left.Type.Equals(typeof(bool)) && node.Left.NodeType != ExpressionType.Not
    && node.Left.NodeType != ExpressionType.Call && node.NodeType != ExpressionType.Equal && node.NodeType != ExpressionType.NotEqual)
            {
                if (options.DbType == DbType.PostgreSQL)
                {
                    sqlStack.Push(" = TRUE");
                }
                else
                {
                    sqlStack.Push(" = 1");
                }
            }
            #endregion

            Visit(node.Left);

            sqlStack.Push("( ");

            return node;
        }

        /// <summary>
        /// 访问方法表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "SubQuery")
            {
                sqlStack.Push(" )");
                Visit(node.Arguments[0]);
                sqlStack.Push("( ");
            }
            else
            {
                var delegates = options.DbType.MethodMapping();
                if (delegates.ContainsKey(node.Method.Name))
                {
                    delegates[node.Method.Name].Invoke(this, node, sqlStack);
                    template = "";
                }
                else
                {
                    throw new NotImplementedException($"Not Implemented {node.Method.Name} Method.");
                }
            }
            return node;
        }

        /// <summary>
        /// 访问条件表达式树
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            sqlStack.Push(" END");
            Visit(node.IfFalse);
            sqlStack.Push(" ELSE ");
            Visit(node.IfTrue);
            sqlStack.Push(" THEN ");
            Visit(node.Test);
            sqlStack.Push("CASE WHEN ");
            return node;
        }

        /// <summary>
        /// 访问对象表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Type.Name.StartsWith("<>f__AnonymousType"))
            {
                for (int i = node.Members.Count - 1; i >= 0; i--)
                {
                    if (options.ResolveSqlType == ResolveSqlType.Select)
                    {
                        var columnAuttribute = typeof(ColumnAttribute);
                        var name = node.Members[i].IsDefined(columnAuttribute) ? node.Members[i].GetCustomAttribute<ColumnAttribute>().Name : node.Members[i].Name;
                        sqlStack.Push($"{options.DbType.MappingIdentifier().Insert(1, name)}");
                        sqlStack.Push(" AS ");
                    }
                    Visit(node.Arguments[i]);
                    if (i > 0)
                    {
                        sqlStack.Push(",");
                    }
                }
            }
            return node;
        }

        /// <summary>
        /// 访问成员初始化表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            for (int i = node.Bindings.Count - 1; i >= 0; i--)
            {
                if (node.Bindings[i].BindingType == MemberBindingType.Assignment)
                {
                    var memberAssignment = node.Bindings[i] as MemberAssignment;
                    if (options.ResolveSqlType == ResolveSqlType.Select)
                    {
                        var columnAuttribute = typeof(ColumnAttribute);
                        var name = memberAssignment.Member.IsDefined(columnAuttribute) ? memberAssignment.Member.GetCustomAttribute<ColumnAttribute>().Name : memberAssignment.Member.Name;
                        sqlStack.Push($"{options.DbType.MappingIdentifier().Insert(1, name)}");
                        sqlStack.Push(" AS ");
                    }
                    Visit(memberAssignment.Expression);
                    if (i > 0)
                    {
                        sqlStack.Push(",");
                    }
                }
            }
            return node;
        }

        /// <summary>
        /// 访问对象数组表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            for (int i = node.Expressions.Count - 1; i >= 0; i--)
            {
                Visit(node.Expressions[i]);
                if (i > 0)
                {
                    sqlStack.Push(",");
                }
            }
            return node;
        }

        /// <summary>
        /// 访问列表初始化表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitListInit(ListInitExpression node)
        {
            if (node.CanReduce)
            {
                var blockExpression = node.Reduce() as BlockExpression;
                var expressions = blockExpression.Expressions.Skip(1).SkipLast(1).ToList();
                for (int i = expressions.Count - 1; i >= 0; i--)
                {
                    var methodCallExpression = expressions[i] as MethodCallExpression;
                    foreach (var item in methodCallExpression.Arguments)
                    {
                        Visit(item);
                    }
                    if (i > 0)
                    {
                        sqlStack.Push(",");
                    }
                }
            }
            return node;
        }

        /// <summary>
        /// 访问成员表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            #region Datetime特殊处理
            if (node.Type.Equals(typeof(DateTime)))
            {
                if (node.Expression == null)
                {
                    memberInfos.Push(new MemberInfoEx()
                    {
                        ArrayIndex = arrayIndexs,
                        Member = node.Member
                    });
                    return VisitConstant(Expression.Constant(DateTime.Now));
                }
            }
            if (node.Member.DeclaringType.Equals(typeof(DateTime)))
            {
                sqlStack.Push(" )");
                Visit(node.Expression);
                sqlStack.Push("( ");
                sqlStack.Push($"{node.Member.Name.ToUpper()}");
                return node;
            }
            #endregion

            #region Boolean特殊处理
            if (options.ResolveSqlType == ResolveSqlType.Where && node.Type.Equals(typeof(bool)) && visitBinary == false)
            {
                if (options.DbType == DbType.PostgreSQL)
                {
                    sqlStack.Push(" = TRUE");
                }
                else
                {
                    sqlStack.Push(" = 1");
                }
            }
            #endregion

            if (node.Expression != null)
            {
                if (node.Expression.NodeType == ExpressionType.Parameter)
                {
                    var name = node.Member.IsDefined(typeof(ColumnAttribute)) ? node.Member.GetCustomAttribute<ColumnAttribute>().Name : node.Member.Name;
                    if (options.IgnoreIdentifier)
                    {
                        sqlStack.Push(name);
                    }
                    else
                    {
                        sqlStack.Push(options.DbType.MappingIdentifier().Insert(1, name));
                    }
                }
                else if (node.Expression.NodeType == ExpressionType.MemberAccess || node.Expression.NodeType == ExpressionType.Constant)
                {
                    memberInfos.Push(new MemberInfoEx()
                    {
                        ArrayIndex = arrayIndexs,
                        Member = node.Member
                    });
                }
            }
            if (arrayIndexs.Count > 0)
            {
                arrayIndexs = new Stack<int>();
            }
            return base.VisitMember(node);
        }

        /// <summary>
        /// 访问常量表达式
        /// </summary>
        /// <param name="node">节点</param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value;
            if (memberInfos.Count > 0)
            {
                value = memberInfos.GetValue(value, out var cacheKey);//获取成员变量值

                var parameterName = Guid.NewGuid().ToString().Replace("-", "");
                if (cache.MemberInfoCache.ContainsKey(cacheKey))//如果已经存在缓存直接从缓存中取参数名称
                {
                    var names = cache.MemberInfoCache[cacheKey].ParameterName.Split(",");
                    sqlStack.Push(string.Join(",", names.Select(s => $"{options.DbType.MappingParameterSymbol()}{s}")));
                }
                else
                {
                    if (value is IQuery)//子查询处理
                    {
                        var subQuery = value as IQuery;

                        dbParameters.Append(subQuery.QueryBuilder.DbParameters);
                        subQuerySql.Add(parameterName, subQuery.QueryBuilder.ToSql());

                        sqlStack.Push(parameterName);
                    }
                    else if (value is IList)//数组和列表集合处理
                    {
                        var list = value as IList;
                        var parNames = new List<string>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            var newName = $"{parameterName}_{i}";
                            parNames.Add(newName);
                            dbParameters.Add(newName, list[i]);
                        }
                        sqlStack.Push(string.Join(",", parNames.Select(s => $"{options.DbType.MappingParameterSymbol()}{s}")));
                        parameterName = string.Join(",", parNames);
                    }
                    else//普通成员变量处理
                    {
                        dbParameters.Add(parameterName, FromatTemplate(value));
                        sqlStack.Push($"{options.DbType.MappingParameterSymbol()}{parameterName}");
                    }
                    cache.MemberInfoCache.Add(cacheKey, new ObjMemberInfo()
                    {
                        TargetType = node.Type,
                        Target = node.Value,
                        ParameterName = parameterName,
                        MemberInfos = memberInfos,
                        Template = template
                    });
                }

                memberInfos = new Stack<MemberInfoEx>();
            }
            else
            {
                if (node.Type.Equals(typeof(bool)))
                {
                    if (sqlStack.Count == 0)
                    {
                        value = Convert.ToInt32(value);
                        value = $"{value} = 1";
                    }
                    else if (options.DbType != DbType.PostgreSQL)
                    {
                        value = Convert.ToInt32(value);
                    }
                }
                value = AddQuotes(node.Type, FromatTemplate(value));
                sqlStack.Push(Convert.ToString(value));
            }
            return base.VisitConstant(node);
        }

        /// <summary>
        /// 格式化模板
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        private object FromatTemplate(object value)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return value;
            }
            return string.Format(template, value);
        }

        /// <summary>
        /// 添加引号
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private static object AddQuotes(Type type, object value)
        {
            if (type.IsValueType && !type.Equals(typeof(DateTime)))
            {
                return value;
            }
            return $"'{value}'";
        }

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns></returns>
        public ResolveSqlResult Build()
        {
            var result = new ResolveSqlResult();
            result.SqlString = string.Join("", sqlStack);
            foreach (var item in subQuerySql)
            {
                result.SqlString = result.SqlString.Replace(item.Key, item.Value);
            }
            result.DbParameters = dbParameters;
            cache.Data = string.Join("", sqlStack);
            result.Cache = cache;

            //初始化
            sqlStack = new Stack<string>();
            dbParameters = new Dictionary<string, object>();
            cache = new ExpressionCache<string>();
            subQuerySql.Clear();
            return result;
        }
    }
    #endregion
}

