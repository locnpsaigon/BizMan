using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace BizMan.Helpers.Security
{
    public class BizManPrinciple : IPrincipal
    {
        public IIdentity Identity { get; private set; }

        public BizManPrinciple(String Username)
        {
            this.Identity = new GenericIdentity(Username);
        }

        public Boolean IsInRole(string userRoles)
        {
            foreach (var role in Roles)
            {
                if (userRoles.Contains(role))
                {
                    return true;
                }   
            }
            return false;
        }

        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreationDate { get; set; }
        public string[] Roles { get; set; }
    }
}