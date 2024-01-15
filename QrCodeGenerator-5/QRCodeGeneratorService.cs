using iText.IO.Image;
using QrCodeGenerator_5.Properties;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace QrCodeGenerator
{
    public class QRCodeGeneratorService
    {
        public Bitmap GenerateQRCode(string content, bool isDotted)
        {
            
            Bitmap icon = new Bitmap(Resources.Zauner_small);

            QRCodeData qrCodeData;
            Bitmap qr;

            var qrGenerator = new QRCodeGenerator();

            qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            //using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            if (isDotted)
            {
                ArtQRCode qrCode = new ArtQRCode(qrCodeData);
                qr = qrCode.GetGraphic(50, Color.DarkBlue, Color.White, Color.White, icon, 0.8, true);


            }
            else
            {
                QRCode qrCode = new QRCode(qrCodeData);
                qr = qrCode.GetGraphic(50, Color.DarkBlue, Color.White, icon, 25, 0, true);
            }


            return qr;
        }
        public int[] BitmapToIntArray(Bitmap bitmap)
        {
            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bitmap.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);

            // Unlock the bits.
            bitmap.UnlockBits(bmpData);

            // Convert byte array to int array
            int[] intArray = new int[bitmap.Width * bitmap.Height];
            int byteIndex = 0;
            for (int i = 0; i < intArray.Length; i++)
            {
                // Assuming 24bppRgb or 32bppArgb pixel format
                int blue = rgbValues[byteIndex++];
                int green = rgbValues[byteIndex++];
                int red = rgbValues[byteIndex++];
                int alpha = bmpData.PixelFormat == PixelFormat.Format24bppRgb ? 255 : rgbValues[byteIndex++]; // Alpha channel for 32bppArgb

                intArray[i] = (alpha << 24) | (red << 16) | (green << 8) | blue;
            }

            return intArray;
        }

    }
}
