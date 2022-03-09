using ChatClient.Models;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services
{
    public class ChatService : IChatService
    {

        public event Action<User> ParticipantLoggedIn;
        public event Action<string, string> NewTextMessage;
        public event Action<string, byte[]> NewImageMessage;
        public event Action<string> ParticipantTyping;


        private IHubProxy hubProxy;
        private HubConnection connection;
        private string url = "http://localhost:9090/signalchat";

        public async Task ConnectAsync()
        {
            connection = new HubConnection(url);
            hubProxy = connection.CreateHubProxy("ChatHub");

            hubProxy.On<User>("ParticipantLogin", (u) => ParticipantLoggedIn?.Invoke(u));
            hubProxy.On<string, string>("UnicastTextMessage", (n, m) => NewTextMessage?.Invoke(n, m));
            hubProxy.On<string, byte[]>("UnicastPictureMessage", (n, m) => NewImageMessage?.Invoke(n, m));
            hubProxy.On<string>("ParticipantTyping", (p) => ParticipantTyping?.Invoke(p));


            ServicePointManager.DefaultConnectionLimit = 10;
            await connection.Start();
        }

        public async Task<List<User>> LoginAsync(string name)
        {
            return await hubProxy.Invoke<List<User>>("Login", new object[] { name });
        }

        public async Task SendUnicastMessageAsync(string recepient, string msg)
        {
            await hubProxy.Invoke("UnicastTextMessage", new object[] { recepient, msg });
        }

        public async Task SendUnicastMessageAsync(string recepient, byte[] img)
        {
            await hubProxy.Invoke("UnicastImageMessage", new object[] { recepient, img });
        }

        public async Task TypingAsync(string recepient)
        {
            await hubProxy.Invoke("Typing", recepient);
        }
    }
}
