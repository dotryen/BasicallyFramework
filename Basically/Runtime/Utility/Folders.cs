using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Basically.Utility {
    public static class Folders {
        public static readonly string ROOT = Directory.GetCurrentDirectory() + "\\";
        public static string LOGS => ROOT + "\\" + "Logs";
    }
}
