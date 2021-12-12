/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using Microsoft.Extensions.Configuration;
using magic.signals.contracts;
using magic.data.common.helpers;

namespace magic.data.common.slots
{
    /// <summary>
    /// [data.connect] slot, for connecting to a database instance,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.connect")]
    public class Connect : DataSlot
    {
        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public Connect(IConfiguration configuration)
            : base(".connect", configuration)
        { }
    }
}
