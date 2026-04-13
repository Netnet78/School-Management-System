using School_Management.Core.Enums;

namespace School_Management.Core.Interfaces.Presentation
{
    public interface IMessageService
    {
        public MessageResult Show(string message, string title = "Message", MessageButton button = MessageButton.OK, MessageIcon icon = MessageIcon.None, int? autoHide = null);
    }
}
