using Assets.Scripts.kascodingNL.Abstractions;
using System.Collections.Generic;
using System.Diagnostics;

namespace Assets.Scripts.kascodingNL.Logic
{
    class ProcessSnapshotter : CheckBase
    {
        public List<ProcessSnapshot> snapshots = new List<ProcessSnapshot>();

        public override void Awake()
        {

        }

        public override void Start()
        {
            
        }

        public override void CheckCheat()
        {
            snapshots.Clear();
            foreach(Process process in Process.GetProcesses())
            {
                snapshots.Add(new ProcessSnapshot(process));
            }
        }
    }
}
