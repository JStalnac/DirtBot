using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DirtBot
{
    public static class ModuleManager
    {
        public static void LoadAllModules(Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException("assembly");

            
        }
    }
}
