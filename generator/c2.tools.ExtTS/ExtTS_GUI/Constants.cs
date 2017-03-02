using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTS_GUI
{
    public static class Constants
    {
        public static readonly string ExtJS_Classic_Path = "classic\\classic\\src";
        public static readonly string ExtJS_Modern_Path = "modern\\modern\\src";
        public static readonly string ExtJS_Core_Path = "packages\\core\\src";

        public static readonly string JSDuckBin_Path = "0.tools\\jsduck-6.0.0-beta.exe";
        public static readonly string JSDuckOut_BasePath = "2.docs";

        public static readonly string ExtTSOut_BasePath = "3.out";

        #region Process running timeouts
        public static readonly int JSDuckTimeout = 600;
        #endregion Process running timeouts
    }
}
