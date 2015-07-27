
namespace Authentication.API.Models
{
    using System.ComponentModel.DataAnnotations;

    public class EmailModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Please provide a valid email address")]
        [DataType(DataType.EmailAddress)]
        public string From { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]       
        public string Subject { get; set; }

        [Required]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 50)] 
        public string Body { get; set; }
    }
}