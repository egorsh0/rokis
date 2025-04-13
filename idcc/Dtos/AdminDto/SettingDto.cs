namespace idcc.Dtos.AdminDto;

public record ISetting
{
    public int Id { get; set; }
}

public record AnswerTime : ISetting
{
    public string Grade { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}

public record Count : ISetting
{
    public string Code { get; set; }
    public string Description { get; set; }
    public int Value { get; set; }
}

public record GradeLevel : ISetting
{
    public string Grade { get; set; }
    public double Level { get; set; }
}

public record Persent : ISetting
{
    public string Code { get; set; }
    public string Description { get; set; }
    public double Value { get; set; }
}

public record Weight : ISetting
{
    public string Grade { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}