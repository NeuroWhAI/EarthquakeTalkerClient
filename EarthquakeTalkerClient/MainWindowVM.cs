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
using System.Windows.Threading;

namespace EarthquakeTalkerClient
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public MainWindowVM()
        {
            
        }

        //################################################################################################

        public IContext Context { get; set; } = null;

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

        private Dictionary<string, List<string>> m_keywords = new Dictionary<string, List<string>>();
        private Dictionary<string, MediaPlayer> m_alarms = new Dictionary<string, MediaPlayer>();

        //################################################################################################

        public void Init()
        {
            try
            {
                string host, port;

                using (var sr = new StreamReader("server.txt"))
                {
                    host = sr.ReadLine();
                    port = sr.ReadLine();
                }


                m_client = new Client(host, int.Parse(port));

                m_client.MessageReceived += Client_MessageReceived;
                m_client.ProtocolFailed += Client_ProtocolFailed;
                m_client.ProtocolSucceeded += Client_ProtocolSucceeded;

                m_client.Start();
            }
            catch (Exception)
            {
                this.State = "서버 정보를 읽을 수 없습니다.";
            }


            try
            {
                using (var sr = new StreamReader("keywords.txt"))
                {
                    string currentTarget = string.Empty;
                    List<string> currentKeywords = null;

                    while (!sr.EndOfStream)
                    {
                        string text = sr.ReadLine();

                        if (text.Length < 2)
                        {
                            continue;
                        }

                        if (text[0] == '$')
                        {
                            currentTarget = text.Substring(1);


                            var alarm = new MediaPlayer();
                            alarm.Open(new Uri(Path.Combine("Alarms", currentTarget), UriKind.Relative));
                            alarm.Stop();

                            m_alarms[currentTarget] = alarm;


                            currentKeywords = new List<string>();

                            m_keywords[currentTarget] = currentKeywords;
                        }
                        else if (text[0] != '#' && currentKeywords != null)
                        {
                            currentKeywords.Add(text);
                        }
                    }

                    sr.Close();
                }
            }
            catch (Exception)
            {
                this.State = "키워드 설정을 읽을 수 없습니다.";
            }
        }

        public void WhenWindowClosing()
        {
            m_client?.Stop();
        }

        private void Client_ProtocolFailed()
        {
            Context.BeginInvoke(() =>
            {
                this.State = "연결 중";
            });
        }

        private void Client_MessageReceived(Message msg, bool isNew)
        {
            Context.BeginInvoke(() =>
            {
                this.State = "연결됨";
                this.State2 = "최근 수신 시간 : " + DateTime.Now.ToLongTimeString();

                this.Messages.Add(msg);

                if (this.Messages.Count > 32)
                {
                    this.Messages.RemoveAt(0);
                }
            });


            if (isNew)
            {
                string source = $"{msg.Level} Level\n{msg.Sender}\n{msg.Text}";

                string finalAlarm = string.Empty;

                foreach (var kv in m_keywords)
                {
                    string alarm = kv.Key;
                    var commandList = kv.Value;

                    bool? triggered = null;

                    foreach (string command in commandList)
                    {
                        char op = command[0];
                        string keyword = command.Substring(1);

                        bool srcHasKey = source.Contains(keyword);

                        switch (op)
                        {
                            case '*':
                                if (triggered.HasValue)
                                    triggered &= srcHasKey;
                                else
                                    triggered = srcHasKey;
                                break;

                            case '+':
                                if (triggered.HasValue)
                                    triggered |= srcHasKey;
                                else
                                    triggered = srcHasKey;
                                break;

                            case '~':
                                if (srcHasKey)
                                    triggered = false;
                                break;

                            case '!':
                                if (srcHasKey)
                                    triggered = true;
                                break;
                        }
                    }

                    if (triggered.HasValue && triggered.Value)
                    {
                        finalAlarm = alarm;
                    }
                }

                if (!string.IsNullOrEmpty(finalAlarm)
                    && m_alarms.ContainsKey(finalAlarm))
                {
                    PlayAlarm(m_alarms[finalAlarm]);
                }
            }
        }

        private void Client_ProtocolSucceeded()
        {
            Context.BeginInvoke(() =>
            {
                this.State = "연결됨";
            });
        }

        private void PlayAlarm(MediaPlayer player)
        {
            Context.BeginInvoke(() =>
            {
                player.Stop();
                player.Play();
            });
        }
    }
}
