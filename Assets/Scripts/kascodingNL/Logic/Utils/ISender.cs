using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.kascodingNL
{
    public class ISender
    {
        public string senderHash { get; private set; }

        public ISender(string hash)
        {
            senderHash = hash;
        }
    }
}
