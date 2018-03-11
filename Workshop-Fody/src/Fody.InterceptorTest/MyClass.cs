namespace Fody.InterceptorTest
{
    using System;
    using System.Threading;

    sealed class MyClass
    {
        [ExecutionTracing]
        public void MyMethod(bool throwsException)
        {
            if (!throwsException)
            {
                Console.WriteLine("... MyClass.MyMethod starts...");

                Thread.Sleep(new Random().Next(100, 500)); // simulate instructions...

                Console.WriteLine("... MyClass.MyMethod end");
            }
            else
            {
                throw new InvalidOperationException("Boom!");
            }
        }
    }
}
