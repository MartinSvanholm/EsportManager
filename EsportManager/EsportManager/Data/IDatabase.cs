using SQLite;

namespace EsportManager.Data;

public interface IDatabase
{
    SQLiteConnection Connection { get; set; }
}