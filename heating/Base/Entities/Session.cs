
using System;

namespace Base.Entities
{
    /// <summary>
    /// Login/Logoutdaten der Benutzer
    /// </summary>
    public class Session : EntityObject
    {
        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }
        public DateTime Login { get; set; }
        public DateTime? Logout { get; set; }

    }
}
