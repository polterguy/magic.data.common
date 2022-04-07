﻿/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using magic.node.extensions;

namespace magic.data.common.helpers
{
    /// <summary>
    /// Helper class to convert values from database to lambda values.
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Converts the given database value to the relevant native .Net type.
        /// for instance, if given DBNull as type, it will return simply "null" value, etc.
        /// </summary>
        /// <param name="value">Database value.</param>
        /// <returns>The value as the equivalent CLR type created from its DB type.</returns>
        public static object GetValue(object value)
        {
            /*
             * Notice, most databases will return DBNull instead of null, hence in order
             * to make sure we return it in "Hyperlambda style", we convert these values
             * into CLR null values.
             */
            if (value is DBNull)
                return null;

            /*
             * Making sure we associate a timezone with our date, defaulting
             * to UTC is not specified from database adapter.
             */
            if (value is DateTime date)
                return date.EnsureTimezone(true);

            // Default, no conversion required.
            return value;
        }
    }
}
