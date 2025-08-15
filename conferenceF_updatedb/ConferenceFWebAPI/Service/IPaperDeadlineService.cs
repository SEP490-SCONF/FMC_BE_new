namespace ConferenceFWebAPI.Service
{
    public interface IPaperDeadlineService
    {
        Task SchedulePaperReminders(int paperId);
    }

}
