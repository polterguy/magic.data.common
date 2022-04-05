/*
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
            if (value == null || value is DBNull)
                return null;

            /*
             * Trying to make sure we as intelligently as possible can handle all different types
             * that can be returned from any database.
             */
            switch (value.GetType().FullName)
            {
                case "System.String":
                case "System.Byte":
                case "System.SByte":
                case "System.Boolean":
                case "System.Decimal":
                case "System.Float":
                case "System.Double":
                case "System.Single":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int64":
                case "System.UInt64":
                case "System.Int16":
                case "System.UInt16":
                case "System.Enum":
                case "System.Byte[]":
                case "System.Guid":

                    // No conversion required.
                    break;

                case "System.DateTime":

                    // Making sure we always return UTC.
                    value = ((DateTime)value).EnsureUtc();
                    break;

                default:

                    // Returning object as string.
                    value = value.ToString();
                    break;
            }

            // Default, no conversion required.
            return value;
        }
    }
}
