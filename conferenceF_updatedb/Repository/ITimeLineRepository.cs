using BussinessObject.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface ITimeLineRepository
    {
        Task<List<TimeLine>> GetTimeLinesByConferenceAsync(int conferenceId);
        Task<TimeLine?> GetTimeLineByIdAsync(int id);
        Task<TimeLine> CreateTimeLineAsync(TimeLine timeLine);
        Task<bool> UpdateTimeLineAsync(TimeLine timeLine);
    }
}
