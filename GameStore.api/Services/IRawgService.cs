namespace GameStore.api;

public interface IRawgService
{
    Task<ServiceResult<List<RawgGameSearchResultDto>>> SearchAsync(string query);
    Task<ServiceResult<GameDetailsDto>> ImportAsync(int rawgId);
}
