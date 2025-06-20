namespace UCI.Middleware.Entities.Enums.Ivass
{
    /// <summary>
    /// Represents the status of a submission in the processing pipeline
    /// </summary>
    public enum SubmissionStatusType
    {
        /// <summary>
        /// File has been uploaded to the system
        /// </summary>
        Uploaded = 1,

        /// <summary>
        /// File size has been validated
        /// </summary>
        SizeValidated = 2,

        /// <summary>
        /// XML schema has been validated
        /// </summary>
        SchemaValidated = 3,

        /// <summary>
        /// UCI specific validation has been completed
        /// </summary>
        UciValidated = 4,

        /// <summary>
        /// File has been sent for processing
        /// </summary>
        Sent = 5,

        /// <summary>
        /// Processing has been completed successfully
        /// </summary>
        Completed = 6
    }

    public static class SubmissionStatusExtensions
    {
        /// <summary>
        /// Gets the next expected status in the pipeline
        /// </summary>
        public static SubmissionStatusType? GetNextStatus(this SubmissionStatusType status)
        {
            return status switch
            {
                SubmissionStatusType.Uploaded => SubmissionStatusType.SizeValidated,
                SubmissionStatusType.SizeValidated => SubmissionStatusType.SchemaValidated,
                SubmissionStatusType.SchemaValidated => SubmissionStatusType.UciValidated,
                SubmissionStatusType.UciValidated => SubmissionStatusType.Sent,
                SubmissionStatusType.Sent => SubmissionStatusType.Completed,
                SubmissionStatusType.Completed => null, // Final state
                _ => null
            };
        }

        /// <summary>
        /// Gets the integer value of the enum
        /// </summary>
        public static int GetId(this SubmissionStatusType status)
        {
            return (int)status;
        }
    }
}