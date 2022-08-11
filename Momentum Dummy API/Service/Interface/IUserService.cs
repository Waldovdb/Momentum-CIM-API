using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Momentum_Dummy_API.Models;

namespace Momentum_Dummy_API
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
    }
}
