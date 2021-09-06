using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DataTransferObjects
{
    public class FsmStateChangedInfoDto
    {
        public string Fsm { get; set; }
        public string LastState { get; set; }
        public string ActState { get; set; }
        public string Input { get; set; }
    }

}
