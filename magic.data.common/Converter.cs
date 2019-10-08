/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System;

namespace magic.data.common
{
    /// <summary>
    /// Helper class to convert values from database to lambda values.
    /// </summary>
    public static class Converter
    {
        public static object GetValue(object value)
        {
            if (value == null || value.GetType() == typeof(DBNull))
                return null;
            return value;
        }
    }
}
