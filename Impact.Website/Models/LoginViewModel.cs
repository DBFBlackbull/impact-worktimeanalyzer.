
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace Impact.Website.Models
{
	public class LoginViewModel
	{
        [Required]
		public string Username { get; set; }
        [Required]
        public string Password { get; set; }
	}
}