using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbCommon
{
    public static class GlobalVariables
    {

        public static string syncLog;
        public static string LocalDbConnString;
        public static Dictionary<int, DbMaintenanceModel> dbmDictionary = new Dictionary<int, DbMaintenanceModel>();
        public static Dictionary<int, Boolean> dbmTaskDictionary = new Dictionary<int, Boolean>();
        public static bool isServiceStarted; 
        //public Dictionary dictionary = new Dictionary<int, PhobosDbController.DbMaintenanceModel >();
    }
}
