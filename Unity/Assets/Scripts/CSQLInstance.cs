using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;

public class CSQLInstance
{
	/*
    string databasePathName;
    SqliteConnection connection = new SqliteConnection();
    bool connectionOpen = false;

    public bool openConnection
    {
        get { return connectionOpen; }
        set { if (value != connectionOpen) { connectionOpen = value; if (value) { connection.Open(); } else { connection.Close(); } } }
    }
    
    public CSQLInstance(string databasePathNameWithoutExtension)
    {
        if (databasePathNameWithoutExtension == null || databasePathNameWithoutExtension == "")
            databasePathNameWithoutExtension = "TEMP";
        databasePathName = string.Format("Data Source={0}.db", databasePathNameWithoutExtension);
        connection.ConnectionString = databasePathName;
        //connection.Open();
    }

    ~CSQLInstance()
    {
        openConnection = false;
    }

    public void Query(string query)
    {
        SqliteCommand command = new SqliteCommand(query);
        command.Connection = connection;
        openConnection = true;
        try
        {
            command.ExecuteNonQuery();
        }
        finally
        {
            openConnection = false;
        }
    }
	 * */
}
