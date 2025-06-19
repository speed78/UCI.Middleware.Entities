using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCI.Middleware.Entities.Entities.Ivass
{
    [Table("ClaimDetailErrorsResponse")]
    public class ClaimDetailErrorResponse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        [StringLength(50)]
        public required string ErrorCode { get; set; }

        [Required]
        [StringLength(1000)]
        public required string XPath { get; set; }

        [StringLength(4000)]
        public string? Message { get; set; }

        [ForeignKey("ClaimId")]
        public virtual ClaimErrorResponse Claim { get; set; } = null!;

        [ForeignKey("ErrorCode")]
        public virtual ErrorType Error { get; set; } = null!;
    }
}
