using System.ComponentModel.DataAnnotations;

namespace RateApp.Models
{
    public class UserRatingViewModel
    {
        [Required(ErrorMessage = "Please select a rating between 1 and 5.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int RatingValue { get; set; }

        public string Comment { get; set; }

        [Required]
        public int UserId { get; set; }  // This will keep track of the supplier being rated
        public int RaterId { get; internal set; }
        public object RatedUserId { get; internal set; }

        [Required]
        public string OTP { get; set; }
    }
}