using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class Participant: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ObservableCollection<Message> Chatter { get; set; }


        private bool _hasSentNewMessage;
        public bool HasSentNewMessage
        {
            get { return _hasSentNewMessage; }
            set { _hasSentNewMessage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSentNewMessage))); }
        }

        private bool _isTyping;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsTyping
        {
            get { return _isTyping; }
            set { _isTyping = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTyping))); }
        }

       
    }
}
