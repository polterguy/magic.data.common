/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using Xunit;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

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
        public void CreateMultipleValues()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            values.Add(new Node("field2", "world"));
            node.Add(values);
            var builder = new SqlCreateBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("insert into 'foo' ('field1', 'field2') values (@0, @1)", sql);
            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("howdy", arg1.Get<string>());
            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@1", arg2.Name);
            Assert.Equal("world", arg2.Get<string>());
        }

        [Fact]
        public void CreateNullValue()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            values.Add(new Node("field2"));
            node.Add(values);
            var builder = new SqlCreateBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("insert into 'foo' ('field1', 'field2') values (@0, null)", sql);
            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("howdy", arg1.Get<string>());
        }

        [Fact]
        public void CreateMultipleValuesThrows_01()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            node.Add(values);
            node.Add(new Node("values"));
            var builder = new SqlCreateBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void CreateMultipleValuesThrows_02()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            node.Add(values);
            var builder = new SqlCreateBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void CreateNoTable()
        {
            // Creating node hierarchy.
            var node = new Node();
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            node.Add(values);
            Assert.Throws<ArgumentException>(() => new SqlCreateBuilder(node, "'").Build());
        }

        [Fact]
        public void CreateGenerateOnly()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("generate", true));
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            node.Add(values);
            var builder = new SqlCreateBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.True(builder.IsGenerateOnly);
            var result = builder.Build();
            var sql = result.Get<string>();
            var arg1 = result.Children.First();
            Assert.Equal("insert into 'foo' ('field1') values (@0)", sql);
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("howdy", arg1.Get<string>());
        }

        class SqlMockCreateBuilder : SqlCreateBuilder
        {
            public SqlMockCreateBuilder(Node node, ISignaler signaler)
                : base(node, "'")
            { }
        }

        [Fact]
        public void CreateUsingParseGenerate()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("generate", true));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            node.Add(values);
            var result = SqlBuilder.Parse<SqlMockCreateBuilder>(null, node);
            Assert.Null(result);

            // Extracting SQL + params, and asserting correctness.
            var sql = node.Get<string>();
            var arg1 = node.Children.First();
            Assert.Equal("insert into 'foo' ('field1') values (@0)", sql);
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("howdy", arg1.Get<string>());
        }

        [Fact]
        public void CreateUsingParse()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            node.Add(values);
            var result = SqlBuilder.Parse<SqlMockCreateBuilder>(null, node);
            Assert.NotNull(result);

            // Extracting SQL + params, and asserting correctness.
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
        public void ReadAggregate()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var columns = new Node("columns");
            columns.Add(new Node("count(*)"));
            node.Add(columns);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select count(*) from 'foo' limit 25", sql);
        }

        [Fact]
        public void ReadWhereMultipleLevels()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            and1.Add(new Node("foo1", 5));
            and1.Add(new Node("foo2", "howdy"));
            var or1 = new Node("or");
            or1.Add(new Node("foo3", "jalla"));
            or1.Add(new Node("foo4", "balla"));
            and1.Add(or1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('foo1' = @0 and 'foo2' = @1 and ('foo3' = @2 or 'foo4' = @3)) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);

            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@1", arg2.Name);
            Assert.Equal("howdy", arg2.Value);

            var arg3 = result.Children.Skip(2).First();
            Assert.Equal("@2", arg3.Name);
            Assert.Equal("jalla", arg3.Value);

            var arg4 = result.Children.Skip(3).First();
            Assert.Equal("@3", arg4.Name);
            Assert.Equal("balla", arg4.Value);
        }

        [Fact]
        public void ReadIn()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var in1 = new Node("in");
            var inValues = new Node("field1");
            inValues.Add(new Node("", 5));
            inValues.Add(new Node("", 7));
            inValues.Add(new Node("", 9));
            in1.Add(inValues);
            where.Add(in1);
            node.Add(where);
            System.Console.WriteLine(node.ToHyperlambda());
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            System.Console.WriteLine(sql);
            Assert.Equal("select * from 'foo' where 'field1' in (@0,@1,@2) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5L, arg1.Value);

            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@1", arg2.Name);
            Assert.Equal(7L, arg2.Value);

            var arg3 = result.Children.Skip(2).First();
            Assert.Equal("@2", arg3.Name);
            Assert.Equal(9L, arg3.Value);
        }

        [Fact]
        public void ReadNamespaced()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo.bar"));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo'.'bar' limit 25", sql);
        }

        [Fact]
        public void ReadWithColumns()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "table"));
            var columns = new Node("columns");
            columns.Add(new Node("foo"));
            columns.Add(new Node("bar"));
            node.Add(columns);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select 'foo','bar' from 'table' limit 25", sql);
        }

        [Fact]
        public void ReadWithLimitOffset()
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
        public void ReadWithLimitOffsetThrows_01()
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
        public void ReadWithLimitOffsetThrows_02()
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

        [Fact]
        public void ReadWithOrder()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("order", "fieldOrder"));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' order by 'fieldOrder' limit 25", sql);
        }

        [Fact]
        public void ReadWithOrderThrows_01()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("order", "fieldOrder"));
            node.Add(new Node("order", "fieldOrder"));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithOrderThrows_02()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("order", "fieldOrder"));
            node.Add(new Node("direction", "desc"));
            node.Add(new Node("direction", "desc"));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithOrderThrows_03()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("order", "fieldOrder"));
            node.Add(new Node("direction", "throws"));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithOrderDescending()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("order", "fieldOrder"));
            node.Add(new Node("direction", "desc"));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' order by 'fieldOrder' desc limit 25", sql);
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
        public void UpdateMultipleValues()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            values.Add(new Node("field2", "world"));
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
            Assert.Equal("update 'foo' set 'field1' = @v0, 'field2' = @v1 where ('field2' = @0)", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@v0", arg1.Name);
            Assert.Equal("howdy", arg1.Get<string>());

            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@v1", arg2.Name);
            Assert.Equal("world", arg2.Get<string>());

            var arg3 = result.Children.Skip(2).First();
            Assert.Equal("@0", arg3.Name);
            Assert.Equal("value2", arg3.Get<string>());
        }

        [Fact]
        public void UpdateNullValue()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            values.Add(new Node("field1", "howdy"));
            values.Add(new Node("field2"));
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
            Assert.Equal("update 'foo' set 'field1' = @v0, 'field2' = null where ('field2' = @0)", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@v0", arg1.Name);
            Assert.Equal("howdy", arg1.Get<string>());

            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@0", arg2.Name);
            Assert.Equal("value2", arg2.Get<string>());
        }

        [Fact]
        public void UpdateNoValues_01()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var values = new Node("values");
            node.Add(values);
            var where = new Node("where");
            var and = new Node("and");
            and.Add(new Node("field2", "value2"));
            where.Add(and);
            node.Add(where);
            var builder = new SqlUpdateBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void UpdateNoValues_02()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and = new Node("and");
            and.Add(new Node("field2", "value2"));
            where.Add(and);
            node.Add(where);
            var builder = new SqlUpdateBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
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

        [Fact]
        public void DeleteThrowsMultipleWhere()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and = new Node("and");
            and.Add(new Node("field1", "value1"));
            where.Add(and);
            node.Add(where);
            node.Add(new Node("where"));
            var builder = new SqlDeleteBuilder(node, "'");
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void DeleteThrowsWrongBoolean()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and = new Node("xor");
            and.Add(new Node("field1", "value1"));
            where.Add(and);
            node.Add(where);
            var builder = new SqlDeleteBuilder(node, "'");
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void DeleteWithOr()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var or = new Node("or");
            or.Add(new Node("field1", "value1"));
            or.Add(new Node("field2", "value2"));
            where.Add(or);
            node.Add(where);
            var builder = new SqlDeleteBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("delete from 'foo' where ('field1' = @0 or 'field2' = @1)", sql);
            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("value1", arg1.Get<string>());
            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@1", arg2.Name);
            Assert.Equal("value2", arg2.Get<string>());
        }
    }
}
