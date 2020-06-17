using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace VocabT
{
    public class DatabaseContext
    {
        private readonly string _connectionString;
        private IDbConnection Con => new NpgsqlConnection(_connectionString);

        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;

            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public async Task<List<Word>> GetWords()
        {
            using var db = Con;
            var words = await db.QueryAsync<Word>(Sql.SelectWords);
            return words.ToList();
        }

        public async Task<List<Word>> GetWordsWithStatus(LearningStatus status)
        {
            using var db = Con;
            var words = await db.QueryAsync<Word>(Sql.SelectWordsWithStatus, new { status });
            return words.ToList();
        }
    }
}
