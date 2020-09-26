/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using magic.node;
using magic.node.extensions;

namespace magic.data.common.helpers
{
    /// <summary>
    /// Common base class for SQL generators requiring a where clause.
    /// </summary>
    public abstract class SqlWhereBuilder : SqlBuilder
    {
        /*
         * These are the default built in comparison operators, resolving to a function that
         * is responsible for handling a particular comparison operators for you.
         */
        readonly static Dictionary<string, Action<StringBuilder, Node, Node, string>> _comparisonOperators = CreateDefaultComparisonOperators();

        /// <summary>
        /// Creates a new SQL builder.
        /// </summary>
        /// <param name="node">Root node to generate your SQL from.</param>
        /// <param name="escapeChar">Escape character to use for escaping table names etc.</param>
        protected SqlWhereBuilder(Node node, string escapeChar)
            : base(node, escapeChar)
        { }

        #region [ -- Public static methods -- ]

        /// <summary>
        /// Adds a new comparison operator into the comparison operator resolver,
        /// allowing you to use a custom comparison operator, resolving to some function,
        /// responsible for injecting SQL into your resulting SQL somehow.
        /// </summary>
        /// <param name="key">Key for your operator.</param>
        /// <param name="functor">Function to invoke once comparison operator is encountered.</param>
        public static void AddComparisonOperator(
            string key,
            Action<StringBuilder, Node, Node, string> functor)
        {
            _comparisonOperators[key] = functor;
        }

        /// <summary>
        /// Appends arguments into builder if args is not null, and references argument
        /// in SQL - Otherwise assuming we are to append the value of the colummn node
        /// as the right hand side of the comparison, which might be true for joins
        /// for instance.
        /// </summary>
        /// <param name="args">Arguments node, if this is null, no arguments will be appended into args node.</param>
        /// <param name="colNode">Column node, containing actual comparison condition.</param>
        /// <param name="builder">Where to append the resulting SQL.</param>
        /// <param name="escapeChar">Escape character for table names.</param>
        public static void AppendArgs(
            Node args,
            Node colNode,
            StringBuilder builder,
            string escapeChar)
        {
            if (args == null)
            {
                // No args node given, assuming direct comparison.
                var rhs = string.Join(
                    ".",
                    colNode.GetEx<string>()
                        .Split('.')
                        .Select(x => EscapeColumnName(x, escapeChar)));
                builder.Append(rhs);
                return;
            }

            // Plain argument, referencing it in SQL, and adding to args collection.
            var argNo = args.Children.Count(x => x.Name.StartsWith("@") && x.Name.Skip(1).First() != 'v');
            var argName = "@" + argNo;
            builder.Append(argName);
            args.Add(new Node(argName, colNode.GetEx<object>()));
        }

        #endregion

        #region [ -- Protected helper methods and properties -- ]

        /// <summary>
        /// Builds the 'where' parts of the SQL statement.
        /// </summary>
        /// <param name="builder">String builder to put the results into.</param>
        /// <param name="args">Where to put arguments created during parsing.</param>
        protected virtual void AppendWhere(StringBuilder builder, Node args)
        {
            // Finding where node, if any, and doing some basic sanity checking.
            var whereNodes = Root.Children.Where(x => x.Name == "where");
            if (whereNodes.Count() > 1)
                throw new ArgumentException($"Syntax error in '{GetType().FullName}', too many [where] nodes");

            // Checking that we actually have a [where] declaration at all.
            if (!whereNodes.Any())
                return; // No where statement supplied, or not children in [where] argument.

            // Extracting actual where node, and doing some more sanity checking.
            var where = whereNodes.First();
            if (!where.Children.Any() || !where.Children.Where(x => x.Children.Any()).Any())
                return; // Empty [where], [and] or [or] collection.

            // Appending actual "where" parts into SQL.
            builder.Append(" where ");
            AppendBooleanLevel(where, args, builder);
        }

        /// <summary>
        /// Iterates through all children of specified node, and building one [or]/[and]
        /// level for each of its children.
        /// </summary>
        /// <param name="args">Where to append arguments, if requested by caller. Notice,
        /// the args node might be null in cases we are for instance invoking this method for
        /// a [join] invocation.</param>
        /// <param name="builder">Where to append SQL.</param>
        /// <param name="conditionLevel">Where node for current level.</param>
        protected void AppendBooleanLevel(
            Node conditionLevel,
            Node args,
            StringBuilder builder)
        {
            /*
             * Recursively looping through each level, and appending its parts
             * as a "name/value" collection, making sure we add each value as an
             * SQL parameter.
             */
            foreach (var idx in conditionLevel.Children)
            {
                switch (idx.Name)
                {
                    case "or":
                    case "and":
                        BuildWhereLevel(
                            args,
                            builder,
                            idx,
                            false /* No outer most level paranthesis */);
                        break;

                    default:
                        throw new ArgumentException($"I don't understand '{idx.Name}' as a boolean operator, only [or] and [and] at this level");
                }
            }
        }

        #endregion

        #region [ -- Private helper methods -- ]

        /*
         * Building one "where level" (within one set of paranthesis),
         * and recursivelu adding a new level for each "and" and "or"
         * parts we can find in our level.
         */
        void BuildWhereLevel(
            Node args,
            StringBuilder builder,
            Node booleanNode,
            bool paranthesis = true)
        {
            if (paranthesis && booleanNode.Children.Any())
                builder.Append("(");

            var no = 0;
            foreach (var idx in booleanNode.Children)
            {
                if (no++ > 0)
                    builder.Append(" " + booleanNode.Name + " ");

                switch (idx.Name)
                {
                    case "and":
                    case "or":

                        // Recursively invoking self.
                        BuildWhereLevel(
                            args,
                            builder,
                            idx);
                        break;

                    default:

                        CreateCondition(
                            args,
                            builder,
                            idx);
                        break;
                }
            }

            if (paranthesis && booleanNode.Children.Any())
                builder.Append(")");
        }

        /*
         * Creates a single condition for a where clause.
         */
        void CreateCondition(
            Node args,
            StringBuilder builder,
            Node comparison)
        {
            // Field comparison of some sort.
            var columnName = comparison.Name;
            if (columnName.StartsWith("\\"))
            {
                // Allowing for escaped column names, to suppor columns containing "." as a part of their names.
                columnName = EscapeColumnName(columnName.Substring(1));
            }
            else if (columnName.Contains("."))
            {
                // Possibly an oeprator, hence checking operator dictionary for a match.
                var entities = columnName.Split('.');
                var keyword = entities.Last();
                if (_comparisonOperators.ContainsKey(keyword))
                {
                    columnName = string.Join(
                        ".",
                        entities
                            .Take(entities.Count() - 1)
                            .Select(x => EscapeColumnName(x)));
                    builder.Append(columnName);
                    _comparisonOperators[keyword](builder, args, comparison, EscapeChar);
                    return;
                }

                // Checking if last entity is escaped.
                var tmp = new List<string>();
                if (keyword.StartsWith("\\"))
                {
                    keyword = keyword.Substring(1);
                    tmp.AddRange(entities.Take(entities.Count() - 1));
                    tmp.Add(keyword);
                    entities = tmp.ToArray();
                }
                columnName = string.Join(
                    ".",
                    entities.Select(x => EscapeColumnName(x)));
            }
            else
            {
                columnName = EscapeColumnName(columnName);
            }

            // This is the default logic to apply, if no operators was specified.
            builder.Append(columnName).Append(" = ");
            AppendArgs(args, comparison, builder, EscapeChar);
        }

        /*
         * Helper method to escape column names in static methods.
         */
        static string EscapeColumnName(string column, string escapeChar)
        {
            return escapeChar + 
                column.Replace(escapeChar, escapeChar + escapeChar) +
                escapeChar;
        }

        /*
         * Creates our default built in comparison operators.
         */
        static Dictionary<string, Action<StringBuilder, Node, Node, string>> CreateDefaultComparisonOperators()
        {
            var result = new Dictionary<string, Action<StringBuilder, Node, Node, string>>();

            // Plain default comparison operators.
            foreach (var idx in new (string, string) [] {
                ("eq", "="),
                ("neq", "!="),
                ("mt", ">"),
                ("mteq", ">="),
                ("lt", "<"),
                ("lteq", "<="),
                ("like", "like")})
            {
                result[idx.Item1] = (builder, args, colNode, escapeChar) => {
                    builder.Append($" {idx.Item2} ");
                    AppendArgs(args, colNode, builder, escapeChar);
                };
            }

            // Special case for "in" comparison operator.
            result["in"] = (builder, args, colNode, escapeChar) => {
                builder.Append(" in (");
                var argNo = args.Children.Count(x => x.Name.StartsWith("@") && x.Name.Skip(1).First() != 'v');
                builder.Append(string.Join(",", colNode.Children.Select(x =>
                {
                    args.Add(new Node("@" + argNo, x.GetEx<object>()));
                    return "@" + argNo++;
                }))).Append(")");
            };
            return result;
        }

        #endregion
    }
}
