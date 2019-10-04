using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private List<string> extjsPaths;
        private string extjsVer;
        private string extjsToolkit;
        private Action<string> log;
        
        public string OutputPath
        {
            get
            {
                return Path.Combine(workingDirectory, Constants.JSDuckOut_BasePath, "ext-" + extjsVer + "-" + extjsToolkit);
            }
        }
        public JSDuck(string wd, string ep, string ev, string et, Action<string> lfn = null) :
            this(wd, new List<string> { ep }, ev, et, lfn) { }

        public JSDuck(string wd, List<string> eps, string ev, string et, Action<string> lfn = null)
        {
            if (wd == Environment.CurrentDirectory)
            {
                workingDirectory = ".\\";
            }
            else
            {
                workingDirectory = wd;
            }

            extjsPaths = eps;
            extjsVer = ev;
            extjsToolkit = et;
            log = lfn;
        }

        public Process Run(bool async = false, int serialTimeout = 60000)
        {
            var jsDuckBin_Path = Path.Combine(workingDirectory, Constants.JSDuckBin_Path);
            var jsDuck_OutPath = OutputPath;
            var procArgs = '"' + string.Join("\" \"", extjsPaths) + '"' + " --output " + '"' + jsDuck_OutPath + '"';

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
                    if (!string.IsNullOrWhiteSpace(args.Data)) log(args.Data);
                };

                jsDuckProc.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrWhiteSpace(args.Data))
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
        /// <param name="wd">Working/base directory</param>
        /// <param name="ep">ExtJS Path</param>
        /// <param name="ev">ExtJS Version</param>
        /// <param name="et">ExtJS Toolkit</param>
        /// <param name="lfn">Logger function name</param>
        /// <returns></returns>
        public static bool Run(string wd, string ep, string ev, string et, Action<string> lfn = null)
        {
            new JSDuck(wd, ep, ev, et, lfn).Run();

            return true;
        }

        /// <summary>
        /// Convenience method to run without directly instantiating
        /// </summary>
        /// <param name="wd">Working/base directory</param>
        /// <param name="ep">ExtJS Path</param>
        /// <param name="ev">ExtJS Version</param>
        /// <param name="et">ExtJS Toolkit</param>
        /// <param name="lfn">Logger function name</param>
        /// <returns></returns>
        public static Process RunAsync(string wd, string ep, string ev, string et, Action<string> lfn = null)
        {
            return new JSDuck(wd, ep, ev, et, lfn).Run(true);
        }

        /// <summary>
        /// Asynchronously run this instance.
        /// </summary>
        /// <returns>The process object.</returns>
        public Process RunAsync()
        {
            return Run(true);
        }

        /// <summary>
        /// Poll for job finish but every 250ms check if timeout or cancel request has been issued.
        /// </summary>
        /// <param name="jsdp">The JSDuck process object to watch for finish.</param>
        /// <param name="timeout">The timeout in seconds to wait the process to finish.</param>
        /// <param name="log">The log function to update output to console.</param>
        /// <param name="cancelRequested">The function to handle the cancellation process.</param>
        /// <param name="e">The event data object to watch for cancelation stimulus.</param>
        public static void PollExecution(Process jsdp, int timeout, Action<string> log, Func<DoWorkEventArgs, bool> cancelRequested,
            DoWorkEventArgs e)
        {
            var timeLimit = DateTime.Now.AddSeconds(timeout);

            // Poll for job finish but every 250ms check if timeout or cancel
            // request has been issued.
            while (!jsdp.WaitForExit(250))
            {
                if (timeLimit < DateTime.Now)
                {
                    log("JSDuck process did not finish in " + timeout + " seconds. Interrupting.");

                    Util.KillJSDuck(jsdp, log);
                    break;
                }

                if (cancelRequested(e))
                {
#if DEBUG
                    log("JSDuck process cancel request received. Killing JSDuck and child processes.");
#endif
                    Util.KillJSDuck(jsdp, log);
                    return;
                }
            }
        }
    }
}
