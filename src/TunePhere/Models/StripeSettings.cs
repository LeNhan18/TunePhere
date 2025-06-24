using System.ComponentModel.DataAnnotations;

namespace TunePhere.Models
{
    public class StripeSettings
    {
        [Required]
        public string PublicKey { get; set; }

        [Required]
        public string SecretKey { get; set; }
    }
}
