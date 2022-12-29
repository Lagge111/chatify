using System;
using System.ComponentModel;
using System.Windows.Input;
using ChatApplication.Commands;
using ChatApplication.Models;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ChatApplication.Assets;

namespace ChatApplication.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Boolean unableToSendMessage;
        private Boolean checkedHistory;
        private Message _msg;
        private SearchWord _sw;
        private User _user;
        private NetworkHandler networkHandler;
        private ICommand _beepCommand;
        private ICommand _startCommand;
        private ICommand _sendCommand;
        private ICommand _searchCommand;
        private ICommand _removeFilterCommand;
        private ICommand _listenCommand;
        private ICommand _loadCommand;
        private ICommand _disconnectCommand;
        private ICommand _returnCommand;
        private ICommand _denieCommand;
        private ICommand _acceptCommand;

        public MainViewModel()
        {
            HelpFunctions.CreateFile();
            _user = new User();
            _msg = new Message();
            _sw = new SearchWord();
            unableToSendMessage = false;
            checkedHistory = false;
            networkHandler = new NetworkHandler(_user);

        Task task = new Task(() =>
            {
                while (true)
                {
                    if (networkHandler.Connection != null && networkHandler.Connection.Data != null)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Message deserializedMessage = JsonConvert.DeserializeObject<Message>(networkHandler.Connection.Data);
                            Messages.Add(new Message() { Msg = deserializedMessage.Msg, Username = User.Name });
                            networkHandler.Connection.Data = null;      
                        });
                    }
                    if (networkHandler.Connection != null && networkHandler.Connection.Info != null)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            SystemMessages.Clear();
                            SystemMessages.Add(new Message() { Msg = networkHandler.Connection.Info, Username = User.Name });
                            networkHandler.Connection.Info = null;
                        });
                    }
                    if (networkHandler != null && networkHandler.Info != null)
                    {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        SystemMessages.Clear();
                        SystemMessages.Add(new Message() { Msg = networkHandler.Info, Username = User.Name });
                        networkHandler.Info = null;
                    });
                    }
                    Thread.Sleep(100);
                }
            });
            task.Start();

            foreach (Chat c in HelpFunctions.GetChatList())
            {
                Chats.Add(c);
            }
        }

        public ObservableCollection<Chat> Chats { get; set; } = new ObservableCollection<Chat>();
        public ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
        public ObservableCollection<Message> SystemMessages { get; set; } = new ObservableCollection<Message>();
      
        public Message Message {
            get
            {
                return _msg;
            }
            set
            {
                _msg = value;
            }
        }
        
        public SearchWord SearchWord
        {
            get
            {
                return _sw;
            }
            set
            {
                _sw = value;
            }
        }
        
        public User User
        {
            get
            {
                return _user;
            }
        }

        public bool CanExecute
        {
            get
            {
                if (User == null)
                {
                    return false;
                }
                return (!String.IsNullOrWhiteSpace(User.Name) && !String.IsNullOrWhiteSpace(User.Port.ToString()) && !String.IsNullOrWhiteSpace(User.PAddress));
            }
        }

        public bool CanSend
        {
            get
            {

                if (networkHandler.Connection != null)
                {
                    if (networkHandler.Connection.CanSend == true)
                    {
                        return true;
                    }
                }
                return (false);
            }
        }

        public bool CanDisconnect
        {
            get
            {
                if (networkHandler.Connection != null)
                {
                    if (networkHandler.Connection.Stream != null)
                    {
                        return true;
                    }
                } 
                return (false);
            }
        }


        public bool CanBeep
        {
            get
            {
              
            if (networkHandler.Connection == null)
                {
                 return false;
                 }


                return (true);
            }
        }

        public bool CanAcceptDenie
        {
            get
            {
                if(networkHandler.Connection != null)
                {
                    if (networkHandler.Connection.ChatRequestStatus == "pending")
                    {
                        return true;
                    }
                }
                return (false);
            }
        }
        
        public ICommand BeepCommand
        {
            get
            {
                    return _beepCommand ?? (_beepCommand = new SimpleCommand(() => networkHandler.Connection.SendBeep(), () => CanBeep));
            }

        }
        
        public ICommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new SimpleCommand(() => networkHandler.InitClient(), () => CanExecute));
            }

        }
        
        public ICommand SendCommand
        {
            get
            {
                return _sendCommand ?? (_sendCommand = new SimpleCommand(() => SendMessage(), () => CanSend));
            }
        }

        public ICommand AcceptCommand
        {
            get
            {
                return _acceptCommand ?? (_acceptCommand = new SimpleCommand(() => networkHandler.Connection.AcceptRequest(), () => CanAcceptDenie));
            }
        }

        public ICommand DenieCommand
        {
            get
            {
                return _denieCommand ?? (_denieCommand = new SimpleCommand(() => networkHandler.Connection.DenieRequest(), () => CanAcceptDenie));
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new SimpleCommand(() => SearchForUser(), () => true));
            }
        }
        
        public ICommand RemoveFilterCommand
        {
            get
            {
                return _removeFilterCommand ?? (_removeFilterCommand = new SimpleCommand(() => RemoveFilter(), () => true));
            }
        }
        
        public ICommand ListenCommand
        {
            get
            {
                return _listenCommand ?? (_listenCommand = new SimpleCommand(() => networkHandler.StartListen(), () => CanExecute));
            }
        }
        
        public ICommand LoadCommand
        {
            get
            {
                return _loadCommand ?? (_loadCommand = new Commands.ParameterCommand(LoadChat, param => true));
            }
        }
        
        public ICommand DisconnectCommand
        {
            get
            {
                return _disconnectCommand ?? (_disconnectCommand = new SimpleCommand(() => networkHandler.StopListen(), () => CanDisconnect));
            }
        }

        public ICommand ReturnCommand
        {
            get
            {
                return _returnCommand ?? (_returnCommand = new SimpleCommand(() => ReturnToChat(), () => true));
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

        public void SendMessage()
        {
            if (networkHandler.Connection != null && unableToSendMessage == false)
            {
                if (networkHandler.Connection.Client != null && networkHandler.Connection.Client.Connected == true && networkHandler.Connection.Stream != null)
                {
                    SystemMessages.Clear();
                    Message message = new Message() { Msg = _user.Name + ": " + _msg.Msg, Username = _user.Name, Type = "message" };
                    networkHandler.Connection.SendMessage(message);
                    Messages.Add(message);
                }
                else if (networkHandler.Connection.Client == null)
                {
                    SystemMessages.Clear();
                    SystemMessages.Add(new Message() { Msg = "Not connected to someone", Username = User.Name });
                    Console.WriteLine("Not connected to someone");
                }
                else if (networkHandler.Connection.Client.Connected == false)
                {
                    Console.WriteLine("No longer connected");
                    SystemMessages.Clear();
                    SystemMessages.Add(new Message() { Msg = "No longer connected", Username = User.Name });
                }
                else if (networkHandler.Connection.Stream == null)
                {
                    Console.WriteLine("No stream");
                    SystemMessages.Clear();
                    SystemMessages.Add(new Message() { Msg = "No stream established", Username = User.Name });
                }
            }
            else
            {
                SystemMessages.Clear();
                SystemMessages.Add(new Message() { Msg = "No connection started", Username = User.Name });
                Console.WriteLine("No connection started");
            }
        }

        public void ReturnToChat()
        {

            if (networkHandler.Connection != null && unableToSendMessage == true)
            {
                LoadChat(networkHandler.Connection._time);
                unableToSendMessage = false;
            }
            else if (checkedHistory == true && unableToSendMessage == true)
            {
                Messages.Clear();
                unableToSendMessage = false;
            }
            
        }

        public void LoadChat(object obj)
        {
            unableToSendMessage = true;
            checkedHistory= true;
            Messages.Clear();
            foreach (Message message in HelpFunctions.DetermineChat(obj.ToString(), _user))
            {
                Messages.Add(message);
            }
        }

        public void SearchForUser()
        {
            Chats.Clear();
            foreach (Chat c in HelpFunctions.SearchForUser(_sw.Sw))
            {
                Chats.Add(c);
            }
        }

        public void RemoveFilter()
        {
            Chats.Clear();
            foreach (Chat c in HelpFunctions.SearchForUser(""))
            {
                Chats.Add(c);
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
    }
}