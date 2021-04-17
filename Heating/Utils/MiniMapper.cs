using System;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public static class MiniMapper
    {

        /// <summary>
        /// Diese Methode ist eine allgemeine Methode zum Kopieren von 
        /// oeffentlichen Objekteigenschaften.
        /// </summary>
        /// <param name="target">Zielobjekt</param>
        /// <param name="source">Quelleobjekt</param>
        public static void CopyProperties(object target, object source)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));
            Type sourceType = source.GetType();
            Type targetType = target.GetType();
            foreach (var piSource in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(pi => pi.CanRead))
            {
                if (piSource.PropertyType.FullName != null && !piSource.PropertyType.FullName.StartsWith("System.Collections.Generic.ICollection"))  // kein Navigationproperty
                {
                    PropertyInfo piTarget;
                    piTarget = targetType
                                         .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                         .SingleOrDefault(pi => pi.Name.Equals(piSource.Name)
                                                                && pi.PropertyType == piSource.PropertyType
                                                                && pi.CanWrite);
                    if (piTarget != null)
                    {
                        object value = piSource.GetValue(source, null);
                        piTarget.SetValue(target, value, null);
                    }
                }
            }
        }




    }
}
