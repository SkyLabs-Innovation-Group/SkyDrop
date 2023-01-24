using System;
using SkyDrop.Core.DataModels;

namespace SkyDrop.Core.Exceptions
{
    public class PortalUnreachableException : System.Exception
    {
        public string SkynetPortal;
        public PortalUnreachableException(string portal, string message) : base(message + portal)
        {
            SkynetPortal = portal;
        }
    }
}

