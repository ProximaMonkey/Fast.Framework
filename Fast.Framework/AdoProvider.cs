using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Fast.Framework.Interfaces;
using Fast.Framework.Extensions;
using Fast.Framework.Models;


namespace Fast.Framework
{

    /// <summary>
    /// Ado实现类
    /// </summary>
    public class AdoProvider : IAdo
    {

        /// <summary>
        /// 数据库提供者工厂
        /// </summary>
        public DbProviderFactory DbProviderFactory { get; }

        /// <summary>
        /// 数据库选项
        /// </summary>
        public DbOptions DbOptions { get; }

        /// <summary>
        /// 连接对象
        /// </summary>
        private readonly DbConnection conn;

        /// <summary>
        /// 执行对象
        /// </summary>
        private readonly DbCommand cmd;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dbOptions">数据库选项</param>
        public AdoProvider(IOptionsSnapshot<DbOptions> dbOptions) : this(dbOptions.Value)
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="dbOptions">数据选项</param>
        public AdoProvider(DbOptions dbOptions)
        {
            DbOptions = dbOptions;
            DbProviderFactories.RegisterFactory(DbOptions.ProviderName, DbOptions.FactoryName);
            DbProviderFactory = DbProviderFactories.GetFactory(DbOptions.ProviderName);
            conn = DbProviderFactory.CreateConnection();
            cmd = conn.CreateCommand();
            conn.ConnectionString = DbOptions.ConnectionStrings;
            if (DbOptions.DbType == Models.DbType.Oracle)
            {
                cmd.GetType().GetProperty("BindByName").SetValue(cmd, true);
            }
        }

        /// <summary>
        /// 开启事务异步
        /// </summary>
        public async Task BeginTranAsync()
        {
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            cmd.Transaction = await conn.BeginTransactionAsync();
        }

        /// <summary>
        /// 提交事务异步
        /// </summary>
        public async Task CommitTranAsync()
        {
            await cmd.Transaction.CommitAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        /// 回滚事务异步
        /// </summary>
        /// <returns></returns>
        public async Task RollbackTranAsync()
        {
            try
            {
                if (cmd.Transaction != null)
                {
                    await cmd.Transaction.RollbackAsync();
                    cmd.Transaction = null;
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                return true;
            }
            finally
            {
                conn.Close();
            }
        }


        /// <summary>
        /// 测试连接异步
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }
                return true;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        /// <summary>
        /// 准备命令
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="connection">连接</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="dbParameters">数据库参数</param>
        /// <returns></returns>
        public async Task<bool> PrepareCommand(DbCommand command, DbConnection connection, DbTransaction transaction, CommandType commandType, string commandText, List<DbParameter> dbParameters)
        {
            var mustCloseConnection = false;
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
                mustCloseConnection = true;
            }
            if (transaction != null)
            {
                command.Transaction = transaction;
                mustCloseConnection = false;
            }
            command.CommandType = commandType;
            command.CommandText = commandText;
            if (dbParameters != null && dbParameters.Any())
            {
                command.Parameters.AddRange(dbParameters.ToArray());
            }
            return mustCloseConnection;
        }

        /// <summary>
        /// 执行非查询异步
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="dbParameters">数据库参数</param>
        /// <returns></returns>
        public async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, List<DbParameter> dbParameters = null)
        {
            var mustCloseConnection = await PrepareCommand(cmd, conn, cmd.Transaction, commandType, commandText, dbParameters);
            try
            {
                return await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                cmd.Parameters.Clear();
                if (mustCloseConnection)
                {
                    await conn.CloseAsync();
                }
            }
        }

        /// <summary>
        /// 执行标量异步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="dbParameters">数据库参数</param>
        /// <returns></returns>
        public async Task<T> ExecuteScalarAsync<T>(CommandType commandType, string commandText, List<DbParameter> dbParameters = null)
        {
            var mustCloseConnection = await PrepareCommand(cmd, conn, cmd.Transaction, commandType, commandText, dbParameters);
            try
            {
                return (await cmd.ExecuteScalarAsync()).ChanageType<T>();
            }
            finally
            {
                cmd.Parameters.Clear();
                if (mustCloseConnection)
                {
                    await conn.CloseAsync();
                }
            }
        }

        /// <summary>
        /// 执行阅读器异步
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="dbParameters">数据库参数</param>
        /// <returns></returns>
        public async Task<DbDataReader> ExecuteReaderAsync(CommandType commandType, string commandText, List<DbParameter> dbParameters = null)
        {
            var mustCloseConnection = await PrepareCommand(cmd, conn, cmd.Transaction, commandType, commandText, dbParameters);
            try
            {
                if (mustCloseConnection)
                {
                    return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                }
                else
                {
                    return await cmd.ExecuteReaderAsync();
                }
            }
            finally
            {
                cmd.Parameters.Clear();
            }
        }

        /// <summary>
        /// 执行数据集异步
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="dbParameters">数据库参数</param>
        /// <returns></returns>
        public async Task<DataSet> ExecuteDataSetAsync(CommandType commandType, string commandText, List<DbParameter> dbParameters = null)
        {
            var ds = new DataSet();
            using (var adapter = DbProviderFactory.CreateDataAdapter())
            {
                var mustCloseConnection = await PrepareCommand(cmd, conn, cmd.Transaction, commandType, commandText, dbParameters);
                try
                {
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds);
                }
                finally
                {
                    cmd.Parameters.Clear();
                    if (mustCloseConnection)
                    {
                        await conn.CloseAsync();
                    }
                }
            }
            return ds;
        }

        /// <summary>
        /// 执行数据表格异步
        /// </summary>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令文本</param>
        /// <param name="dbParameters">数据库参数</param>
        /// <returns></returns>
        public async Task<DataTable> ExecuteDataTableAsync(CommandType commandType, string commandText, List<DbParameter> dbParameters = null)
        {
            var ds = await ExecuteDataSetAsync(commandType, commandText, dbParameters);
            if (ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }
            return null;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="parameterValue">参数值</param>
        /// <param name="parameterDirection">参数方向</param>
        /// <returns></returns>
        public DbParameter CreateParameter(string parameterName, object parameterValue, ParameterDirection parameterDirection = ParameterDirection.Input)
        {
            var dbParameter = DbProviderFactory.CreateParameter();
            dbParameter.ParameterName = $"{DbOptions.DbType.MappingParameterSymbol()}{parameterName}";
            dbParameter.Direction = parameterDirection;
            dbParameter.Value = parameterValue ?? DBNull.Value;
            return dbParameter;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="keyValues">键值</param>
        /// <returns></returns>
        public List<DbParameter> CreateParameter(Dictionary<string, object> keyValues)
        {
            var dbParameters = new List<DbParameter>();
            foreach (var item in keyValues)
            {
                dbParameters.Add(CreateParameter(item.Key, item.Value));
            }
            return dbParameters;
        }
    }
}
