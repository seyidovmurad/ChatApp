using ChatClient.Commands;
using ChatClient.Data;
using ChatClient.Models;
using ChatClient.Models.Abstractions;
using ChatClient.Services;
using Microsoft.EntityFrameworkCore;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ChatClient.ViewModels
{

    public static class ChatViewModelExtensions
    {
        public static void DetachLocal<T>(this DbContext context, T t, string entryId)
    where T : Entity
        {
            var local = context.Set<T>()
                .Local
                .FirstOrDefault(entry => entry.Id.Equals(entryId));
            if (local != null)
            {
                context.Entry(local).State = EntityState.Detached;
            }
            context.Entry(t).State = EntityState.Modified;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class ChatViewModel : BaseViewModel
    {
        private readonly IChatService _chatService;
        private TaskFactory ctxTaskFactory;
        public AppDbContext dbContext { get; set; }

        Thread t;

        Random rand = new Random();

        public string Id { get; set; }

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

        #region Login Command
        private ICommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ?? (_loginCommand =
                    new RelayCommandAsync(() => Login(), (o) => CanLogin()));
            }
        }

        private async Task<bool> Login()
        {
            var rand = new Random();
            try
            {
                List<User> users = new List<User>();
                foreach (var doc in Doctors)
                {
                    await _chatService.LoginAsync(doc.Name);
                }
                users = await _chatService.LoginAsync(_userName);
                if (users != null)
                {
                    users.ForEach(u => Participants.Add(new Participant { Name = u.Name, Id = Id }));
                    if (Participants.Where(p => p.Name == _userName).Count() == 1)
                    {
                        dbContext.Add(new Participant() { Name = UserName, Id = Id });
                        dbContext.SaveChanges();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception) { return false; }
        }

        private bool CanLogin()
        {
            return true;
        }
        #endregion

        #region Typing Command
        //private ICommand _typingCommand;
        //public ICommand TypingCommand
        //{
        //    get
        //    {
        //        return _typingCommand ?? (_typingCommand =
        //            new RelayCommandAsync(() => Typing(), (o) => CanUseTypingCommand()));
        //    }
        //}

        //private async Task<bool> Typing()
        //{
        //    try
        //    {
        //        await _chatService.TypingAsync(SelectedParticipant.Name);
        //        return true;
        //    }
        //    catch (Exception) { return false; }
        //}

        //private bool CanUseTypingCommand()
        //{
        //    return true;
        //}
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
                var recepient = SelectedDoctor.Name;
                await _chatService.SendUnicastMessageAsync(recepient, _textMessage);
                return true;
            }
            catch (Exception) { return false; }
            finally
            {
                Message msg = new Message
                {
                    Id = Id,
                    Author = UserName,
                    Text = _textMessage,
                    Time = DateTime.Now,
                    Doctor = SelectedDoctor,
                    User = Participants.FirstOrDefault(p => p.Name == UserName)
                };
                SelectedDoctor.Chatter.Add(msg);
                dbContext.DetachLocal<Message>(msg, msg.Id);
                dbContext.Add(msg);
                Text = string.Empty;
            }
        }

        private bool CanSendTextMessage()
        {
            return true;
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

        //    var img = await ctxTaskFactory.StartNew(() => File.ReadAllBytes(pic));

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

        private ObservableCollection<Participant> _participants;

        public ObservableCollection<Participant> Participants
        {
            get { return _participants; }
            set
            {
                _participants = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Doctor> _doctors;

        public ObservableCollection<Doctor> Doctors
        {
            get { return _doctors; }
            set
            {
                _doctors = value;
                OnPropertyChanged();
            }
        }

        private Doctor _selectedDoctor;
        public Doctor SelectedDoctor
        {
            get { return _selectedDoctor; }
            set
            {
                _selectedDoctor = value;
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
            _chatService.ParticipantLoggedIn += ParticipantLogin;
            ConnectCommand.Execute(null);

            ctxTaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());

            dbContext = new AppDbContext();

            Participants = new ObservableCollection<Participant>(dbContext.Participants.Include(r => r.Chatter).ThenInclude(rs => rs.Doctor).ToList());
            Doctors = new ObservableCollection<Doctor>(dbContext.Doctors.Include(r => r.Chatter).ThenInclude(rs => rs.User).ToList());


            Id = rand.Next(1, 100000).ToString();

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
            var sender = Participants.Where(u => string.Equals(u.Name, name)).FirstOrDefault();
            ctxTaskFactory.StartNew(() => sender.Chatter.Add(cm)).Wait();

            //if (!(SelectedParticipant != null && sender.Name.Equals(SelectedParticipant.Name)))
            //{
            //    ctxTaskFactory.StartNew(() => sender.HasSentNewMessage = true).Wait();
            //}
        }

        private void NewTextMessage(string name, string msg)
        {
            var sender = Participants.Where((u) => string.Equals(u.Name, name)).FirstOrDefault();
            var user = Doctors.FirstOrDefault(p => p.Name == SelectedDoctor.Name);
            Message cm = new Message { Author = name, Text = msg, Time = DateTime.Now, Doctor = user, User = sender, Id = Id };
            ctxTaskFactory.StartNew(() => sender.Chatter.Add(cm)).Wait();
            //ctxTaskFactory.StartNew(() =>
            //{
            //    dbContext.Add(cm);
            //});

            //dbContext.SaveChanges();

            //if (!(SelectedParticipant != null && sender.Name.Equals(SelectedParticipant.Name)))
            //{
            //    ctxTaskFactory.StartNew(() => sender.HasSentNewMessage = true).Wait();
            //}
        }

        private void Typing(string name)
        {
            //var person = Participants.Where((p) => string.Equals(p.Name, name)).FirstOrDefault();
            //if (person != null && !person.IsTyping)
            //{
            //    person.IsTyping = true;
            //    Observable.Timer(TimeSpan.FromMilliseconds(1500)).Subscribe(t => person.IsTyping = false);
            //}
        }

        private void ParticipantLogin(User u)
        {

            //var ptp = Participants.FirstOrDefault(p => string.Equals(p.Name, u.Name));

            //ctxTaskFactory.StartNew(() => Participants.Add(new Participant
            //{
            //    Name = u.Name,
            //    Id = Id
            //})).Wait();

            //dbContext.Participants.Add(new Participant { Name = u.Name, Id = Id });
            //dbContext.SaveChanges();

        }
        #endregion
    }
}
