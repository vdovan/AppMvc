namespace AppMvc.Services.Mail
{
    public class MailConfig
    {
        public string? Mail { get; set; }
        public string? DisplayName { get; set; }
        public string? Pwd { get; set; }
        public string? SmtpHost { get; set; }
        public int Port { get; set; }
    }
}