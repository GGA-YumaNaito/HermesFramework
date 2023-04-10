using System;
using System.Collections.Generic;
using System.Linq;

namespace Hermes.API
{
    /// <summary>APIエラーコード.</summary>
    public enum eAPIErrorCode
    {
        /// <summary>None.</summary>
        [eAPIErrorCodeExtensions.Attributes("None")] None = 0,
        /// <summary>なんらかのエラー.</summary>
        [eAPIErrorCodeExtensions.Attributes("SomeError")] SomeError = 9999,
    }

    /// <summary>APIエラーコード拡張.</summary>
    public static class eAPIErrorCodeExtensions
    {
        private static Dictionary<eAPIErrorCode, Attributes> errors;
        [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class Attributes : Attribute
        {
            public readonly string label;
            public Attributes(string label)
            {
                this.label = label;
            }
        }
        static eAPIErrorCodeExtensions()
        {
            var type = typeof(eAPIErrorCode);
            var lookup = type.GetFields()
            .Where(fi => fi.FieldType == type)
            .SelectMany(fi => fi.GetCustomAttributes(false),

             (fi, Attribute) => new { Type = (eAPIErrorCode)fi.GetValue(null), Attribute })
            .ToLookup(a => a.Attribute.GetType());
            errors = lookup[typeof(Attributes)].ToDictionary(a => a.Type, a => (Attributes)a.Attribute);
        }
        public static string Label(this eAPIErrorCode type)
        {
            return errors[type].label;
        }
    }
}