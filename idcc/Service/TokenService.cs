using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using idcc.Context;
using idcc.Dtos;
using idcc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace idcc.Service;

public interface ITokenService
{
    Task<LoginDto> CreateTokensAsync(ApplicationUser user);
    Task<LoginDto?> RefreshAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
}

public class TokenService : ITokenService
{
    private const string _alphabet =
        "abcdefghijklmnopqrstuvwxyz" +
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
        "0123456789";
    
    private readonly IdccContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TokenService> _logger;

    private readonly int _accessMinutes;
    private readonly int _refreshDays;
    private readonly byte[] _jwtKey;

    public TokenService(IdccContext context,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TokenService> logger)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

        _accessMinutes = _configuration.GetValue("Jwt:AccessMinutes", 15);
        _refreshDays = _configuration.GetValue("Jwt:RefreshDays", 30);
        _jwtKey = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);
    }

    public async Task<LoginDto> CreateTokensAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var cred = new SigningCredentials(new SymmetricSecurityKey(_jwtKey),
            SecurityAlgorithms.HmacSha256);
        
        // ── Access token ─────────────────────────────────────
        
        var newStamp = await _userManager.GetSecurityStampAsync(user);
        // ── Базовые claims ───────────────────────────────────
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            //new("IP", HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""),
            //new("UserAgent", Request.Headers["User-Agent"].ToString()),
            new("AspNet.Identity.SecurityStamp", newStamp)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessToken = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessMinutes),
            Issuer = _configuration["Jwt:Issuer"]!,
            Audience = _configuration["Jwt:Audience"]!,
            SigningCredentials = cred
        });
        
        var utcTime = DateTime.UtcNow;
        user.PasswordLastChanged = utcTime;
        await _userManager.UpdateAsync(user);
        var access = tokenHandler.WriteToken(accessToken);
        
        // ── Refresh token ─────────────────────────────────────

        var refresh = new RefreshToken()
        {
            Token = RandomNumberGenerator.GetString(_alphabet, 64),
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(_refreshDays),
            UserId = user.Id
        };
        _context.RefreshTokens.Add(refresh);
        await _context.SaveChangesAsync();
        WriteRefreshCookie(refresh.Token, refresh.Expires);
        
        return new LoginDto(access, _accessMinutes * 60, roles.ToList());
    }

    public async Task<LoginDto?> RefreshAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (token == null || !token.IsActive)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            return null;
        }

        // помечаем старый
        token.Revoked = DateTime.UtcNow;

        var login = await CreateTokensAsync(token.User);
        await _context.SaveChangesAsync();
        return login;
    }
    
    public async Task RevokeAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken && r.IsActive);

        if (token is null) return;

        token.Revoked = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked for user {UserId}", token.UserId);
    }
    
    private void WriteRefreshCookie(string token, DateTime expiresUtc)
    {
        var opts = new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Expires  = expiresUtc
        };
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("refreshToken", token, opts);
    }
}