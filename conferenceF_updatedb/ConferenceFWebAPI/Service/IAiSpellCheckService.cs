using ConferenceFWebAPI.Services.PdfTextExtraction;
using System.Drawing;

namespace ConferenceFWebAPI.Service
{
    public interface IAiSpellCheckService
    {
        Task<string> CheckSpellingAsync(string text);
        Task<List<string>> GetMisspelledWordsAsync(string text);



    }
}
