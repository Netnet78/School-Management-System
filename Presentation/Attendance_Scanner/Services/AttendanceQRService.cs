using School_Management.Core.Models;
using School_Management.Infrastructure.Repositories;
using School_Management.Presentation.Shared.Components;
using School_Management.Presentation.Shared.Enums;


namespace Attendance_Scanner.Services
{
    public interface IAttendanceQRService
    {
        Task<Student?> GetStudentByQRCode(string code);
    }

    public class AttendanceQRService : IAttendanceQRService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentQRRepository _studentQRRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMessageService _messageService;

        public AttendanceQRService(IStudentRepository sr, IStudentQRRepository sqr, IAttendanceRepository atr, IMessageService ims)
        {
            _studentRepository = sr;
            _studentQRRepository = sqr;
            _attendanceRepository = atr;
            _messageService = ims;
        }

        public async Task<Student?> GetStudentByQRCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                _messageService.Show("QR Code cannot be null or empty!", "Argument Exception", System.Windows.MessageBoxButton.OK, MessageBoxIcon.Error);
                return null;
            }

            StudentQR? studentQR = await _studentQRRepository.GetByQRValueAsync(code);

            if (studentQR == null)
            {
                _messageService.Show(
                    "QR Code does not exist!",
                    "Null Value Error",
                    System.Windows.MessageBoxButton.OK,
                    MessageBoxIcon.Error);
                return null;
            }

            if (studentQR.IsActive == false)
            {
                _messageService.Show("QR Code has been replaced and expired!", "Expired QR Code", icon: MessageBoxIcon.Error);
                return null;
            }

            return studentQR?.Student;
        }
    }
}
