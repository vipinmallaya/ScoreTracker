using CommunityToolkit.Mvvm.Input;
using ScoreTracker.Logic;
using ScoreTracker.Models;
using ScoreTracker.Repository;
using ScoreTracker.Repository.Abstraction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreTracker.ViewModel
{
    public partial class MainPageViewModel : BaseViewModel
    {
        private ILocalDataRepository localDataRepository;
        private Task initializationTask;
        private MatchBuilder matchBuilder;
        private ObservableCollection<GameDetails> games;

        public ObservableCollection<GameDetails> Games
        {
            get => games;
            set => SetProperty(ref games, value);
        }

        public IAsyncRelayCommand StartNewGameCommand
        {
            get;
            set;
        }
        public IAsyncRelayCommand<GameDetails> GameSelectedCommand
        {
            get;
            set;
        }

        public IAsyncRelayCommand<GameDetails> GameDeletedCommand
        {
            get;
            set;
        }

        private GameDetails selectedGame;

        public GameDetails SelectedGame
        {
            get => selectedGame;
            set => SetProperty(ref selectedGame, value);
        }

        public MainPageViewModel()
        {
            StartNewGameCommand = new AsyncRelayCommand(StartNewGameActionAsync);
            GameSelectedCommand = new AsyncRelayCommand<GameDetails>(GameSelectedActionAsync);
            GameDeletedCommand = new AsyncRelayCommand<GameDetails>(GameDeletedAction);

            localDataRepository = new LocalDataRepository();
            initializationTask = localDataRepository.Initalize();
            matchBuilder = new MatchBuilder(localDataRepository);
        }

        private async Task GameDeletedAction(GameDetails arg)
        {
            try
            {
                var deleteResult = await matchBuilder.DeleteMatch(arg);

                if (deleteResult > 0)
                {
                    var result = await matchBuilder.GetAllMatchesAsync();
                    Games.Clear();

                    //Todo::Optimize
                    Games = new ObservableCollection<GameDetails>(result);
                }
            }
            catch (Exception ex)
            { 
                 
            }
        }

        protected override async Task PageAppearingAction()
        {
            if (!initializationTask.IsCompleted)
            {
                await initializationTask;
            }

            SelectedGame = null;
            await matchBuilder.GetAllMatchesAsync().ContinueWith(task => Games = new ObservableCollection<GameDetails>(task.Result));
            await base.PageAppearingAction();
        }

        private async Task GameSelectedActionAsync(GameDetails selectedGame)
        {
            if (selectedGame == null)
            {
                return;
            }

            var gameBuilder = await matchBuilder.InitializeGame(selectedGame);
            await Shell.Current.GoToAsync("//MainPage/GamePage", new Dictionary<string, object> { { "GameBuilder", gameBuilder } });
        }

        private async Task StartNewGameActionAsync()
        {
            try
            {
                await localDataRepository.Initalize();

                await matchBuilder.GetNewTeam1("Home", Colors.Red);
                await matchBuilder.GetNewTeam2("Away", Colors.Blue);
                await matchBuilder.GenerateGame();

                var gameBuilder = matchBuilder.InitializeGame();

                await Shell.Current.GoToAsync("//MainPage/GamePage", new Dictionary<string, object> { { "GameBuilder", gameBuilder } });
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
