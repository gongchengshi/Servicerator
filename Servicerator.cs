using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace Gongchengshi
{
    public class Servicerator : ServiceBase
    {
        public Servicerator()
        {
            AutoLog = true;
            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
        }

        public string[] ProcessArgs { set; get; }

        private Process _process;

        protected override void OnStart(string[] args)
        {
            try
            {
                if (args.Length != 0)
                {
                    // Do this in case new arguments are provided at startup.
                    ProcessArgs = args;
                }

                if (ProcessArgs == null || ProcessArgs.Length == 0)
                {
                    throw new Exception("Servicerator requires at least one argument specifying the name of the executable.");
                }

                var filename = ProcessArgs[0];

                if (!File.Exists(filename))
                {
                    throw new FileNotFoundException(string.Format("{0} does not exist.", filename));
                }

                EventLog.WriteEntry(string.Format("Starting {0}...", filename));

                var startInfo = new ProcessStartInfo
                                    {
                                        CreateNoWindow = true,
                                        ErrorDialog = false,
                                        RedirectStandardError = true,
                                        RedirectStandardInput = true,
                                        RedirectStandardOutput = true,
                                        UseShellExecute = false,
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        FileName = filename,
                                        WorkingDirectory = Path.GetDirectoryName(filename)
                                    };

                if (ProcessArgs.Length > 1)
                {
                    startInfo.Arguments = string.Join(" ", ProcessArgs, 1, ProcessArgs.Length - 1);
                }

                _process = new Process {StartInfo = startInfo, EnableRaisingEvents = true};
                _process.OutputDataReceived += OutputDataReceived;
                _process.ErrorDataReceived += OutputDataReceived;

                var fullCommand = string.Format("{0} {1}", filename, startInfo.Arguments);

                if (_process.Start())
                {
                    EventLog.WriteEntry(string.Format("Successfully started '{0}'.", fullCommand),
                                        EventLogEntryType.Information);
                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();
                    _process.Exited += ProcessExited;
                }
                else
                {
                    throw new Exception(string.Format("Failed to start '{0}'.", fullCommand));
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);

                throw;
            }
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // Do nothing for right now
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            Stop();
        }

        protected override void OnStop()
        {
            try
            {
                _process.Kill();
                _process.WaitForExit();
            }
            catch
            {
                // Will throw if already exited, or if process could not be 
                // terminated.  Ignore these.
            }
            
            _process.Dispose();
        }

        protected override void OnShutdown()
        {
            OnStop();
        }
    }
}
