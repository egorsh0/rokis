using idcc.Dtos;

namespace idcc.Builders;

public class TopicNormalizerBuilder
{
    public Dictionary<string, Func<double, float>> Build(List<List<FinalTopicData>> allUsersTopics)
    {
        var allScoresByTopic = new Dictionary<string, List<double>>();

        foreach (var userTopics in allUsersTopics)
        {
            foreach (var topic in userTopics)
            {
                if (!allScoresByTopic.TryGetValue(topic.Topic, out var list))
                {
                    list = new List<double>();
                    allScoresByTopic[topic.Topic] = list;
                }
                list.Add(topic.Score);
            }
        }

        var result = new Dictionary<string, Func<double, float>>();

        var globalMin = double.MaxValue;
        var globalMax = double.MinValue;

        foreach (var values in allScoresByTopic.Values)
        {
            foreach (var score in values)
            {
                if (score > 0.00001) // фильтруем мусорные значения
                {
                    globalMin = Math.Min(globalMin, score);
                    globalMax = Math.Max(globalMax, score);
                }
            }
        }

        if (globalMin == double.MaxValue || globalMax == double.MinValue)
        {
            globalMin = 0.0;
            globalMax = 1.0;
        }

        foreach (var topic in allScoresByTopic.Keys)
        {
            result[topic] = score =>
            {
                if (Math.Abs(globalMax - globalMin) < 1e-6)
                    return 0.5f;

                var normalized = (float)((score - globalMin) / (globalMax - globalMin));
                return Math.Clamp(normalized, 0.05f, 1f);
            };
        }
        return result;
    }
}