using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCI.Middleware.Entities.Entities.Ivass
{
    [Table("ClaimsSubmissions")]
    public class ClaimsSubmission
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public required string InputFileName { get; set; }

        [Required]
        [StringLength(1024)]
        public required string InputFileFullPath { get; set; }

        [StringLength(255)]
        public string? OutputFileName { get; set; }

        [StringLength(1024)]
        public string? OutputFileFullPath { get; set; }

        [StringLength(20)]
        public string? Protocol { get; set; }

        [StringLength(4000)]
        public string? ValidationError { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        public DateTime? SendDate { get; set; }

        public DateTime? LastResponseAttemptDate { get; set; }

        public DateTime? ResponseDate { get; set; }

        [Required]
        public int SubmissionStatusId { get; set; }

        [ForeignKey("SubmissionStatusId")]
        public virtual SubmissionStatus SubmissionStatus { get; set; } = null!;

        public Guid? CorrespondentId { get; set; }

        [ForeignKey("CorrespondentId")]
        public virtual Correspondent? Correspondent { get; set; }

        public virtual ICollection<FlowErrorResponse> FlowErrors { get; set; } = new List<FlowErrorResponse>();
        public virtual ICollection<ClaimErrorResponse> Claims { get; set; } = new List<ClaimErrorResponse>();
    }
}

