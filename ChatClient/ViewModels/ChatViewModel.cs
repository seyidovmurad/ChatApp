using ChatClient.Commands;
using ChatClient.Models;
using ChatClient.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatClient.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ChatViewModel: BaseViewModel
    {
        private readonly IChatService _chatService;



        #region Connect
        private ICommand _connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ?? (_connectCommand = new RelayCommandAsync(() => Connect()));
            }
        }

        private async Task<bool> Connect()
        {
            try
            {
                await _chatService.ConnectAsync();
                ErrorMessage = string.Empty;
                return true;
            }
            catch (Exception) { ErrorMessage = "Check your network"; return false; }
        }
        #endregion

        #region Typing Command
        private ICommand _typingCommand;
        public ICommand TypingCommand
        {
            get
            {
                return _typingCommand ?? (_typingCommand =
                    new RelayCommandAsync(() => Typing(), (o) => CanUseTypingCommand()));
            }
        }

        private async Task<bool> Typing()
        {
            try
            {
                await _chatService.TypingAsync(SelectedParticipant.Name);
                return true;
            }
            catch (Exception) { return false; }
        }

        private bool CanUseTypingCommand()
        {
            return true;
        }
        #endregion

        #region Send Text Message Command
        private ICommand _sendTextMessageCommand;
        public ICommand SendTextMessageCommand
        {
            get
            {
                return _sendTextMessageCommand ?? (_sendTextMessageCommand =
                    new RelayCommandAsync(() => SendTextMessage(), (o) => CanSendTextMessage()));
            }
        }

        private async Task<bool> SendTextMessage()
        {
            try
            {
                var recepient = _selectedParticipant.Name;
                await _chatService.SendUnicastMessageAsync(recepient, _textMessage);
                return true;
            }
            catch (Exception) { return false; }
            finally
            {
                Message msg = new Message
                {
                    Author = UserName,
                    Text = _textMessage,
                    Time = DateTime.Now,
                };
                SelectedParticipant.Chatter.Add(msg);
                Text = string.Empty;
            }
        }

        private bool CanSendTextMessage()
        {
            return (!string.IsNullOrEmpty(Text) && 
                _selectedParticipant != null);
        }
        #endregion

        #region Send Picture Message Command
        //private ICommand _sendImageMessageCommand;
        //public ICommand SendImageMessageCommand
        //{
        //    get
        //    {
        //        return _sendImageMessageCommand ?? (_sendImageMessageCommand =
        //            new RelayCommandAsync(() => SendImageMessage(), (o) => CanSendImageMessage()));
        //    }
        //}

        //private async Task<bool> SendImageMessage()
        //{
        //    var pic = dialogService.OpenFile("Select image file", "Images (*.jpg;*.png)|*.jpg;*.png");
        //    if (string.IsNullOrEmpty(pic)) return false;

        //    var img = await Task.Run(() => File.ReadAllBytes(pic));

        //    try
        //    {
        //        var recepient = _selectedParticipant.Name;
        //        await _chatService.SendUnicastMessageAsync(recepient, img);
        //        return true;
        //    }
        //    catch (Exception) { return false; }
        //    finally
        //    {
        //        ChatMessage msg = new ChatMessage { Author = UserName, Picture = pic, Time = DateTime.Now, IsOriginNative = true };
        //        SelectedParticipant.Chatter.Add(msg);
        //    }
        //}

        //private bool CanSendImageMessage()
        //{
        //    return (IsConnected && _selectedParticipant != null && _selectedParticipant.IsLoggedIn);
        //}
        #endregion

        #region Properties
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasErrorMessage));
            }
        }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        private string _textMessage;
        public string Text
        {
            get { return _textMessage; }
            set
            {
                _textMessage = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Participant> _participants = new ObservableCollection<Participant>();

        public ObservableCollection<Participant> Participants
        {
            get { return _participants; }
            set
            {
                _participants = value;
                OnPropertyChanged();
            }
        }

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get { return _selectedParticipant; }
            set
            {
                _selectedParticipant = value;
                if (SelectedParticipant.HasSentNewMessage) SelectedParticipant.HasSentNewMessage = false;
                OnPropertyChanged();
            }
        }
        #endregion
        public ChatViewModel(ChatService service)
        {
            _chatService = service;
            _chatService.ParticipantTyping += Typing;
            _chatService.NewTextMessage += NewTextMessage;
            _chatService.NewImageMessage += NewImageMessage;
            ConnectCommand.Execute(null);
        }
        #region Methods
        private void NewImageMessage(string name, byte[] pic)
        {
            var imgsDirectory = Path.Combine(Environment.CurrentDirectory, "Image Messages");
            if (!Directory.Exists(imgsDirectory)) Directory.CreateDirectory(imgsDirectory);

            var imgsCount = Directory.EnumerateFiles(imgsDirectory).Count() + 1;
            var imgPath = Path.Combine(imgsDirectory, $"IMG_{imgsCount}.jpg");

            ImageConverter converter = new ImageConverter();
            using (Image img = (Image)converter.ConvertFrom(pic))
            {
                img.Save(imgPath);
            }

            Message cm = new Message { Author = name, Picture = imgPath, Time = DateTime.Now };
            var sender = _participants.Where(u => string.Equals(u.Name, name)).FirstOrDefault();
            Task.Run(() => sender.Chatter.Add(cm)).Wait();

            if (!(SelectedParticipant != null && sender.Name.Equals(SelectedParticipant.Name)))
            {
                Task.Run(() => sender.HasSentNewMessage = true).Wait();
            }
        }

        private void NewTextMessage(string name, string msg)
        {
            Message cm = new Message{ Author = name, Text = msg, Time = DateTime.Now };
            var sender = _participants.Where((u) => string.Equals(u.Name, name)).FirstOrDefault();
            Task.Run(() => sender.Chatter.Add(cm)).Wait();

            if (!(SelectedParticipant != null && sender.Name.Equals(SelectedParticipant.Name)))
            {
                Task.Run(() => sender.HasSentNewMessage = true).Wait();
            }
        }

        private void Typing(string name)
        {
            var person = Participants.Where((p) => string.Equals(p.Name, name)).FirstOrDefault();
            if (person != null && !person.IsTyping)
            {
                person.IsTyping = true;
                Observable.Timer(TimeSpan.FromMilliseconds(1500)).Subscribe(t => person.IsTyping = false);
            }
        }
        #endregion
    }
}
