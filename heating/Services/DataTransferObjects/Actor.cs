
using Serilog;

using System;

namespace Services.DataTransferObjects
{
    public class Actor : Item
    {
        public Actor(string actorName, int id) : base(actorName, id)
        {
        }
    }



}
