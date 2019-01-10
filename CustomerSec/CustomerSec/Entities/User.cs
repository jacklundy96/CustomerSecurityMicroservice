using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerSec.Entities
{
    public class User 
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }

        public List<string> GetAll()
        {
            return new List<string>() { Email, Username, Password , PhoneNumber};
        }
    }
}
