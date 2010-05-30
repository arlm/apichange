
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.ApiChanges
{

    class RemoteClass : MarshalByRefObject
    {
        public RemoteClass()
        {
            throw new DataMisalignedException("Test exception");
        }
    }

    public class PublicBaseClass : IDisposable
    {
        #region IDisposable Members
        public void DoSomeThing(List<int> l)
        {

        }

        public static void StaticMethod()
        {

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void DoSomeThing(List<float> l)
        {
            StaticMethod();
        }

        public bool CheckForUpdates(Uri deploymentManifestUri, out IDisposable deploymentTask, out IList<IDisposable> chainedDeploymentManifests)
        {
            deploymentTask = null;
            chainedDeploymentManifests = null;
            return false;
        }
 


        #endregion

        public static event Action PublicStaticEvent;
        public event Action PublicEvent;

        public static Func<int> PublicStaticField;
        public int PublicIntField;
        public string PublicStringField;
    }
}