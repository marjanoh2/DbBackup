using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DbCommon
{
    public class DbMaintenanceModel
    {
        private int _id;
        private string _ClientDbAddress;
        private string _ClientDbName;
        private string _ClientDbUserName;
        private string _ClientDbPassword;
        private double _BackupInterval;
        private DateTime _BackupTime;
        private DateTime _LastBackup;
        private string _BackupDestination;

        public double BackupInterval
        {
            get { return this._BackupInterval; }
            set { this._BackupInterval = value; }
        }


        public DateTime BackupTime
        {
            get { return this._BackupTime; }
            set { this._BackupTime = value; }
        }


        public DateTime LastBackup
        {
            get { return this._LastBackup; }
            set { this._LastBackup = value; }
        }


        public int Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        public string ClientDbAddress
        {
            get { return this._ClientDbAddress; }
            set { this._ClientDbAddress = value; }
        }
        public string BackupDestination
        {
            get { return this._BackupDestination; }
            set { this._BackupDestination = value; }
        }
        public string ClientDbName
        {
            get { return this._ClientDbName; }
            set { this._ClientDbName = value; }
        }
        public string ClientDbUserName
        {
            get { return this._ClientDbUserName; }
            set { this._ClientDbUserName = value; }
        }
        public string ClientDbPassword
        {
            get { return this._ClientDbPassword; }
            set { this._ClientDbPassword = value; }
        }
        public string ClientConnectionString
        {
            get { return $@"Server={this._ClientDbAddress};Database={this._ClientDbName};User Id={this._ClientDbUserName};Password={this._ClientDbPassword};"; }
        }







        public DbMaintenanceModel()
        {
            _ClientDbAddress = "";
            _ClientDbName = "";
            _ClientDbUserName = "";
            _ClientDbPassword = "";
        }
        public DbMaintenanceModel(string clientDbAddress, string clientDbName, string clientDbUserName, string clientDbPassword)
        {
            _ClientDbAddress = clientDbAddress;
            _ClientDbName = clientDbName;
            _ClientDbUserName = clientDbUserName;
            _ClientDbPassword = clientDbPassword;

        }
    }

    public class DbMaintenanceStatus
    {
        private int _db_id;
        private DateTime _last_backup;
        private string _status_backup;

        public int DbId
        {
            get { return this._db_id; }
            set { this._db_id = value; }
        }

        public DateTime LastBackup
        {
            get { return this._last_backup; }
            set { this._last_backup = value; }
        }

        public string StatusBackup
        {
            get { return this._status_backup; }
            set { this._status_backup = value; }
        }

    }
}
