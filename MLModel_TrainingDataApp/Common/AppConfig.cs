namespace MLModel_TrainingDataApp.Common;

/// <summary>
/// Central configuration for the TempConsole application.
/// </summary>
public static class AppConfig
{
    public const string ApiKey = "WCswBOQ1ox02iNotw1KFYgmOyuJZtcYT";
    public const string DataFolder = @"C:\Users\fredr\source\repos\media-organizer\MLModel_TrainingDataApp\Data";
    
    public static string OutputFile => Path.Combine(DataFolder, "tv_episode_training_data.jsonl");
    public static string CheckpointFile => Path.Combine(DataFolder, "fetch.checkpoint.json");
    public static string ShowsFile => Path.Combine(DataFolder, "shows_list.json");

    public const int PerShowLimit = 10;
    public const int TargetCount = 100_000;
    public const int BaseDelayMs = 250;
}
