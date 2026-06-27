namespace CornerstoneZearing.Interfaces;

public interface IMailService
{
    Task SendAsync(string to, string subject, string body);
}