/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using magic.node;
using magic.node.extensions;
using magic.data.common.helpers;

namespace magic.data.common
{
    /// <summary>
    /// Specialised select SQL builder, to create a select SQL statement by semantically traversing an input node.
    /// </summary>
    public class SqlReadBuilder : SqlWhereBuilder
    {
        /// <summary>
        /// Creates a select SQL statement
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="escapeChar">Escape character to use for escaping table names etc.</param>
        public SqlReadBuilder(Node node, string escapeChar)
            : base(node, escapeChar)
        { }

        /// <summary>
        /// Builds your select SQL statement, and returns a structured SQL statement, plus any parameters.
        /// </summary>
        /// <returns>Node containing insert SQL as root node, and parameters as children.</returns>
        public override Node Build()
        {
            // Return value.
            var result = new Node("sql");

            // Starting build process.
            var builder = new StringBuilder();
            builder.Append("select ");

            // Getting columns.
            GetColumns(builder);

            builder.Append(" from ");

            // Getting table name from base class.
            GetTableName(builder);

            // Getting [where] clause.
            BuildWhere(result, builder);

            // Adding tail.
            GetTail(builder);

            // Returning result to caller.
            result.Value = builder.ToString();
            return result;
        }

        #region [ -- Protected, overridden, and virtual methods -- ]

        protected override void GetTableName(StringBuilder builder)
        {
            var tableNode = Root.Children.FirstOrDefault(x => x.Name == "table") ??
                throw new ArgumentException($"No [table] argument supplied to {GetType().FullName}");
            if (!tableNode.Children.Any())
            {
                // No joins here, hence simply invoking base class implementation, and returning early.
                base.GetTableName(builder);
                return;
            }
            GetTableName(builder, tableNode);
        }

        /// <summary>
        /// Adds limit and offset parts to your SQL if requested by caller.
        /// </summary>
        /// <param name="builder">Where to put the resulting SQL into.</param>
        protected virtual void GetTail(StringBuilder builder)
        {
            // Getting [order].
            GetOrderBy(builder);

            // Getting [limit].
            var limitNodes = Root.Children.Where(x => x.Name == "limit");
            if (limitNodes.Any())
            {
                // Sanity checking.
                if (limitNodes.Count() > 1)
                    throw new ArgumentException($"syntax error in '{GetType().FullName}', too many [limit] nodes");

                var limitValue = limitNodes.First().GetEx<long>();
                if (limitValue > -1)
                    builder.Append(" limit " + limitValue);
            }
            else
            {
                // Defaulting to 25 records, unless [limit] was explicitly given.
                builder.Append(" limit 25");
            }

            // Getting [offset].
            var offsetNodes = Root.Children.Where(x => x.Name == "offset");
            if (offsetNodes.Any())
            {
                // Sanity checking.
                if (offsetNodes.Count() > 1)
                    throw new ArgumentException($"syntax error in '{GetType().FullName}', too many [offset] nodes");

                var offsetValue = offsetNodes.First().GetEx<long>();
                builder.Append(" offset " + offsetValue);
            }
        }

        /// <summary>
        /// Appends the order by clause into builder.
        /// </summary>
        /// <param name="builder">Builder where clause should be appended.</param>
        protected virtual void GetOrderBy(StringBuilder builder)
        {
            var orderNodes = Root.Children.Where(x => x.Name == "order");
            if (orderNodes.Any())
            {
                // Sanity checking.
                if (orderNodes.Count() > 1)
                    throw new ArgumentException($"syntax error in '{GetType().FullName}', too many [order] nodes");

                var orderColumn = EscapeColumnName(orderNodes.First().GetEx<string>());
                builder.Append(" order by " + orderColumn);

                // Checking if [direction] node exists.
                var direction = Root.Children.Where(x => x.Name == "direction");
                if (direction.Any())
                {
                    // Sanity checking.
                    if (direction.Count() > 1)
                        throw new ArgumentException($"syntax error in '{GetType().FullName}', too many [direction] nodes");

                    var dir = direction.First().GetEx<string>();
                    switch (dir)
                    {
                        case "asc":
                        case "desc":
                            builder.Append(" ").Append(dir);
                            break;
                        default:
                            throw new ArgumentException($"I don't know how to sort according to the '{dir}' [direction], only 'asc' and 'desc'");
                    }
                }
            }
            else
            {
                /*
                 * Some databases requires "default order by" statement, such as
                 * for instance MS SQL Server does in cases where we have defined a
                 * "limit" and "offset".
                 */
                GetDefaultOrderBy(builder);
            }
        }

        /// <summary>
        /// Adds the default order by clause for queries.
        /// </summary>
        /// <param name="builder">Where to put the default order by clause.</param>
        protected virtual void GetDefaultOrderBy(StringBuilder builder)
        { }

        #endregion

        #region [ -- Private helper methods -- ]

        void GetColumns(StringBuilder builder)
        {
            var columnsNodes = Root.Children.Where(x => x.Name == "columns");
            if (!columnsNodes.Any())
            {
                // Caller did not explicitly declare columns, hence defaulting to all.
                builder.Append("*");
                return;
            }

            // Sanity checking invocation.
            if (columnsNodes.Count() > 1)
                throw new ArgumentException("You can only declare [columns] once in your lambda.");

            // Adding all columns caller requested to SQL.
            var columnNode = columnsNodes.First();
            var idxNo = 0;
            foreach (var idx in columnNode.Children)
            {
                if (idxNo++ > 0)
                    builder.Append(",");

                if (idx.Name.Contains("(") && idx.Name.Contains(")"))
                    builder.Append(idx.Name); // Aggregate column, avoid escaping.
                else
                    builder.Append(EscapeColumnName(idx.Name));
            }
        }

        /*
         * Joins multiple tables into builder.
         */
        void GetTableName(StringBuilder builder, Node tableNode)
        {
            var primaryTableName = tableNode.GetEx<string>();
            var idxNo = 0;
            foreach (var idx in primaryTableName.Split('.'))
            {
                if (idxNo++ > 0)
                    builder.Append(".");
                builder.Append(EscapeColumnName(idx));
            }

            // Sanity checking invocation.
            if (tableNode.Children.Any(x => x.Name != "join"))
                throw new ArgumentException($"I don't understand anything but [join] as arguments to your [table] nodes");

            // Iterating through all [join] arguments specified.
            foreach (var idx in tableNode.Children)
            {
                // Appending join and its type.
                var joinType = idx.Children
                    .FirstOrDefault(x => x.Name == "type")?
                    .GetEx<string>() ?? "inner";
                builder.Append(" ")
                    .Append(joinType)
                    .Append(" join ");

                // Appending secondary table name, and its "on" parts.
                GetSingleTableName(builder, idx.GetEx<string>());
                builder.Append(" on ");

                // Retrieving and appending all "on" criteria.
                var onNodes = idx.Children.FirstOrDefault(x => x.Name == "on") ??
                    throw new ArgumentException("No [on] arguments supplied to [join]");
                foreach (var idxOn in onNodes.Children)
                {
                    GetSingleTableName(builder, primaryTableName);
                    builder.Append(".")
                        .Append(EscapeColumnName(idxOn.Name));
                    builder.Append(" = "); // TODO: Implement multiple operators.
                    GetSingleTableName(builder, idx.GetEx<string>());
                    builder.Append(".")
                        .Append(EscapeColumnName(idxOn.GetEx<string>()));
                }
            }
        }

        #endregion
    }
}
