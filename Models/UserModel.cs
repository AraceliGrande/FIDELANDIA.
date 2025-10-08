using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIDELANDIA.Models
{
    public class UserModel
    {
        private int _id;
        private string _username;
        private string _password;
        private string _email;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                }
            }
        }

        public string Username { 
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                }
            }
        }
        public string Password { 
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                }
            }
        }
        public string Email { 
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                }
            }
        }

      

    }
}
