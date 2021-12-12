/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using Xunit;
using magic.data.common.helpers;

namespace magic.data.common.tests.tests
{
    public class ConverterTests
    {
        [Fact]
        public void ConvertDBNull()
        {
            var val = Converter.GetValue(DBNull.Value);
            Assert.Null(val);
        }

        [Fact]
        public void ConvertNull()
        {
            var val = Converter.GetValue(null);
            Assert.Null(val);
        }

        [Fact]
        public void ConvertNotNull()
        {
            var val = Converter.GetValue("howdy");
            Assert.Equal("howdy", val);
        }
    }
}
