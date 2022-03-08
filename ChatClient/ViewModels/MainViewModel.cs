using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ChatViewModel chatViewModel { get; }

        public MainViewModel(ChatViewModel chatViewModel)
        {
            this.chatViewModel = chatViewModel;
        }
    }
}
