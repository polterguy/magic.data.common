/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using Xunit;
using magic.node;
using magic.node.extensions;
using magic.data.common.helpers;

namespace magic.data.common.tests
{
    public class DataTests
    {
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
            var and1 = new Node("and");
            and1.Add(new Node("table1.fk1", "table2.pk1"));
            on1.Add(and1);
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
        public void ReadWithCustomComparisonOperator()
        {
            // Creating node hierarchy.
            var node = new Node();
            var where = new Node("where");
            var and1 = new Node("and");
            and1.Add(new Node("foo1.qwerty", "howdy"));
            where.Add(and1);
            node.Add(where);
            node.Add(new Node("table", "foo"));

            // Adding our custom operator.
            SqlWhereBuilder.AddComparisonOperator("qwerty", (builder, args, colNode, escapeChar) => {
                builder.Append(" <> ");
                SqlWhereBuilder.AppendArgs(args, colNode, builder, escapeChar);
            });

            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where 'foo1' <> @0 limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("howdy", arg1.Value);
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
            var and1 = new Node("and");
            and1.Add(new Node("fk1", "pk1"));
            on1.Add(and1);
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
            var and1 = new Node("and");
            and1.Add(new Node("table1.fk1", "table2.pk1"));
            on1.Add(and1);
            join1.Add(on1);
            var join2 = new Node("join", "table3");
            var on2 = new Node("on");
            var and2 = new Node("and");
            and2.Add(new Node("fk2", "pk2"));
            on2.Add(and2);
            join2.Add(on2);
            join1.Add(join2);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'fk1' = 'table2'.'pk1' inner join 'table3' on 'fk2' = 'pk2' limit 25", sql);
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
            var and1 = new Node("and");
            and1.Add(new Node("fk1.neq", "pk1"));
            on1.Add(and1);
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'table1' inner join 'table2' on 'fk1' != 'pk1' limit 25", sql);
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
            var and1 = new Node("and");
            and1.Add(new Node("table1.fk1", "table2.pk1"));
            and1.Add(new Node("table1.fk2", "table2.pk2"));
            on1.Add(and1);
            join1.Add(on1);
            table1.Add(join1);
            node.Add(table1);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'table1' inner join 'table2' on 'table1'.'fk1' = 'table2'.'pk1' and 'table1'.'fk2' = 'table2'.'pk2' limit 25", sql);
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
            var and1 = new Node("and");
            and1.Add(new Node("dbo.table1.fk1", "dbo.table2.pk1"));
            on1.Add(and1);
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
            var and1 = new Node("and");
            and1.Add(new Node("table1.fk1", "table2.pk1"));
            on1.Add(and1);
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
            Assert.Equal("select * from 'foo' where 'foo1' = @0 and 'foo2' = @1 and ('foo3' = @2 or 'foo4' = @3) limit 25", sql);

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
            var or1 = new Node("or");
            or1.Add(new Node("foo1", 5));
            or1.Add(new Node("foo2", "howdy"));
            var and2 = new Node("and");
            and2.Add(new Node("foo3", "jalla"));
            and2.Add(new Node("foo4", "balla"));
            or1.Add(and2);
            where.Add(or1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where 'foo1' = @0 or 'foo2' = @1 or ('foo3' = @2 and 'foo4' = @3) limit 25", sql);

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
            var in1 = new Node("field1.in");
            in1.Add(new Node("", 5));
            in1.Add(new Node("", 7));
            or1.Add(in1);
            where.Add(or1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where 'foo1' = @0 or 'foo2' = @1 or 'field1' in (@2,@3) limit 25", sql);

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
            var in1 = new Node("field1.in");
            in1.Add(new Node("", 5L));
            in1.Add(new Node("", 7L));
            in1.Add(new Node("", 9L));
            and1.Add(in1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
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
        public void ReadInNamespaced()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var in1 = new Node("foo.field1.in");
            in1.Add(new Node("", 5L));
            in1.Add(new Node("", 7L));
            in1.Add(new Node("", 9L));
            and1.Add(in1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where 'foo'.'field1' in (@0,@1,@2) limit 25", sql);

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
            var in1 = new Node("field1.in");
            in1.Add(new Node("", "howdy"));
            in1.Add(new Node("", "world"));
            in1.Add(new Node("", "jalla"));
            and1.Add(in1);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where 'field1' in (@0,@1,@2) limit 25", sql);

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
        public void ReadInStringsMultiple()
        {
            // Creating node hierarchy.
            var node = new Node();
            node.Add(new Node("table", "foo"));
            var where = new Node("where");
            var and1 = new Node("and");
            var in1 = new Node("field1.in");
            in1.Add(new Node("", "howdy"));
            in1.Add(new Node("", "world"));
            in1.Add(new Node("", "jalla"));
            and1.Add(in1);
            var in2 = new Node("field2.in");
            in2.Add(new Node("", "howdy2"));
            in2.Add(new Node("", "world2"));
            in2.Add(new Node("", "jalla2"));
            and1.Add(in2);
            where.Add(and1);
            node.Add(where);
            var builder = new SqlReadBuilder(node, "'");

            // Extracting SQL + params, and asserting correctness.
            var result = builder.Build();
            var sql = result.Get<string>();
            Assert.Equal("select * from 'foo' where 'field1' in (@0,@1,@2) and 'field2' in (@3,@4,@5) limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal("howdy", arg1.Value);

            var arg2 = result.Children.Skip(1).First();
            Assert.Equal("@1", arg2.Name);
            Assert.Equal("world", arg2.Value);

            var arg3 = result.Children.Skip(2).First();
            Assert.Equal("@2", arg3.Name);
            Assert.Equal("jalla", arg3.Value);

            var arg4 = result.Children.Skip(3).First();
            Assert.Equal("@3", arg4.Name);
            Assert.Equal("howdy2", arg4.Value);

            var arg5 = result.Children.Skip(4).First();
            Assert.Equal("@4", arg5.Name);
            Assert.Equal("world2", arg5.Value);

            var arg6 = result.Children.Skip(5).First();
            Assert.Equal("@5", arg6.Name);
            Assert.Equal("jalla2", arg6.Value);
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
            Assert.Equal("select * from 'foo' where 'field1' = @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1' != @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1' > @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1' < @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1' >= @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1' <= @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1.lteq' = @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1'.'bogus' = @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1'.'lteq' = @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1' like @0 limit 25", sql);

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
            Assert.Equal("select * from 'foo' where 'field1.lteq' = @0 limit 25", sql);

            var arg1 = result.Children.First();
            Assert.Equal("@0", arg1.Name);
            Assert.Equal(5, arg1.Value);
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
    }
}
