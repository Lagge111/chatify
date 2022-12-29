using System;
using System.ComponentModel;

namespace ChatApplication.Models
{
    public class User : INotifyPropertyChanged

    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _paddress = "127.0.0.1";
        private int _port = 8000;
        private string _name;

        public User()
        {
        }
        
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }
        
        public String PAddress
        {
            get
            {
                return _paddress;
            }
            set
            {
                _paddress = value;
                OnPropertyChanged("PAddress");
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
