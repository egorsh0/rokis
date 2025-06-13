namespace idcc.Dtos;

public record UserTopicDto(int Id, int SessionId, TopicDto Topic, GradeDto Grade, double Weight, bool IsFinished, bool WasPrevious, bool Actual, int Count);