using System.Text.Json;
using MLModel_TrainingDataApp.Models;
using MLModel_TrainingDataApp.Refine;

namespace MLModel_TrainingDataApp;

public interface ITrainingDataFileWriter
{
    void SaveInspectionFile(string filename, List<RefinedTrainingEntry> entries);
    string WriteTrainingDataTsv(List<RefinedTrainingEntry> entries);
    string WriteTrainingDataNer(List<RefinedTrainingEntry> entries);
}

public class FileSystemTrainingDataFileWriter : ITrainingDataFileWriter
{
    private readonly string _dataFolder;

    public FileSystemTrainingDataFileWriter(string dataFolder)
    {
        _dataFolder = dataFolder;
    }

    public void SaveInspectionFile(string filename, List<RefinedTrainingEntry> entries)
    {
        if (entries.Count > 0)
        {
            var path = Path.Combine(_dataFolder, filename);
            File.WriteAllLines(path, entries.Select(e => JsonSerializer.Serialize(e)));
            Console.WriteLine($"  Saved {entries.Count} entries to {filename}");
        }
    }

    public string WriteTrainingDataTsv(List<RefinedTrainingEntry> entries)
    {
        var tsvPath = Path.Combine(_dataFolder, "refined_training_data.tsv");
        TabularConverter.ConvertToTsv(entries, tsvPath);
        return tsvPath;
    }

    public string WriteTrainingDataNer(List<RefinedTrainingEntry> entries)
    {
        var nerPath = Path.Combine(_dataFolder, "refined_training_data_ner.txt");
        NerFormatConverter.ConvertToNerFormat(entries, nerPath);
        return nerPath;
    }
}
