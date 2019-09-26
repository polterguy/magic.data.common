﻿/*
 * Magic, Copyright(c) Thomas Hansen 2019 - thomas@gaiasoul.com
 * Licensed as Affero GPL unless an explicitly proprietary license has been obtained.
 */

using System.Text;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.data.common
{
    public class SqlDeleteBuilder : SqlBuilder
    {
        public SqlDeleteBuilder(Node node, ISignaler signaler, string escapeChar)
            : base(node, signaler, escapeChar)
        { }

        public override Node Build()
        {
            // Return value.
            var result = new Node("sql");
            var builder = new StringBuilder();

            // Building SQL text and parameter collection.
            builder.Append("delete from ");

            // Getting table name from base class.
            GetTableName(builder);

            // Getting [where] clause.
            BuildWhere(result, builder);

            // Returning result to caller.
            result.Value = builder.ToString();
            return result;
        }
    }
}
