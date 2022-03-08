namespace ChatServerCS
{
    public interface IClient
    {
        void UnicastTextMessage(string sender, string message);
        void UnicastPictureMessage(string sender, byte[] img);
        void ParticipantTyping(string sender);
    }
}