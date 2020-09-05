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
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ErrorLog.xaml
    /// </summary>
    public partial class ErrorLog : Window
    {
        public ErrorLog()
        {
            InitializeComponent();
        }

        private void frmErrorLog_Loaded(object sender, RoutedEventArgs e)
        {
            lvDb.ItemsSource = new DbAccessLocal().ErrorsGetAll().DefaultView;//new DbAccessLocal().DbMaintenanceGetAll().DefaultView;
        }
    }
}
