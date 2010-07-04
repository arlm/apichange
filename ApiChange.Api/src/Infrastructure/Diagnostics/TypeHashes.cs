
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Infrastructure
{
    /// <summary>
    /// Create a static instance of each class where you want to use tracing. 
    /// It does basically encapsulate the typename and enables fast trace filters.
    /// </summary>
    public class TypeHashes
    {
        string myFullTypeName;
        internal int[] myTypeHashes;

        internal string FullQualifiedTypeName
        {
            get { return myFullTypeName; }
        }

        static char[] mySep = new char[] { '.' };


        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHandle"/> class.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        public TypeHashes(string typeName)
        {
            if (String.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("typeName");
            }

            myFullTypeName = typeName;

            // Generate from the full qualified type name substring for each part between the . characters.
            // Each substring is then hashed so we can later compare not strings but integer arrays which are super
            // fast! Since this is done only once for a type we can afford doing a little more work here and spare
            // huge amount of comparison time later. 
            // If by a rare incident the hash values would collide with another named type we would have enabled
            // tracing by accident for one more type than intended. 
            List<int> hashes = new List<int>();
            foreach (string substr in myFullTypeName.ToLower().Split(mySep))
            {
                hashes.Add(substr.GetHashCode());
            }
            myTypeHashes = hashes.ToArray();
        }

        /// <summary>
        /// Create a TypeHandle which is used by the Tracer class.
        /// </summary>
        /// <param name="t">Type of your enclosing class.</param>
        public TypeHashes(Type t) : this(CheckInput(t))
        {
        }

        static string CheckInput(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException("Type");
            }

            return String.Join(".", new string[] { t.Namespace, t.Name }); ;
        }
    }
}
