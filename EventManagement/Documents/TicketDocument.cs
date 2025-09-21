using EventManagement.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EventManagement.Documents
{
    public class TicketDocument : IDocument
    {
        private readonly ViewTicketViewModel _model;

        public TicketDocument(ViewTicketViewModel model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A6.Landscape());
                    page.Margin(20);
                    page.Background(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));

                    page.Content()
                        .Border(1, Unit.Point).BorderColor(Colors.Grey.Lighten2)
                        .Padding(10)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("YOU ARE CONFIRMED FOR").FontSize(8).LetterSpacing(1).FontColor(Colors.Grey.Medium);
                                column.Item().Text(_model.EventTitle).FontSize(24).Bold();
                                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                column.Spacing(10);

                                row.ConstantItem(10);

                                column.Item().Row(detailsRow =>
                                {
                                    detailsRow.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("ATTENDEE").FontSize(8).FontColor(Colors.Grey.Medium);
                                        col.Item().Text(_model.AttendeeName).SemiBold();

                                        col.Spacing(10);

                                        col.Item().Text("VENUE").FontSize(8).FontColor(Colors.Grey.Medium);
                                        col.Item().Text(_model.Venue).SemiBold();
                                    });

                                    detailsRow.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("DATE & TIME").FontSize(8).FontColor(Colors.Grey.Medium);
                                        col.Item().Text(_model.EventDate.ToString("dd MMM yyyy, h:mm tt")).SemiBold();

                                        col.Spacing(10);

                                        col.Item().Text("REGISTRATION ID").FontSize(8).FontColor(Colors.Grey.Medium);
                                        col.Item().Text($"#{_model.RegistrationId:D6}").SemiBold();
                                    });
                                });
                            });

                            row.ConstantItem(20);

                            row.ConstantItem(100).BorderLeft(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).PaddingLeft(20)
                                .Column(qrColumn =>
                                {
                                    qrColumn.Item().Text("SCAN AT ENTRY").FontSize(8).FontColor(Colors.Grey.Medium).AlignCenter();
                                    qrColumn.Item().Image(_model.QrCodeImageUrl).FitArea();
                                    qrColumn.Item().Text("EVENTFLOW").FontSize(8).FontColor(Colors.Grey.Medium).AlignCenter();
                                });
                        });
                });
        }
    }
}