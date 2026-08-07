using NuciLog.Core;

namespace UniversalNameGenerator.API.Logging
{
    public sealed class MyLogInfoKey : LogInfoKey
    {
        private MyLogInfoKey(string name) : base(name) { }

        public static LogInfoKey Schema => new MyLogInfoKey(nameof(Schema));

        public static LogInfoKey Count => new MyLogInfoKey(nameof(Count));
    }
}
