
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ApiChange.Infrastructure
{
    internal class TraceFilter
    {
        internal TraceFilter Next = null;
        int[] myFilterHashes = null;
        const int MATCHANY = -1;  // Hash value that marks a *
        MessageTypes myMsgTypeFilter = MessageTypes.None;
        Level myLevelFilter;

        string myFilter;

        protected TraceFilter()
        {
        }

        public TraceFilter(string typeFilter, MessageTypes msgTypeFilter, Level levelFilter, TraceFilter next)
        {
            if (String.IsNullOrEmpty(typeFilter))
            {
                throw new ArgumentException("typeFilter was null or empty");
            }

            myFilter = typeFilter;
            Next = next;
            myMsgTypeFilter = msgTypeFilter;
            myLevelFilter = levelFilter;

            string[] parts = typeFilter.Trim().ToLower().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            myFilterHashes = new int[parts.Length];
            Debug.Assert(parts.Length > 0, "Type filter parts should be > 0");
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "*")
                    myFilterHashes[i] = MATCHANY;
                else
                {
                    myFilterHashes[i] = parts[i].GetHashCode();
                }
            }
        }
        
        public virtual bool IsMatch(TypeHashes type, MessageTypes msgTypeFilter, Level level)
        {
            bool lret = ((level & myLevelFilter) != Level.None);

            if (lret)
            {
                bool areSameSize = (myFilterHashes.Length == type.myTypeHashes.Length);

                for (int i = 0; i < myFilterHashes.Length; i++)
                {
                    if (myFilterHashes[i] == MATCHANY)
                    {
                        break;
                    }

                    if (i < type.myTypeHashes.Length)
                    {
                        // The current filter does not match exit
                        // otherwise we compare the next round.
                        if (myFilterHashes[i] != type.myTypeHashes[i])
                        {
                            lret = false;
                            break;
                        }

                        // We are still here when the last arry item matches
                        // This is a full match
                        if (i == myFilterHashes.Length - 1 && areSameSize)
                        {
                            break;
                        }
                    }
                    else // the filter string is longer than the domain. That can never match
                    {
                        lret = false;
                        break;
                    }
                }
            }

            if (lret)
            {
                lret = (msgTypeFilter & myMsgTypeFilter) != MessageTypes.None;
            }

            // If no match try next filter
            if (Next != null && lret == false)
            {
                lret = Next.IsMatch(type, msgTypeFilter, level);
            }

            return lret;
        }
    }
}
