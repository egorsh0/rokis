namespace idcc.Dtos;

public record AnswerTimeDto(string Grade, double Average, double Min, double Max);

public record CountDto(string Code, string Description, int Value);
public record GradeDto(string Name, string Code, string Description);

public record GradeLevelDto(string Grade, double Level);
public record GradeRelationDto(string Start, string End);

public record PersentDto(string Code, string Description, double Value);
public record DirectionDto(string Name, string Code, string Description);
public record DiscountRuleDto(int MinQuantity, int? MaxQuantity, decimal DiscountRate);

public record WeightDto(string Grade, double Min, double Max);