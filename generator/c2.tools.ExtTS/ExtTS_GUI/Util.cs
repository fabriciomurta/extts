using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ExtTS_GUI
{
    public static class Util
    {
        /// <summary>
        /// JSDuck spawns a ruby process in background which does not get
        /// killed when the JSDuck process is killed.
        /// </summary>
        /// <param name="proc">The JSDuck process handle.</param>
        /// <returns>The success of the kill operation</returns>
        public static bool KillJSDuck(Process baseProc, Action<string> log)
        {
            Process parentProc = null;

            // We assume JSDuck problem is only generating the 'ruby' child
            // process and kill any that
            foreach (var process in Process.GetProcessesByName("ruby"))
            {
#if DEBUG
                log?.Invoke("Processing process " + process.ProcessName);
#endif
                try
                {
                    parentProc = ParentProcessUtilities.GetParentProcess(process.Handle);

                    if (parentProc != null && parentProc.Id == baseProc.Id)
                    {
#if DEBUG
                        log?.Invoke("Its base process is our jsduck process!");
#endif
                        process.Kill();

                        if (!process.HasExited)
                        {
                            throw new Exception("Unable to kill child process " + process.ProcessName + " (" + process.Id + "). Parent: " + process.ProcessName + " (" + process.Id + ").");
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is System.ComponentModel.Win32Exception && e.Message == "Access is denied")
                    {
#if DEBUG
                        log?.Invoke("No access to process: " + process.ProcessName + " (" + process.Id + ")");
#endif
                        continue;
                    }

                    log?.Invoke("Unhadled exeption: " + e.GetType().ToString() + ": " + e.Message);
                    throw e;
                }
            }

            // Finally, kill the JSDuck process itself.
            baseProc.Kill();

            return baseProc.HasExited;
        }
    }
}
