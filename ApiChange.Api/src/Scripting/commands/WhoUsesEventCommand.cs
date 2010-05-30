
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Api.Introspection;
using Mono.Cecil;
using System.IO;

namespace ApiChange.Api.Scripting
{
    class WhoUsesEventCommand : QueryCommandBase
    {
        public const string Argument = "whousesevent";
        EventQuery myEventQuery;

        SheetInfo mySearchHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "Declaring Type", Width = TypeWidth },
                new ColumnInfo { Name = "Event Declaration", Width = FieldWidth},
                new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth},
            },

            SheetName = "Event Query"
        };

        SheetInfo myResultHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                 new ColumnInfo { Name = "Type", Width = TypeWidth },
                 new ColumnInfo { Name = "Method", Width = MethodWidth },
                 new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                 new ColumnInfo { Name = "Operation", Width = ReasonWidth},
                 new ColumnInfo { Name = "Event", Width = MatchItemWidth},
                 new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth},
                 new ColumnInfo { Name = "Line", Width = SourceLineWidth},
            },

            SheetName = "Results"
        };

        SheetInfo myImbalancedEventsHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                 new ColumnInfo { Name = "Missing", Width = SourceLineWidth },
                 new ColumnInfo { Name = "AddRemoveCount", Width = SourceLineWidth },
                 new ColumnInfo { Name = "Event", Width = MatchItemWidth},
                 new ColumnInfo { Name = "Type", Width = TypeWidth },
                 new ColumnInfo { Name = "Method", Width = MethodWidth },
                 new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                 new ColumnInfo { Name = "Operation", Width = ReasonWidth },
                 new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth},
                 new ColumnInfo { Name = "Line", Width = SourceLineWidth},
            },

            SheetName = "Results"
        };

        public WhoUsesEventCommand(CommandData parsedArgs)
            : base(parsedArgs)
        {
            AddAdditionalColumnsWhenEnabled(mySearchHeader);
            AddAdditionalColumnsWhenEnabled(myResultHeader);
            AddAdditionalColumnsWhenEnabled(myImbalancedEventsHeader);
        }

        protected override void Validate()
        {
            base.Validate();

            ValidateFileQuery(myParsedArgs.Queries1,
                  "Command -whousesevent <files> is missing. The fields defined in the query are searched in the assembly given here.",
                   "Invalid directory in  -whousesfield {0} query.",
                   "The -whousesfield query {0} did not match any files.");

            ValidateFileQuery(myParsedArgs.Queries2,
                    "-in <files> is missing. These assemblies are searched if they access any fields.",
                    "Invalid directory in -in {0} query.",
                    "The -in query {0} did not match any files.");

            if (myParsedArgs.TypeAndInnerQuery == null)
            {
                AddErrorMessage("The event query was empty. Please enter a query of the form typeName([<visiblity>] <delegatetype> <event name>)." + Environment.NewLine +
                                "Example: *(public * *) searches for users all public events in the -in query.");
                SetInvalid();
            }
        }

        public override bool ExtractAndValidateTypeQuery(string query)
        {
            bool lret = base.ExtractAndValidateTypeQuery(query);

            if (lret == false)
            {
                Out.WriteLine("The Type/event query must be of the form typeName([<visiblity>] <delegatetype> <event name>)");
                Out.WriteLine("Example: *(public * *) searches for users all public events in the -in query.");
            }
            else
            {
                if (base.myInnerQuery.Trim() == "*")
                {
                    myEventQuery = EventQuery.AllEvents;
                }
                else
                {
                    myEventQuery = new EventQuery(base.myInnerQuery);
                }
            }

            return lret;
        }

        public override void Execute()
        {
            base.Execute();
            if (!IsValid)
            {
                Help();
                return;
            }

           if (ExtractAndValidateTypeQuery(myParsedArgs.TypeAndInnerQuery) == false)
                return;

            List<EventDefinition> eventsToSearch = new List<EventDefinition>();
            
            Writer.SetCurrentSheet(mySearchHeader);

            LoadAssemblies(myParsedArgs.Queries1, (cecilAssembly, file) =>
            {
                using (PdbInformationReader pdbreader = new PdbInformationReader(myParsedArgs.SymbolServer))
                {
                    foreach(var type in myTypeQueries.GetMatchingTypes(cecilAssembly))
                    {
                        myEventQuery.GetMatchingEvents(type).ForEach(
                        (ev) =>
                        {
                            var fileLine = pdbreader.GetFileLine((TypeDefinition)ev.DeclaringType);

                            Writer.PrintRow("{0,-70}; {1,-40}; {2}; {3}",
                                () => GetFileInfoWhenEnabled(fileLine.Key),
                                type.Print(),
                                ev.Print(),
                                Path.GetFileName(file),
                                fileLine.Key
                                );

                            lock (eventsToSearch)
                            {
                                eventsToSearch.Add(ev);
                            }
                        });
                    }
                }
            });

            if (eventsToSearch.Count == 0)
            {
                Out.WriteLine("Error: No events to query found. Aborting query.");
                return;
            }

            Writer.PrintRow("",null);
            Writer.PrintRow("",null);


            if (myParsedArgs.EventSubscriptionImbalance == true)
            {
                SearchForImablancedEventSubscriptions(eventsToSearch);
            }
            else
            {
                Writer.SetCurrentSheet(myResultHeader);
                LoadAssemblies(myParsedArgs.Queries2, (cecilAssembly, file) =>
                {
                    using (UsageQueryAggregator aggregator = new UsageQueryAggregator(myParsedArgs.SymbolServer))
                    {
                        new WhoUsesEvents(aggregator, eventsToSearch);
                        aggregator.Analyze(cecilAssembly);

                        aggregator.MethodMatches.ForEach((result) =>
                        {
                            Writer.PrintRow("{0,-80}; {1,-40}; {2}; {3}; {4}; {5}; {6}",
                                () => GetFileInfoWhenEnabled(result.SourceFileName),
                                result.Match.DeclaringType.FullName,
                                result.Match.Print(MethodPrintOption.Full),
                                Path.GetFileName(file),
                                result.Annotations.Reason,
                                result.Annotations.Item,
                                result.SourceFileName,
                                result.LineNumber
                                );
                        });
                    }
                });
            }
        }

        private void SearchForImablancedEventSubscriptions(List<EventDefinition> eventsToSearch)
        {
            Writer.SetCurrentSheet(myImbalancedEventsHeader);

            LoadAssemblies(myParsedArgs.Queries2, (cecilAssembly, file) =>
            {
                using (UsageQueryAggregator aggregator = new UsageQueryAggregator(myParsedArgs.SymbolServer))
                {
                    new WhoUsesEvents(aggregator, eventsToSearch);
                    aggregator.Analyze(cecilAssembly);

                    List<EventUsage> usages = new List<EventUsage>();

                    aggregator.MethodMatches.ForEach((result) =>
                    {
                        usages.Add(new EventUsage(result));
                    });

                    var eventsgroupedbyType = from usage in usages
                                              group usage by usage.UsingType into perType
                                              select perType;

                    foreach (var usingTypes in eventsgroupedbyType)
                    {
                        //Out.WriteLine("Events used by type {0}", usingTypes.Key);

                        Dictionary<string,int> usedEvents = new Dictionary<string,int>();
                        foreach(EventUsage usedEvent in usingTypes)
                        {
                            usedEvents[usedEvent.EventDefiningEventAssembly + usedEvent.EventName] = 0;
                        }

                        foreach (var type in usingTypes)
                        {
                            usedEvents[type.FullQualifiedEventName] += type.AddRemoveCount;
                           
                        }

                        foreach (var unbalances in from ev in usingTypes
                                                   where usedEvents[ev.FullQualifiedEventName] > 0
                                                   select ev)
                        {
                            //Out.WriteLine("Type {0} uses event {1} as {2}", unbalances.UsingType, unbalances.EventName, usedEvents[unbalances.EventDefiningEventAssembly + unbalances.EventName]);
                            PrintUnmatchedEvent(unbalances, usedEvents[unbalances.FullQualifiedEventName]);
                        }
                    }
                }
            });

        }

        private void PrintUnmatchedEvent(EventUsage eventsubscription, int missingCount)
        {
            Writer.PrintRow("{0,-2}; {1,-2}; {2,-80}; {3,-40}; {4}; {5}; {6}; {7}; {8}",
                    () => GetFileInfoWhenEnabled(eventsubscription.UsingSourceFileName),
                    missingCount,
                    eventsubscription.AddRemoveCount,
                    eventsubscription.EventName,
                    eventsubscription.UsingType,
                    eventsubscription.UsingMethod,
                    Path.GetFileName(eventsubscription.UsingAssembly),
                    eventsubscription.AddRemoveCount == 1 ? "Add" : "Remove",
                    eventsubscription.UsingSourceFileName,
                    eventsubscription.UsingLineNumber);
        }


        class EventUsage
        {
            public string EventName;
            public string EventDefiningEventAssembly;

            string myFullEventName;
            public string FullQualifiedEventName
            {
                get
                {
                    return myFullEventName ?? (myFullEventName = EventDefiningEventAssembly + EventName);
                }
            }

            public string UsingType;
            public string UsingMethod;
            public string UsingAssembly;
            public int    AddRemoveCount;
            public string UsingSourceFileName;
            public int    UsingLineNumber;

            public EventUsage(QueryResult<MethodDefinition> result)
            {
                EventName = result.Annotations.Item;
                EventDefiningEventAssembly = result.Annotations[WhoUsesEvents.DefiningAssemblyKey].ToString();
                UsingType = result.Match.DeclaringType.FullName;
                UsingMethod = result.Match.Print(MethodPrintOption.Full);
                UsingAssembly = result.Match.DeclaringType.Module.Image.FileInformation.Name;
                AddRemoveCount = result.Annotations.Reason == WhoUsesEvents.AddEventReason ? 1 : -1;
                UsingSourceFileName = result.SourceFileName;
                UsingLineNumber = result.LineNumber;
            }
        }

    }
}
