using idcc.Models.Dto;

namespace idcc.Infrastructures.Interfaces;

public interface IGraphGenerate
{
    byte[] Generate(List<FinalTopicData> topicDatas);
}