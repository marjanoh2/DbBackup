using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DbCommon;
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SyncStatus;
        private string statusService;
        private string SERVICE_NAME= "DbService";
        public MainWindow()
        {
            InitializeComponent();

            txtStatus.Text = "OK";

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
            dispatcherTimer.Start();
        }




        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            viewJobs();
            viewLog();
            checkService();
        }
        private void checkService()
        {
            var serviceExists = ServiceController.GetServices().Any(s => s.ServiceName == SERVICE_NAME);

            if (!serviceExists)
            {
                statusService = "Not installed";
                btnService.Content = "Install service";
            }
            else
            { 
            ServiceController sc = new ServiceController(SERVICE_NAME);

            switch (sc.Status)
            {
                case ServiceControllerStatus.Running: 
                    statusService = "Running";
                    btnService.Content = "Stop";
                    break;
                case ServiceControllerStatus.Stopped:
                    statusService= "Stopped";
                        btnService.Content = "Start";
                        break;
                case ServiceControllerStatus.Paused:
                    statusService= "Paused";
                        btnService.Content = "Start";
                        break;
                case ServiceControllerStatus.StopPending:
                    statusService= "Stopping";
                    break;
                case ServiceControllerStatus.StartPending:
                    statusService= "Starting";
                    break;
                default:
                    statusService= "Status Changing";
                    break;
            }
              
            }
            lblServiceStatus.Content = statusService;
        }
        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                // ...
            }
        }
        public static void StopService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch
            {
                // ...
            }
        }
        private void viewLog()
        {
            DataTable results;
            results = new DbAccessLocal().LogsGetAll();
            string res = string.Join(Environment.NewLine,
            results.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.ItemArray)));

            txtStatus.Text = res;
            txtStatus.ScrollToEnd();
        }
        private void viewJobs()
        {
            
            lvDb.ItemsSource = new DbAccessLocal().DbMaintenanceGetAll().DefaultView;

        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            winDbMaintenance win = new winDbMaintenance();

            win.ShowDialog();


        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!(lvDb.SelectedItems.Count == 1))
                return;

            winDbMaintenance win = new winDbMaintenance();
            win.DbId = Convert.ToInt32(((System.Data.DataRowView)lvDb.SelectedItems[0]).Row.ItemArray[0]);
            win.ShowDialog();

        }

        private void btnErrorLog_Click(object sender, RoutedEventArgs e)
        {
            //if (!(lvDb.SelectedItems.Count == 1))
            //    return;

            ErrorLog win = new ErrorLog();
            // win.DbId = Convert.ToInt32(((System.Data.DataRowView)lvDb.SelectedItems[0]).Row.ItemArray[0]);
            win.ShowDialog();
        }

        private void btnService_Click(object sender, RoutedEventArgs e)
        {
            if (btnService.Content.Equals("Stop"))
            {
                StopService(SERVICE_NAME, 3000);
            }
            if (btnService.Content.Equals("Start"))
            {
                StartService(SERVICE_NAME, 3000);
            }
            if (btnService.Content.Equals("Install service"))
            {
                InstallService();
            }
        }
        private void InstallService()
        {
            string genArgs = "install";
            string pathToFile = AppDomain.CurrentDomain.BaseDirectory+"DbService.exe";
            Process runProg = new Process();
            try
            {
                runProg.StartInfo.FileName = pathToFile;
                runProg.StartInfo.Arguments = genArgs;
                runProg.StartInfo.CreateNoWindow = true;
                runProg.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not install service " + ex);
            }

        }

        private void btnErrorLog_Copy_Click(object sender, RoutedEventArgs e)
        {
            JobScheduler j = new JobScheduler();
            j.Start();
        }
    }
}
