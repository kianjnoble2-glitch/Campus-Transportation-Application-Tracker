using SQLite;

namespace Kats
{
    public class LocalDBService<T> where T : class, new()
    {
        private const string DB_NAME = "bustrackerapp_db.db3";
        private SQLiteAsyncConnection _connection;
        public LocalDBService()
        {
            Init().Wait();
        }
        public async Task Init()
        {
            if (_connection != null)
            {
                return;
            }
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
            await _connection.CreateTableAsync<T>();
        }


        public async Task<List<T>> GetAllAsync()
        {
            return await _connection.Table<T>().ToListAsync();
        }
    }
}
