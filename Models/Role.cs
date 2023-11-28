using System.ComponentModel.DataAnnotations;

namespace OnlineCourse.Models
{
    public class Role
    {
        [Key]
        public int Role_Id { get; set; }
        [Required]
        public string Role_Name { get; set; }
    }
}
