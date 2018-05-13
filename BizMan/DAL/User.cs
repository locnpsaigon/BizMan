using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.DAL
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string Salt { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public Boolean IsActive { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

    }
}