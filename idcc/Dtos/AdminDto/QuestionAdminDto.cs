namespace idcc.Dtos.AdminDto;

public record QuestionAdminDto(
    string Content,
    string Topic,
    double Weight,
    bool IsMultipleChoice,
    List<AnswerAdminDto> Answers);

public record AnswerAdminDto(string Content, bool IsCorrect);