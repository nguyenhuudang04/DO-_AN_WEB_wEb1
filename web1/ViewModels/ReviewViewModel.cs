using System.ComponentModel.DataAnnotations;

namespace web1.ViewModels
{
    public class ReviewViewModel
    {
        public int ProductId { get; set; }
        public int OrderId { get; set; }

        [Required]
        [Range(1, 5)]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [StringLength(500)]
        [Display(Name = "Comment")]
        public string? Comment { get; set; }
    }
} 