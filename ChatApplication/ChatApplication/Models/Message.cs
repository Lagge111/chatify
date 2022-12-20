using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication.Models
{
    public class Message : INotifyPropertyChanged
    {

        private String _type;
        private String _msg;
        private String _username;
        public event PropertyChangedEventHandler PropertyChanged;
        public Message()
        {
        }

        public String Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public String Msg
        {
            get
            {
                return _msg;
            }
            set
            {
                _msg = value;
                OnPropertyChanged("Msg");
            }
        }

        public String Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
            }
        }

        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }
}


