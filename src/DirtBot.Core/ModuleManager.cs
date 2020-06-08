using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DirtBot.Core
{
    public class ModuleManager
    {
        public IReadOnlyList<IModule> Modules { get; private set; }

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

                    // No abstract types or interfaces please
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
                log.Debug(String.Join("\n", result));
            return result.ToArray();
        }

        /// <summary>
        /// Installs all modules from an array. Replaces the existing <see cref="Modules"/>
        /// </summary>
        /// <param name="types"></param>
        internal void InstallAllModules(Type[] types)
        {
            var result = new List<IModule>();
            var usedInternalNames = new List<string>();

            foreach (var type in types)
            {
                if (!typeof(Module).IsAssignableFrom(type))
                {
                    log.Warning($"Type {type.FullName} isn't a module!");
                    continue;
                }

                if (type.IsNested)
                {
                    if (typeof(Module).IsAssignableFrom(type.DeclaringType))
                        // It's a submodule
                        continue;

                    // It's just a regular nested type
                }

                try
                {
                    var m = Activator.CreateInstance(type, services) as Module;
                    if (String.IsNullOrEmpty(m.Name) || String.IsNullOrEmpty(m.DisplayName))
                    {
                        log.Warning($"Module {type.FullName} doesn't have a name!");
                        continue;
                    }
                    if (usedInternalNames.Contains(m.Name))
                        log.Warning($"The internal name '{m.Name}' is already in use! (module: {type.FullName})");
                    result.Add(m);
                    usedInternalNames.Add(m.Name);
                }
                catch (NotImplementedException)
                {
                    log.Warning($"Module {type.FullName} doesn't have a name!");
                }
                catch (MissingMethodException ex)
                {
                    // The type doesn't have a public contructor.
                    log.Error($"Failed to load module {type.FullName} because it doesn't have a public constructor.");
                    log.Debug(null, ex);
                }
                catch (TargetInvocationException ex)
                {
                    // The module constructor threw an exception.
                    log.Warning($"Constructor for type {type.FullName} failed.", ex.InnerException);
                }
                catch (Exception ex)
                {
                    log.Critical($"Failed to load module {type.FullName}.", ex);
                }
            }

            // The first element is always null somehow.
            if (result.Any())
                result.RemoveAt(0);

            Modules = result.AsReadOnly();
        }
        #endregion
    }
}
