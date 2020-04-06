using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.kascodingNL
{
    class ProcessSnapshot
    {
        public Process snapshottedProcess { get; private set; }
        public int processId { get; private set; }
        public string processName { get; private set; }

        public ProcessSnapshot(Process snapshottedProcess)
        {
            this.snapshottedProcess = snapshottedProcess;
            processId = snapshottedProcess.Id;
            processName = snapshottedProcess.ProcessName;
        }
    }
}
