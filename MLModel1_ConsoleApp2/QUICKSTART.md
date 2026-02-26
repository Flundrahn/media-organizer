## Quick Start: Testing the Retrained Model

### Build and Run

```bash
cd MLModel1_ConsoleApp2
dotnet run
```

### Select a Mode

When prompted, enter:
- **`1`** - Quick test on 13 sample filenames
- **`2`** - Interactive mode (test your own filenames)
- **`3`** - Accuracy evaluation on validation data
- **`4`** - Run all tests

### Example Session

```
??????????????????????????????????????????????????????????
?   ML.NET NER Model - Show Name Extraction Testing      ?
??????????????????????????????????????????????????????????

Available modes:
  1 - Test on sample filenames (batch mode)
  2 - Interactive mode (test single filenames)
  3 - Accuracy evaluation on validation data
  4 - Test all modes

Select mode (1-4): 1

=== Testing NER Model on Sample Filenames ===

Filename                                                   | Predicted Show Name
?????????????????????????????????????????????????????????????????????????????????????????????????????
The.Day.of.the.Jackal.S01E07.1080p.WEB.H264              | The Day of the Jackal
Breaking.Bad.S01E01.Pilot.720p.BluRay.x264               | Breaking Bad
Game.of.Thrones.S08E06.The.Iron.Throne.2160p.WEB-DL     | Game of Thrones
Stranger.Things.S04E01.1080p.NF.WEB-DL.DDP5.1.x264      | Stranger Things
Money.Heist.S03E08.720p.NF.WEB-DL                        | Money Heist
Law.And.Order.Organized.Crime.S04E09.1080p.WEB.h264     | Law And Order Organized Crime
Star.Trek.Voyager.S06E11.Fair.Haven.480p.DVD.x265       | Star Trek Voyager
Nip.Tuck.S05E02.1080p.WEB-DL                             | Nip Tuck
Wild.Wild.West.S04E08.480p                               | Wild Wild West
Dawsons.Creek.S04E09.720p                                | Dawsons Creek
The.Big.Valley.S01E21.Barbary.Red.1080p                  | The Big Valley
The.Twilight.Zone.1985.S01E12.Her.Pilgrim.Soul           | The Twilight Zone
Arcane.S01E09.The.Monster.You.Created.MN                 | Arcane

Results: 13 successful, 0 no match detected
```

### Testing Tips

**For Quick Validation:**
```bash
dotnet run     # Select mode 1
# Should complete in ~5 seconds
```

**For Manual Testing:**
```bash
dotnet run     # Select mode 2
# Enter any filename, see predictions in real-time
# Type 'quit' to exit
```

**For Accuracy Assessment:**
```bash
dotnet run     # Select mode 3
# Tests on 200 validation examples
# Shows accuracy percentage and sample errors
```

### What Each Mode Tests

| Mode | What It Tests | Duration | Best For |
|------|--------------|----------|----------|
| 1 | 13 diverse filenames | ~5 sec | Quick validation |
| 2 | Custom single filenames | Variable | Manual exploration |
| 3 | 200 validation entries | ~30 sec | Accuracy metrics |
| 4 | All tests | ~40 sec | Full assessment |

### Understanding Results

**B-SHOW**: Beginning of show name entity  
**I-SHOW**: Inside/continuation of show name entity  
**O**: Outside (not part of show name)

For example, in `Breaking.Bad.S01E01`:
- `Breaking` = B-SHOW
- `Bad` = I-SHOW
- `S01E01` = O

Result: **Breaking Bad** ?

### Common Test Cases

? **Standard dots**: `Breaking.Bad.S01E01.720p`  
? **With colons**: `Star.Trek.Voyager.S06E11` ? Star Trek Voyager  
? **With ampersands**: `Law.And.Order.Organized.Crime.S04E09`  
? **With slashes**: `Nip.Tuck.S05E02`  
? **Multi-word**: `Game.of.Thrones.S08E06`  
? **With year**: `The.Twilight.Zone.1985.S01E12`  

### Troubleshooting

**Model file not found?**
- Ensure you run from the MLModel1_ConsoleApp2 directory
- Check that `MLModel1.mlnet` exists in the build output

**Can't find training data for accuracy test?**
- The path is hardcoded to: `C:\Users\fredr\source\repos\media-organizer\TempConsole\Data\tv_episode_training_data.jsonl`
- Ensure this file exists or update the path in Program.cs

**Errors on GPU?**
- Verify CUDA is properly installed
- The model is configured to NOT fall back to CPU; GPU is required
