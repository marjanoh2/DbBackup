1. Опис на апликацијата

Апликацијата служи за правење автоматски бекап на повеќе SQL Server бази на податоци во одреден интервал на две локации, 
локално и cloud(Google Drive) (Google drive се уште не е целосно имплементирано)

На почетниот прозорец има неколку копчиња:
- Error log - ги прикажува сите грешки кои настанале при работа на апликацијата. 
- Delete - брише подесувања за одредена база
- Edit - можност за промена на подесување за избраната база. 
- Add - додавање нова база 

Во прозорецот за додавање/менување параметри ги има следниве полиња:
- Server address - патека до SQL серверот 
- Database - име на база за која ќе се прави бекап
- Username - корисничко име за пристап до базата
- Password - лозинка за пристап до базата
- Backup destination - локација на која ќе се запишува бекапот 
Дополнително има копче "Backup now" за правење моментален бекап.

2. Решение на проблемот 

Решението е составено од три проекти:
   1. DbCommon 
   2. DbController
   3. DbService
   
Користени готови компоненти:
   1. Quartz - https://github.com/quartz-scheduler/quartz
   2. Topshelf - https://github.com/Topshelf/Topshelf
   3. FluentResults - https://github.com/altmann/FluentResults
   4. DotNetZip 
   5. SQLite 
   
3. Функцијата BackupDb

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
   
   

