using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;
using System.Collections.Specialized;
using Quartz.Impl;


namespace DbCommon
{
    public class JobScheduler
    {
        private readonly IScheduler scheduler;
        List<DbMaintenanceModel> dbmList;

        public JobScheduler()
        {
            NameValueCollection props = new NameValueCollection
            {
                ["quartz.serializer.type"] = "binary",
                ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz",
                ["quartz.threadPool.threadCount"] = "5",
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            scheduler = factory.GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Start()
        {
            GlobalVariables.isServiceStarted = false;
            try
            {
                scheduler.Start().ConfigureAwait(false).GetAwaiter().GetResult();

                ScheduleJobs();
            }
            catch (Exception ex)
            {
                DbAccessLocal.ErrorInsert(0, 6, ex.ToString());
            }
            GlobalVariables.isServiceStarted = true;
        }

        public void ScheduleJobs()
        {
            var a = new DbAccessLocal().CheckDbSettings();

            dbmList = new DbAccessLocal().GetDbMaintenance(0);


            // RunProgramRunExample().GetAwaiter().GetResult();

            if (dbmList.Count > 0)
            {
                foreach (DbMaintenanceModel dbmItem in dbmList)
                {
                    GlobalVariables.dbmDictionary.Add(dbmItem.Id, dbmItem);
                    GlobalVariables.dbmTaskDictionary.Add(dbmItem.Id, false);

                    // define the job and tie it to our HelloJob class
                    IJobDetail job = JobBuilder.Create<DbMaintenanceJob>()
                    .WithIdentity($"job{dbmItem.Id}", "group1")
                    .UsingJobData("Id", dbmItem.Id)
                    .Build();

                    // Trigger the job to run now, and then repeat every 10 seconds
                    ITrigger trigger = TriggerBuilder.Create()
                   .WithIdentity($"trigger{dbmItem.Id}", "group1")
                   .StartNow()
                   .WithSimpleSchedule(x => x
                   .WithIntervalInSeconds(10)
                   .RepeatForever())
                   .Build();
                    // Tell quartz to schedule the job using our trigger
                    scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }

        public void Stop()
        {
            scheduler.Shutdown().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
