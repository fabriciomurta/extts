using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExtTS_GUI
{
    public partial class ExtTS_GUI : Form
    {
        private string lastSrcDir;
        private string lastSelTk;
        private BackgroundWorker worker;
        private Dictionary<string, string> availableVersions = new Dictionary<string, string>();


        private InitialValues initial;

        public ExtTS_GUI()
        {
            InitializeComponent();

            initial = new InitialValues(Width, Height, workDir_field.Width, progresLog_field.Height, progresLog_field.Width);

            MinimumSize = new Size
            {
                Width = ejsTk_field.Right + 23,
                Height = progresLog_field.Top + exit_button.Height + 90
            };

            worker = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_Completed);
        }

        #region Event handlers
        private void ExtTS_GUI_Load(object sender, EventArgs e)
        {
            workDir_field.Text = Environment.CurrentDirectory;
            
            if (Directory.Exists("extjs"))
            {
                ejsRoot_field.Text = ".\\extjs\\";
            }
        }

        private void workDir_field_TextChanged(object sender, EventArgs e)
        {
#if DEBUG
            log("Selected working directory: " + workDir_field.Text);
#endif
            update_ejsRoot_Field();
        }

        private void browsewd_button_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(workDir_field.Text) && Directory.Exists(workDir_field.Text))
            {
                browse_dialog.SelectedPath = workDir_field.Text;
            }

            browse_dialog.ShowDialog();

            workDir_field.Text = browse_dialog.SelectedPath;
        }


        private void ejsRoot_field_TextChanged(object sender, EventArgs e)
        {
#if DEBUG
            log("Selected ExtJS root path: " + ejsRoot_field.Text);
#endif
            update_Version_Combo();
        }

        private void browseejs_button_Click(object sender, EventArgs e)
        {
            var fullPath = string.Empty;

            if (!string.IsNullOrWhiteSpace(workDir_field.Text) && Directory.Exists(workDir_field.Text) &&
                !string.IsNullOrWhiteSpace(ejsRoot_field.Text))
            {
                fullPath = ejsRoot_field.Text;
                if (fullPath.StartsWith(".\\"))
                {
                    fullPath = Path.Combine(workDir_field.Text, fullPath.Substring(2));
                }

                if (Directory.Exists(fullPath))
                {
                    browse_dialog.SelectedPath = fullPath;
                }
                else
                {
                    browse_dialog.SelectedPath = workDir_field.Text;
                }
            }

            browse_dialog.ShowDialog();

            ejsRoot_field.Text = browse_dialog.SelectedPath;
            update_ejsRoot_Field();
        }


        private void ejsVer_field_TextChanged(object sender, EventArgs e)
        {
#if DEBUG
            log("Selected ExtJS version: " + ejsVer_field.Text);
#endif
            update_Toolkit_Combo();
        }

        private void generate_button_Click(object sender, EventArgs e)
        {
            if (generate_button.Text == "Generate")
            {
                if (!worker.IsBusy)
                {
                    generate_button.Text = "Cancel";
                    worker.RunWorkerAsync();
                }
            }
            else if (generate_button.Text == "Cancel")
            {
                worker.CancelAsync();
                generate_button.Text = "Cancelling...";
                generate_button.Enabled = false;
            }
        }

        private void exit_button_Click(object sender, EventArgs e)
        {
            log("Exitting application.");
            ActiveForm.Close();
        }
        #endregion Event Handlers

        #region Auxiliary methods
        // This delegate enables asynchronous calls for setting
        // the text property on the console text box control.
        delegate void LogAsyncCallback(string text);

        private void log(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return; // do not log at all if line is empty

            if (progresLog_field.InvokeRequired)
            {
                Invoke(new LogAsyncCallback(log), text);
            }
            else
            {
                var currTS = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.ffff");

                progresLog_field.Text = currTS + " - " + text + Environment.NewLine + progresLog_field.Text;
            }
            
        }

        private string getRelPath(string path, string relativeTo = null)
        {
            var wd = Regex.Replace(workDir_field.Text, "\\\\+$", string.Empty);
            var retVal = path;

            if (relativeTo == null)
            {
                relativeTo = Environment.CurrentDirectory;
            }

            if (string.IsNullOrWhiteSpace(wd))
            {
                wd = relativeTo;
            }
            
            if (path.StartsWith(".\\"))
            {
                if (wd != relativeTo)
                {
                    retVal = Path.Combine(wd, path.Substring(2));
                }
            }
            else
            {
                if (wd == relativeTo && path.StartsWith(wd))
                {
                    retVal = ".\\" + path.Substring(wd.Length);
                }
            }

            return retVal;
        }
        #endregion Auxiliary methods

        #region Field business logic handlers
        private void update_ejsRoot_Field()
        {
            var workdir = workDir_field.Text;
            var ejsroot = ejsRoot_field.Text;
            var dirFound = false;

#if DEBUG
            log("Updating ExtJS root...");
#endif

            if (!string.IsNullOrWhiteSpace(workdir) && Directory.Exists(workdir))
            {
                if (string.IsNullOrWhiteSpace(ejsroot))
                {
                    if (!string.IsNullOrEmpty(lastSrcDir))
                    {
                        // Get the path relative to the program working directory
                        // so it can verify if the dir really exists
                        lastSrcDir = getRelPath(lastSrcDir);

                        if (Directory.Exists(lastSrcDir))
                        {
                            dirFound = true;
                        }
                    }

                    if (dirFound)
                    {
                        // Get the path relative to the specified working directory
                        // or absolute if no relationship
                        ejsroot = getRelPath(lastSrcDir, workdir);
                    }
                    else
                    {
                        if (Directory.Exists(getRelPath(".\\extjs")))
                        {
                            ejsroot = ".\\extjs\\";
                        }
                        else
                        {
                            ejsroot = string.Empty;
                        }
                    }
                }
                else
                {
                    lastSrcDir = ejsroot;

                    ejsroot = getRelPath(ejsroot);

                    if (Directory.Exists(ejsroot))
                    {
                        if (ejsroot.StartsWith(workdir))
                        {
                            // relative path to the specified workdir -- not actual application WD
                            ejsroot = getRelPath(ejsroot, workdir);
                        }
                    }
                    else
                    {
                        ejsroot = string.Empty;
                    }
                }
            }
            else
            {
                ejsroot = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(ejsroot))
            {
                log("Please choose a suitable ExtJS sources root directory -- one directory level above ExtJS package has been unpacked.");
            }

            ejsRoot_field.Text = ejsroot;
        }

        private void update_Version_Combo()
        {
            var ejsRoot = getRelPath(ejsRoot_field.Text); // get relative path to actual APP WD
            string currentSelection = string.Empty;
            var jsonSerializer = new JsonSerializer();
            var added = false;
            JObject rawData;
            JToken field;

#if DEBUG
            log("Updating available ExtJS versions from folder: " + ejsRoot);
#endif
            availableVersions.Clear();

            if (!string.IsNullOrWhiteSpace(ejsRoot) && Directory.Exists(ejsRoot))
            {
                foreach (var dir in Directory.GetDirectories(ejsRoot))
                {
                    var packageJsonPath = Path.Combine(dir, "package.json");
                    if (File.Exists(packageJsonPath))
                    {
                        rawData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(packageJsonPath));

                        added = false;
                        if (rawData.TryGetValue("version", out field))
                        {
                            if (field.Type == JTokenType.String)
                            {
#if DEBUG
                                log("ExtJS " + field.Value<string>() + " on: " + packageJsonPath);
#endif
                                availableVersions.Add(field.Value<string>() + " (" + Path.GetFileName(dir) + ")", dir);
                                added = true;
                            }
                        }

                        if (!added)
                        {
                            log("Invalid package.json at: " + packageJsonPath);
                        }
                    }
#if DEBUG
                    else
                    {
                        log("ExtJS sources not found on dir: " + dir);
                    }
#endif
                }
            }

            if (ejsVer_field.Items.Count > 0)
            {
                currentSelection = (string)ejsVer_field.SelectedItem;
            }

            ejsVer_field.Items.Clear();

            var foundSelection = false;
            if (availableVersions.Count > 0)
            {
                foreach (var ver in availableVersions)
                {
                    ejsVer_field.Items.Add(ver.Key);

                    if (ver.Key == currentSelection)
                    {
                        foundSelection = true;
                        ejsVer_field.SelectedItem = ver.Key;
                    }
                }

                if (!foundSelection)
                {
                    ejsVer_field.SelectedIndex = 0;
                }
            }
            else
            {
                log("No ExtJS sources found on the specified directory.");
                ejsVer_field.Text = string.Empty;
            }
        }

        private void update_Toolkit_Combo()
        {
            var ejsRoot = ejsRoot_field.Text;
            var ejsVerValue = ejsVer_field.Text;
            var ejsFolder = string.Empty;
            var selectedAny = false;
            var availableToolkits = new List<string>();
            var knownToolkits = new Dictionary<string, string>();

#if DEBUG
            log("Updating available ExtJS toolkits from folder: " + ejsRoot);
#endif

            if (!string.IsNullOrWhiteSpace(ejsTk_field.Text))
            {
                lastSelTk = ejsTk_field.Text;
            }

            if (!string.IsNullOrWhiteSpace(ejsVerValue) &&
                availableVersions.TryGetValue(ejsVerValue, out ejsFolder) &&
                Directory.Exists(ejsFolder))
            {
                knownToolkits.Add("Classic", Path.Combine(ejsFolder, Constants.ExtJS_Classic_Path));
                knownToolkits.Add("Modern", Path.Combine(ejsFolder, Constants.ExtJS_Modern_Path));
                knownToolkits.Add("Core", Path.Combine(ejsFolder, Constants.ExtJS_Core_Path));
            }

            ejsTk_field.Items.Clear();
            ejsTk_field.Text = string.Empty;

            foreach (var tk in knownToolkits)
            {
                if (Directory.Exists(tk.Value))
                {
                    availableToolkits.Add(tk.Key);
                }
            }

            if (availableToolkits.Count > 1)
            {
                ejsTk_field.Items.Add("All");

                if (lastSelTk == "All")
                {
                    ejsTk_field.SelectedItem = "All";
                    selectedAny = true;
                }
            }

            foreach (var tkName in availableToolkits)
            {
                ejsTk_field.Items.Add(tkName);

                if (lastSelTk == "tkName")
                {
                    ejsTk_field.SelectedItem = tkName;
                    selectedAny = true;
                }
            }

            // If no selection could be maintained, then select first, if any.
            if (!selectedAny && ejsTk_field.Items.Count > 0)
            {
                ejsTk_field.SelectedIndex = 0;
            }

            generate_button.Enabled = !string.IsNullOrWhiteSpace(ejsTk_field.Text);

#if DEBUG
            if (generate_button.Enabled)
            {
                log("Ready to generate .d.ts.");
            }
            else
            {
                log("Fill in all information before you can generate the .d.ts file.");
            }

            log("Generate button enabled state now is: " + generate_button.Enabled);
#endif
        }
        #endregion Field business logic handlers

        #region Asynchronous task handlers
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var me = sender as BackgroundWorker;

            log("Starting .d.ts file generation.");

            var workdir = workDir_field.Text;
            var ejsVerFldContents = tsGetText(ejsVer_field);
            var ejsPath = Path.Combine(availableVersions[ejsVerFldContents], Constants.ExtJS_Classic_Path);
            var ejsVer = ejsVerFldContents.Split(' ')[0]; // assume it will only have space after version number '6.2.1 (ext-6.2.1)'

            if (cancelRequested(e)) return;

            var jsdp = JSDuck.RunAsync(workdir, ejsPath, ejsVer, log);

            JSDuck.PollExecution(jsdp, Constants.JSDuckTimeout, log, cancelRequested, e);

            log("Finished generating .d.ts file.");
        }

        private bool cancelRequested(DoWorkEventArgs e)
        {
            if (worker.CancellationPending)
            {
                log("Operation aborted by user request.");
                e.Cancel = true;
                return true;
            }

            return false;
        }

        private void worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            generate_button.Text = "Generate";

            if (e.Cancelled)
            {
                log("Generation process cancelled by user request.");
                generate_button.Enabled = true;
            }
            else if (e.Error != null)
            {
                log("Task finished with error: " + e.Error.Message);

#if DEBUG
                log("Stack trace: " + Environment.NewLine + e.Error.StackTrace);
#endif
            }
            else
            {
                log("Finished running the generation process.");
            }
        }

        /// <summary>
        /// Get component's Text value in a thread-safe way.
        /// </summary>
        /// <param name="c">Control handle</param>
        /// <returns>The .Text property/field of the control.</returns>
        private string tsGetText(Control c)
        {
            var retVal = string.Empty;

            if (c.InvokeRequired)
            {
                Invoke((MethodInvoker)delegate ()
                {
                    retVal = c.Text;
                });
            }
            else
            {
                retVal = c.Text;
            }

            return retVal;
        }
        #endregion Assynchronous task handlers
    }
}
