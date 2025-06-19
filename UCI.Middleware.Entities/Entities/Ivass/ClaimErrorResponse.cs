using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCI.Middleware.Entities.Entities.Ivass
{
    [Table("ClaimsErrorResponse")]
    public class ClaimErrorResponse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid SubmissionId { get; set; }

        [Required]
        [StringLength(50)]
        public required string ClaimCode { get; set; }

        [ForeignKey("SubmissionId")]
        public virtual ClaimsSubmission Submission { get; set; } = null!;

        public virtual ICollection<ClaimDetailErrorResponse> ClaimErrors { get; set; } = new List<ClaimDetailErrorResponse>();
    }
}
