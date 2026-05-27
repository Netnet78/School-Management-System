using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SchoolManagement.Assets;
using SchoolManagement.Core.Features.Reports.Models;
using SchoolManagement.Infrastructure.Features.Reports.Contracts;
using SchoolManagement.Infrastructure.Shared.Extensions;
using System.Diagnostics;

namespace SchoolManagement.Infrastructure.Features.Reports.Export.Rendering
{
    public class DefaultCardRenderer : ICardRenderer
    {
        public void RenderCard(IContainer container, ReportItemGroup cardGroup)
        {
            string cardPath = Path.Combine(ResourcePaths.Images, 
                "name_card_template.png");

            container.Layers(layers =>
            {
                Debug.WriteLineIf(File.Exists(cardPath), "Student card found!");
                // Background template
                layers.PrimaryLayer()
                    .Image(cardPath);

                // Overlay layer
                layers.Layer().Element(layer =>
                {
                    foreach (var item in cardGroup.Items)
                    {
                        layer.TranslateX(item.XPos)
                             .TranslateY(item.YPos)
                             .Element(e =>
                             {
                                 if (item.Value is byte[] imageBytes)
                                 {
                                     var (width, height) = imageBytes.GetImageSize();
                                     e.Width(width)
                                      .Height(height)
                                      .Image(imageBytes);
                                 }
                                 else if (item.Value is string text)
                                 {
                                     var textDescriptor = e.Text(text);

                                     textDescriptor.FontSize(
                                         item.FontSize > 0
                                             ? item.FontSize
                                             : 12);

                                     if (item.IsBold)
                                         textDescriptor.Bold();

                                     if (!string.IsNullOrWhiteSpace(item.FontColor))
                                     {
                                         textDescriptor.FontColor(
                                             Color.FromHex(item.FontColor));
                                     }
                                 }
                             });
                    }
                });
            });
        }

    }
}
