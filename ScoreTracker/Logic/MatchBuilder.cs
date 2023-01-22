using ScoreTracker.Models;
using ScoreTracker.Repository.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreTracker.Logic
{
    public class MatchBuilder
    {
        private ILocalDataRepository localDataRepository;

        public Team Team1 { get; private set; }
        public Team Team2 { get; private set; }
        public Game Game { get; private set; }

        public MatchBuilder(ILocalDataRepository localDataRepository) => this.localDataRepository = localDataRepository;

        public async Task<MatchBuilder> GetNewTeam1(string name, Color color)
        {
            Team1 = new Team()
            {
                TeamId = Guid.NewGuid().ToString(),
                Name = name,
                Color = color
            };

            var result = await localDataRepository.SaveTeamDetailsAsync(Team1);

            return result == 1 ? this : null;
        }

        public async Task<MatchBuilder> GetNewTeam2(string name, Color color)
        {
            Team2 = new Team()
            {
                TeamId = Guid.NewGuid().ToString(),
                Name = name,
                Color = color
            };

            var result = await localDataRepository.SaveTeamDetailsAsync(Team2);

            return result == 1 ? this : null;
        }

        public async Task<MatchBuilder> GenerateGame()
        {
            Game = new Game()
            {
                Team1Id = Team1.TeamId,
                Team2Id = Team2.TeamId,
                StartTime = DateTime.Now,
                Status = GameStatus.NotStarted,
            };

            var result = await localDataRepository.SaveGameDetailsAsync(Game);
            return result == 1 ? this : null;
        }

        internal Task<GameBuilder> InitializeGame(GameDetails selectedGame)
        {
            Team1 = new Team()
            {
                TeamId = selectedGame.Team1Id,
                Name = selectedGame.T1Name,
                Color = selectedGame.Team1Color
            };

            Team2 = new Team()
            {
                TeamId = selectedGame.Team2Id,
                Name = selectedGame.T2Name,
                Color = selectedGame.Team2Color
            };

            Game = new Game()
            {
                GameID = selectedGame.GameID,
                Team1Id = selectedGame.Team1Id,
                Team2Id = selectedGame.Team2Id,
                Status = selectedGame.Status,
                T1Point = selectedGame.T1Point,
                T2Point = selectedGame.T2Point,
                T1SetCount = selectedGame.T1SetCount,
                T2SetCount = selectedGame.T2SetCount,
                ServeTeamId = selectedGame.ServeTeamId,
                StartTime = selectedGame.StartTime,
                EndTime = selectedGame.EndTime,
                PointLevel = selectedGame.PointLevel,
            };

            return new GameBuilder(localDataRepository, Game, Team1, Team2).RestoreGame(Game);
        }

        public Task<List<GameDetails>> GetAllMatchesAsync()
        {
            return localDataRepository.GetAllGameDetailssAsync();
        }

        public Task<int> DeleteMatch(GameDetails game)
        {
            return localDataRepository.DeleteMatch(game);
        }

        public GameBuilder InitializeGame()
        {
            return new GameBuilder(localDataRepository, Game, Team1, Team2).InitializeGame();
        }
    }
}
