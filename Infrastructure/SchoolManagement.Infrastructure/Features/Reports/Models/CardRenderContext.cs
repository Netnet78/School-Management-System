namespace SchoolManagement.Infrastructure.Features.Reports.Models
{
    public sealed record CardRenderContext
    {
        public float SlotWidth { get; init; }

        public float SlotHeight { get; init; }

        public float ScaleX { get; init; } = 1f;

        public float ScaleY { get; init; } = 1f;

        public float OffsetX { get; init; }

        public float OffsetY { get; init; }
    }
}
