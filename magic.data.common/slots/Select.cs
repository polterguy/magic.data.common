/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using Microsoft.Extensions.Configuration;
using magic.signals.contracts;
using magic.data.common.helpers;

namespace magic.data.common.slots
{
    /// <summary>
    /// [data.select] slot, for executing some SQL towards a database and returning a record result,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.select")]
    public class Select : DataSlot
    {
        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public Select(IConfiguration configuration)
            : base(".select", configuration)
        { }
    }
}
