using Moq;
using SchoolManagement.Application.Features.Files.Contracts;
using SchoolManagement.Application.Features.Reports.Generators;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Students.Contracts;
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
                    null,
                    null,
                    It.IsAny<IEnumerable<SortCriteria<StudentClass>>?>(),
                    It.IsAny<string[]?>()))
                .ReturnsAsync(Enumerable.Empty<StudentClass>());

            var photoFetchService = new Mock<IPhotoFetchService>();
            var generator = new StudentCardGenerator(
                studentClassRepository.Object,
                photoFetchService.Object);

            var result = await generator.GenerateAsync(new StudentCardFilter());

            var cardResult = Assert.IsType<CardReportResult>(result);
            Assert.Equal(ReportTag.StudentCard, cardResult.ReportTag);
            Assert.Empty(cardResult.CardGroups ?? []);
        }
    }
}
