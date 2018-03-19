using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EarthquakeTalkerClient
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            this.ViewModel = this.DataContext as MainWindowVM;


            m_timer.Interval = TimeSpan.FromMilliseconds(2_000);
            m_timer.Tick += Timer_Tick;
            m_timer.Start();
        }

        //################################################################################################

        private MainWindowVM ViewModel
        { get; set; } = null;

        private DispatcherTimer m_timer = new DispatcherTimer();

        //################################################################################################

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_timer.Stop();

            this.ViewModel.WhenWindowClosing();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (this.listMessage.Items.Count > 0)
            {
                this.listMessage.SelectedIndex = this.listMessage.Items.Count - 1;
                this.listMessage.ScrollIntoView(this.listMessage.SelectedItem);
            }
        }
    }
}
