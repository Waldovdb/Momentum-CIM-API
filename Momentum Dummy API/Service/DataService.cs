using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;

namespace Momentum_Dummy_API.Service
{
    public class DataService : IDataService
    {
        private readonly string _dbConnection;
        private readonly string _dbConnectionPresence;

        #region [ Default Constructor ]
        public DataService()
        {
            _dbConnection = $"Data Source=10.1.31.211\\PRESENCE;Initial Catalog=InovoCIM;User Id=InovoCIM;Password=083B@b123;";
            _dbConnectionPresence = $"Data Source=10.1.31.211\\PRESENCE;Initial Catalog=SQLPR1;User Id=InovoCIM;Password=083B@b123;";
        }
        #endregion

        //-----------------------------//

        #region [ Get Connection String ]
        public string GetConnectionString(string Type)
        {
            return _dbConnection;
        }
        #endregion

        #region [ Count Async ]
        public async Task<long> CountAsync<T>(string Table) where T : class
        {
            long total = 0;
            try
            {
                using var conn = new SqlConnection(_dbConnection);
                total = await conn.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM {Table}");
                return total;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        //------------------//

        #region [ Insert Single ]
        public async Task<bool> InsertSingle<T, U>(string Query, U Input) where T : class
        {
            try
            {
                if (Input != null)
                {
                    using var conn = new SqlConnection(_dbConnection);
                    await conn.ExecuteAsync(Query, Input, commandTimeout: 1500);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region [ Insert Many ]
        public async Task<bool> InsertMany<T, U>(string Query, List<U> InputList) where T : class
        {
            if (InputList.Count > 0)
            {
                foreach (var item in InputList)
                {
                    try
                    {
                        using var conn = new SqlConnection(_dbConnection);
                        await conn.ExecuteAsync(Query, item, commandTimeout: 1500);
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                        continue;
                    }
                }
                return true;
            }
            return false;
        }
        #endregion

        //------------------//

        #region [ Select Single ]
        public async Task<T> SelectSingle<T, U>(string Query, U Input) where T : class, new()
        {
            try
            {
                using var conn = new SqlConnection(_dbConnection);
                var data = await conn.QueryAsync<T>(Query, Input, commandTimeout: 1500);
                return data.FirstOrDefault();
            }
            catch (Exception ex)
            {

            }
            return new T();
        }
        #endregion

        #region [ Select Single ]
        public async Task<T> SelectSinglePresence<T, U>(string Query, U Input) where T : class, new()
        {
            try
            {
                using var conn = new SqlConnection(_dbConnectionPresence);
                var data = await conn.QueryAsync<T>(Query, Input, commandTimeout: 1500);
                return data.FirstOrDefault();
            }
            catch (Exception ex)
            {

            }
            return new T();
        }
        #endregion

        #region [ Select Many ]
        public async Task<List<T>> SelectMany<T, U>(string Query, U Input) where T : class, new()
        {
            try
            {
                using var conn = new SqlConnection(_dbConnection);
                var data = await conn.QueryAsync<T>(Query, Input, commandTimeout: 1500);
                return data.ToList();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return null;
        }
        #endregion

        //------------------//

        #region [ Update Single ]
        public async Task<bool> UpdateSingle<T, U>(string Query, U Input) where T : class
        {
            try
            {
                if (Input != null)
                {
                    using var conn = new SqlConnection(_dbConnection);
                    await conn.ExecuteAsync(Query, Input, commandTimeout: 1500);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Update Single ]
        public async Task<bool> StoredProcLong<T, U>(string Query, U Input) where T : class
        {
            try
            {
                if (Input != null)
                {
                    using var conn = new SqlConnection(_dbConnection);
                    await conn.ExecuteAsync(Query, Input, commandTimeout: 3600);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        //------------------//

        #region [ Delete Single ]
        public async Task<bool> DeleteSingle<T, U>(string Query, U Input) where T : class
        {
            try
            {
                if (Input != null)
                {
                    using var conn = new SqlConnection(_dbConnection);
                    await conn.ExecuteAsync(Query, Input, commandTimeout: 1500);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Delete Custom ]
        public async Task<bool> DeleteCustom(string Query, string Connection)
        {
            try
            {
                using (var conn = new SqlConnection(Connection))
                {
                    await conn.ExecuteAsync(Query, new { }, commandTimeout: 1500);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        //------------------//

        #region [ Truncate ]
        public bool Truncate(string Table)
        {
            try
            {
                Table = Table.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
                Table = string.Format("[dbo].[{0}]", Table);

                using (var conn = new SqlConnection(_dbConnection))
                {
                    conn.Execute($"TRUNCATE TABLE {Table}", new { });
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Truncate ]
        public bool Truncate(string Table, string Connection)
        {
            try
            {
                Table = Table.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
                Table = string.Format("[dbo].[{0}]", Table);

                using (var conn = new SqlConnection(Connection))
                {
                    conn.Execute($"TRUNCATE TABLE {Table}", new { });
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Bulk Upload ]
        public bool BulkUpload(DataTable model, string Table)
        {
            try
            {
                Table = Table.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
                Table = string.Format("[dbo].[{0}]", Table);

                using (SqlBulkCopy SqlBulk = new SqlBulkCopy(_dbConnection))
                {
                    SqlBulk.DestinationTableName = Table;
                    SqlBulk.BatchSize = 9500;
                    SqlBulk.BulkCopyTimeout = 1500;
                    SqlBulk.WriteToServer(model);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Bulk Upload ]
        public bool BulkUpload(DataTable model, string Table, string Connection)
        {
            try
            {
                Table = Table.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
                Table = string.Format("[dbo].[{0}]", Table);

                using (SqlBulkCopy SqlBulk = new SqlBulkCopy(Connection))
                {
                    SqlBulk.DestinationTableName = Table;
                    SqlBulk.BatchSize = 9500;
                    SqlBulk.BulkCopyTimeout = 1500;
                    SqlBulk.WriteToServer(model);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}
