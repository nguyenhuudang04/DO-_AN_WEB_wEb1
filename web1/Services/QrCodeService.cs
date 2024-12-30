using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;

namespace web1.Services
{
    public class QrCodeService
    {
        private readonly ILogger<QrCodeService> _logger;

        public QrCodeService(ILogger<QrCodeService> logger)
        {
            _logger = logger;
        }

        public string GenerateQrCode(string content)
        {
            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);
                    return Convert.ToBase64String(qrCodeBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                throw;
            }
        }

        public string GeneratePaymentQrCode(decimal amount, string orderId)
        {
            try
            {
                string bankName = "BIDV";
                string accountNumber = "31410001234567";
                string accountName = "NGUYEN VAN A";
                
                string content = $"Thong tin thanh toan:\n" +
                               $"Ngan hang: {bankName}\n" +
                               $"So tai khoan: {accountNumber}\n" +
                               $"Ten tai khoan: {accountName}\n" +
                               $"So tien: {amount:N0} VND\n" +
                               $"Ma don hang: #{orderId}\n" +
                               $"Noi dung: Thanh toan don hang #{orderId}";
                
                return GenerateQrCode(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payment QR code");
                throw;
            }
        }
    }
} 