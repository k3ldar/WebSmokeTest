using System.Collections.Generic;

namespace WebSmokeTest.Data
{
    public class User
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int UserId { get; set; }

        public bool ForceChangePassword { get; set; }

        public List<string> Claims { get; set; }
    }
}
