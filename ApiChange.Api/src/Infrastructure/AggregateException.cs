﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Security;
using System.Globalization;

namespace ApiChange.Infrastructure
{
    [Serializable, DebuggerDisplay("Count = {InnerExceptions.Count}")]
    public class AggregateException : Exception
    {
        // Fields
        private ReadOnlyCollection<Exception> m_innerExceptions;

        const string DefaultMessage = "Aggregated Exception";

        // Methods
        public AggregateException()
            : base(DefaultMessage)
        {
            this.m_innerExceptions = new ReadOnlyCollection<Exception>(new Exception[0]);
        }

        public AggregateException(IEnumerable<Exception> innerExceptions)
            : this(DefaultMessage, innerExceptions)
        {
        }

        public AggregateException(params Exception[] innerExceptions)
            : this(DefaultMessage, innerExceptions)
        {
        }

        public AggregateException(string message)
            : base(message)
        {
            this.m_innerExceptions = new ReadOnlyCollection<Exception>(new Exception[0]);
        }

        [SecurityCritical]
        protected AggregateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            Exception[] list = info.GetValue("InnerExceptions", typeof(Exception[])) as Exception[];
            if (list == null)
            {
                throw new SerializationException("Could not deserialize one or all innner exceptions");
            }
            this.m_innerExceptions = new ReadOnlyCollection<Exception>(list);
        }

        public AggregateException(string message, IEnumerable<Exception> innerExceptions)
            : this(message, (innerExceptions == null) ? null : ((IList<Exception>)new List<Exception>(innerExceptions)))
        {
        }

        public AggregateException(string message, Exception innerException)
            : base(message, innerException)
        {
            if (innerException == null)
            {
                throw new ArgumentNullException("innerException");
            }
            this.m_innerExceptions = new ReadOnlyCollection<Exception>(new Exception[] { innerException });
        }

        private AggregateException(string message, IList<Exception> innerExceptions)
            : base(message, ((innerExceptions != null) && (innerExceptions.Count > 0)) ? innerExceptions[0] : null)
        {
            if (innerExceptions == null)
            {
                throw new ArgumentNullException("innerExceptions");
            }
            Exception[] list = new Exception[innerExceptions.Count];
            for (int i = 0; i < list.Length; i++)
            {
                list[i] = innerExceptions[i];
                if (list[i] == null)
                {
                    throw new ArgumentException("One of the inner exceptions was null.");
                }
            }
            this.m_innerExceptions = new ReadOnlyCollection<Exception>(list);
        }

        public AggregateException(string message, params Exception[] innerExceptions)
            : this(message, (IList<Exception>)innerExceptions)
        {
        }

        public AggregateException Flatten()
        {
            List<Exception> innerExceptions = new List<Exception>();
            List<AggregateException> list2 = new List<AggregateException> {
            this
        };
            int num = 0;
            while (list2.Count > num)
            {
                IList<Exception> list3 = list2[num++].InnerExceptions;
                for (int i = 0; i < list3.Count; i++)
                {
                    Exception item = list3[i];
                    if (item != null)
                    {
                        AggregateException exception2 = item as AggregateException;
                        if (exception2 != null)
                        {
                            list2.Add(exception2);
                        }
                        else
                        {
                            innerExceptions.Add(item);
                        }
                    }
                }
            }
            return new AggregateException(this.Message, innerExceptions);
        }

        public override Exception GetBaseException()
        {
            Exception innerException = this;
            for (AggregateException exception2 = this; (exception2 != null) && (exception2.InnerExceptions.Count == 1); exception2 = innerException as AggregateException)
            {
                innerException = innerException.InnerException;
            }
            return innerException;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            Exception[] array = new Exception[this.m_innerExceptions.Count];
            this.m_innerExceptions.CopyTo(array, 0);
            info.AddValue("InnerExceptions", array, typeof(Exception[]));
        }

        public void Handle(Func<Exception, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            List<Exception> innerExceptions = null;
            for (int i = 0; i < this.m_innerExceptions.Count; i++)
            {
                if (!predicate(this.m_innerExceptions[i]))
                {
                    if (innerExceptions == null)
                    {
                        innerExceptions = new List<Exception>();
                    }
                    innerExceptions.Add(this.m_innerExceptions[i]);
                }
            }
            if (innerExceptions != null)
            {
                throw new AggregateException(this.Message, innerExceptions);
            }
        }

        public override string ToString()
        {
            string str = base.ToString();
            for (int i = 0; i < this.m_innerExceptions.Count; i++)
            {
                str = string.Format(CultureInfo.InvariantCulture, "{0}{1}Excepton[{2}]: {3}{4}{5}", new object[] { str, Environment.NewLine, i, this.m_innerExceptions[i].ToString(), "<---", Environment.NewLine });
            }
            return str;
        }

        // Properties
        public ReadOnlyCollection<Exception> InnerExceptions
        {
            get
            {
                return this.m_innerExceptions;
            }
        }
    }

}
