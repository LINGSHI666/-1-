using BF1ServerTools.Data;
using BF1ServerTools.Utils;

using Microsoft.Data.Sqlite;

namespace BF1ServerTools.Helper;

public static class SQLiteHelper
{
    /// <summary>
    /// SQLite数据库连接
    /// </summary>
    private static SqliteConnection connection = null;

    /// <summary>
    /// 数据库文件路径
    /// </summary>
    private static string F_ServerDB_Path = BF1ServerTools.Utils.FileUtil.D_Data_Path + @"\Server.db";

    /// <summary>
    /// 线程锁
    /// </summary>
    private static readonly object ObjFlag = new();

    /// <summary>
    /// 数据库初始化
    /// </summary>
    public static bool Initialize()
    {
        try
        {
            var connStr = new SqliteConnectionStringBuilder("Data Source=" + F_ServerDB_Path)
            {
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            connection = new SqliteConnection(connStr);
            connection.Open();

            if (connection.State == ConnectionState.Open)
            {
                LoggerHelper.Info("SQLite数据库初始化成功");

                CreateTable("score_kick");
                CreateTable("kick_ok");
                CreateTable("kick_no");
                CreateTable("change_team");

                LoggerHelper.Info("SQLite数据库默认表创建成功");
                return true;
            }

            LoggerHelper.Info("SQLite数据库初始化失败");
            return false;
        }
        catch (Exception ex)
        {
            LoggerHelper.Error("SQLite数据库初始化异常", ex);
            return false;
        }
    }

    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public static void UnInitialize()
    {
        if (connection != null)
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }

    /// <summary>
    /// 执行SQL命令，执行对数据表的增加、删除、修改操作
    /// </summary>
    /// <param name="sqlStr"></param>
    public static void ExecuteNonQuery(string sqlStr)
    {
        using var cmd = new SqliteCommand(sqlStr, connection);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 执行SQL命令，返回查询结果中第 1 行第 1 列的值
    /// </summary>
    /// <param name="sqlStr"></param>
    /// <returns></returns>
    public static int ExecuteScalar(string sqlStr)
    {
        using var cmd = new SqliteCommand(sqlStr, connection);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 创建数据库表
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="createTable"></param>
    public static void CreateTable(string tableName)
    {
        CreateTable(tableName, "rank INTEGER, name TEXT, personaId INTEGER, type TEXT, message1 TEXT, message2 TEXT, message3 TEXT, date TEXT");
    }

    /// <summary>
    /// 创建数据库表
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="createTable"></param>
    public static void CreateTable(string tableName, string createTable)
    {
        string sql = $@"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        if (ExecuteScalar(sql) == 0 && createTable != "")
        {
            sql = $@"CREATE TABLE {tableName} ( id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, {createTable} )";
            ExecuteNonQuery(sql);
        }
    }

    //////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 增加数据库记录
    /// </summary>
    /// <param name="tableName">score_kick, kick_ok, kick_no, change_team</param>
    /// <param name="info"></param>
    public static void AddLog(string tableName, SQLiteLogInfo info)
    {
        lock (ObjFlag)
        {
            using var command = connection.CreateCommand();
            command.CommandText =
            $@"
                INSERT INTO {tableName}
                ( rank, name, personaId, type, message1, message2, message3, date ) 
                VALUES
                ( $rank, $name, $personaId, $type, $message1, $message2, $message3, $date )
            ";
            command.Parameters.AddWithValue("$rank", info.Rank);
            command.Parameters.AddWithValue("$name", info.Name);
            command.Parameters.AddWithValue("$personaId", info.PersonaId);
            command.Parameters.AddWithValue("$type", info.Type);
            command.Parameters.AddWithValue("$message1", info.Message1);
            command.Parameters.AddWithValue("$message2", info.Message2);
            command.Parameters.AddWithValue("$message3", info.Message3);
            command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ffff"));
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// 查询日志，只要前500条
    /// </summary>
    /// <param name="tableName">score_kick, kick_ok, kick_no, change_team</param>
    /// <returns></returns>
    public static List<SQLiteLogInfo> QueryLog(string tableName)
    {
        lock (ObjFlag)
        {
            List<SQLiteLogInfo> logInfos = new();

            var count = ExecuteScalar($@"SELECT * FROM {tableName}");

            string sql = string.Empty;
            if (count < 500)
                sql = $@"SELECT * FROM {tableName} ORDER BY date DESC";
            else
                sql = $@"SELECT * FROM {tableName} ORDER BY date DESC LIMIT 500";

            int index = 1;

            using var cmd = new SqliteCommand(sql, connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                logInfos.Add(new()
                {
                    Index = index++,
                    Rank = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    PersonaId = reader.GetInt64(3),
                    Type = reader.GetString(4),
                    Message1 = reader.GetString(5),
                    Message2 = reader.GetString(6),
                    Message3 = reader.GetString(7),
                    Date = reader.GetString(8)
                });
            }

            return logInfos;
        }
    }
}
