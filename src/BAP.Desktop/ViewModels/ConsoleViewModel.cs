using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BAP.Desktop.ViewModels;

public partial class ConsoleViewModel : ObservableObject
{
    public ObservableCollection<ConsoleEntry> Entries { get; } = [];

    public void WriteLine(string text, ConsoleEntryLevel level = ConsoleEntryLevel.Info)
    {
        Entries.Add(new ConsoleEntry(DateTime.Now, text, level));

        // Keep last 500 entries
        while (Entries.Count > 500)
            Entries.RemoveAt(0);
    }

    [RelayCommand]
    private void Clear() => Entries.Clear();
}

public record ConsoleEntry(DateTime Timestamp, string Text, ConsoleEntryLevel Level)
{
    public string LevelTag => Level switch
    {
        ConsoleEntryLevel.Success => "OK",
        ConsoleEntryLevel.Warning => "WARN",
        ConsoleEntryLevel.Error => "ERR",
        _ => "INFO"
    };

    public string LevelColor => Level switch
    {
        ConsoleEntryLevel.Success => "#2ECC71",
        ConsoleEntryLevel.Warning => "#F5A623",
        ConsoleEntryLevel.Error => "#E74C3C",
        _ => "#B0B0C0"
    };

    public string LevelBadgeBackground => Level switch
    {
        ConsoleEntryLevel.Success => "#1A2ECC71",
        ConsoleEntryLevel.Warning => "#1AF5A623",
        ConsoleEntryLevel.Error => "#1AE74C3C",
        _ => "#1A6B6B7B"
    };
}

public enum ConsoleEntryLevel
{
    Info,
    Success,
    Warning,
    Error
}
