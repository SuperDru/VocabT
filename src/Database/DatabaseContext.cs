using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace VocabT
{
    public class DatabaseContext
    {
        private static DatabaseContext _instance;

        public static DatabaseContext Instance
        {
            get
            {
                _instance ??= new DatabaseContext("Server=127.0.0.1;Port=5432;Database=vocab_db;User Id=postgres;Password=mydb;");
                return _instance;

            }
        }

        private readonly string _connectionString;
        private IDbConnection Con => new NpgsqlConnection(_connectionString);

        private DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;

            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public async Task<Word> GetWord(string name)
        {
            using var db = Con;
            return await db.QueryFirstOrDefaultAsync<Word>(Sql.SelectWordWithEng, new { eng = name });
        }

        public async Task<List<Word>> GetWordsWithTranslations(string[] translations)
        {
            using var db = Con;
            var records = await db.QueryAsync<Word>(Sql.SelectWordsWithTranslations, new { translations });
            return records.ToList();
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

        public async Task AddWord(string eng, string[] rus, string hint = null)
        {
            using var db = Con;
            await db.ExecuteAsync(Sql.InsertNewWord, new { eng, rus, hint });
        }

        public async Task UpdateWords(List<Word> words)
        {
            using var db = Con;
            db.Open();
            using var tx = db.BeginTransaction(IsolationLevel.Snapshot);

            foreach (var word in words)
            {
                await db.ExecuteAsync(Sql.UpdateWord, new DynamicParameters(word));
            }

            tx.Commit();
        }

        public async Task UpdateSeq(List<Word> excludeWords)
        {
            using var db = Con;
            await db.ExecuteAsync(Sql.UpdatePositiveSeq, new {words = excludeWords.Select(x => x.Eng)});
        }
    }
}
