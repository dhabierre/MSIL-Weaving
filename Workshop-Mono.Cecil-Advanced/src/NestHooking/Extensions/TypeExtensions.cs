namespace System
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Ex : x.P[1].ParameterType.IsGenericInstance(typeof(Func<,>))
        /// </summary>
        /// <param name="type">x.P[1].ParameterType</param>
        /// <param name="genTypeDef">typeof(Func<,>)</param>
        /// <returns></returns>
        public static bool IsGenericInstance(this Type type, Type genTypeDef)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            if (type.GetGenericTypeDefinition() != genTypeDef)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ex : x.P[1].ParameterType.IsGenericInstance(typeof(Func<,>), new[] { typeof(int), typeof(bool) })
        /// </summary>
        /// <param name="type">x.P[1].ParameterType</param>
        /// <param name="genTypeDef">typeof(Func<,>)</param>
        /// <param name="args">new[] { typeof(int), typeof(bool) }</param>
        /// <returns></returns>
        public static bool IsGenericInstance(this Type type, Type genTypeDef, params Type[] args)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            if (type.GetGenericTypeDefinition() != genTypeDef)
            {
                return false;
            }

            var typeArgs = type.GetGenericArguments();

            if (typeArgs.Length != args.Length)
            {
                return false;
            }

            for (int i = 0; i != args.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }

                if (args[i] != typeArgs[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
