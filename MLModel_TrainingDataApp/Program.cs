using MLModel_TrainingDataApp;
using MLModel_TrainingDataApp.Common;
using MLModel_TrainingDataApp.Modes;
using MLModel_TrainingDataApp.OpenSubtitles;

var arguments = args.Length > 0 ? args : Array.Empty<string>();

switch (arguments.FirstOrDefault()?.ToLowerInvariant())
{
    case "--test":
        TestMode.Execute(AppConfig.OutputFile);
        break;

    case "--predict":
        PredictMode.Execute(arguments);
        break;

    case "--refine":
        RefineMode.Execute(AppConfig.DataFolder, AppConfig.OutputFile);
        break;

    default:
        await ExecuteFetchModeAsync();
        break;
}

// ────────────────────────────────────────────────────────────────────────────
// Default mode (fetch): Download TV episode data for training
// ────────────────────────────────────────────────────────────────────────────

async Task ExecuteFetchModeAsync()
{
    var client = OpenSubtitlesClient.Create(AppConfig.ApiKey);
    await using var writer = await TrainingDataWriter.CreateAsync(AppConfig.OutputFile);

    var fetchMode = new FetchMode(
        AppConfig.CheckpointFile,
        AppConfig.ShowsFile,
        client,
        writer,
        AppConfig.PerShowLimit,
        AppConfig.TargetCount,
        AppConfig.BaseDelayMs);

    await fetchMode.ExecuteAsync();
}
