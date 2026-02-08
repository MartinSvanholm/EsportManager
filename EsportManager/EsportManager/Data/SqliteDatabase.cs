using EsportManager.Features.Devices.Models;
using SQLite;

namespace EsportManager.Data;

public class SqliteDatabase : IDatabase
{
    public SqliteDatabase()
    {
        if (Connection is not null)
            return;
            
        Connection = new SQLiteConnection(DatabasePath);

        InitTables();
    }

    private const string DatabaseFilename = "EsportManager.db3";
    private static string DatabasePath => Path.Combine(Environment.CurrentDirectory, DatabaseFilename);

    public SQLiteConnection Connection { get; set; }
     
    private void InitTables()
    {
        Connection.CreateTable<DeviceDb>();
    }
}