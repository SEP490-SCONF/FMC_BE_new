namespace ConferenceFWebAPI.Service
{
    public interface IAiSpellCheckService
    {
        Task<string> CheckSpellingAsync(string text);
    }
}
