/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using Xunit;
using magic.node;
using magic.node.extensions;

namespace magic.data.common.tests
{
    public class LoggingTests
    {
        [Fact]
        public void Create()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            node.Add(values);
            var builder = new SqlCreateBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            var arg1 = result.Children.First();
            Assert.Equal("insert into 'foo' ('field1') values (@0)", sql);
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("howdy", arg1.Get<string>());
        }

        [Fact]
        public void CreateThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlCreateBuilder(null, "'"));
            Assert.Throws<ArgumentNullException>(() => new SqlCreateBuilder(new Node(), null));
        }

        [Fact]
        public void Read()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' limit 25", sql);
        }

        [Fact]
        public void Update()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            node.Add(values);
            var where = new Node("where");
            var and = new Node("and");
            and.Add(new Node("field2", "value2"));
            where.Add(and);
            node.Add(where);
            var builder = new SqlUpdateBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("update 'foo' set 'field1' = @v0 where ('field2' = @0)", sql);
            var arg1 = result.Children.First();
            Assert.Equal("@v0", arg1.Name);
            Assert.Equal("howdy", arg1.Get<string>());
            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@0", arg2.Name);
            Assert.Equal("value2", arg2.Get<string>());
        }

        [Fact]
        public void Delete()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and = new Node("and");
            and.Add(new Node("field1", "value1"));
            where.Add(and);
            node.Add(where);
            var builder = new SqlDeleteBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("delete from 'foo' where ('field1' = @0)", sql);
            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("value1", arg1.Get<string>());
        }
    }
}
