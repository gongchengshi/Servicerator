using System.Collections.Generic;
using System.ServiceProcess;

namespace Gongchengshi
{
    static class Program
    {
        /// <summary>
        /// To create a service, on command line type:
        /// 
        /// sc create [ServiceName] binPath= "[FullPath to Servicerator exe] [ServiceName] [Full path to executable (and arguments)]"
        /// 
        /// use \" to quote paths and arguments with spaces
        /// </summary>
        static void Main(string[] args)
        {
            var service = new Servicerator {ServiceName = args[0]};

            if (args.Length > 1)
            {
                var processArgs = new List<string>(args);
                processArgs.RemoveAt(0);

                service.ProcessArgs = processArgs.ToArray();
            }

            ServiceBase.Run(service);
        }
    }
}
