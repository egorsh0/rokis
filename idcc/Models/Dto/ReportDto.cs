namespace idcc.Models.Dto;

public record ReportDto
{
    public string Name { get; set; }
    
    public DateTime StartSession { get; set; }
    public DateTime EndSession { get; set; }
    public double Score { get; set; }
    
    public Grade Grade { get; set; }
}

public record TopicData
{
    public string Name { get; set; }
    public double Score { get; set; }
}