/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using Microsoft.Extensions.Configuration;
using magic.signals.contracts;
using magic.data.common.helpers;

namespace magic.data.common.slots
{
    /// <summary>
    /// [data.execute] slot, for executing some SQL towards a database,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.execute")]
    public class Execute : DataSlotBase
    {
        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public Execute(IConfiguration configuration)
            : base(".execute", configuration)
        { }
    }
}
