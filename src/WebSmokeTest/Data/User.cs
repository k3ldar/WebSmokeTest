using System.Collections.Generic;

namespace SmokeTest.Data
{
    public class User
    {
        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public long UserId { get; set; }

        public bool ForceChangePassword { get; set; }

        public List<string> Claims { get; set; }
    }
}
