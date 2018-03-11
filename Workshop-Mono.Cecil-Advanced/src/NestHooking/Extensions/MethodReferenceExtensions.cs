namespace Mono.Cecil
{
    using System;
    using System.Linq;

    public static class MethodReferenceExtensions
    {
        public static MethodReference MakeGeneric(this MethodReference method, params TypeReference[] args)
        {
            if (args == null || !args.Any())
            {
                return method;
            }

            if (args.Length != method.GenericParameters.Count)
            {
                throw new ArgumentException("Invalid number of generic type arguments supplied", nameof(args));
            }

            var genericMethod = new GenericInstanceMethod(method);

            foreach (var arg in args)
            {
                genericMethod.GenericArguments.Add(arg);
            }

            return genericMethod;
        }
    }
}
