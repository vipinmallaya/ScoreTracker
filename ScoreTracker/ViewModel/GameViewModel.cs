using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using ScoreTracker.Logic;
using ScoreTracker.Models;
using ScoreTracker.Repository;
using ScoreTracker.Repository.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ScoreTracker.ViewModel
{
    public partial class GameViewModel : BaseViewModel
    {
        private GameBuilder gameBuilder;

        private const int MATCHWINPOINT = 25;

        private int maxPoint = MATCHWINPOINT;
        private Team team1;
        private Team team2;
        private Game game;
        private string gameTime;
        private string breakTime;
        private System.Timers.Timer gameTimer;
        private System.Timers.Timer breakTimer;
        private bool breakOpted;
        private string breakTeam;
        private bool breakEnabled;

        public Team Team1
        {
            get => team1;
            set => SetProperty(ref team1, value);
        }

        public string GameTime
        {
            get => gameTime;
            set => SetProperty(ref gameTime, value);
        }

        public Game Game
        {
            get => game;
            set => SetProperty(ref game, value);
        }

        public Team Team2
        {
            get => team2;
            set => SetProperty(ref team2, value);
        }

        public string BreakTeam
        {
            get => breakTeam;
            set => SetProperty(ref breakTeam, value);
        }
        public string BreakTime
        {
            get => breakTime;
            set => SetProperty(ref breakTime, value);
        }
        public bool BreakOpted
        {
            get => breakOpted;
            set => SetProperty(ref breakOpted, value);
        }


        public bool BreakEnabled
        {
            get => breakEnabled;
            set => SetProperty(ref breakEnabled, value);
        }

        public AsyncRelayCommand<string> PointCommand { get; set; }
        public AsyncRelayCommand<string> SetServeCommand { get; set; }
        public AsyncRelayCommand<string> BreakCommand { get; set; }
        public AsyncRelayCommand UndoCommand { get; set; }
        public AsyncRelayCommand ShareScoreCommand { get; set; }
        public AsyncRelayCommand SwitchSideCommand { get; set; }
        public AsyncRelayCommand StartTimerCommand { get; set; }
        public AsyncRelayCommand Team1NameChangedCommand { get; }
        public AsyncRelayCommand Team2NameChangedCommand { get; }

        public GameViewModel()
        {
            PointCommand = new AsyncRelayCommand<string>(PointActionAsync);
            SetServeCommand = new AsyncRelayCommand<string>(SetServeActionAsync);
            BreakCommand = new AsyncRelayCommand<string>(BreakActionAsync);
            UndoCommand = new AsyncRelayCommand(UndoActionAsync);
            ShareScoreCommand = new AsyncRelayCommand(ShareScoreActionAsync);
            SwitchSideCommand = new AsyncRelayCommand(SwitchSideActionAsync);
            StartTimerCommand = new AsyncRelayCommand(StartTimerActionAsync);
            Team1NameChangedCommand = new AsyncRelayCommand(Team1NameChangedActionAsync);
            Team2NameChangedCommand = new AsyncRelayCommand(Team2NameChangedActionAsync);

            Team1 = new Team
            {
                Color = Colors.Red
            };
            Team2 = new Team
            {
                Color = Colors.Blue
            };

            game = new Game();
            Team1.Serve = Team2.Serve = true;

            gameTimer = new System.Timers.Timer(1000);
            breakTimer = new System.Timers.Timer(1000);

            gameTimer.AutoReset = true;
            gameTimer.Elapsed += GameTimer_Elapsed;
            GameTime = "Start Game";

            breakTimer.AutoReset = true;
            breakTimer.Elapsed += BreakTimer_Elapsed;
        }

        private Task Team1NameChangedActionAsync()
        {
            return gameBuilder.SaveTeam1Async();
        }

        private Task Team2NameChangedActionAsync()
        {
            return gameBuilder.SaveTeam2Async();
        }

        public override void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            gameBuilder = query["GameBuilder"] as GameBuilder;
            if (gameBuilder != null)
            {
                Team1 = gameBuilder.Team1;
                Team2 = gameBuilder.Team2;
            }

            BreakEnabled = (App.Current as App).AppFeature.Break;
        }

        private Task StartTimerActionAsync()
        {
            gameTimer.Start();
            return Task.CompletedTask;
        }

        private void GameTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timeDiff = DateTime.Now.Subtract(Game.StartTime);
            MainThread.BeginInvokeOnMainThread(() => GameTime = $"{timeDiff.Minutes} : {timeDiff.Seconds}");
        }

        private Task SwitchSideActionAsync()
        {
            var tempTeam = Team2;
            Team2 = Team1;
            Team1 = tempTeam;

            return Task.CompletedTask;
        }

        private async Task ShareScoreActionAsync()
        {
            var shareMessage = GetScoreDetails();
            await Share.Default.RequestAsync(new ShareTextRequest(shareMessage, "Score Tracker"));
        }

        private Task UndoActionAsync()
        {
            gameBuilder.UndoLastScore();
            return Task.CompletedTask;
        }

        private Task BreakActionAsync(string team)
        {
            if (team == "-1")
            {
                BreakOpted = false;
                BreakTeam = string.Empty;
                BreakTime = string.Empty;
                return Task.CompletedTask;
            }

            BreakOpted = true;
            BreakTime = "2:00";

            BreakTeam = team == "1" ? GetTeam1Name() : GetTeam2Name();
            Team1.BreakStartTime = team == "1" ? DateTime.Now : DateTime.MinValue;
            Team2.BreakStartTime = team == "2" ? DateTime.Now : DateTime.MinValue;

            breakTimer.Start();

            return Task.CompletedTask;
        }

        private Task SetServeActionAsync(string team)
        {
            Team1.Serve = team == "1";
            Team2.Serve = team == "2";

            return Task.WhenAll(gameBuilder.SaveTeam1Async(), gameBuilder.SaveTeam2Async());
        }

        private async Task PointActionAsync(string team)
        {
            switch (team)
            {
                case "1":
                    await gameBuilder.TeamWonAPoint(Team1);

                    break;
                case "2":
                    await gameBuilder.TeamWonAPoint(Team2);
                    break;
            }

            if (Team1.Point + 1 == this.maxPoint &&
                Team2.Point + 1 == this.maxPoint)
            {
                maxPoint++;
            }

            if (Team1.Point >= this.maxPoint ||
                Team2.Point >= this.maxPoint)
            {
                await TeamWonTheMatch(team);
            }
            else
            {
                await SetServeActionAsync(team);
            }
        }

        private async Task TeamWonTheMatch(string teamName)
        {
            var result = default(bool);
            var winningTeam = default(Team);
            var gameCount = Team1.SetCount + Team2.SetCount;

            switch (teamName)
            {
                case "1":

                    winningTeam = Team1;
                    Team1.Serve = true;
                    Team2.Serve = false;

                    break;
                case "2":

                    winningTeam = Team2;

                    Team2.Serve = true;
                    Team1.Serve = false;

                    break;
            }
            result = await App.Current.MainPage.DisplayAlert("Match Over", $"{winningTeam.Name ?? "Home"} Won the match", "Ok", "Cancel");

            if (result)
            {
                await gameBuilder.TeamWonAGameAsync(winningTeam);

                gameCount = Team1.SetCount + Team2.SetCount;

                gameTimer.Stop();

                GameTime = "Timer Paused";

                Team1.Point = Team2.Point = 0;
                maxPoint = MATCHWINPOINT;

                await SwitchSideActionAsync();
            }
            else
            {
                await UndoActionAsync();
            }

            //TODO::Optimize / Refactor
            if ((Team1.SetCount >= 2 || Team2.SetCount >= 2) && Math.Abs((Team1.SetCount - Team2.SetCount)) >=  1)
            {
                var matchwinner = Team1.SetCount >= 2 ? Team1 : Team2;
                result = await App.Current.MainPage.DisplayAlert("Game Over", $"{matchwinner.Name ?? "Home"} Won the Game, Do you want to start another match",  "Yes", "No");
                if(!result)
                {
                    await gameBuilder.FinishGame();
                    await Shell.Current.GoToAsync("..");
                }
            }
        }

        private string GetScoreDetails()
        {
            string team1Name = GetTeam1Name();
            var team2Name = GetTeam2Name();

            var shareMessage = new StringBuilder($"Game Date : {DateTime.Now.ToString("dd-MMM-yy HH:mm ")} \n Score Card \n \n {team1Name} : {team2Name} \nSets Status \n {Team1.SetCount} : {Team2.SetCount} \n\nPoints \n");

            var pointLevls = string.Join("\n",gameBuilder.Game.PointLevel.Split(";"));

            shareMessage.Append(pointLevls); 

            return shareMessage.ToString();
        }

        private string GetTeam2Name()
        {
            return string.IsNullOrWhiteSpace(Team2.Name) ? "Visitors" : Team2.Name;
        }

        private string GetTeam1Name()
        {
            return string.IsNullOrWhiteSpace(Team1.Name) ? "Home" : Team1.Name;
        }

        private void BreakTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var breakTeam = Team1.BreakStartTime == DateTime.MinValue ? Team2 : Team1;
            var diff = DateTime.Now.Subtract(breakTeam.BreakStartTime);
            var timeDiff = new TimeSpan(00, 2, 00).Subtract(diff);
            if (timeDiff.TotalSeconds > 0)
            {
                MainThread.BeginInvokeOnMainThread(() => BreakTime = $"{timeDiff.Minutes.ToString("##")} : {timeDiff.Seconds.ToString("##")}");
            }
            else
            {
                BreakOpted = false;
            }
        }

        protected override async Task PageDisAppearingAction()
        {
            await gameBuilder.Clear();
            gameBuilder.Dispose();
            gameBuilder = null;

            await base.PageDisAppearingAction();
        }
    }
}
