namespace ChessTournaments.Identity.Shared.Infrastructure;

public class Sender(IServiceProvider serviceProvider, ILogger<Sender> logger) : ISender
{
    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default
    )
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        logger.LogDebug(
            "Processing request {RequestType} with response type {ResponseType}",
            requestType.FullName ?? requestType.Name,
            responseType.IsGenericType
                ? responseType.ToString()
                : responseType.FullName ?? responseType.Name
        );

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

        var handler = serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            logger.LogError(
                "No handler registered for request type {RequestType}",
                requestType.Name
            );
            throw new InvalidOperationException(
                $"No handler registered for request type {requestType.Name}"
            );
        }

        var handleMethod = handlerType.GetMethod(
            nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle)
        );

        if (handleMethod == null)
        {
            logger.LogError(
                "Handle method not found on handler for request type {RequestType}",
                requestType.Name
            );
            throw new InvalidOperationException(
                $"Handle method not found on handler for request type {requestType.Name}"
            );
        }

        try
        {
            logger.LogDebug(
                "Invoking handler for {RequestType}",
                requestType.FullName ?? requestType.Name
            );
            var result = handleMethod.Invoke(handler, [request, cancellationToken]);

            if (result is Task<TResponse> task)
            {
                var response = await task;
                logger.LogDebug(
                    "Successfully processed request {RequestType}",
                    requestType.FullName ?? requestType.Name
                );
                return response;
            }

            throw new InvalidOperationException(
                $"Handler for request type {requestType.Name} did not return a Task<{responseType.Name}>"
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing request {RequestType}", requestType.Name);
            throw;
        }
    }
}
