using idcc.Dtos;

namespace idcc.Infrastructures.Interfaces;

public interface IGraphGenerate
{
    byte[]? Generate(List<FinalTopicData> topicDatas, float resize);
}