namespace idcc.Dtos;

public class AuthResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();
    // Или добавлять UserId, если нужно
    public string? UserId { get; set; }
    public bool LinkedToCompany  { get; set; }
    public CompanyInfoDto? Company { get; set; }
}