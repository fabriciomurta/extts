using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTS_GUI
{
    public class JSDuck
    {
        private string workingDirectory;
        private string extjsPath;
        private string extjsVer;
        private Action<string> log;

        public JSDuck(string wd, string ep, string ev, Action<string> lfn = null)
        {
            if (wd == Environment.CurrentDirectory)
            {
                workingDirectory = ".\\";
            }
            else
            {
                workingDirectory = wd;
            }
            
            extjsPath = ep;
            extjsVer = ev;
            log = lfn;
        }

        public Process Run(bool async = false, int serialTimeout = 60000)
        {
            var jsDuckBin_Path = Path.Combine(workingDirectory, Constants.JSDuckBin_Path);
            var jsDuck_OutPath = Path.Combine(workingDirectory, Constants.JSDuckOut_BasePath, "ext-" + extjsVer);
            var procArgs = '"' + extjsPath + '"' + " --output " + '"' + jsDuck_OutPath + '"';

            if (!File.Exists(jsDuckBin_Path))
            {
                throw new FileNotFoundException("JSDuck executable not found.", jsDuckBin_Path);
            }

            if (!Directory.Exists(workingDirectory))
            {
                throw new FileNotFoundException("Work directory does not exist.", workingDirectory);
            }

            // Create the docs output directory if it does not exist.
            if (!Directory.Exists(Path.Combine(workingDirectory, Constants.JSDuckOut_BasePath)))
            {
                Directory.CreateDirectory(Path.Combine(workingDirectory, Constants.JSDuckOut_BasePath));
            }

            var jsDuckProc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    WorkingDirectory = Environment.CurrentDirectory,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = procArgs,
                    FileName = jsDuckBin_Path
                }
            };

            if (log != null)
            {
#if DEBUG
                log("Running: " + jsDuckBin_Path + " " + procArgs);
#endif

                // Pipe output to logger if present.
                jsDuckProc.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data.Length > 0) log(args.Data);
                };

                jsDuckProc.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data.Length > 0)
                        log("*** " + (args.Data.StartsWith("Warning:") ? "" : "Error: ") + args.Data);
                };
            }

            jsDuckProc.Start();
            jsDuckProc.BeginOutputReadLine();
            jsDuckProc.BeginErrorReadLine();

            // wait max 1 minute for process to finish...
            if (!async)
            {
                if (!jsDuckProc.WaitForExit(serialTimeout))
                {
                    Util.KillJSDuck(jsDuckProc, log);
                    throw new TimeoutException("The JSDuck process did not finish in " + (serialTimeout / 1000) + " seconds. Executable path: " + jsDuckProc.StartInfo.FileName);
                }
            }

            return jsDuckProc;
        }

        /// <summary>
        /// Convenience method to run without directly instantiating
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="ep"></param>
        /// <param name="ev"></param>
        /// <param name="lfn"></param>
        /// <returns></returns>
        public static bool Run(string wd, string ep, string ev, Action<string> lfn = null)
        {
            new JSDuck(wd, ep, ev, lfn).Run();

            return true;
        }

        /// <summary>
        /// Convenience method to run without directly instantiating
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="ep"></param>
        /// <param name="ev"></param>
        /// <param name="lfn"></param>
        /// <returns></returns>
        public static Process RunAsync(string wd, string ep, string ev, Action<string> lfn = null)
        {
            return new JSDuck(wd, ep, ev, lfn).Run(true);
        }
    }
}
