/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        VmValue.cs
 * PURPOSE:     Struct that contains all possible VM value types for VariableRegistry.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Globalization;
using Weaver.Messages;

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Smallest unit that contains all possible VM value types for VariableRegistry.
    /// </summary>
    public readonly struct VmValue
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        public EnumTypes Type { get; }

        /// <summary>
        /// Gets the int64.
        /// </summary>
        public long Int64 { get; }

        /// <summary>
        /// Gets the double.
        /// </summary>
        public double Double { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="VmValue"/> is bool.
        /// </summary>
        public bool Bool { get; }

        /// <summary>
        /// Gets the string.
        /// </summary>
        public string? String { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VmValue"/> struct.
        /// </summary>
        private VmValue(EnumTypes type, long i, double d, bool b, string? s)
        {
            Type = type;
            Int64 = i;
            Double = d;
            Bool = b;
            String = s;
        }

        /// <summary>
        /// Froms the int.
        /// </summary>
        public static VmValue FromInt(long value) =>
            new(EnumTypes.Wint, value, default, default, null);

        /// <summary>
        /// Froms the double.
        /// </summary>
        public static VmValue FromDouble(double value) =>
            new(EnumTypes.Wdouble, default, value, default, null);

        /// <summary>
        /// Froms the bool.
        /// </summary>
        public static VmValue FromBool(bool value) =>
            new(EnumTypes.Wbool, default, default, value, null);

        /// <summary>
        /// From string.
        /// </summary>
        public static VmValue FromString(string? value) =>
            new(EnumTypes.Wstring, default, default, default, value);

        /// <summary>
        /// Converts to string.
        /// </summary>
        public override string ToString()
        {
            return Type switch
            {
                EnumTypes.Wint => Int64.ToString(),
                EnumTypes.Wdouble => Double.ToString(CultureInfo.InvariantCulture),
                EnumTypes.Wbool => Bool.ToString(),
                EnumTypes.Wstring => String ?? "<null>",
                _ => "<unknown>"
            };
        }

        /// <summary>
        /// Deconstructs the VMValue into a raw object value and the EnumType.
        /// </summary>
        /// <param name="value">Outputs the boxed value based on Type.</param>
        /// <param name="type">Outputs the EnumTypes discriminator.</param>
        public void Deconstruct(out object? value, out EnumTypes type)
        {
            type = Type;

            value = Type switch
            {
                EnumTypes.Wint => Int64,
                EnumTypes.Wdouble => Double,
                EnumTypes.Wbool => Bool,
                EnumTypes.Wstring => String,
                _ => null
            };
        }
    }
}