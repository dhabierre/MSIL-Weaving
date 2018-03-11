[module: Fody.InterceptorTest.ExecutionTracing]

namespace Fody.InterceptorTest
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Module)]
    class ExecutionTracingAttribute : Attribute
    {
        private MethodBase method = null;
        private Stopwatch stopwatch = null;

        public void Init(object instance, MethodBase method, object[] args)
        {
            this.method = method;

            Console.WriteLine($"{this.method.DeclaringType.FullName}.{this.method.Name} > Init");
        }

        public void OnEntry()
        {
            this.stopwatch = Stopwatch.StartNew();

            Console.WriteLine($"{this.method.DeclaringType.FullName}.{this.method.Name} > OnEntry");
        }

        public void OnExit()
        {
            this.stopwatch.Stop();

            Console.WriteLine($"{this.method.DeclaringType.FullName}.{this.method.Name} > OnExit");
            Console.WriteLine($"{this.method.DeclaringType.FullName}.{this.method.Name} > Duration: {this.stopwatch.ElapsedMilliseconds}ms");
        }

        public void OnException(Exception e)
        {
            Console.WriteLine($"{this.method.DeclaringType.FullName}.{this.method.Name} > OnException: {e.GetType()}: {e.Message}");
        }
    }
}
