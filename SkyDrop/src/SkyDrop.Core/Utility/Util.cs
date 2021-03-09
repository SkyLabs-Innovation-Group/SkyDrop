using System;
namespace SkyDrop.Core.Utility
{
    public static class Util
    {
        public const string Portal = "https://siasky.net";

        public static string GetSkylinkUrl(string skylink)
        {
            return $"{Portal}/{skylink}";
        }
    }
}
