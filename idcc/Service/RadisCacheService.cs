using StackExchange.Redis;

namespace idcc.Service;

public class RadisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RadisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }
    
    public async Task<string> GetCacheValueAsync(string key)
    {
        var connection = _connectionMultiplexer.GetDatabase();
        return (await connection.StringGetAsync(key))!;
    }

    public async Task SetCacheValueAsync(string key, string value)
    {
        var connection = _connectionMultiplexer.GetDatabase();
        await connection.StringSetAsync(key, value);
    }
}