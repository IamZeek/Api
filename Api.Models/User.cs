using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }
        public string Password { get; set; }
        public string?  Token { get; set; }
        public string? Role { get; set; }


    }
}
