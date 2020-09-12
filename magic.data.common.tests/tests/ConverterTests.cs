/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using Xunit;

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
