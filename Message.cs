using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baigiamasis
{
    internal struct Message
    {
        public string UserId;
        public string Msg;
        public int UsersFunction; //0 none, 1 list, 2 add, 3 remove
        public List<User>? Users;
    }
}
