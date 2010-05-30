
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ApiChange.Api.Introspection
{
    public class UsageVisitor
    {
        protected UsageQueryAggregator Aggregator
        {
            get;
            private set;
        }

        public UsageVisitor(UsageQueryAggregator aggregator)
        {
            if (aggregator == null)
                throw new ArgumentNullException("aggregator");

            Aggregator = aggregator;

            // subscribe ourself to the aggregator when the query is constructed
            Aggregator.AddQuery(this); 
        }

        public virtual void VisitType(TypeDefinition type)
        {
        }

        public virtual void VisitField(FieldDefinition field)
        {
        }

        public virtual void VisitMethod(MethodDefinition method)
        {
        }

        public virtual void VisitLocals(VariableDefinitionCollection locals, MethodDefinition declaringMethod)
        {
        }

        public virtual void VisitAssemblyReference(AssemblyNameReference assemblyRef, AssemblyDefinition current)
        {
        }

        public virtual void VisitMethodBody(MethodBody body)
        {
        }

        protected static T ThrowIfNull<T>(string name, T arg)
        {
            if (arg == null)
                throw new ArgumentNullException(name);

            return arg;
        }

        protected bool IsMatching(HashSet<string> typeNameHash, List<TypeDefinition> searchTypes, TypeReference currentType, out TypeDefinition foundType)
        {

            // check if type itself does match
            if (typeNameHash.Contains(currentType.Name))
            {
                foreach (TypeDefinition searchType in searchTypes)
                {
                    if (currentType.IsEqual(searchType, false))
                    {
                        foundType = searchType;
                        return true;
                    }
                }
            }

            // check if type is a type with generic type parameters
            GenericInstanceType genArg = currentType as GenericInstanceType;
            if (genArg != null)
            {
                foreach (TypeReference generic in genArg.GenericArguments)
                {
                    if (IsMatching(typeNameHash, searchTypes, generic, out foundType))
                        return true;
                }
            }

            foundType = null;
            return false;
        }
    }
}
