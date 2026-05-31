using System.Collections.ObjectModel;
using TAM.Helpers;
using TAM.Models;
using TAM.Services;

namespace TAM.ViewModels;

public class AuditLogViewModel : BaseViewModel, IRefreshable
{
    private ObservableCollection<AuditLogEntry> _entries = new();
    private int _fetchCount = 50;
    private int _totalCount;

    public ObservableCollection<AuditLogEntry> Entries { get => _entries; set => SetProperty(ref _entries, value); }
    public int FetchCount { get => _fetchCount; set { SetProperty(ref _fetchCount, value); } }
    public int TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand LoadCommand { get; }

    public AuditLogViewModel()
    {
        RefreshCommand = new RelayCommand(_ => Refresh());
        LoadCommand = new RelayCommand(_ => LoadEntries());
        Refresh();
    }

    public void Refresh() => LoadEntries();

    private void LoadEntries()
    {
        var count = FetchCount > 0 ? FetchCount : 50;
        TotalCount = AuditService.Instance.GetTotalCount();
        Entries = new ObservableCollection<AuditLogEntry>(AuditService.Instance.GetEntries(count));
    }
}
