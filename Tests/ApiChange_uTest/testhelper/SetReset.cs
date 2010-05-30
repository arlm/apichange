

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace UnitTests
{
    public class SetReset<T> : IDisposable
    {
        T myOld;
        Action<T> mySetter;
        const BindingFlags LocationFilter = BindingFlags.IgnoreCase | 
                                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | 
                                            BindingFlags.Static | BindingFlags.FlattenHierarchy;


        public SetReset(Action<T> setter, T old, T newValue)
        {
            if (setter == null)
                throw new ArgumentNullException("setter");

            myOld = old;
            mySetter = setter;
            setter(newValue);
        }

        public SetReset(Action<T> setter, Func<T> getter, T newValue)
        {
            if (setter == null)
                throw new ArgumentNullException("setter");
            if (getter == null)
                throw new ArgumentNullException("getter");

            myOld = getter();
            mySetter = setter;
            setter(newValue);
        }

        public SetReset(object instance, string propertyName, T newValue)
        {
            Type instanceType = instance as Type;
            if (instanceType == null)
            {
                instanceType = instance.GetType();
            }

            if (instanceType == null)
            {
                throw new ArgumentNullException("Could not get instance type");
            }

            PropertyInfo propInfo = instanceType.GetProperty(propertyName, LocationFilter);
            if (propInfo == null)
            {
                throw new ArgumentException(
                    String.Format("The property {0} could not be located from type {1}", propertyName, instanceType.FullName));
            }

            myOld = (T)propInfo.GetValue(instance, null);
            mySetter = (newV) => propInfo.SetValue(instance, newV, null);

            mySetter(newValue);
        }

        #region IDisposable Members

        public void Dispose()
        {
            mySetter(myOld);
        }

        #endregion
    }
}
