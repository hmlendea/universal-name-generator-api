using NuciLog.Core;

namespace UniversalNameGenerator.API.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name) : base(name) { }

        public static Operation GenerateNames => new MyOperation(nameof(GenerateNames));

        public static Operation GetSchemas => new MyOperation(nameof(GetSchemas));
    }
}
