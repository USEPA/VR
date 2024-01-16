using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace L58.EPAVR
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static string ToFormattedText(Enum value)
        {
            string val = value.ToString();
            var builder = new System.Text.StringBuilder();

            for (var i = 0; i < val.Length; i++)
            {
                if (char.IsUpper(val[i]))
                    builder.Append(" ");
                builder.Append(val[i]);
            }
            return builder.ToString();
        }
    }
}

