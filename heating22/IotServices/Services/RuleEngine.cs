using Core.Entities;

using Microsoft.Extensions.Hosting;

using Serilog;

using Services.Contracts;
using Services.Fsm;

namespace Services
{
    public class RuleEngine
    {
        private static readonly Lazy<RuleEngine> lazy = new(() => new RuleEngine());
        public static RuleEngine Instance { get { return lazy.Value; } }


    }
}
