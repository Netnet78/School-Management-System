namespace SchoolManagement.Core.Features.Reports.Models
{
    public sealed record CardSheetLayout
    {
        public string PageSize { get; init; } = "A4";

        public bool Landscape { get; init; }

        public int Columns { get; init; } = 1;

        public int Rows { get; init; } = 1;

        public float Margin { get; init; } = 15f;

        public float HorizontalSpacing { get; init; }

        public float VerticalSpacing { get; init; }

        public bool ShowHeader { get; init; }

        public bool ShowFooter { get; init; }

        public string? HeaderText { get; init; }

        public string? FooterText { get; init; }

        public int CardsPerPage => Math.Max(1, Columns * Rows);
    }
}
