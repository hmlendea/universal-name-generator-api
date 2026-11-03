using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

using NUnit.Framework;

namespace UniversalNameGenerator.API.ArchitectureTests
{
    public abstract class ArchitectureTestBase
    {
        protected static readonly Architecture Architecture = LoadArchitecture();

        protected static readonly System.Reflection.Assembly ProductionAssembly = typeof(UniversalNameGenerator.API.Controllers.NamesController).Assembly;

        protected static Architecture LoadArchitecture()
        {
            System.Reflection.Assembly productionAssembly = typeof(UniversalNameGenerator.API.Controllers.NamesController).Assembly;

            return new ArchLoader()
                .LoadAssemblies(productionAssembly)
                .Build();
        }

        protected static void AssertExpectedTypes<T>(IEnumerable<T> types, string description)
        {
            Assert.That(types, Is.Not.Null, $"The selector for {description} returned null.");
            Assert.That(types.Any(), Is.True, $"The selector for {description} matched no types.");
        }

        protected static string GetProductionSourceDirectory()
        {
            string? repositoryDirectory = FindProjectDirectory();

            Assert.That(repositoryDirectory, Is.Not.Null, "The production project directory was not found.");
            return Path.Combine(repositoryDirectory!, "UniversalNameGenerator.API");
        }

        private static string? FindProjectDirectory()
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);

            while (directory is not null)
            {
                string projectFile = Path.Combine(
                    directory.FullName,
                    "UniversalNameGenerator.API",
                    "UniversalNameGenerator.API.csproj");

                if (File.Exists(projectFile))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}
