using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Common.Logging;

public class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> _logger;

    public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Sending request to {Url} - Method {Method} - Version {Version}", request.RequestUri, request.Method, request.Version);
            
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Received a success response from {Url}", response.RequestMessage.RequestUri);
            }
            else
            {
                _logger.LogError("Received a non-success status code {StatusCode} from {Url}", (int)response.StatusCode, response.RequestMessage.RequestUri);
            }
        }
        catch (HttpRequestException ex)
            when(ex.InnerException is SocketException {SocketErrorCode: SocketError.ConnectionRefused})
        {
            var hostWithPort = request.RequestUri.IsDefaultPort 
                ? request.RequestUri.DnsSafeHost 
                : $"({request.RequestUri.DnsSafeHost}:{request.RequestUri.Port})";
            
            _logger.LogCritical(ex, "Unable to connect to {Host}. Please check the " +
                                    "Configuration/settings to ensure the correct URL for the service " + 
                                    "has been configured", hostWithPort);
        }

        return new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            RequestMessage = request,
        };
    }
}