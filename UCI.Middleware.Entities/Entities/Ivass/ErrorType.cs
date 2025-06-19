using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCI.Middleware.Entities.Entities.Ivass
{
    [Table("ErrorsType")]
    public class ErrorType
    {
        [Key]
        [Required]
        [StringLength(50)]
        public required string Code { get; set; }

        [Required]
        [StringLength(500)]
        public required string Summary { get; set; }

        public virtual ICollection<FlowErrorResponse> FlowErrors { get; set; } = new List<FlowErrorResponse>();
        public virtual ICollection<ClaimDetailErrorResponse> ClaimErrors { get; set; } = new List<ClaimDetailErrorResponse>();
    }
}
