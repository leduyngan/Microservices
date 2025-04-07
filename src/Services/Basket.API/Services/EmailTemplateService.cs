using System.Text;
using Shared.Configurations;

namespace Basket.API.Services;

public class EmailTemplateService
{
    protected readonly BackgroundJobSettings BackgroundJobSettings;

    public EmailTemplateService(BackgroundJobSettings settings)
    {
        BackgroundJobSettings = settings;
    }


    private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string _tmplFolder = Path.Combine(_baseDirectory, "EmailTemplates");

    protected string ReadEmailTemplateContent(string templateEmailName, string fomart = "html")
    {
        var filePath = Path.Combine(_tmplFolder, templateEmailName + "." + fomart);
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var streamReader = new StreamReader(fileStream, Encoding.Default);
        var emailText = streamReader.ReadToEnd();
        streamReader.Close();
        
        return emailText;
    }
}