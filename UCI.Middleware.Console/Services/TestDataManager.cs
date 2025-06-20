namespace UCI.Middleware.Console.Services
{
    /// <summary>
    /// Classe per gestire i dati condivisi durante i test
    /// </summary>
    public class TestDataManager
    {
        public Guid LastSubmissionId { get; set; } = Guid.Empty;
        public List<Guid> CreatedSubmissionIds { get; set; } = new();

        public void AddCreatedSubmission(Guid submissionId)
        {
            CreatedSubmissionIds.Add(submissionId);
            LastSubmissionId = submissionId;
        }

        public void Reset()
        {
            LastSubmissionId = Guid.Empty;
            CreatedSubmissionIds.Clear();
        }
    }
}