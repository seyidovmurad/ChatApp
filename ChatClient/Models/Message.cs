using ChatClient.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class Message : Entity
    {
        public string Text { get; set; }
        public string Author { get; set; }
        public DateTime Time { get; set; }
        public string? Picture { get; set; }

        ////////
        ///

        public string UserId { get; set; }

        public Participant User { get; set; }

        public string DoctorId { get; set; }

        public Doctor Doctor { get; set; }

    }
}
