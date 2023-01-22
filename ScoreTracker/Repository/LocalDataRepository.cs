using ScoreTracker.Models;
using ScoreTracker.Repository.Abstraction;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreTracker.Repository
{
    internal class LocalDataRepository : ILocalDataRepository
    {
        public const string DatabaseFilename = "TodoSQLite.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        private SQLiteAsyncConnection sqliteConnection;
        public LocalDataRepository()
        {
            sqliteConnection = new SQLiteAsyncConnection(DatabasePath, Flags, false);
        }

        public async Task Initalize()
        {
            //await sqliteConnection.CreateTableAsync<Team>();
            //await sqliteConnection.CreateTableAsync<Game>();
            //await sqliteConnection.CreateTableAsync<Score>();

            //await Task.WhenAll(sqliteConnection.CreateTableAsync<Team>(),  sqliteConnection.CreateTableAsync<Game>(),sqliteConnection.CreateTableAsync<Score>());

            await sqliteConnection.CreateTablesAsync(CreateFlags.None, typeof(Team), typeof(Game), typeof(Score));
        }

        public Task<int> SaveTeamDetailsAsync(Team team)
        {
            lock (this)
            {
                return sqliteConnection.InsertOrReplaceAsync(team);
            }
        }

        public Task<int> SaveGameDetailsAsync(Game game)
        {
            lock (this)
            {
                return sqliteConnection.InsertOrReplaceAsync(game);
            }
        }

        public Task<List<GameDetails>> GetAllGameDetailssAsync()
        {
            return sqliteConnection.QueryAsync<GameDetails>("Select Game.*, t1.Name 'T1Name', t1.stColor 'T1Color', t2.Name 'T2Name', t2.stColor 'T2Color' from Game inner join Team t1 on t1.Teamid=Game.Team1Id inner join Team t2 on t2.TeamId = Game.Team2Id");
        }

        public Task<Game> GetGameAsync(int gameId)
        {
            return sqliteConnection.Table<Game>().FirstOrDefaultAsync(item => string.Equals(item.GameID, gameId));
        }

        public Task<List<Team>> GetTeams(int[] teams)
        {
            return sqliteConnection.QueryAsync<Team>($"select * from {nameof(Team)} where {nameof(Team.TeamId)} in '{string.Join("','", teams)}'");
        }

        public Task SaveScoreAsync(Score score)
        {
            lock (this)
            {
                return sqliteConnection.InsertAsync(score);
            }
        }
        public Task ClearScoresForGameAsync(Game game)
        {
            lock (this)
            {
                return sqliteConnection.ExecuteAsync($"delete from score where gameid='{game.GameID}'");
            }
        }

        public Task RemoveScoreAsync(Score game)
        {
            lock (this)
            {
                return sqliteConnection.ExecuteAsync($"delete from score where Id ={game.Id}");
            }
        }

        public Task<List<Score>> GetGameScoreAsync(Game game)
        {
            return sqliteConnection.QueryAsync<Score>($"select * from score where GameId='{game.GameID}' order by scoretime desc");
        }

        public Task<int> DeleteMatch(Game game )
        {
            return sqliteConnection.ExecuteAsync($"delete from {nameof(Game)} where {nameof(Game.GameID)} = '{game.GameID}'");
        }
    }
}
