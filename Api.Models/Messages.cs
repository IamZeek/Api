using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models
{
    public class Messages
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Sender { get; set; }
        public string Receiver { get; set; }
        [Required]
        public string Content { get; set; }

    }
}
