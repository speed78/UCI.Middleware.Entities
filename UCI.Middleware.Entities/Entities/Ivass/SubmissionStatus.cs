using System.ComponentModel.DataAnnotations;

namespace UCI.Middleware.Entities.Entities.Ivass
{
    /// <summary>
    /// Represents the status of a claims submission in the IVASS system.
    /// </summary>
    public class SubmissionStatus
    {
        /// <summary>
        /// Gets or sets the unique identifier for the submission status.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the description of the submission status.
        /// Must not exceed 200 characters.
        /// </summary>
        [Required]
        [StringLength(200)]
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the collection of claims submissions associated with this status.
        /// </summary>
        public virtual ICollection<ClaimsSubmission> ClaimsSubmissions { get; set; } = new List<ClaimsSubmission>();
    }
}
