using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;

namespace EarthquakeTalkerClient
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public MainWindowVM()
        {
            BindingOperations.EnableCollectionSynchronization(this.Messages, m_lockMsgList);

            this.Messages.CollectionChanged += (s, e) => NotifyPropertyChanged("Messages");


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
                m_client.ProtocolSucceeded += Client_ProtocolSucceeded;

                m_client.Start();
            }
            catch (Exception)
            {
                this.State = "서버 정보를 읽을 수 없습니다.";
            }


            m_mediaNormal.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "normal.mp3")));
            m_mediaHigh.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "high.mp3")));
        }

        //################################################################################################

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<Message> m_msgList = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages
        {
            get { return m_msgList; }
            set
            {
                m_msgList = value;
                NotifyPropertyChanged("Messages");
            }
        }
        private readonly object m_lockMsgList = new object();

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

        private string m_stateMessage2 = "Loading";
        public string State2
        {
            get { return m_stateMessage2; }
            set
            {
                m_stateMessage2 = value;

                NotifyPropertyChanged("State2");
            }
        }

        private Client m_client = null;

        private MediaPlayer m_mediaNormal = new MediaPlayer();
        private MediaPlayer m_mediaHigh = new MediaPlayer();

        //################################################################################################

        public void WhenWindowClosing()
        {
            m_client?.Stop();
        }

        private void Client_ProtocolFailed()
        {
            this.State = "연결 중";
        }

        private void Client_MessageReceived(Message msg, bool isNew)
        {
            this.State = "연결됨";
            this.State2 = "최근 수신 시간 : " + DateTime.Now.ToLongTimeString();

            lock (m_lockMsgList)
            {
                this.Messages.Add(msg);

                if (this.Messages.Count > 32)
                {
                    this.Messages.RemoveAt(0);
                }
            }


            if (isNew)
            {
                if (msg.Level <= Message.Priority.Normal)
                {
                    PlayNormalAlarm();
                }
                else
                {
                    PlayHighAlarm();
                }
            }
        }

        private void Client_ProtocolSucceeded()
        {
            this.State = "연결됨";
        }

        private void PlayNormalAlarm()
        {
            PlayAlarm(m_mediaNormal);
        }

        private void PlayHighAlarm()
        {
            PlayAlarm(m_mediaHigh);
        }

        private void PlayAlarm(MediaPlayer player)
        {
            player.Dispatcher.Invoke(() =>
            {
                player.Stop();
                player.Play();
            });
        }
    }
}
