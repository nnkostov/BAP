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

public record ConsoleEntry(DateTime Timestamp, string Text, ConsoleEntryLevel Level);

public enum ConsoleEntryLevel
{
    Info,
    Success,
    Warning,
    Error
}
