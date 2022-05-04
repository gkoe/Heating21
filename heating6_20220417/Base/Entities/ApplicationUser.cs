
using Microsoft.AspNetCore.Identity;

using System.Collections.Generic;

namespace Base.Entities
{
    /// <summary>
    /// Erweitert den IdentityUser um ein Namensfeld
    /// IdentityUser verwendet die Mailadresse als Namen
    /// Es werden auch die Sessions des Benutzers gespeichert
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
