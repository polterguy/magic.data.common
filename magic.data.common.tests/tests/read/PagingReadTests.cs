/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using Xunit;
using magic.node;
using magic.node.extensions;

namespace magic.data.common.tests.tests.read
{
    public class PagingReadTests
    {
        [Fact]
        public void LimitAndOffset()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("limit", 10));
            node.Add(new Node("offset", 5));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' limit 10 offset 5", sql);
        }

        [Fact]
        public void NegativeLimit()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("limit", -1));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo'", sql);
        }

        [Fact]
        public void MultipleLimits_Throws()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("limit", 10));
            node.Add(new Node("limit", 10));
            node.Add(new Node("offset", 5));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void MultipleOffsets_Throws()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("limit", 10));
            node.Add(new Node("offset", 5));
            node.Add(new Node("offset", 5));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }
    }
}
