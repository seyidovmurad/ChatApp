using ChatClient.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class Doctor : Entity
    {

        public string Name { get; set; }

        public ObservableCollection<Message> Chatter { get; set; } = new ObservableCollection<Message>();


    }
}
