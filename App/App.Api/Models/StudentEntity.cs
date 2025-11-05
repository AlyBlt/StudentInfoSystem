using System.ComponentModel.DataAnnotations;

namespace App.Api.Models
{
    public class StudentEntity
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "First names is required.")]
        [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "First Name should contain letter only.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last names is required.")]
        [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$", ErrorMessage = "Last Name should contain letter only.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Student number is required.")]
        public string StudentNumber { get; set; }

        [Range(1, 12, ErrorMessage = "Grade should be between 1-12.")]
        public int Grade { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}
