using ScoreTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreTracker.Repository.Abstraction
{
    public interface ILocalDataRepository
    {
        Task Initalize();
        Task<int> SaveTeamDetailsAsync(Team team);
        Task<int> SaveGameDetailsAsync(Game game);
        Task<List<GameDetails>> GetAllGameDetailssAsync(); 
        Task<Game> GetGameAsync(int gameId);
        Task<List<Team>> GetTeams(int[] teams);
        Task SaveScoreAsync(Score score);
        Task ClearScoresForGameAsync(Game game);
        Task RemoveScoreAsync(Score score);
        Task<List<Score>> GetGameScoreAsync(Game game);
        Task<int> DeleteMatch(Game game);
    }
}
