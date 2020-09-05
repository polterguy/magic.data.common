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
using magic.data.common.helpers;

namespace magic.data.common.tests
{
    public class DataTests
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
        public void ReadNoTable()
        {
            // Creating node hierarchy.
            var node = new Node();
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithJoin()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            join1.Add(new Node("type", "inner"));
            var on1 = new Node("on");
            on1.Add(new Node("fk1", "pk1"));
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'fk1' = 'table2'.'pk1' limit 25", sql);
        }

        [Fact]
        public void ReadWithBogusJoinTypeThrows()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            join1.Add(new Node("type", "innerXX"));
            var on1 = new Node("on");
            on1.Add(new Node("fk1", "pk1"));
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithRecursiveJoin()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            join1.Add(new Node("type", "inner"));
            var on1 = new Node("on");
            on1.Add(new Node("fk1", "pk1"));
            join1.Add(on1);
            var join2 = new Node("join", "table3");
            var on2 = new Node("on");
            on2.Add(new Node("fk2", "pk2"));
            join2.Add(on2);
            join1.Add(join2);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'fk1' = 'table2'.'pk1' inner join 'table3' on 'table2'.'fk2' = 'table3'.'pk2' limit 25", sql);
        }

        [Fact]
        public void ReadWithJoinOperator()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            join1.Add(new Node("type", "inner"));
            var on1 = new Node("on");
            var on1Crit = new Node("fk1", "pk1");
            on1Crit.Add(new Node("operator", "!="));
            on1.Add(on1Crit);
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'fk1' != 'table2'.'pk1' limit 25", sql);
        }

        [Fact]
        public void ReadWithBogusJoinOperatorThrows_01()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            join1.Add(new Node("type", "inner"));
            var on1 = new Node("on");
            var on1Crit = new Node("fk1", "pk1");
            on1Crit.Add(new Node("operator", "!===="));
            on1.Add(on1Crit);
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithBogusJoinOperatorThrows_02()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            join1.Add(new Node("type", "inner"));
            var on1 = new Node("on");
            var on1Crit = new Node("fk1", "pk1");
            on1Crit.Add(new Node("operatorXX", "!="));
            on1.Add(on1Crit);
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithBogusJoinOperatorThrows_03()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            join1.Add(new Node("type", "inner"));
            var on1 = new Node("on");
            var on1Crit = new Node("fk1", "pk1");
            on1Crit.Add(new Node("operator", "!="));
            on1Crit.Add(new Node("operator", "!="));
            on1.Add(on1Crit);
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithJoinMultipleCriteria()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            join1.Add(new Node("type", "inner"));
            var on1 = new Node("on");
            on1.Add(new Node("fk1", "pk1"));
            on1.Add(new Node("fk2", "pk2"));
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'fk1' = 'table2'.'pk1', 'table1'.'fk2' = 'table2'.'pk2' limit 25", sql);
        }

        [Fact]
        public void ReadWithJoinNamespaced()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "dbo.table1");
            var join1 = new Node("join", "dbo.table2");
            join1.Add(new Node("type", "inner"));
            var on1 = new Node("on");
            on1.Add(new Node("fk1", "pk1"));
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'dbo'.'table1' inner join 'dbo'.'table2' on 'dbo'.'table1'.'fk1' = 'dbo'.'table2'.'pk1' limit 25", sql);
        }

        [Fact]
        public void ReadWithJoinNoType()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            var on1 = new Node("on");
            on1.Add(new Node("fk1", "pk1"));
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'fk1' = 'table2'.'pk1' limit 25", sql);
        }

        [Fact]
        public void ReadWithJoinThrows_01()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("joinXX", "table2");
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithJoinThrows_02()
        {
            // Creating node hierarchy.
            var node = new Node();
            var table1 = new Node("table", "table1");
            var join1 = new Node("join", "table2");
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadNegativeLimit()
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
        public void ReadMultipleColumnsThrows()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var columns = new Node("columns");
            columns.Add(new Node("count(*)"));
            node.Add(columns);
            var columns2 = new Node("columns");
            columns2.Add(new Node("count(*)"));
            node.Add(columns2);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWhereMultipleLevels_01()
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
        public void ReadWhereMultipleLevels_02()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("or");
            and1.Add(new Node("foo1", 5));
            and1.Add(new Node("foo2", "howdy"));
            var or1 = new Node("and");
            or1.Add(new Node("foo3", "jalla"));
            or1.Add(new Node("foo4", "balla"));
            and1.Add(or1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('foo1' = @0 or 'foo2' = @1 or ('foo3' = @2 and 'foo4' = @3)) limit 25", sql);

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
        public void ReadWhereMultipleLevels_03()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var or1 = new Node("or");
            or1.Add(new Node("foo1", 5));
            or1.Add(new Node("foo2", "howdy"));
            var in1 = new Node("in");
            var inColumns = new Node("field1");
            inColumns.Add(new Node("", 5));
            inColumns.Add(new Node("", 7));
            in1.Add(inColumns);
            or1.Add(in1);
            where.Add(or1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('foo1' = @0 or 'foo2' = @1 or 'field1' in (@2,@3)) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);

            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@1", arg2.Name);
            Assert.Equal("howdy", arg2.Value);

            var arg3 = result.Children.Skip(2).First();
            Assert.Equal("@2", arg3.Name);
            Assert.Equal(5, arg3.Value);

            var arg4 = result.Children.Skip(3).First();
            Assert.Equal("@3", arg4.Name);
            Assert.Equal(7, arg4.Value);
        }

        [Fact]
        public void ReadIn()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var in1 = new Node("in");
            var inValues = new Node("field1");
            inValues.Add(new Node("", 5L));
            inValues.Add(new Node("", 7L));
            inValues.Add(new Node("", 9L));
            in1.Add(inValues);
            and1.Add(in1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1' in (@0,@1,@2)) limit 25", sql);

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
        public void ReadEmptyWhere()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' limit 25", sql);
        }

        [Fact]
        public void ReadInStrings()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var in1 = new Node("in");
            var inValues = new Node("field1");
            inValues.Add(new Node("", "howdy"));
            inValues.Add(new Node("", "world"));
            inValues.Add(new Node("", "jalla"));
            in1.Add(inValues);
            and1.Add(in1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1' in (@0,@1,@2)) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("howdy", arg1.Value);

            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@1", arg2.Name);
            Assert.Equal("world", arg2.Value);

            var arg3 = result.Children.Skip(2).First();
            Assert.Equal("@2", arg3.Name);
            Assert.Equal("jalla", arg3.Value);
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
        public void ReadWithOrderAndTableName()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("order", "foo.fieldOrder"));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' order by 'foo'.'fieldOrder' limit 25", sql);
        }

        [Fact]
        public void ReadWithMultipleOrderAndTableName()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            node.Add(new Node("order", "foo.fieldOrder1, foo.fieldOrder2"));
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' order by 'foo'.'fieldOrder1','foo'.'fieldOrder2' limit 25", sql);
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
        public void ReadWithOperators_01()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("field1.eq", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1' = @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_02()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("field1.neq", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1' != @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_03()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("field1.mt", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1' > @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_04()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("field1.lt", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1' < @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_05()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("field1.mteq", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1' >= @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_06()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("field1.lteq", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1' <= @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_07()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("\\field1.lteq", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1.lteq' = @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_08()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("field1.bogus", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1'.'bogus' = @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_09()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("field1.\\lteq", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1'.'lteq' = @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_10()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("field1.like", "howdy%");
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1' like @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("howdy%", arg1.Value);
        }

        [Fact]
        public void ReadWithOperators_11()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var cond1 = new Node("\\field1.lteq", 5);
            and1.Add(cond1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where ('field1.lteq' = @0) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
        }

        [Fact]
        public void ReadWithBogusColumnName()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var columns = new Node("columns");
            columns.Add(new Node("foo.bar.howdy", 5));
            node.Add(columns);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void ReadWithEscapedColumnName()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var columns = new Node("columns");
            columns.Add(new Node("\\foo.bar.howdy", 5));
            node.Add(columns);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select 'foo.bar.howdy' from 'foo' limit 25", sql);
        }

        [Fact]
        public void ReadWithPrefixedColumnName()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var columns = new Node("columns");
            columns.Add(new Node("bar.howdy", 5));
            node.Add(columns);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select 'bar'.'howdy' from 'foo' limit 25", sql);
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

        [Fact]
        public void CreateSlot()
        {
            var lambda = Common.Evaluate(@"sql.create
   table:table1
   values
      field1:value1");
            Assert.Equal("insert into 'table1' ('field1') values (@0)", lambda.Children.First().Get<string>());
            Assert.Equal("@0", lambda.Children.First().Children.First().Name);
            Assert.Equal("value1", lambda.Children.First().Children.First().Get<string>());
        }

        [Fact]
        public void ReadSlot()
        {
            var lambda = Common.Evaluate(@"sql.read
   table:table1");
            Assert.Equal("select * from 'table1' limit 25", lambda.Children.First().Get<string>());
        }

        [Fact]
        public void UpdateSlot()
        {
            var lambda = Common.Evaluate(@"sql.update
   table:table1
   values
      field1:value1");
            Assert.Equal("update 'table1' set 'field1' = @v0", lambda.Children.First().Get<string>());
            Assert.Equal("@v0", lambda.Children.First().Children.First().Name);
            Assert.Equal("value1", lambda.Children.First().Children.First().Get<string>());
        }

        [Fact]
        public void DeleteSlot()
        {
            var lambda = Common.Evaluate(@"sql.delete
   table:table1");
            Assert.Equal("delete from 'table1'", lambda.Children.First().Get<string>());
        }

        [Fact]
        public void ReadSlotJoin_01()
        {
            var lambda = Common.Evaluate(@"sql.read
   limit:-1
   table:table1
      join:table2
         type:inner
         on
            field1:field2");
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'field1' = 'table2'.'field2'", lambda.Children.First().Get<string>());
        }

        [Fact]
        public void ReadSlotJoin_02()
        {
            var lambda = Common.Evaluate(@"sql.read
   limit:-1
   table:table1
      join:table2
         type:inner
         on
            field1:field2
            field3:field4");
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'field1' = 'table2'.'field2', 'table1'.'field3' = 'table2'.'field4'", lambda.Children.First().Get<string>());
        }

        [Fact]
        public void ReadSlotJoin_03()
        {
            var lambda = Common.Evaluate(@"sql.read
   limit:-1
   table:table1
      join:table2
         type:inner
         on
            field1:field2
         join:table3
            type:outer
            on
               field3:field4");
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'field1' = 'table2'.'field2' outer join 'table3' on 'table2'.'field3' = 'table3'.'field4'", lambda.Children.First().Get<string>());
        }

        [Fact]
        public void ReadSlotColumnsAs()
        {
            var lambda = Common.Evaluate(@"sql.read
   table:table1
   columns
      table1.foo1
         as:howdy
      table1.foo2
         as:world");
            Assert.Equal("select 'table1'.'foo1' as 'howdy','table1'.'foo2' as 'world' from 'table1' limit 25", lambda.Children.First().Get<string>());
        }
    }
}
