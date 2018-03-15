using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace EarthquakeTalkerClient
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public MainWindowVM()
        {
            try
            {
                string host, ip;

                using (var sr = new StreamReader("server.txt"))
                {
                    host = sr.ReadLine();
                    ip = sr.ReadLine();
                }


                m_client = new Client(host, int.Parse(ip));

                m_client.MessageReceived += Client_MessageReceived;
                m_client.ProtocolFailed += Client_ProtocolFailed;

                m_client.Start();
            }
            catch (Exception)
            {
                this.State = "서버 정보를 읽을 수 없습니다.";
            }
        }

        //################################################################################################

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public List<Message> Messages
        { get; set; } = new List<Message>();

        private string m_stateMessage = "Loading";
        public string State
        {
            get { return m_stateMessage; }
            set
            {
                m_stateMessage = value;

                NotifyPropertyChanged("State");
            }
        }

        private Client m_client = null;

        //################################################################################################

        public void WhenWindowClosing()
        {
            m_client?.Stop();
        }

        private void Client_ProtocolFailed()
        {
            this.State = "연결 중";
        }

        private void Client_MessageReceived(Message msg)
        {
            this.State = "연결됨 " + DateTime.Now.ToLongTimeString();

            this.Messages.Add(msg);

            if (this.Messages.Count > 32)
            {
                this.Messages.RemoveAt(0);
            }

            NotifyPropertyChanged("Messages");
        }
    }
}
