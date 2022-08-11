using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Momentum_Dummy_API.Models;

namespace Momentum_Dummy_API.Service
{
    public class UserService : IUserService
    {
        private List<User> _users = new List<User>
        {
            new User { Id = 1, Username = "momentumtest", Password = "TOkETONeHoca" },
            new User { Id = 2, Username = "inovotest", Password = "ABLeORYgElde"}
        };
        public async Task<User> Authenticate(string username, string password)
        {
            var user = await Task.Run(() => _users.SingleOrDefault(x => x.Username == username && x.Password == password));
            if (user == null)
                return null;
            return user;
        }
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await Task.Run(() => _users);
        }
    }
}
