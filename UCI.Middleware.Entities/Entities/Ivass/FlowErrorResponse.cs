using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCI.Middleware.Entities.Entities.Ivass
{
    [Table("FlowErrorsResponse")]
    public class FlowErrorResponse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid SubmissionId { get; set; }

        [Required]
        [StringLength(50)]
        public required string ErrorCode { get; set; }

        [StringLength(4000)]
        public string? Message { get; set; }

        [ForeignKey("SubmissionId")]
        public virtual ClaimsSubmission Submission { get; set; } = null!;

        [ForeignKey("ErrorCode")]
        public virtual ErrorType Error { get; set; } = null!;
    }
}
