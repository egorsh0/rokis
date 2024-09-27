namespace idcc.Infrastructures.Interfaces;

public interface IScoreCalculate
{
    double GetScore(double weight, double k, int answered, int total);
}