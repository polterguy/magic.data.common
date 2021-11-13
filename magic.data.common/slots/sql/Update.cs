/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Linq;
using magic.node;
using magic.signals.contracts;

namespace magic.data.common.slots.sql
{
    /// <summary>
    /// [mssql.update] slot for updating a record in some table.
    /// </summary>
    [Slot(Name = "sql.update")]
    public class Update : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var builder = new SqlUpdateBuilder(input, "'");
            var result = builder.Build();
            input.Value = result.Value;
            input.Clear();
            input.AddRange(result.Children.ToList());
        }
    }
}
