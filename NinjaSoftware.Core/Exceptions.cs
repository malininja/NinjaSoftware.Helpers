using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NinjaSoftware.Core
{
    /// <summary>
    /// Used for user input validation.
    /// When validation brakes, throw this exception in bussines logic.
    /// It will bubble to client which will, hopefully :-), handle it properly, and display message to user.
    /// </summary>
    public class UserException : Exception
    {
        public UserException(string message) : base(message) { }
    }
}
