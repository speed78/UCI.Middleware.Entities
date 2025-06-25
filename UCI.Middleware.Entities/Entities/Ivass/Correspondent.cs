using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCI.Middleware.Entities.Entities.Ivass
{
    /// <summary>
    /// Represents a correspondent entity in the IVASS system.
    /// </summary>
    [Table("Correspondents")]
    public class Correspondent
    {
        /// <summary>
        /// Gets or sets the unique identifier for the correspondent.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the code that identifies the correspondent.
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("Code")]
        public required string Code { get; set; }

        /// <summary>
        /// Gets or sets the BDS (Business Data Service) identifier for the correspondent.
        /// </summary>
        [StringLength(100)]
        [Column("BdsIdentifier")]
        public required string BdsIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the UCI code for the correspondent.
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("UciCode")]
        public required string UciCode { get; set; }

        /// <summary>
        /// Gets or sets the conventional display name for the correspondent.
        /// </summary>
        [Required]
        [StringLength(200)]
        [Column("ConventionalName")]
        public required string ConventionalName { get; set; }

        /// <summary>
        /// Gets or sets the type of the correspondent.
        /// </summary>
        public bool Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the correspondent should receive notifications.
        /// </summary>
        [Column("ReceiveNotifications")]
        public bool ReceiveNotifications { get; set; } = false;

        /// <summary>
        /// Gets or sets the email address for receiving notifications.
        /// </summary>
        /// <remarks>
        /// This field is only used when <see cref="ReceiveNotifications"/> is set to true.
        /// </remarks>
        [StringLength(255)]
        [Column("NotificationEmail")]
        [EmailAddress]
        public string? NotificationEmail { get; set; }
    }
}
