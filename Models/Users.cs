using System.ComponentModel.DataAnnotations;

namespace schoolEnrollment.Models
{
    public class Users
    {
        [Key]
        public int User_Id { get; set; }
        [Required]
        public string User_Account { get; set; }
        [Required]
        public string User_Password { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Role_Id { get; set; }
        public string? Phone { get; set; }

        public string? Email { get; set; }

        public byte[]? User_Img { get; set; }

    }
}
