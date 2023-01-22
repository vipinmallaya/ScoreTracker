using ScoreTracker.Models;
using ScoreTracker.Repository.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreTracker.Logic
{
    public class GameBuilder : IDisposable
    {
        private ILocalDataRepository localDataRepository;
        public Game Game { get; private set; }
        public Team Team1 { get; private set; }
        public Team Team2 { get; private set; }
        public Stack<Score> Scores { get; private set; } = new Stack<Score>();

        public GameBuilder(ILocalDataRepository localDataRepostory, Game game, Team team1, Team team2)
        {
            this.localDataRepository = localDataRepostory;

            Game = game;
            Team1 = team1;
            Team2 = team2;
        }

        public GameBuilder InitializeGame()
        {
            Game.StartTime = DateTime.Now;
            Team1.Serve = true;
            Team2.Serve = true;
            return this;
        }

        public async Task<GameBuilder> RestoreGame(Game restoreGame)
        {
            try
            {
                Team1.Serve = Team1.TeamId == Game.ServeTeamId;
                Team2.Serve = Team2.TeamId == Game.ServeTeamId;

                Team1.Point = Game.T1Point;
                Team2.Point = Game.T2Point;

                Team1.SetCount = Game.T1SetCount;
                Team2.SetCount = Game.T2SetCount;

                var scores = await localDataRepository.GetGameScoreAsync(Game);
                foreach (var item in scores)
                {
                    if (item.scoreWinningTeamId == Game.Team1Id)
                    {
                        item.Team = Team1;
                    }
                    else
                    {
                        item.Team = Team2;
                    }
                }

                Scores = new Stack<Score>(scores);

            }
            catch (Exception EX)
            {

                throw;
            }

            return this;
        }

        public async Task<GameBuilder> TeamWonAPoint(Team team)
        {
            Game.ServeTeamId = team.TeamId;
            Game.Status = GameStatus.InProgress;

            team.Point++;
            var score = new Score
            {
                ScoreTime = DateTime.Now,
                Id = DateTime.Now.Ticks,
                GameID = Game.GameID,
                scoreWinningTeamId = team.TeamId,
                Team = team
            };


            if (Game.ServeTeamId == Game.Team1Id)
            {
                Game.T1Point = team.Point;
            }
            else
            {
                Game.T2Point = team.Point;
            }

            Scores.Push(score);
            try
            {
                await Task.WhenAll(localDataRepository.SaveScoreAsync(score), localDataRepository.SaveGameDetailsAsync(Game));
            }
            catch (Exception ex)
            {
                throw;
            }

            return this;
        }

        internal async Task TeamWonAGameAsync(Team winningTeam)
        {
            winningTeam.SetCount++;

            if (winningTeam.TeamId == Game.Team1Id)
            {
                Game.T1SetCount = winningTeam.SetCount;
            }
            else
            {
                Game.T2SetCount = winningTeam.SetCount;
            }

            if ((Game.T1SetCount + Game.T2SetCount) == 1)
            {
                Game.PointLevel = $"{Team1.Name} : {Team2.Name}";
            }

            Game.PointLevel = $"{Game.PointLevel};{Game.T1Point} : {Game.T2Point}";

            ResetGame(GameStatus.InProgress);

            await Task.WhenAll(localDataRepository.ClearScoresForGameAsync(Game), localDataRepository.SaveGameDetailsAsync(Game));
        }

        public async Task FinishGame()
        {
            ResetGame(GameStatus.Finished);

            await localDataRepository.SaveGameDetailsAsync(Game);
        }

        private void ResetGame(GameStatus gameStatus)
        {
            Game.T2Point = Game.T1Point = 0;
            Game.Status = gameStatus;
            Scores.Clear();
        }

        public GameBuilder UndoLastScore()
        {
            if (Scores.TryPop(out var score))
            {
                score.Team.Point--;
            }
            if (score != null)
            {
                if (score.Team.TeamId == Game.Team1Id)
                {
                    Game.T1Point = score.Team.Point;
                }
                else
                {
                    Game.T2Point = score.Team.Point;
                }

                Task.WaitAll(localDataRepository.RemoveScoreAsync(score), localDataRepository.SaveGameDetailsAsync(Game));
            }
            return this;
        }

        public Task SaveTeam1Async()
        {
            return localDataRepository.SaveTeamDetailsAsync(Team1);
        }
        public Task SaveTeam2Async()
        {
            return localDataRepository.SaveTeamDetailsAsync(Team2);
        }

        public async Task Clear()
        {
            Scores.Clear();
            await Task.WhenAll(this.SaveTeam1Async(), this.SaveTeam2Async());
        }

        public void Dispose()
        {
            Game = null;
            Team1 = null;
            Team2 = null;
            Scores = null;
        }
    }
}
