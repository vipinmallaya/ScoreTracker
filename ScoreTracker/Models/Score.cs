using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace ScoreTracker.Models
{
    public record Score
    {
        [Ignore]
        public Team Team { get; set; }

        [PrimaryKey]
        public long Id { get; set; }
        public string GameID { get; set; }

        public string scoreWinningTeamId  { get; set; } 
        
        public DateTime ScoreTime { get; set; } 
    }
}
