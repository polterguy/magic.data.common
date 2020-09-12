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
    /// Common base class for SQL generators requiring q where clause.
    /// </summary>
    public abstract class SqlWhereBuilder : SqlBuilder
    {
        #region [ -- Comparison operator resolver -- ]

        /*
         * These are the default built in comparison operators, resolving to a function that
         * is responsible for handling a particular comparison operators for you.
         */
        readonly static Dictionary<string, Func<StringBuilder, Node, Node, string, int, int>> _comparisonOperators =
            new Dictionary<string, Func<StringBuilder, Node, Node, string, int, int>>
        {
            {"eq", (builder, args, colNode, escapeChar, level) => 
                DefaultOperator(
                    "=",
                    builder,
                    args,
                    colNode,
                    escapeChar,
                    level)
            },
            {"neq", (builder, args, colNode, escapeChar, level) =>
                DefaultOperator(
                    "!=",
                    builder,
                    args,
                    colNode,
                    escapeChar,
                    level)
            },
            {"mt", (builder, args, colNode, escapeChar, level) =>
                DefaultOperator(
                    ">",
                    builder,
                    args,
                    colNode,
                    escapeChar,
                    level)
            },
            {"mteq", (builder, args, colNode, escapeChar, level) =>
                DefaultOperator(
                    ">=",
                    builder,
                    args,
                    colNode,
                    escapeChar,
                    level)
            },
            {"lt", (builder, args, colNode, escapeChar, level) =>
                DefaultOperator(
                    "<",
                    builder,
                    args,
                    colNode,
                    escapeChar,
                    level)
            },
            {"lteq", (builder, args, colNode, escapeChar, level) =>
                DefaultOperator(
                    "<=",
                    builder,
                    args,
                    colNode,
                    escapeChar,
                    level)
            },
            {"like", (builder, args, colNode, escapeChar, level) =>
                DefaultOperator(
                    "like",
                    builder,
                    args,
                    colNode,
                    escapeChar,
                    level)
            },

            // Notice, resolves to custom implementation method.
            {"in", (builder, args, colNode, escapeChar, level) =>
                InOperator(builder, args, colNode, level)
            },
        };

        #endregion

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
        /// Adds a new comparison operator into the resolver, allowing you to
        /// use a custom comparison operator.
        /// </summary>
        /// <param name="key">Key for your operator.</param>
        /// <param name="functor">Function to invoke once comparison operator is encountered.</param>
        public static void AddComparisonOperator(
            string key,
            Func<StringBuilder, Node, Node, string, int, int> functor)
        {
            _comparisonOperators[key] = functor;
        }

        /// <summary>
        /// Appends arguments into builder if we are supposed to do that.
        /// </summary>
        /// <param name="args">Arguments node.</param>
        /// <param name="colNode">Column node, containing actual comparison condition.</param>
        /// <param name="builder">Where to append the resulting SQL.</param>
        /// <param name="level">What argument number we are currently at.</param>
        /// <param name="escapeChar">Escape character for table names.</param>
        /// <returns>How many arguments have in total been appended to the args node.</returns>
        public static int AppendArgs(
            Node args,
            Node colNode,
            StringBuilder builder,
            int level,
            string escapeChar)
        {
            if (args == null)
            {
                // Join invocation.
                var rhs = string.Join(
                    ".",
                    colNode.GetEx<string>()
                        .Split('.')
                        .Select(x => EscapeColumnName(x, escapeChar)));
                builder.Append(rhs);
                return level;
            }
            else
            {
                // Normal argument.
                var argName = "@" + level;
                builder.Append(argName);
                args.Add(new Node(argName, colNode.GetEx<object>()));
                return ++level;
            }
        }

        #endregion

        #region [ -- Protected helper methods and properties -- ]

        /// <summary>
        /// Builds the 'where' parts of the SQL statement.
        /// </summary>
        /// <param name="args">Where to put arguments created during parsing.</param>
        /// <param name="builder">String builder to put the results into.</param>
        protected virtual void BuildWhere(Node args, StringBuilder builder)
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
            if (!where.Children.Any())
                return; // Empty [where] collection.

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
                            idx.Name,
                            0,
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
        int BuildWhereLevel(
            Node args,
            StringBuilder builder,
            Node level,
            string logicalOperator,
            int levelNo,
            bool paranthesis = true)
        {
            if (paranthesis)
                builder.Append("(");

            var idxNo = 0;
            foreach (var idxCol in level.Children)
            {
                if (idxNo++ > 0)
                    builder.Append(" " + logicalOperator + " ");

                switch (idxCol.Name)
                {
                    case "and":
                    case "or":

                        // Recursively invoking self.
                        levelNo = BuildWhereLevel(
                            args,
                            builder,
                            idxCol,
                            idxCol.Name,
                            levelNo);
                        break;

                    default:

                        levelNo = CreateCondition(
                            args,
                            builder,
                            levelNo,
                            idxCol);
                        break;
                }
            }

            if (paranthesis)
                builder.Append(")");
            return levelNo;
        }

        /*
         * Creates a single condition for a where clause.
         */
        int CreateCondition(
            Node args,
            StringBuilder builder,
            int level,
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
                    return _comparisonOperators[keyword](builder, args, comparison, EscapeChar, level);
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
            builder.Append(columnName)
                .Append(" = ");
            return AppendArgs(args, comparison, builder, level, EscapeChar);
        }

        /*
         * Default operator comparison implementation.
         */
        static int DefaultOperator(
            string oper,
            StringBuilder builder,
            Node args,
            Node colNode,
            string escapeChar,
            int level)
        {
            builder.Append($" {oper} ");
            return AppendArgs(args, colNode, builder, level, escapeChar);
        }

        /*
         * In operator implementation.
         */
        static int InOperator(
            StringBuilder builder,
            Node result,
            Node colNode,
            int level)
        {
            builder.Append(" in (");
            var idxNo = 0;
            foreach (var idx in colNode.Children.Select(x => x.GetEx<object>()).ToArray())
            {
                if (idxNo++ > 0)
                    builder.Append(",");
                builder.Append("@" + level);
                result.Add(new Node("@" + level, idx));
                ++level;
            }
            builder.Append(")");
            return level;
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

        #endregion
    }
}
