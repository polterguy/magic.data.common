/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.data.common.slots
{
    /// <summary>
    /// [data.connect] slot, for connecting to a database instance,
    /// according to your configuration settings.
    /// </summary>
    [Slot(Name = "data.connect")]
    public class Connect : ISlot, ISlotAsync
    {
        readonly IConfiguration _configuration;

        /// <summary>
        /// Creates a new instance of your type.
        /// </summary>
        /// <param name="configuration">Configuration for your application.</param>
        public Connect(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var databaseType = 
                input.Children.FirstOrDefault(x => x.Name == "database-type")?.GetEx<string>() ??
                _configuration["magic:databases:default"];
            input.Children.FirstOrDefault(x => x.Name == "database-type")?.UnTie();
            signaler.Signal($"{databaseType}.connect", input);
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler used to raise the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            var databaseType = 
                input.Children.FirstOrDefault(x => x.Name == "database-type")?.GetEx<string>() ??
                _configuration["magic:databases:default"];
            input.Children.FirstOrDefault(x => x.Name == "database-type")?.UnTie();
            await signaler.SignalAsync($"{databaseType}.connect", input);
        }
    }
}
