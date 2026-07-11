using Moq;
using SchoolManagement.Application.Features.Auth.Contracts;
using SchoolManagement.Application.Features.Files.Contracts;
using SchoolManagement.Application.Features.Reports.Generators;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Shared.Models;

namespace SchoolManagement.Tests.Features.Reports
{
    public class StudentCardGeneratorTests
    {
        [Fact]
        public async Task GenerateAsync_SetsStudentCardReportTag()
        {
            var studentClassRepository = new Mock<IStudentClassRepository>();
            studentClassRepository
                .Setup(r => r.FindAsync(
                    It.IsAny<IEnumerable<FilterCondition<StudentClass>>?>(),
                    1,
                    10,
                    It.IsAny<IEnumerable<SortCriteria<StudentClass>>?>(),
                    It.IsAny<string[]?>()))
                .ReturnsAsync(Enumerable.Empty<StudentClass>());

            studentClassRepository
                .Setup(r => r.CountAsync(
                    It.IsAny<IEnumerable<FilterCondition<StudentClass>>?>(),
                    null,
                    null))
                .ReturnsAsync(0);

            var photoFetchService = new Mock<IPhotoFetchService>();
            var authorizationService = new Mock<IAuthorizationService>();
            var generator = new StudentCardGenerator(
                studentClassRepository.Object,
                photoFetchService.Object,
                authorizationService.Object);

            var result = await generator.GenerateAsync(new StudentCardReportRequest());

            var cardResult = Assert.IsType<CardReportResult>(result);
            Assert.Equal("student-card", cardResult.ReportTag);
            Assert.Empty(cardResult.CardGroups ?? []);
        }
    }
}
