using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

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

        public event Action ProtocolSucceeded;
        public event Action<Message, bool> MessageReceived;
        public event Action ProtocolFailed;

        public string Host
        { get; set; }

        public int Port
        { get; set; }

        private Guid m_latestGuid = Guid.Empty;
        private bool m_onRunning = false;
        private Thread m_worker = null;

        //################################################################################################

        public void Start()
        {
            Stop();

            m_onRunning = true;

            m_worker = new Thread(new ThreadStart(DoJob));
            m_worker.Start();
        }

        public void Stop()
        {
            m_onRunning = false;

            if (m_worker != null)
            {
                m_worker.Join();
                m_worker = null;
            }
        }

        private void DoJob()
        {
            int startOffset = 3;

            while (m_onRunning)
            {
                try
                {
                    StartProtocol(startOffset);

                    startOffset -= 1;
                    if (startOffset < 0)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);

                    ProtocolFailed?.Invoke();

                    Thread.Sleep(2000);
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

                Thread.Sleep(3000);
            }
        }

        private void StartProtocol(int offset)
        {
            using (var client = new TcpClient(this.Host, this.Port))
            using (var stream = client.GetStream())
            {
                SendString(stream, "neurowhai");

                SendString(stream, "recent");
                SendString(stream, offset.ToString());

                if (ReadStringFromStream(stream) == "msg")
                {
                    var msg = ReadMessageFromStream(stream);
                    m_latestGuid = msg.Id;

                    if (CheckGlob(msg))
                    {
                        RequestGlob(msg.Text);
                    }

                    ProtocolSucceeded?.Invoke();
                    MessageReceived?.Invoke(msg, false);
                }
                else
                {
                    ProtocolSucceeded?.Invoke();
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

                    if (CheckGlob(msg))
                    {
                        RequestGlob(msg.Text);
                    }

                    ProtocolSucceeded?.Invoke();
                    MessageReceived?.Invoke(msg, true);
                }
                else
                {
                    ProtocolSucceeded?.Invoke();
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

        private bool CheckGlob(Message msg)
        {
            string[] imageTypes =
            {
                ".png", ".jpg", ".bmp", ".jpeg", ".gif", // TODO: More...?
            };

            if (Util.CheckImageUri(msg.Text)
                && !msg.Text.TrimStart().StartsWith("http"))
            {
                return !File.Exists(msg.Text);
            }

            return false;
        }

        private void RequestGlob(string path)
        {
            try
            {
                using (var client = new TcpClient(this.Host, this.Port))
                using (var stream = client.GetStream())
                {
                    SendString(stream, "neurowhai");

                    SendString(stream, "glob");
                    SendString(stream, path);

                    if (ReadStringFromStream(stream) == "glob")
                    {
                        SaveGlobFromStream(stream, path);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private void SaveGlobFromStream(NetworkStream stream, string path)
        {
            if (ReadStringFromStream(stream) != path)
            {
                throw new Exception("Path is invalid.");
            }


            int size = ReadIntFromStream(stream);

            if (size <= 0)
            {
                throw new Exception(size + " is invalid.");
            }


            var buffer = new byte[size];
            ReadBytes(stream, buffer, 0, buffer.Length);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, buffer);
        }

        private void SendString(NetworkStream stream, string str)
        {
            var strBuffer = Encoding.UTF8.GetBytes(str);
            var lenBuffer = BitConverter.GetBytes(strBuffer.Length);


            stream.Write(lenBuffer, 0, lenBuffer.Length);
            stream.Write(strBuffer, 0, strBuffer.Length);
        }

        private int ReadIntFromStream(NetworkStream stream)
        {
            byte[] buffer = new byte[4];

            ReadBytes(stream, buffer, 0, buffer.Length);

            return BitConverter.ToInt32(buffer, 0);
        }

        private string ReadStringFromStream(NetworkStream stream)
        {
            int num = ReadIntFromStream(stream);

            if (num <= 0 || num > 65535)
            {
                throw new Exception(num + " is invalid.");
            }


            var buffer = new byte[num];


            ReadBytes(stream, buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer);
        }

        private void ReadBytes(NetworkStream stream, byte[] buffer, int offset, int count)
        {
            const int CHUNK_SIZE = 1024;

            while (count > 0)
            {
                var task = stream.ReadAsync(buffer, offset, Math.Min(count, CHUNK_SIZE));
                task.Wait(10_000);

                if (task.IsCompleted == false || task.Result <= 0)
                {
                    throw new Exception("Connection reset.");
                }

                offset += task.Result;
                count -= task.Result;
            }
        }
    }
}
