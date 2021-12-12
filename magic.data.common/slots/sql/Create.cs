/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Linq;
using magic.node;
using magic.signals.contracts;
using magic.data.common.builders;

namespace magic.data.common.slots.sql
{
    /// <summary>
    /// [sql.create] slot for creating an insert SQL, with parameters for you.
    /// </summary>
    [Slot(Name = "sql.create")]
    public class Create : ISlot
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var builder = new SqlCreateBuilder(input, "'");
            var result = builder.Build();
            input.Value = result.Value;
            input.Clear();
            input.AddRange(result.Children.ToList());
        }
    }
}
