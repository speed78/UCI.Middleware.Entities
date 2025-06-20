using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCI.Middleware.Entities.Entities.Aia
{
    [Table("CorrespondentScores")]
    public class CorrespondentScore
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("ScoreId")]
        public int ScoreId { get; set; }

        [Required]
        [Column("IdCompany")]
        public int IdCompany { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("FileName")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("FileFullPath")]
        public string FileFullPath { get; set; } = string.Empty;

        [ForeignKey("ScoreId")]
        public virtual Score Score { get; set; } = null!;
    }
}
