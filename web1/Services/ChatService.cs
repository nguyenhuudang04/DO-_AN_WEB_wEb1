using Microsoft.Extensions.Configuration;

namespace web1.Services
{
    public class ChatService
    {
        private readonly string _apiKey;
        private readonly string _propertyId;
        private readonly string _ticketEmail;

        public ChatService(IConfiguration configuration)
        {
            _apiKey = configuration["TawkTo:ApiKey"];
            _propertyId = configuration["TawkTo:PropertyId"];
            _ticketEmail = configuration["TawkTo:TicketEmail"];
        }

        public void SetVisitorAttributes(string name, string email)
        {
            // Có thể thêm logic để set thông tin visitor qua Tawk.to API
        }
    }
} 