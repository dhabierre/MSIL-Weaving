namespace Mono.Cecil
{
    using System;
    using System.Linq;

    public static class ModuleDefinitionExtensions
    {
        public static TypeReference GetReference(this ModuleDefinition moduleDefinition, Type type)
        {
            var typeRefs = moduleDefinition.GetTypeReferences();

            return typeRefs.FirstOrDefault(t => t.FullName == type.FullName);
        }

        public static TypeReference GetReference<T>(this ModuleDefinition moduleDefinition)
        {
            return GetReference(moduleDefinition, typeof(T));
        }
    }
}
