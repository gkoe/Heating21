using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public string SecretKey { get; set; } = "";
        
        /// <summary>
        /// 
        /// </summary>
        public string ValidAudience { get; set; } = "";
        
        /// <summary>
        /// 
        /// </summary>
        public string ValidIssuer { get; set; } = "";
    }
}
