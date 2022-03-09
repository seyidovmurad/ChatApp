using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR;

namespace ChatServerCS
{
    public class ChatHub : Hub<IClient>
    {
        private static ConcurrentBag<User> User = new ConcurrentBag<User>();

        public void UnicastTextMessage(string recepient, string message)
        {
            var sender = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(sender) &&
                !string.IsNullOrEmpty(message))
            {
                User client = User.FirstOrDefault(user => user.Name == recepient);
                Clients.Client(client.ID).UnicastTextMessage(sender, message);
            }
        }

        public void UnicastImageMessage(string recepient, byte[] img)
        {
            var sender = Clients.CallerState.UserName;
            if (!string.IsNullOrEmpty(sender) &&
                img != null)
            {
                User client = User.FirstOrDefault(user => user.Name == recepient);
                Clients.Client(client.ID).UnicastPictureMessage(sender, img);
            }
        }

        public void Typing(string recepient)
        {
            if (string.IsNullOrEmpty(recepient)) return;
            var sender = Clients.CallerState.UserName;
            User client = User.FirstOrDefault(user => user.Name == recepient);
            Clients.Client(client.ID).ParticipantTyping(sender);
        }

        public List<User> Login(string name)
        {
            if (!User.Select(u => u.Name).Contains(name))
            {
                Console.WriteLine($"++ {name} logged in");
                User newUser = new User { Name = name, ID = Context.ConnectionId};
                User.Add(newUser); 
                Clients.CallerState.UserName = name; //Hal hazirda app-e login eden
                Clients.Others.ParticipantLogin(newUser);
                return User.ToList();
            }
            return null;
        }


    }
}