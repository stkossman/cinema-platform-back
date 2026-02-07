namespace Cinema.Application.Common.Contracts;

public class TicketPurchasedMessage
{
    public Guid OrderId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public string TicketDownloadUrl { get; set; } = string.Empty;

    public TicketPurchasedMessage() { }
    
    public TicketPurchasedMessage(Guid orderId, string email, string name, string title, DateTime date, string url)
    {
        OrderId = orderId;
        UserEmail = email;
        UserName = name;
        MovieTitle = title;
        SessionDate = date;
        TicketDownloadUrl = url;
    }
}