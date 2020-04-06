using Assets.Scripts.kascodingNL.Abstractions;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.kascodingNL.Logic
{
    class MovementSnapshotter : CheckBase
    {
        public SortedDictionary<int, Quaternion> rotationSnapshots { get; private set; }

        public override void Awake()
        {
            rotationSnapshots = new SortedDictionary<int, Quaternion>();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
