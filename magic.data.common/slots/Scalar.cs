/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using magic.node.contracts;
using magic.signals.contracts;
using magic.data.common.helpers;

namespace magic.data.common.slots
{
    /// <summary>
    /// [data.scalar] slot, for executing some SQL towards a database and returning a scalar result,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.scalar")]
    public class Scalar : DataSlotBase
    {
        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public Scalar(IMagicConfiguration configuration)
            : base(".scalar", configuration)
        { }
    }
}
