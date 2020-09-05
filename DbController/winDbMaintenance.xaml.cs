using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DbCommon;
using FluentResults;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for winDbMaintenance.xaml
    /// </summary>
    public partial class winDbMaintenance : Window
    {
        public winDbMaintenance()
        {
            InitializeComponent();
        }

        public int DbId;
        private DbMaintenanceModel _dbm;
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int r;
            double syncInterval=0;

            DbMaintenanceModel dbm = new DbMaintenanceModel();
            dbm.ClientDbAddress = txtServerAddress.Text;
            dbm.ClientDbName = txtServerDbName.Text;
            dbm.ClientDbUserName = txtServerUsername.Text;
            dbm.ClientDbPassword = txtServerPassword.Password;
            dbm.BackupDestination = txtBackupDestination.Text;
            dbm.BackupTime = Convert.ToDateTime("2020-01-01 07:00:00");

            //double.TryParse(txtSyncInterval.Text, out syncInterval);
 
            dbm.Id = DbId;

            if (DbId > 0)
            {
                r = new DbAccessLocal().UpdatedDbMaintenance(dbm);
            }
            else
            {
                r = new DbAccessLocal().AddDbMaintenance(dbm);
            }


        }
        private void FillDetails(int id)
        {
            _dbm = new DbAccessLocal().GetDbMaintenance(id)[0];

            txtServerAddress.Text = _dbm.ClientDbAddress;
            txtServerDbName.Text = _dbm.ClientDbName;
            txtServerUsername.Text = _dbm.ClientDbUserName;
            txtServerPassword.Password = _dbm.ClientDbPassword;

            txtBackupDestination.Text = _dbm.BackupDestination;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DbId > 0)
            {
                FillDetails(DbId);
            }
        }







        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            
            Result ret = DbSchema.BackupDb(_dbm);

            if (ret.IsSuccess)
            {
                MessageBox.Show("Backup success");
            }
            else
            {
                MessageBox.Show("Backup error: " + ret.Reasons[0].Message);
            }
        }



       
           
    }
}
