namespace idcc.Endpoints;

public static class PingEndpoint
{
    public static void RegisterPingEndpoints(this IEndpointRouteBuilder routes)
    {
        var pings = routes.MapGroup("/api/v1/ping");
      
        pings.MapGet("", () => Results.Ok("Pong"));
    }
}