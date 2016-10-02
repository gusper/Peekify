using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peekify.App
{
    internal static class Utils
    {
        internal static string GetAppDataPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dirName = new FileInfo(appDataPath).FullName;
            dirName += @"\Peekify";

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            return dirName;
        }
    }
}
