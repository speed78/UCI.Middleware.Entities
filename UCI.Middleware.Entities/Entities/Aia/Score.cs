using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCI.Middleware.Entities.Entities.Aia
{
    [Table("Scores")]
    public class Score
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime ReceivedDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1024)]
        public string FileFullPath { get; set; } = string.Empty;

        public virtual ICollection<CorrespondentScore> CorrespondentScores { get; set; } = new List<CorrespondentScore>();
    }
}
