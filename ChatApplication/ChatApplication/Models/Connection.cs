using ChatApplication.Assets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;


namespace ChatApplication.Models
{
    public class Connection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Boolean objectCreated = false;
        public string _time;
        public Boolean userAdded;
        private TcpListener _listner = null;
        private TcpClient _client;
        private NetworkStream _stream;
        private String _data;
        private String _info;
        private User _user;
        private Chat theChat;
        private DateTime time;
        private Boolean _canSend;


        private string chatRequestStatus;

        public Connection(User user)
        {
            _canSend = false;
            userAdded = false;
            _user = user;
            time = DateTime.Now;
            chatRequestStatus= "waiting";

            theChat = new Chat()
            {
                users = new List<string>() { },
                time = time,
                messages = new List<string>() { }
            };
        }

        public Chat Chat
        {
            get
            {
                return theChat;
            }
            set
            {
                theChat = value;
            }
        }

        public Boolean CanSend
        {
            get
            {
                return _canSend;
            }
            set
            {
                _canSend = value;
            }
        }

        public TcpClient Client
        {
            get
            {
                return _client;
            }

            set
            {
                _client = value;
            }
        }

        public String Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
            }
        }


        public String Info
        {
            get
            {
                return _info;
            }

            set
            {
                _info = value;
            }
        }

        public String ChatRequestStatus
        {
            get
            {
                return chatRequestStatus;
            }

            set
            {
                chatRequestStatus = value;
            }
        }

        public TcpListener Listner
        {
            get
            {
                return _listner;
            }

            set
            {
                _listner = value;
            }
        }

        public NetworkStream Stream
        {
            get
            {
                return _stream;
            }

            set
            {
                _stream = value;
            }
        }

        public void ConnectionHandler(string ipadress, int port, string type)
        {
            int i = 0;
            Byte[] bytes = new Byte[256];

            if (type == "s")
            {
                try
                {
                    _client = new TcpClient(ipadress, port);
                    _stream = _client.GetStream();
                    _info = "Chat request sent - waiting for answer...";
                    if (_stream != null)
                    {

                        while ((i = _stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            HandleData(i, bytes);
                            if (_stream == null)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                    _info = "Lost connection"; 
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("ArgumentNullException: {0}", e);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                    _info = "Nobody listening on this port";
                }
            }
            else if (type == "l")
            {
                try
                {
                    IPAddress localAddr = IPAddress.Parse(ipadress);
                    _listner = new TcpListener(localAddr, port);
                    _listner.Start();
                    while (true)
                    {
                        while (chatRequestStatus == "waiting")
                        {
                            _info = "Waiting for a connection... ";
                            Client = _listner.AcceptTcpClient();
                            _info = "Another user wants to chat, do you want to accept or denie?";
                            chatRequestStatus = "pending";
                        }

                        while (chatRequestStatus== "pending")
                        {
                        }

                        if (chatRequestStatus == "denied")
                        {
                            _stream = Client.GetStream();
                            Message rejectedMessage = new Message() { Type = "rejected"};
                            Send(rejectedMessage);
                            _info="You denied the chat request";
                            _stream = null;
                            break;
                        }

                        _info = "Connected!";
                        _canSend = true;
                        Console.WriteLine("Connected!");
                        _data = null;
                        _stream = Client.GetStream();
                        Message acceptedMessage = new Message() { Type = "accepted" };
                        Send(acceptedMessage);
                        if (_stream != null)
                        {
                            while ((i = _stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                HandleData(i, bytes);
                                if (_stream == null)
                                {
                                    _canSend = false;
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                    _info = "Lost connection";
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
                finally
                {
                    Listner.Stop();
                }

            }
        }

        public void HandleData(int i, Byte[] bytes)
        {
            Message deserializedMessage = JsonConvert.DeserializeObject<Message>(System.Text.Encoding.ASCII.GetString(bytes, 0, i));

            if (deserializedMessage.Type == "message")
            {
                _data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("Received a message");
            }
            else if (deserializedMessage.Type == "beep")
            {
                Console.WriteLine("Received a beep");
                Console.Beep();
            }
            else if (deserializedMessage.Type == "time" && objectCreated == false)
            {
                _time = deserializedMessage.Msg;
                Console.WriteLine("Time for chat is: " + _time);
                objectCreated = true;
            }
            else if (deserializedMessage.Type == "abort")
            {
                _info = "The other user has disconnected";
                ShutDownConnection();
            }
            else if (deserializedMessage.Type == "accepted")
            {
                _canSend = true;
                _info = "Connected!";
                
            } else if (deserializedMessage.Type == "rejected")
            {
                _info = "Chat request rejected!";
                ShutDownConnection();
            } else
            {
                Console.WriteLine("An unexpected message type was sent");
            }
        }

        public void Send(Message message)
        {
            if (_stream != null && _stream.CanWrite)
            {
                string json = JsonConvert.SerializeObject(message, Formatting.Indented);
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(json);
                _stream.Write(data, 0, data.Length);
            }
        }

        public void SendBeep()
        {
            Message message = new Message() { Type = "beep" };
            Send(message);
        }

        public void AcceptRequest()
        {
            chatRequestStatus = "accepted";
        }

        public void DenieRequest()
        {
            chatRequestStatus= "denied";
        }

        public void SendMessage(Message incomingMessage)
        {
            Message message = new Message() { Msg = incomingMessage.Msg, Username = _user.Name, Type = "message" };
            var deserializedChatList = HelpFunctions.GetChatList();

            if (objectCreated == false)
            {
                //Sharing the same time for all users chatobject - in case they are sharing save file
                Message timeMessage = new Message() { Msg = time.ToString(), Type = "time" };
                Send(timeMessage);
                _time = time.ToString();

                Chat.users.Add(_user.Name);
                Chat.messages.Add(message.Msg);
                deserializedChatList.Add(theChat);
                objectCreated = true;
                userAdded = true;
            }
            else
            {
                foreach (Chat c in deserializedChatList)
                {
                    if (c.time.ToString() == _time)
                    {
                        if (c.users.Count > 0 && userAdded == false)
                        {
                            c.partner = c.users[0];
                            c.users.Add(_user.Name);
                            userAdded = true;
                        }
                        if (c.users.Count >= 2 && userAdded == true && c.partner == null)
                        {
                            c.partner = c.users[1];
                        }

                        c.messages.Add(message.Msg);
                        break;
                    }
                }
            }
            string json = JsonConvert.SerializeObject(deserializedChatList, Formatting.Indented);
            File.WriteAllText(HelpFunctions.GetFile(), json);
            Send(message);
        }

        public void ShutDownConnection()
        {
            _canSend = false;
            _stream = null;
            _client.GetStream().Close();
            _client.Close();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}








