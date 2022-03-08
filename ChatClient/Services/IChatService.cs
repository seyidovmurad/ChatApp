using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services
{
    public interface IChatService
    {
        event Action<string, string> NewTextMessage;
        event Action<string, byte[]> NewImageMessage;
        event Action<string> ParticipantTyping;

        Task ConnectAsync();
        Task SendUnicastMessageAsync(string recepient, string msg);
        Task SendUnicastMessageAsync(string recepient, byte[] img);
        Task TypingAsync(string recepient);
    }
}
