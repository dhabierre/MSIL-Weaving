namespace Fody.InterceptorTest
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var myClass = new MyClass();

            // if you get a System.IO.FileNotFoundException: ... MethodDecoratorInterfaces, Version=0.9.1.6, Culture=neutral, PublicKeyToken=0de10386fb6c39dd...
            // => try with MethodDecoratorInterfaces 'Copy Local' = true

            Console.WriteLine(">> Call 1: without exception...");
            Console.WriteLine();

            myClass.MyMethod(throwsException: false);

            Console.WriteLine();
            Console.WriteLine(">> Call 2: with exception...");
            Console.WriteLine();

            try
            {
                myClass.MyMethod(throwsException: true);
            }
            catch
            {
            }

            Console.ReadLine();
        }
    }
}
