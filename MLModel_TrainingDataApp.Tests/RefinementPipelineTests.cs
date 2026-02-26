using System.Text.Json;
using MLModel_TrainingDataApp.Models;
using Moq;

namespace MLModel_TrainingDataApp.Tests;

public class RefinementPipelineTests
{
    [Theory]
    [InlineData(@"{ ""filename"":""Mickey Mouse Clubhouse_S05E03_Mickey\u0027s Pirate Adventure Part 1 (Part 1 of 2)_track5_[rum]"",""show_name"":""Mickey Mouse Clubhouse"",""episode_name"":""\u0022Mickey Mouse Clubhouse\u0022 Mickey\u0027s Mystery"",""season_number"":5,""episode_number"":1,""year"":2013} ")]
    [InlineData(@"{ ""filename"":""Tenkuu_Senki_Shurato_-_37_[Arcadia]"",""show_name"":""Shurato"",""episode_name"":""The Shadow that Attacks Tenkuukai:  Shoot the Charm-water"",""season_number"":1,""episode_number"":37,""year"":1989}")]
    [InlineData(@"{ ""filename"":""[LIME]_Galaxy_Angel_21"",""show_name"":""Galaxy Angel"",""episode_name"":""Deco Pizza"",""season_number"":1,""episode_number"":21,""year"":2001}")]
    [InlineData(@"{ ""filename"":""Community_S6E09_DVD_25fps_23.976fps_[dan]"",""show_name"":""Community"",""episode_name"":""Grifting 101"",""season_number"":6,""episode_number"":9,""year"":2015}")]
    [InlineData(@"{ ""filename"":""Chip \u0027n Dale\u0027s Rescue Rangers.S01E09.Disney\u002B.1080p.H264.AAC"",""show_name"":""Chip \u0027n\u0027 Dale\u0027s Rescue Rangers"",""episode_name"":""Risky Beesness"",""season_number"":1,""episode_number"":9,""year"":1989}")]
    [InlineData(@"{ ""filename"":""Girls_und_Panzer 07 @ lovestoorey210.blogspot.com @ BY @ love_stoorey210"",""show_name"":""Girls und Panzer"",""episode_name"":""Up Next is Anzio!"",""season_number"":1,""episode_number"":7,""year"":2012}")]
    [InlineData(@"{ ""filename"":""My Next Life as a Villainess_ All Routes Lead to Doom episode 9"",""show_name"":""My Next Life as a Villainess All Routes Lead to Doom!"",""episode_name"":""Things Got Crazy at a Slumber Party..."",""season_number"":1,""episode_number"":9,""year"":2020}")]
    public void Process_IfJsonIsValid_ShouldWriteToFileWithValidRefinedData(string json)
    {
        // Arrange
        var mockWriter = new Mock<ITrainingDataFileWriter>();
        var sut = new RefinementPipeline(mockWriter.Object);
        var rawEntry = JsonSerializer.Deserialize<TrainingEntry>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize for test case with json: {json}");
        
        // Create the expected refined entry
        var expectedRefined = RefinedTrainingEntry.FromRaw(rawEntry);

        // Act
        var result = sut.Process([rawEntry]);

        // Assert
        mockWriter.Verify(w => w.WriteTrainingDataTsv(It.Is<List<RefinedTrainingEntry>>(entries =>
            entries.Count == 1 &&
            entries[0].ShowName == expectedRefined.ShowName &&
            entries[0].EpisodeName == expectedRefined.EpisodeName &&
            entries[0].SeasonNumber == expectedRefined.SeasonNumber &&
            entries[0].EpisodeNumber == expectedRefined.EpisodeNumber &&
            entries[0].Year == expectedRefined.Year
        )), Times.Once);

        // NOTE: being lazy here, just check if we write filename, don't bother checking the full refined training entry
        mockWriter.Verify(w => w.SaveInspectionFile(rawEntry.Filename,
                                                    It.IsAny<List<RefinedTrainingEntry>>()),
                                                    Times.Never);
    }
}
