using System.ComponentModel.DataAnnotations;

namespace schoolEnrollment.Models
{
    public class Course
    {
        [Key]
        public int Course_Id { get; set; }
        [Required]
        public string Course_Name { get; set; }
        [Required]
        public DateTime Start_Time { get; set; }
        [Required]
        public DateTime End_Time { get; set; }
        [Required]
        public int Course_Hours { get; set; }
        [Required]
        public decimal Course_Price { get; set; }
        public string? Course_Description { get; set; }
        

        public byte[]? Course_Img { get; set; }
        public byte[]? Course_File { get; set; }
    }
}
