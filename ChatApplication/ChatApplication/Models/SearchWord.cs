using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication.Models
{
    public class SearchWord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private String _sw;

        public String Sw
        {
            get
            {
                return _sw;
            }
            set
            {
                _sw = value;
                OnPropertyChanged("Sw");
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
