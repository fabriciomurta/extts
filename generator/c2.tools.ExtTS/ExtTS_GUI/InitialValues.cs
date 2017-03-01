using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTS_GUI
{
    public class InitialValues
    {
        private int appHeight;
        public int AppHeight {
            get
            {
                return appHeight;
            }
        }

        private int appWidth;
        public int AppWidth
        {
            get
            {
                return appWidth;
            }
        }

        private int pathFieldWidth;
        public int PathFieldWidth
        {
            get
            {
                return pathFieldWidth;
            }
        }

        private int consoleHeight;
        public int ConsoleHeight
        {
            get
            {
                return consoleHeight;
            }
        }

        private int consoleWidth;
        public int ConsoleWidth
        {
            get
            {
                return consoleWidth;
            }
        }

        public InitialValues(int appW, int appH, int pfW, int cH, int cW)
        {
            appWidth = appW;
            appHeight = appH;
            pathFieldWidth = pfW;
            consoleHeight = cH;
            consoleWidth = cW;
        }
    }
}
