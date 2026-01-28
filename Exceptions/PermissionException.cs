using System;

namespace SHLAPI
{
    public class PermissionException:Exception
    {
        public PermissionException(string msg):base(msg)
        {
        }
    }
}