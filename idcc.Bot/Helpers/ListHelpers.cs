namespace idcc.Bot.Helpers;

public static class ListHelpers
{
    public static T[] ShuffleArray<T>(T[]? array)
    {
        var random = new Random();
        return array.OrderBy(x => random.Next()).ToArray();
    }
}