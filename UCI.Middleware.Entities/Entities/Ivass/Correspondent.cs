using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCI.Middleware.Entities.Entities.Ivass
{
    [Table("Correspondents")]
    public class Correspondent
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Code")]
        public required string Code { get; set; }

        [StringLength(100)]
        [Column("BdsIdentifier")]
        public required string BdsIdentifier { get; set; }

        [Required]
        [StringLength(20)]
        [Column("UciCode")]
        public required string UciCode { get; set; }

        [Required]
        [StringLength(200)]
        [Column("ConventionalName")]
        public required string ConventionalName { get; set; }

        public bool Type { get; set; }

        [Column("ReceiveNotifications")]
        public bool ReceiveNotifications { get; set; } = false;

        [StringLength(255)]
        [Column("NotificationEmail")]
        [EmailAddress]
        public string? NotificationEmail { get; set; }
    }


}
