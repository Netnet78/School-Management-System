namespace SchoolManagement.Presentation.Shared.Contracts
{
    public interface IMessageService
    {
        public MessageResult Show(string message, string title = "Message", MessageButton button = MessageButton.OK, MessageIcon icon = MessageIcon.None, int? autoHide = null);
    }
}
