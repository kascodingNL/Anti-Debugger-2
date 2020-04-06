using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.kascodingNL.Abstractions
{
    public abstract class CheckBase
    {
        public virtual void CheckCheat() { }
        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }
    }
}
