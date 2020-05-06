using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DirtBot.Core
{
    internal class ModuleManager
    {
        public IReadOnlyList<Module> Modules { get; private set; }

        readonly IServiceProvider services;
        Logger log;

        public ModuleManager(IServiceProvider services)
        {
            this.services = services;
            var level = services.GetRequiredService<DirtBot>().LogLevel;
            log = new Logger("Modules", level);
        }

        #region Loading
        /// <summary>
        /// Loads all types that derive from <see cref="Module"/> from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly that the types will be loaded from</param>
        /// <returns></returns>
        internal Type[] LoadAllModules(Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            List<Type> result = new List<Type>();
            log.Info($"Loading modules from assembly {assembly.GetName().Name} ({assembly.Location})");

            foreach (var type in assembly.GetTypes())
            {
                // TODO: Check this
                if (typeof(Module).IsAssignableFrom(type))
                {
                    // The internal modules are loaded from the current assembly. May be changing
                    if (type == typeof(Module))
                        continue;

                    // No abstract types please
                    if (type.IsAbstract)
                    {
                        log.Warning($"Type {type.FullName} was not loaded because it is abstract");
                        continue;
                    }

                    // This can't be handled
                    if (type.ContainsGenericParameters)
                    {
                        log.Warning($"Type {type.FullName} was not loaded because it contains generic parametres");
                        continue;
                    }

                    result.Add(type);
                }
            }

            log.Info($"Found {result.Count} module(s) from {assembly.GetName().Name}");
            if (result.Count != 0)
                log.Info(String.Join(", ", result));
            return result.ToArray();
        }

        /// <summary>
        /// Installs all modules from an array. Will replace the existing <see cref="ModuleManager.Modules"/>
        /// </summary>
        /// <param name="types"></param>
        internal void InstallAllModules(Type[] types)
        {
            var result = new List<Module>();
            foreach (var type in types)
            {
                try
                {
                    // Stops new instances of modules from being created if they
                    // already exist in CommandsNext because it creates instances too.
                    var m = AddModule(type);
                    if (m != null)
                        result.Add(m);
                }
                catch (MissingMethodException ex)
                {
                    // The type doesn't have a public contructor.
                    log.Error($"Failed to load module {type.FullName} because it doesn't have a public constructor.");
                    log.Debug(null, ex);
                }
            }
            Modules = result.AsReadOnly();
        }

        /// <summary>
        /// Returns an instance of an module if it hasn't already been added to the Discord client's CommandsNext else returns null.
        /// </summary>
        /// <param name="moduleType">A <see cref="Module"/> that will be loaded.</param>
        /// <returns></returns>
        internal Module AddModule(Type moduleType)
        {
            if (!typeof(Module).IsAssignableFrom(moduleType))
                throw new ArgumentException($"{nameof(moduleType)} must derive from {nameof(Module)}!");

            var client = services.GetRequiredService<DiscordClient>();
            var cn = client.GetCommandsNext();

            // Don't add duplicates
            if (cn.RegisteredCommands.Values.Any(x => x.Module.ModuleType == moduleType))
                return null;

            return Activator.CreateInstance(moduleType, services) as Module;
        }
        #endregion
    }
}
