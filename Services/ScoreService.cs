using TheMonolith.Controllers;
using TheMonolith.Data;
using TheMonolith.Database.Repositories;
using TheMonolith.Models;

namespace TheMonolith.Services;

public class ScoreService
{
    private static readonly Dictionary<string, double> Weights = new()
    {
        [Score.GoogleType] = 1,
        [Score.LocationType] = 1,
        [Score.LocationType + LocationController.Regnecentralen.Name] = 2
    };

    private readonly ScoreRepository _scoreRepository;
    private readonly UserScoreRepository _userScoreRepository;

    public ScoreService(ScoreRepository scoreRepository, UserScoreRepository userScoreRepository)
    {
        _scoreRepository = scoreRepository;
        _userScoreRepository = userScoreRepository;
    }

    public async Task<double> GetWeightedScore(User user)
    {
        var score = 0.0;
        var entries = _scoreRepository.GetUserScores(user);
        await foreach (var (_, type, value) in entries)
        {
            var weight = Weights.GetValueOrDefault(type, 1.0);
            score += weight * value;
        }

        var userScores = await _userScoreRepository.ScoresForTarget(user.Id);
        score += userScores.Select(score => (double)score - 2.5).Sum();

        return score;
    }
}