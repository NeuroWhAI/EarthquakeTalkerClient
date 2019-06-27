using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthquakeTalkerClient
{
    internal static class Util
    {
        public static bool CheckImageUri(string text)
        {
            string[] imageTypes =
            {
                ".png", ".jpg", ".bmp", ".jpeg", ".gif", // TODO: More...?
            };

            return text.Contains('\n') == false
                && imageTypes.Any(imgType => text.TrimEnd().EndsWith(imgType));
        }
    }
}
