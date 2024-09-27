namespace idcc.Infrastructures.Interfaces;

public interface IWeightCalculate
{
    double GetNewWeight(double actual, double current, double max, double gainPercent, double lessPercent, bool isCorrect);
}