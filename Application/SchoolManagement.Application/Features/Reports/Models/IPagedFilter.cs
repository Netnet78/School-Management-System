namespace SchoolManagement.Application.Features.Reports.Models
{
    public interface IPagedFilter
    {
        int Page { get; set; }
        int? PageSize { get; set; }
    }
}
