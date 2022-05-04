
using System;
using System.Linq;
using System.Reflection;

namespace Base.Helper
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


        /// <summary>
        /// Vergleicht zwei Objekte unterschiedlichen Typs, indem gleichnamige Properties mit gleichem Typ
        /// mittels Equals verglichen werden. Stimmen alle gleichnamigen Properties überein, gilt der 
        /// Vergleich als erfüllt.
        /// Ist der Propertytyp unterschiedlich, wird kein Vergleich durchgeführt.
        /// </summary>
        /// <param name="obj1">beliebiges Objekt mit öffentlichen Properties</param>
        /// <param name="obj2">beliebiges Objekt mit öffentlichen Properties</param>
        /// <returns>true, wenn es zumindest ein Property gibt, das in beiden Objekten vorkommt und ungleich ist</returns>
        public static bool AnyPropertyValuesDifferent(object obj1, object obj2)
        {
            if (obj1 == null) throw new ArgumentNullException(nameof(obj1));
            if (obj2 == null) throw new ArgumentNullException(nameof(obj2));
            Type leftType = obj1.GetType();
            Type rightType = obj2.GetType();
            foreach (var piLeft in leftType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(pi => pi.CanRead))
            {
                if (piLeft.PropertyType.FullName != null && !piLeft.PropertyType.FullName.StartsWith("System.Collections.Generic.ICollection"))  // kein Navigationproperty
                {
                    PropertyInfo piRight;
                    piRight = rightType
                                         .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                         .SingleOrDefault(pi => pi.Name.Equals(piLeft.Name)
                                                                && pi.PropertyType == piLeft.PropertyType
                                                                && pi.CanRead);
                    if (piRight != null)
                    {
                        object valueLeft = piLeft.GetValue(obj1, null);
                        object valueRight = piRight.GetValue(obj2, null);
                        // Properties sind unterschiedlich, wenn eines null und das andere nicht null ist
                        if (valueLeft == null && valueRight != null ||
                            valueRight == null && valueLeft != null)
                        {
                            return true;
                        }
                        else if (valueLeft != null && valueRight != null
                            && !valueLeft.Equals(valueRight))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }




    }
}
