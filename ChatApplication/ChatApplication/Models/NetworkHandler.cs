using ChatApplication.Models;
using ChatApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Collections.ObjectModel;

namespace ChatApplication.Assets
{
    public class NetworkHandler
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private Models.Connection connection;
        private User _user;
        Thread listenThread, clientThread, stopThread;
        private string _info;

        public NetworkHandler(User user)
        {
            _user = user;
        }

        public ObservableCollection<Chat> Chats { get; set; } = new ObservableCollection<Chat>();

        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();

        public Connection Connection
        {
            get
            {
                return connection;
            }

            set
            {
                connection = value;
                OnPropertyChanged("Connection");
            }
        }

        public User User
        {
            get
            {
                return _user;
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

        private bool _canOnlyRead = false;
        public bool CanOnlyRead
        {
            get
            {
                return _canOnlyRead;
            }
            set
            {
                _canOnlyRead = value;
                OnPropertyChanged("CanOnlyRead");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void InitClient()
        {
            object s = "s";
            Messages.Clear();
            if (clientThread == null || !clientThread.IsAlive)
            {
                CanOnlyRead = true;
                clientThread = new Thread(EstablishConnection);
                clientThread.Start(s);
                if (stopThread == null || !stopThread.IsAlive)
                {
                    stopThread = new Thread(Disconnect);
                    stopThread.Start();
                }
            }
            else
            {
                if (connection.Client != null)
                {
                    connection.Client.Close();
                }
                clientThread.Abort();
                clientThread = new Thread(EstablishConnection);
                clientThread.Start(s);
                stopThread.Abort();
                stopThread = new Thread(Disconnect);
                stopThread.Start();
            }
        }

        public void StopListen()
        {
            if (connection.Client != null)
            {
                Message message = new Message() { Msg = "Disconnect", Username = User.Name, Type = "abort" };
                connection.Send(message);
                CanOnlyRead = false;

                foreach (Chat c in HelpFunctions.GetChatList())
                {
                    if (c.time.ToString() == connection._time)
                    {
                        Chats.Add(c);
                    }
                }
                connection.objectCreated = false;
                connection.userAdded = false;
                Messages.Clear();

                _info = "Connection Lost";
                //MessageBox.Show("Connection Lost", "Chatify by A3 Studio", MessageBoxButton.OK);
                stopThread.Abort();

                if (connection.Listner != null)
                {
                    listenThread.Abort();
                    connection.Listner.Stop();
                }
                else
                {
                    clientThread.Abort();

                }
                connection.Client.Close();
            }
        }
        public void Disconnect()
        {
            while (connection?.Data != "Disconnect")
            {
            }
            foreach (Chat c in HelpFunctions.GetChatList())
            {
                if (c.time.ToString() == connection._time)
                {
                    Chats.Add(c);
                }
            }
            connection.objectCreated = false;
            connection.userAdded = false;

            if (connection.Client != null)
            {
                CanOnlyRead = false;

                _info = "Connection Lost";
                //MessageBox.Show("Connection Lost", "Chatify by A3 Studio", MessageBoxButton.OK);

                if (connection.Listner != null)
                {
                    listenThread.Abort();
                    connection.Listner.Stop();
                }
                else
                {
                    clientThread.Abort();
                }
                connection.Client.Close();
            }
        }

        public void StartListen()
        {
            object l = "l";
            Messages.Clear();
            if (listenThread == null || !listenThread.IsAlive)
            {
                CanOnlyRead = true;
                listenThread = new Thread(EstablishConnection);
                listenThread.Start(l);
                if (stopThread == null || !stopThread.IsAlive)
                {
                    stopThread = new Thread(Disconnect);
                    stopThread.Start();
                }
            }
            else
            {
                if (connection.Listner != null)
                {
                    connection.Listner.Stop();
                }
                listenThread.Abort();
                listenThread = new Thread(EstablishConnection);
                listenThread.Start(l);
                stopThread.Abort();
                stopThread = new Thread(Disconnect);
                stopThread.Start();
            }
        }

        public void EstablishConnection(object type)
        {
            connection = new Models.Connection(_user);
            if (connection != null)
            {
                if (type.ToString() == "l")
                {
                    try
                    {
                        int result;
                        if (User.Port >= 1000 && int.TryParse(User.Port.ToString(), out result))
                        {
                            Console.WriteLine(User.Port);
                            connection.ConnectionHandler(User.PAddress, User.Port, "l");
                        }
                        else
                        {
                            CanOnlyRead = false;
                            throw (new ConnectionException("Invalid port"));
                        }
                    }
                    catch (ConnectionException e)
                    {
                        Console.WriteLine("NoPortEx: {0}", e.Message);
                        _info = "Invalid port";
                        //MessageBox.Show("Invalid port", "Chatify by A3 Studio", MessageBoxButton.OK);
                    }
                }
                else if (type.ToString() == "s")
                {
                    Console.WriteLine(User.Port);
                    connection.ConnectionHandler(User.PAddress, User.Port, "s");
                }
            }
            else
            {
                Console.WriteLine("Could not create a new connection");
            }
        }
        public class ConnectionException : Exception
        {
            public ConnectionException(string message) : base(message)
            {
            }
        }
    }
}
