using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DBPQRPermanent.Services
{
    public class QRService
    {
        public byte[] GenerateQRCode(string data)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new QRCode(qrCodeData))
                using (var bitmap = qrCode.GetGraphic(20))
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return stream.ToArray();
                }
            }
        }
    }
}
