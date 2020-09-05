using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;
using System.IO.Compression;
using System.Net;
using System.Data.SqlClient;
using Ionic.Zip;

using FluentResults;

namespace DbCommon
{   
    public class DbSchema
    {

        public static Result BackupDb(DbMaintenanceModel dbm)
        {
            //string backupDir = @"D:\DB_Backups";
            int DeletionDays = 10;
            //if (DeletionDays > 0)
            //    DeleteOldBackups();

            DateTime dtNow = DateTime.Now;

            try
            {
                
                if (!(Directory.Exists(dbm.BackupDestination)) )
                    throw new Exception("Directory"+ dbm.BackupDestination + " not exists"); 

                using (SqlConnection sqlConnection = new SqlConnection(dbm.ClientConnectionString))
                {
                    sqlConnection.Open();

                        
                        string backupFileNameWithoutExt = String.Format("{0}\\{1}_{2:yyyy-MM-dd_hh-mm-ss-tt}", dbm.BackupDestination, dbm.ClientDbName , dtNow);
                        string backupFileNameWithExt = String.Format("{0}.bak", backupFileNameWithoutExt);
                        string zipFileName = String.Format("{0}.zip", backupFileNameWithoutExt);

                        string cmdText = string.Format("BACKUP DATABASE {0}\r\nTO DISK = '{1}'", dbm.ClientDbName, backupFileNameWithExt);

                        using (SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConnection))
                        {
                            sqlCommand.CommandTimeout = 0;
                            sqlCommand.ExecuteNonQuery();
                        }
                    sqlConnection.Close();

                    using (ZipFile zip = new ZipFile())
                        {
                            zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                            zip.AddFile(backupFileNameWithExt);
                            zip.Save(zipFileName);
                        }

                        File.Delete(backupFileNameWithExt);
                    

                    

                   //var upload = new GoogleDrive().Upload(zipFileName);

                   //  if (upload.IsFailed)
                   // {
                   //     throw new Exception("Upload error " + upload.Reasons[0].Message);
                   // }
                }
            }
            catch (Exception ex)
            {             
                DbAccessLocal.ErrorInsert(dbm.Id, 1, ex.Message);
                return Results.Fail(ex.Message);
            }


            return Results.Ok();
        }
    }
}
