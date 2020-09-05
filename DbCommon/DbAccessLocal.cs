using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.Data;
using System.Data.SqlClient;


namespace DbCommon
{
    public class DbAccessLocal
    {

        public int CheckDbSettings()
        {

            int result = -1;
            //Proverka dali postoi db fajlojt
            int LocalDbVer = -1;
            List<string> sqlCmd = new List<string>();

            if (!isFilePresent("settings.sqlite"))
            {
                SQLiteConnection.CreateFile("settings.sqlite");

                LocalDbVer = 0;
            }


            SQLiteConnection m_dbConnection;
            m_dbConnection =
            new SQLiteConnection("Data Source=settings.sqlite;Version=3;");

            GlobalVariables.LocalDbConnString = m_dbConnection.ConnectionString;


            if (LocalDbVer == 0)
            {
                //sqlCmd = new List<string>();
                sqlCmd.Add("create table app_settings (db_ver int)");
                sqlCmd.Add("create table db_maintenance(id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, server_db_address varchar(50), server_db_name varchar(50), server_db_uid varchar(50), server_db_pwd varchar(50), backup_interval, backup_time  datetime)");
                sqlCmd.Add("insert into app_settings(db_ver) values(1)");
            }
            else
            {
                LocalDbVer = GetLocalDbVersion();
            }
            if (LocalDbVer < 2)
            {
                sqlCmd.Add("create table db_maintenance_status (db_id int, last_backup datetime, status_backup varchar(200), last_db_update datetime)");
                sqlCmd.Add("create table app_error (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, db_id int, type_job int, error_time datetime, error_details varchar(500) )");
                sqlCmd.Add("update app_settings set db_ver=2");
            }
            if (LocalDbVer < 3)
            {
                //sqlCmd.Add("create table db_maintenance_status (db_id int, last_backup datetime, status_backup varchar(200), last_db_update datetime, status_db_update varchar(200),last_sync datetime, status_sync varchar(200) )");
                sqlCmd.Add("alter table app_error error_details varchar(500) )");
                sqlCmd.Add("update app_settings set db_ver=3");
            }
            if (LocalDbVer < 4)
            {
                sqlCmd.Add("create table db_log (db_id int, log varchar(500),log_time datetime)");
                sqlCmd.Add("update app_settings set db_ver=4");
            }
            if (LocalDbVer < 5)
            {
                sqlCmd.Add("alter table db_maintenance add backup_destination varchar(300)");
                sqlCmd.Add("update app_settings set db_ver=5");
            }

            //sqlCmd.Add("drop table app_error");
            //sqlCmd.Add("create table app_error (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, db_id int, type_job int, error_time datetime, error_details varchar(500) )");
            //sqlCmd.Add("update app_settings set db_ver=2");
            ExecuteSqlCmd(sqlCmd);

            return result;

            //string sql = "create table app_settings (db_ver int)";
            //"create table db_maintenance (id int, server_db_address varchar(50), server_db_name varchar(50), server_db_uid varchar(50), server_db_pwd varchar(50),  master_db_address varchar(50), master_db_name varchar(50), master_db_uid varchar(50), master_db_pwd varchar(50), sync_interval int, backup_interval, update_schema_interval,backup_time  datetime, update_schema_time datetme  )"
        }

        public bool isFilePresent(string fileName)
        {
            return System.IO.File.Exists(string.Format(@"{0}\{1}", System.IO.Directory.GetCurrentDirectory(), fileName));
        }
        private int ExecuteSqlCmd(List<string> commands)
        {
            int result = -1;
            foreach (string cmd in commands)
            {
                result = ExecuteNonQuery(cmd);
            }
            return result;
        }
        public static int ExecuteNonQuery(string command)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = command;
                    cmd.Prepare();
                    /// cmd.Parameters.AddWithValue("@Lang", langTitle);
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException e)
                    {
                        //...
                    }
                }
                conn.Close();
            }
            return result;
        }

        public int GetLocalDbVersion()
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "select db_ver from app_settings ";
                    cmd.Prepare();
                    /// cmd.Parameters.AddWithValue("@Lang", langTitle);
                    try
                    {
                        result = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    catch (SQLiteException e)
                    {
                        //...
                    }
                }
                conn.Close();
            }
            return result;
        }

        public List<DbMaintenanceModel> GetDbMaintenance(int Id)
        {
            List<DbMaintenanceModel> langs = new List<DbMaintenanceModel>();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
                {
                    conn.Open();
                    string sql = $@"SELECT * FROM db_maintenance WHERE Id = {Id} or {Id}=0 ";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DbMaintenanceModel dbm = new DbMaintenanceModel();
                                dbm.Id = Convert.ToInt32(reader["id"]);
                                dbm.ClientDbAddress = reader[@"server_db_address"].ToString();
                                dbm.ClientDbName = reader["server_db_name"].ToString();
                                dbm.ClientDbPassword = reader["server_db_pwd"].ToString();
                                dbm.ClientDbUserName = reader["server_db_uid"].ToString();
                   
                                dbm.BackupDestination = reader["backup_destination"].ToString();
                              



                                langs.Add(dbm);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (SQLiteException e)
            {
                //...
            }
            return langs;
        }

        public int AddDbMaintenance(DbMaintenanceModel dbMaintenance)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "INSERT INTO db_maintenance(server_db_address,server_db_name,server_db_pwd,server_db_uid,backup_destination )"
                        + "VALUES (@server_db_address,@server_db_name,@server_db_pwd,@server_db_uid,@backup_destination)";

                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@server_db_address", dbMaintenance.ClientDbAddress);
                    cmd.Parameters.AddWithValue("@server_db_name", dbMaintenance.ClientDbName);
                    cmd.Parameters.AddWithValue("@server_db_uid", dbMaintenance.ClientDbUserName);
                    cmd.Parameters.AddWithValue("@server_db_pwd", dbMaintenance.ClientDbPassword);
    
                    cmd.Parameters.AddWithValue("@backup_destination", dbMaintenance.BackupDestination);
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException e)
                    {
                        //...
                    }
                }
                conn.Close();
            }
            return result;
        }

        public int UpdatedDbMaintenance(DbMaintenanceModel dbMaintenance)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "UPDATE db_maintenance "
                        + "SET server_db_address = @server_db_address "
                        + ", server_db_name = @server_db_name "
                        + ", server_db_pwd = @server_db_pwd "
                        + ", server_db_uid = @server_db_uid "
                        + ", backup_destination = @backup_destination "
                        + " WHERE Id = @Id";
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@server_db_address", dbMaintenance.ClientDbAddress);
                    cmd.Parameters.AddWithValue("@server_db_name", dbMaintenance.ClientDbName);
                    cmd.Parameters.AddWithValue("@server_db_uid", dbMaintenance.ClientDbUserName);
                    cmd.Parameters.AddWithValue("@server_db_pwd", dbMaintenance.ClientDbPassword);
                    cmd.Parameters.AddWithValue("@backup_destination", dbMaintenance.BackupDestination);
                    cmd.Parameters.AddWithValue("@Id", dbMaintenance.Id);
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException ex)
                    {
                        // ...
                    }
                }
                conn.Close();
            }
            return result;
        }

        public int DbMaintenanceStatusUpdate(DbMaintenanceStatus dbMaintenanceStatus)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "UPDATE db_maintenance_status "
                        + "SET last_backup = @last_backup "
                        + ", status_backup = @status_backup "                     
                        + " WHERE db_id = @db_id";
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@last_backup", dbMaintenanceStatus.LastBackup);
                    cmd.Parameters.AddWithValue("@status_backup", dbMaintenanceStatus.StatusBackup);
                    cmd.Parameters.AddWithValue("@db_id", dbMaintenanceStatus.DbId);
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException)
                    {
                        // ...
                    }
                }
                conn.Close();
            }
            return result;
        }
       
        public static int DbMaintenanceStatusUpdateBackup(DbMaintenanceStatus dbMaintenanceStatus)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "UPDATE db_maintenance_status "
                        + "SET last_backup = @last_backup "
                        + ", status_backup = @status_backup "
                        + " WHERE db_id = @db_id";
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@last_backup", dbMaintenanceStatus.LastBackup);
                    cmd.Parameters.AddWithValue("@status_backup", dbMaintenanceStatus.StatusBackup);
                    cmd.Parameters.AddWithValue("@db_id", dbMaintenanceStatus.DbId);
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException)
                    {
                        // ...
                    }
                }
                conn.Close();
            }
            return result;
        }
      
        public int DbMaintenanceStatusInsert(DbMaintenanceStatus dbMaintenanceStatus)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "Insert into db_maintenance_status(db_id,last_backup,status_backup) "
                        + "values(@db_id,@last_backup,@status_backup)";
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@last_backup", dbMaintenanceStatus.LastBackup);
                    cmd.Parameters.AddWithValue("@status_backup", dbMaintenanceStatus.StatusBackup);
                    cmd.Parameters.AddWithValue("@db_id", dbMaintenanceStatus.DbId);
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (SQLiteException)
                    {
                        // ...
                    }
                }
                conn.Close();
            }
            return result;
        }
        public DbMaintenanceStatus DbMaintenanceStatusGet(int Id)
        {
            // List<DbMaintenanceModel> langs = new List<DbMaintenanceModel>();
            DbMaintenanceStatus dbm = new DbMaintenanceStatus();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
                {
                    conn.Open();
                    string sql = $@"SELECT * FROM db_maintenance_status WHERE db_id = {Id} ";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                dbm.DbId = Convert.ToInt32(reader["db_id"]);
                                dbm.LastBackup = (DateTime)reader["last_backup"];
                                dbm.StatusBackup = reader["status_backup"].ToString();
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (SQLiteException e)
            {
                //...
            }
            return dbm;
        }
        public DataTable DbMaintenanceGetAll()
        {

            DataTable table = new DataTable();

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
                {
                    string sqlQuery = @"SELECT dbm.Id,dbm.server_db_name,strftime('%d-%m-%Y %H:%M:%S', dbs.last_backup)  FROM db_maintenance dbm left join db_maintenance_status dbs on dbs.db_id=dbm.Id ";
                    using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, conn))
                    {

                        SQLiteDataAdapter ds = new SQLiteDataAdapter(cmd);
                        ds.Fill(table);
                    }
                }
                return table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void ErrorInsert(int DbId, int TypeJob, string ErrorDetails)
        {
            //Dali da se vnesuvaat porakite deka nema konekcija?? Ili djabe ke se polni tabelata. Dovolno e izvestuvanje preku statusot deka nema konekcija???
            //sqlCmd.Add("create table app_error (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, db_id int, type_job int, error_time datetime, error varchar(500) )");

            int result = -1;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "Insert into app_error(db_id,type_job,error_time,error_details) "
                            + "values(@db_id,@type_job,@error_time,@error_details)";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@db_id", DbId);
                        cmd.Parameters.AddWithValue("@type_job", TypeJob);
                        cmd.Parameters.AddWithValue("@error_time", DateTime.Now);
                        cmd.Parameters.AddWithValue("@error_details", ErrorDetails);

                        try
                        {
                            result = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            // ...
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex) { }
            //return result;
        }
        public int LogInsert(int DbId, string Log)
        {
            //Dali da se vnesuvaat porakite deka nema konekcija?? Ili djabe ke se polni tabelata. Dovolno e izvestuvanje preku statusot deka nema konekcija???
            //sqlCmd.Add("create table app_error (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, db_id int, type_job int, error_time datetime, error varchar(500) )");

            int result = -1;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "Insert into db_log(db_id,log,log_time) "
                            + "values(@db_id,@log,@log_time)";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@db_id", DbId);
                        cmd.Parameters.AddWithValue("@log", Log);
                        cmd.Parameters.AddWithValue("@log_time", DateTime.Now);

                        try
                        {
                            result = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            // ...
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex) { }
            return result;
        }
        public DataTable LogsGetAll()
        {

            DataTable table = new DataTable();
            // sqlCmd.Add("create table app_error (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, db_id int, type_job int, error_time datetime, error_details varchar(500) )");
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
                {
                    string sqlQuery = @"SELECT log from db_log order by log_time ";
                    using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, conn))
                    {

                        SQLiteDataAdapter ds = new SQLiteDataAdapter(cmd);
                        ds.Fill(table);
                    }
                }
                return table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public DataTable ErrorsGetAll()
        {

            DataTable table = new DataTable();
            // sqlCmd.Add("create table app_error (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, db_id int, type_job int, error_time datetime, error_details varchar(500) )");
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
                {
                    string sqlQuery = @"SELECT id, '' as db_name, error_time, type_job, error_details from app_error order by error_time ";
                    using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, conn))
                    {

                        SQLiteDataAdapter ds = new SQLiteDataAdapter(cmd);
                        ds.Fill(table);
                    }
                }
                return table;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public int ErrorDeleteAll()
        {
            //Dali da se vnesuvaat porakite deka nema konekcija?? Ili djabe ke se polni tabelata. Dovolno e izvestuvanje preku statusot deka nema konekcija???
            //sqlCmd.Add("create table app_error (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, db_id int, type_job int, error_time datetime, error varchar(500) )");

            int result = -1;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(GlobalVariables.LocalDbConnString))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "delete from app_error";
                        cmd.Prepare();

                        try
                        {
                            result = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            // ...
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex) { }
            return result;
        }


    }
}
