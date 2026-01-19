using AlphaLogistics.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class RegisterUserDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Phone { get; set; }

       // public string? Address { get; set; }

        //[Required]
        public int RoleId { get; set; }  // User Role

        public IFormFile? ProfileImage { get; set; }
    }
}
