using ScoreTracker.Models;
using ScoreTracker.Pages;
using System.Text.Json;
namespace ScoreTracker;

public partial class App : Application
{
    public Feature AppFeature { get; set; }
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        LoadFeatures();
        Routing.RegisterRoute("MainPage/GamePage", typeof(GamePage));
    }

    async Task LoadFeatures()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("Features.json");
        using var reader = new StreamReader(stream);

        var contents = reader.ReadToEnd();
        AppFeature = JsonSerializer.Deserialize<Feature>(contents);
    }
}
