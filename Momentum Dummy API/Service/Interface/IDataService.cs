using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Momentum_Dummy_API.Service
{
    public interface IDataService
    {
        string GetConnectionString(string Type);

        Task<long> CountAsync<T>(string Table) where T : class;

        Task<bool> InsertSingle<T, U>(string Query, U Input) where T : class;
        Task<bool> InsertMany<T, U>(string Query, List<U> InputList) where T : class;

        Task<T> SelectSingle<T, U>(string Query, U Input) where T : class, new();
        Task<T> SelectSinglePresence<T, U>(string Query, U Input) where T : class, new();
        Task<List<T>> SelectMany<T, U>(string Query, U Input) where T : class, new();

        Task<bool> UpdateSingle<T, U>(string Query, U Input) where T : class;
        Task<bool> DeleteSingle<T, U>(string Query, U Input) where T : class;

        Task<bool> StoredProcLong<T, U>(string Query, U Input) where T : class;

        Task<bool> DeleteCustom(string Query, string Connection);

        bool Truncate(string Table);
        bool Truncate(string Table, string Connection);

        bool BulkUpload(DataTable model, string Table);
        bool BulkUpload(DataTable model, string Table, string Connection);

    }
}
