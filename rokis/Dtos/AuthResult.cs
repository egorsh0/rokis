using rokis.Infrastructures;

namespace rokis.Dtos;

public class AuthResult
{
    public bool Succeeded { get; set; }
    public MessageCode MessageCode { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? UserId { get; set; }
    public bool LinkedToCompany  { get; set; }
    public CompanyInfoDto? Company { get; set; }
}

public record LoginDto(string Token, int ExpiresIn, List<string> Roles);