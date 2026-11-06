using System;
using System.Collections.Generic;

namespace Experiment.CUI
{
    internal sealed class Program
    {
        private sealed class TestClass
        {
            private readonly Lazy<string> _text;

            public TestClass(Func<string> textBuilder)
            {
                _text = new Lazy<string>(textBuilder);
            }

            public string Text => _text.Value;
        }

        private static void Main()
        {
            foreach (var item in Generator())
                Console.WriteLine(item.Text);

            Console.WriteLine();
            _ = Console.ReadLine();

        }

        private static IEnumerable<TestClass> Generator()
        {
            for (var count = 0; count < 10; ++count)
                yield return new TestClass(() => $"--{count}--");
        }
    }
}
