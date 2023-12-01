using System.ComponentModel.DataAnnotations;

namespace schoolEnrollment.Models
{
    public class Enrollment
    {
        [Key]
        public int Enrollment_Id { get; set; }
        [Required]
        public int User_Id { get; set; }
        [Required]
        public int Course_Id { get; set; }
        [Required]
        public DateTime Enrollment_Time { get; set; }
        [Required]
        public string Enrollment_Status { get; set; }

        public string? Payment_Amount { get; set; }
        
        public DateTime? Payment_Time { get; set; }
        public string? Payment_Status { get; set; }
    }
}
