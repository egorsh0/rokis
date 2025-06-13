namespace idcc.Endpoints;

public static class PingEndpoint
{
    public static void RegisterPingEndpoints(this IEndpointRouteBuilder routes)
    {
        var pings = routes.MapGroup("/api/ping");
      
        pings.MapGet("", () => Results.Ok("Pong"));
    }
}