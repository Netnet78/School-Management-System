using SchoolManagement.Assets;

namespace SchoolManagement.Tests.Features.Reports
{
    public class FontResourceResolverTests
    {
        [Fact]
        public void ResolveFontSource_KnownEmbeddedFonts_ReturnPackUris()
        {
            var expected = new Dictionary<string, string>
            {
                ["Khmer OS Battambang"] = "pack://application:,,,/SchoolManagement.Assets;component/Fonts/#Khmer OS Battambang",
                ["Khmer OS Bokor"] = "pack://application:,,,/SchoolManagement.Assets;component/Fonts/#Khmer OS Bokor",
                ["Khmer OS Muol Light"] = "pack://application:,,,/SchoolManagement.Assets;component/Fonts/#Khmer OS Muol Light",
                ["Khmer OS Muol"] = "pack://application:,,,/SchoolManagement.Assets;component/Fonts/#Khmer OS Muol",
                ["Noto Sans Khmer"] = "pack://application:,,,/SchoolManagement.Assets;component/Fonts/#Noto Sans Khmer",
            };

            foreach (var kvp in expected)
            {
                string resolved = FontResourceResolver.ResolveFontSource(kvp.Key);
                Assert.Equal(kvp.Value, resolved);
            }
        }

        [Fact]
        public void ResolveFontSource_SystemFont_ReturnsTrimmedOriginalName()
        {
            string resolved = FontResourceResolver.ResolveFontSource("  Times New Roman  ");

            Assert.Equal("Times New Roman", resolved);
        }

        [Fact]
        public void ResolveFontSource_EmptyValue_FallsBackToTimesNewRoman()
        {
            string resolved = FontResourceResolver.ResolveFontSource(string.Empty);

            Assert.Equal("Times New Roman", resolved);
        }
    }
}
