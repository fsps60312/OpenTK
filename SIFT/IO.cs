using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SIFT
{
    public static class IO
    {
        public static string Read(string path)
        {
            return new StreamReader(path, Encoding.UTF8).ReadToEnd();
        }
        public static string ReadResource(string path)
        {
            return new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(path), Encoding.UTF8).ReadToEnd();
        }
    }
}
