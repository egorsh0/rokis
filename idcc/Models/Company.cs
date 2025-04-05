using System.ComponentModel.DataAnnotations;

namespace idcc.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(12)]
        public required string Inn {get; set;} 
        
        [MaxLength(255)]
        public required string Email { get; set; }
        
        [MaxLength(255)]
        public required string PasswordHash { get; set; }
    }
}