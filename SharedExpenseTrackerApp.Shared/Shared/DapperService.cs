using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedExpenseTrackerApp.Domain.Shared
{
    public class DapperService<T>
    {
        private readonly string _connectionString;

        public DapperService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IEnumerable<T>> QueryAsync(string query, object? data = null)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.QueryAsync<T>(query, data);
        } 

        public async Task<T> QueryFirstOrDefault(string query, object? data = null)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            var item = await db.QueryFirstOrDefaultAsync<T>(query, data);
            return item!;
        }
        public async Task<int> ExecuteAsync(string query, object? param = null)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.ExecuteAsync(query, param);
        }


    }
}
