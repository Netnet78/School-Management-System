using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Presentation;
using School_Management.Presentation.Shared.Components;

namespace School_Management.Presentation.Shared.Services
{
    public class MessageService : IMessageService
    {
        /// <summary>
        /// Show a customized message box
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="autoHide"></param>
        /// <returns></returns>
        public MessageResult Show(
            string message,
            string title = "Message",
            MessageButton button = MessageButton.OK,
            MessageIcon icon = MessageIcon.None,
            int? autoHide = null
        )
        {
            return CustomMessageBox.Show(message, title, button, icon, autoHide);
        }
    }
}
