using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Serilog;
//using PhobosDbService;
using System.IO;
using System.Timers;

namespace DbService
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureService.Configure();

        }
    }
}
