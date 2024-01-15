using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using Image = iText.Layout.Element.Image;

namespace QrCodeGenerator
{
    public class PDFService
    {
        public void AddQrCodeToPdf(string pdfPath, Bitmap qrCodeImage, float size, float positionX, float positionY)
        {
            try
            {
                var img = ImageToByteArray(qrCodeImage);
                string dest = pdfPath.Replace(".pdf", "_QR.pdf");


                PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfPath), new PdfWriter(dest));
                pdfDocument.SetDefaultPageSize(PageSize.A3);


                if (pdfDocument != null)
                {
                    PdfPage page = pdfDocument.GetPage(1);
                    page.SetIgnorePageRotationForContent(true);
                    var rotation = page.GetRotation();
                    PageSize pageSize = (PageSize)page.GetPageSizeWithRotation();
                    size = size * pageSize.GetHeight() / 100;
                    positionX = (positionX / 100) * pageSize.GetWidth();
                    positionY = (positionY / 100) * pageSize.GetHeight();

                    // Document to add layout elements: paragraphs, images etc
                    Document document = new Document(pdfDocument, pageSize, true);

                    // Load image from disk
                    ImageData imageData = ImageDataFactory.Create(img);

                    // Create layout image object and provide parameters. Page number = 1
                    Image image = new Image(imageData).ScaleAbsolute(size, size).SetFixedPosition(positionX, positionY);

                    // This adds the image to the page
                    document.Add(image);

                    // When you use Document, you should close it rather than PdfDocument instance
                    document.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing {pdfPath}: {ex.Message}", ex);
            }
        }
        public byte[] ImageToByteArray(System.Drawing.Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
