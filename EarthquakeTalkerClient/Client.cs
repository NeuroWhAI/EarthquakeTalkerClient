using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace EarthquakeTalkerClient
{
    public class Client
    {
        public Client(string host, int port)
        {
            this.Host = host;
            this.Port = port;
        }

        //################################################################################################

        public event Action<Message> MessageReceived;
        public event Action ProtocolFailed;

        public string Host
        { get; set; }

        public int Port
        { get; set; }

        private Guid m_latestGuid = Guid.Empty;
        private bool m_onRunning = false;

        //################################################################################################

        public void Start()
        {
            m_onRunning = true;

            Task.Factory.StartNew(DoJob);
        }

        public void Stop()
        {
            m_onRunning = false;
        }

        private void DoJob()
        {
            while (m_onRunning)
            {
                try
                {
                    StartProtocol();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);

                    ProtocolFailed?.Invoke();

                    Task.Delay(2000).Wait();
                }
            }

            while (m_onRunning)
            {
                try
                {
                    UpdateProtocol();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);

                    ProtocolFailed?.Invoke();
                }

                Task.Delay(3000).Wait();
            }
        }

        private void StartProtocol()
        {
            using (var client = new TcpClient(this.Host, this.Port))
            using (var stream = client.GetStream())
            {
                SendString(stream, "neurowhai");

                SendString(stream, "recent");
                SendString(stream, "0");

                if (ReadStringFromStream(stream) == "msg")
                {
                    var msg = ReadMessageFromStream(stream);
                    m_latestGuid = msg.Id;

                    MessageReceived?.Invoke(msg);
                }
            }
        }

        private void UpdateProtocol()
        {
            using (var client = new TcpClient(this.Host, this.Port))
            using (var stream = client.GetStream())
            {
                SendString(stream, "neurowhai");

                SendString(stream, "after");
                SendString(stream, m_latestGuid.ToString());

                if (ReadStringFromStream(stream) == "msg")
                {
                    var msg = ReadMessageFromStream(stream);
                    m_latestGuid = msg.Id;

                    MessageReceived?.Invoke(msg);
                }
            }
        }

        private Message ReadMessageFromStream(NetworkStream stream)
        {
            string id = ReadStringFromStream(stream);
            string time = ReadStringFromStream(stream);
            string level = ReadStringFromStream(stream);
            string sender = ReadStringFromStream(stream);
            string text = ReadStringFromStream(stream);

            var msg = new Message()
            {
                Id = Guid.Parse(id),
                CreationTime = DateTime.FromBinary(long.Parse(time)),
                Level = (Message.Priority)(int.Parse(level)),
                Sender = sender,
                Text = text,
            };

            return msg;
        }

        private void SendString(NetworkStream stream, string str)
        {
            var strBuffer = Encoding.UTF8.GetBytes(str);
            var lenBuffer = BitConverter.GetBytes(strBuffer.Length);


            stream.Write(lenBuffer, 0, lenBuffer.Length);
            stream.Write(strBuffer, 0, strBuffer.Length);
        }

        private string ReadStringFromStream(NetworkStream stream)
        {
            int num = 0;
            byte[] buffer = new byte[4];
            Task<int> task = null;


            task = stream.ReadAsync(buffer, 0, buffer.Length);
            task.Wait(5_000);

            if (task.IsCompleted == false || task.Result <= 0)
            {
                throw new Exception("Connection reset.");
            }

            num = BitConverter.ToInt32(buffer, 0);

            if (num <= 0 || num > 65535)
            {
                throw new Exception(num + " is invalid.");
            }


            buffer = new byte[num];


            task = stream.ReadAsync(buffer, 0, buffer.Length);
            task.Wait(10_000);

            if (task.IsCompleted == false || task.Result <= 0)
            {
                throw new Exception("Connection reset.");
            }

            return Encoding.UTF8.GetString(buffer);
        }
    }
}
