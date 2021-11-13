/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using magic.node;
using magic.signals.services;
using magic.signals.contracts;
using magic.node.extensions.hyperlambda;

namespace magic.data.common.tests
{
    public static class Common
    {
        static public Node Evaluate(string hl, bool config = true)
        {
            var signaler = Initialize(config);
            var lambda = HyperlambdaParser.Parse(hl);
            signaler.Signal("eval", lambda);
            return lambda;
        }

        static async public Task<Node> EvaluateAsync(string hl)
        {
            var signaler = Initialize();
            var lambda = HyperlambdaParser.Parse(hl);
            await signaler.SignalAsync("eval", lambda);
            return lambda;
        }

        public static ISignaler Initialize(bool config = true)
        {
            var services = new ServiceCollection();
            services.AddTransient<ISignaler, Signaler>();
            var types = new SignalsProvider(InstantiateAllTypes<ISlot>(services));
            services.AddTransient<ISignalsProvider>((svc) => types);
            var provider = services.BuildServiceProvider();
            return provider.GetService<ISignaler>();
        }

        #region [ -- Private helper methods -- ]

        static IEnumerable<Type> InstantiateAllTypes<T>(ServiceCollection services) where T : class
        {
            var type = typeof(T);
            var result = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !x.FullName.StartsWith("Microsoft", StringComparison.InvariantCulture))
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var idx in result)
            {
                services.AddTransient(idx);
            }
            return result;
        }

        #endregion
    }
}
