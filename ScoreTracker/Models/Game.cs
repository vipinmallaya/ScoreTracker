using CommunityToolkit.Mvvm.ComponentModel;
using ScoreTracker.Repository.Abstraction;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreTracker.Models
{
    public class Game  // Game is a single instance of a match. 
    {
        [PrimaryKey]
        public string GameID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public GameStatus Status { get; set; }
        public string ServeTeamId { get; set; }
        public string Team1Id { get; set; }
        public string Team2Id { get; set; } 
        public int T1SetCount { get; set; }
        public int T2SetCount { get; set; }
        public int T1Point { get; set; }
        public int T2Point { get; set; }
        public string PointLevel { get; set; }

        public Game() => GameID = Guid.NewGuid().ToString();
    }

    public class GameDetails : Game
    {
        private string t1Color;
        private string t2Color;

        public string T1Name { get; set; }
        public string T2Name { get; set; }
        public Color Team1Color
        {
            get
            {

                return Color.FromArgb(t1Color);
            }
            set
            {
                t1Color = value.ToRgbaHex();
            }
        }

        public Color Team2Color
        {
            get
            {

                return Color.FromArgb(t2Color);
            }
            set
            {
                t2Color = value.ToRgbaHex();
            }
        }

        public string T1Color
        {
            get => t1Color;
            set => t1Color = value;
        }

        public string T2Color
        {
            get => t2Color;
            set => t2Color = value;
        }
    }

    public enum GameStatus
    {
        NotStarted,
        InProgress,
        Finished,
        Abandoned,
        Timeout
    }
}
