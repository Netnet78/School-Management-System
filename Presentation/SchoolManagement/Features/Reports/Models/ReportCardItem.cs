using CommunityToolkit.Mvvm.ComponentModel;
using SchoolManagement.Application.Features.Reports.Models;
using SchoolManagement.Core.Features.Reports.Models;

namespace SchoolManagement.Presentation.Features.Reports.Models
{
    public partial class ReportCardItem : ObservableObject
    {
        public ReportDefinition Definition { get; }

        public string DisplayName => Definition.DisplayNameKhmer;

        public string DisplayNameEn => Definition.DisplayName;

        public string Description => Definition.Description;

        public string IconKind => Definition.IconKind;

        public ReportTag Key => Definition.Key;

        public int SortOrder => Definition.SortOrder;

        [ObservableProperty]
        private bool _isSelected;

        public ReportCardItem(ReportDefinition definition)
        {
            Definition = definition;
        }
    }
}
