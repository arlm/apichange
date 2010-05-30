
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Introspection
{
    static class GenericTypeMapper
    {
        class GenericType
        {
            public string GenericTypeName;
            public List<GenericType> Arguments = new List<GenericType>();
            public GenericType Parent;

            public GenericType(string typeName, GenericType parent)
            {
                GenericTypeName = typeName;

                int idx = typeName.IndexOf('`');
                if (idx != -1)
                    GenericTypeName = typeName.Substring(0, idx);
                Parent = parent;
            }
        }

        public static string TransformGenericTypeNames(string typeName, Func<string,string> typeNameTransformer)
        {
            if (typeNameTransformer == null)
                throw new ArgumentNullException("typeNameTransformer");

            if (String.IsNullOrEmpty(typeName))
                return typeName;

            string normalizedName = typeName.Replace(" ", "");


            string formattedType = normalizedName;

            GenericType root = ParseGenericType(normalizedName);
            if (root != null)
            {
                TransformGeneric(root, typeNameTransformer);

                StringBuilder sb = new StringBuilder();
                FormatExpandedGeneric(sb, root);
                formattedType = sb.ToString();
            }
            return formattedType;
        }

        static void TransformGeneric(GenericType type, Func<string, string> typeNameTransformer)
        {
            if (type == null)
                return;

            type.GenericTypeName = typeNameTransformer(type.GenericTypeName);
            foreach (var typeArg in type.Arguments)
            {
                TransformGeneric(typeArg, typeNameTransformer);
            }
        }


        public static string ConvertClrTypeNames(string typeName)
        {
            if (String.IsNullOrEmpty(typeName))
                return typeName;

            string normalizedName = typeName.Replace(" ","");

            // No generic type then we need no mapping
            if( typeName.IndexOf('<') == -1 )
            {
                return TypeMapper.ShortToFull(typeName);
            }

            GenericType root = ParseGenericType(normalizedName);

            StringBuilder sb = new StringBuilder();
            FormatExpandedGeneric(sb, root);
            string formattedType = sb.ToString();

            return formattedType;
        }

        private static GenericType ParseGenericType(string normalizedName)
        {
            StringBuilder curArg = new StringBuilder();
            GenericType root = null;
            GenericType curType = null;

            // Func< Func<Func<int,int>,bool> >
            // Func`1< Func`2< Func`2<System.Int32,System.Int32>, System.Boolean> >
            // Func<int,bool,int>
            for (int i = 0; i < normalizedName.Length; i++)
            {
                if (normalizedName[i] == '<')
                {
                    if (curType == null)
                    {
                        curType = new GenericType(curArg.ToString(), null);
                        root = curType;
                    }
                    else
                    {
                        var newGeneric = new GenericType(curArg.ToString(), curType);
                        curType.Arguments.Add(newGeneric);
                        curType = newGeneric;
                    }
                    curArg.Length = 0;
                }
                else if (normalizedName[i] == '>')
                {
                    if (curArg.Length > 0)
                        curType.Arguments.Add(new GenericType(TypeMapper.ShortToFull(curArg.ToString()), null));

                    if (curType.Parent != null)
                        curType = curType.Parent;
                    curArg.Length = 0;
                }
                else if (normalizedName[i] == ',')
                {
                    if (curArg.Length > 0)
                        curType.Arguments.Add(new GenericType(TypeMapper.ShortToFull(curArg.ToString()), null));
                    curArg.Length = 0;
                }
                else
                {
                    curArg.Append(normalizedName[i]);
                }
            }
            return root;
        }

        static void FormatExpandedGeneric(StringBuilder sb, GenericType type)
        {
            sb.Append(type.GenericTypeName);
            if (type.Arguments.Count > 0)
            {
                sb.AppendFormat("`{0}", type.Arguments.Count);
                sb.Append("<");
                for (int i = 0; i < type.Arguments.Count; i++)
                {
                    var curGen = type.Arguments[i];
                    if (curGen.Arguments.Count > 0)
                    {
                        FormatExpandedGeneric(sb, curGen);
                    }
                    else
                    {
                        sb.Append(curGen.GenericTypeName);
                    }
                    if (i != type.Arguments.Count - 1)
                        sb.Append(',');
                }
                sb.Append(">");
            }

        }
      
    }
}
