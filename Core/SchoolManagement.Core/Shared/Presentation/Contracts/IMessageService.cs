using SchoolManagement.Core.Shared.Enums;

namespace SchoolManagement.Core.Shared.Presentation.Contracts
{
    public interface IMessageService
    {
        public MessageResult Show(string message, string title = "Message", MessageButton button = MessageButton.OK, MessageIcon icon = MessageIcon.None, int? autoHide = null);
    }
}
