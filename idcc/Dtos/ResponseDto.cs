namespace idcc.Dtos;

public record ResponseDto
{
    public List<string> Messages { get; init; }
    public string Message { get; init; }

    public ResponseDto(List<string> messages)
    {
        Messages = messages;
    }

    public ResponseDto(string message)
    {
        Message = message;
    }
}