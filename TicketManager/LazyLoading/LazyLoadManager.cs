
using NSCore.Models;
using TicketManager.Models;
using TicketManager.Services;

namespace TicketManager.LazyLoading;

public class LazyLoadManager
{
private readonly DataService _dataService;
    private LazyLoad<Ticket> _currentState;

    public LazyLoadManager(DataService dataService)
    {
        _dataService = dataService;

        _currentState = new LazyLoad<Ticket>
        {
            HasMoreRecords = true,
            NextFrom = 0,
            Result = new List<Ticket>()
        };
    }

    public List<Ticket> CurrentItems => _currentState.Result ?? new List<Ticket>();
    public bool HasMoreItems => _currentState.HasMoreRecords;

    /// <summary>
    /// Loads the next batch of tickets asynchronously.
    /// </summary>
    /// <returns>A list of tickets from the next batch.</returns>
    public async Task<List<Ticket>> LoadNextBatchAsync()
    {
        if (!_currentState.HasMoreRecords)
        {
            return CurrentItems;
        }
        _currentState = await _dataService.LoadDataAsync(_currentState.NextFrom);
        return _currentState.Result ?? new List<Ticket>();
    }
}
