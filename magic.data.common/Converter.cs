/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;

namespace magic.data.common
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
            if (value == null || value is DBNull)
                return null;

            /*
             * Notice, internally we always treat everything as UTC,
             * and we assume everything we get from database is always UTC.
             */
            if (value is DateTime dt)
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);

            // Default, no conversion required.
            return value;
        }
    }
}
