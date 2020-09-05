using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using System.IO;
using System.IO.Compression;
using System.Net;
using System.Data;
using Quartz;
using FluentResults;

namespace DbCommon
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class DbMaintenanceJob : IJob
    {
        private DbMaintenanceModel db1;
        private DbMaintenanceStatus dbs;
        private Boolean TaskWorking;

        private Boolean HasToDbBackup;


        private DateTime LastDbBackup = DateTime.Now;

    


        public async Task Execute(IJobExecutionContext context)
        {


            JobKey key = context.JobDetail.Key;

            JobDataMap dataMap = context.MergedJobDataMap;  // Note the difference from the previous example

            db1 = GlobalVariables.dbmDictionary[dataMap.GetInt("Id")];// new DbAccessLocal().GetDbMaintenance(dataMap.GetInt("Id"))[0];


            TaskWorking = GlobalVariables.dbmTaskDictionary[dataMap.GetInt("Id")];

            await DoJobs();
        }




        private async Task DoJobs()
        {


            if (!(GlobalVariables.isServiceStarted))
                { return; }

            if (TaskWorking)
                return;

            dbs = new DbAccessLocal().DbMaintenanceStatusGet(db1.Id);

            if (dbs.DbId == 0)
            { LastDbBackup = new DateTime(1900, 01, 01); }
            else { LastDbBackup = dbs.LastBackup; }

            if ((DateTime.Now - LastDbBackup).TotalSeconds > (db1.BackupTime.AddSeconds(86400) -db1.BackupTime).TotalSeconds )
            { HasToDbBackup = true;}

            if (HasToDbBackup)
            { DoDbBackup(); }

        }

 
        private async void DoDbBackup()
        {
            GlobalVariables.dbmTaskDictionary[db1.Id] = true;

            Result res = DbSchema.BackupDb(db1);

            if (res.IsSuccess)
            {
                dbs.LastBackup = DateTime.Now;
                dbs.StatusBackup = "OK";
            }
            else
            {
                dbs.LastBackup = DateTime.Now; //TODO vo slucaj ako ima problem da ne gi zaglavuva drugite zadaci
                dbs.StatusBackup = res.Reasons[0].ToString();
            }

            GlobalVariables.dbmTaskDictionary[db1.Id] = false;
        }



    }
}
