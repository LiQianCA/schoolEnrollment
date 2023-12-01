using System.ComponentModel.DataAnnotations;

namespace schoolEnrollment.Models
{
    public class SelectList
    {
        [Key]
        public int Select_Id { get; set; }
        [Required]
        public int User_Id { get; set; }
        [Required]
        public int Course_Id { get; set; }
        [Required]
        public DateTime Select_Time { get; set; }
    }
}
