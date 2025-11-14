/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Helper
 * FILE:        ReflectionHelper.cs
 * PURPOSE:     Helper to check reflection-based XML documentation.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace CoreBuilder.Helper
{
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets all public types from an assembly path.
        /// </summary>
        public static Type[] GetPublicTypes(string assemblyPath)
        {
            var asm = Assembly.LoadFrom(assemblyPath);
            return asm.GetExportedTypes();
        }

        /// <summary>
        /// Checks if a member has XML documentation.
        /// Requires that XML docs are generated and available alongside the assembly.
        /// </summary>
        public static bool HasXmlDoc(MemberInfo member)
        {
            // naive check: look for XML doc file next to assembly
            var xmlPath = member.Module.Assembly.Location + ".xml";
            if (!System.IO.File.Exists(xmlPath)) return false;

            var xml = XDocument.Load(xmlPath);
            string memberName = GetXmlMemberName(member);

            var node = xml.Descendants("member")
                          .FirstOrDefault(e => (string)e.Attribute("name") == memberName);
            return node != null;
        }

        /// <summary>
        /// Gets the name of the XML member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>Member Name.</returns>
        private static string GetXmlMemberName(MemberInfo member)
        {
            return member.MemberType switch
            {
                MemberTypes.TypeInfo => "T:" + (member is Type t ? t.FullName : member.Name),
                MemberTypes.Method => "M:" + member.DeclaringType!.FullName + "." + member.Name,
                MemberTypes.Property => "P:" + member.DeclaringType!.FullName + "." + member.Name,
                MemberTypes.Field => "F:" + member.DeclaringType!.FullName + "." + member.Name,
                _ => member.Name
            };
        }
    }

}
