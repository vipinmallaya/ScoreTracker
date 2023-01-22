using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreTracker.Models
{
    [INotifyPropertyChanged]
    public partial class Team
    {

        private string name;
        private int point;
        private int setCount;
        private bool serve;
        private Color color;
        private DateTime breakStartTime;


        [PrimaryKey]
        public string TeamId { get; set; }
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        [Ignore]
        public int Point
        {
            get => point;
            set => SetProperty(ref point, value);
        }

        [Ignore]
        public int SetCount
        {
            get => setCount;
            set => SetProperty(ref setCount, value);
        }

        [Ignore]
        public bool Serve
        {
            get => serve;
            set => SetProperty(ref serve, value);
        }

        [Ignore]
        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public string stColor
        {
            get
            {
                return color.ToRgbaHex().ToString();
            }
            set
            {
                color = Color.FromArgb(value);
            }
        }

        [Ignore]
        public DateTime BreakStartTime
        {
            get { return breakStartTime; }
            set { breakStartTime = value; }
        }

        public Team()
        {

        }
    }
}
