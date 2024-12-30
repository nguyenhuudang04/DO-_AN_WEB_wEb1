using System.ComponentModel.DataAnnotations;

namespace web1.Models
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public string? Title { get; set; }
        
        public string? Description { get; set; }

        public string? Link { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Display order must be greater than or equal to 0")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
} 