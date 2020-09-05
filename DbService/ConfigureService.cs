using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using DbCommon;
namespace DbService
{
    internal static class ConfigureService
    {
        internal static void Configure()
        {
            HostFactory.Run(configure =>
            {
                configure.Service<JobScheduler>(service =>
                {
                    service.ConstructUsing(s => new JobScheduler());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                //Setup Account that window service use to run.  
                configure.RunAsLocalSystem();
                configure.StartAutomaticallyDelayed();
                configure.SetServiceName("DbService");
                configure.SetDisplayName("DbService");
                configure.SetDescription("Database backup Service");
            });
        }
    }
}
