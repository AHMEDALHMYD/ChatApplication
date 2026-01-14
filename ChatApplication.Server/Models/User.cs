using System.ComponentModel.DataAnnotations;

namespace ChatApplication.Server.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool IsOnline { get; set; }

        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    }
}
