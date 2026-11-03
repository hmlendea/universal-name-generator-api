using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace UniversalNameGenerator.API.ArchitectureTests
{
    [TestFixture]
    public sealed class SourceFileConventionTests : ArchitectureTestBase
    {
        private static readonly string[] ExcludedDirectories = ["bin", "obj"];

        [Test]
        public void HandwrittenProductionFilesShouldContainExactlyOneDeclaredType()
        {
            IEnumerable<string> sourceFiles = GetProductionSourceFiles();

            Assert.That(sourceFiles.Any(), Is.True, "No eligible production source files were found.");

            foreach (string sourceFile in sourceFiles)
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile), path: sourceFile);
                int declaredTypeCount = syntaxTree.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Count()
                    + syntaxTree.GetRoot().DescendantNodes().OfType<DelegateDeclarationSyntax>().Count();

                Assert.That(
                    declaredTypeCount,
                    Is.EqualTo(1),
                    $"{sourceFile} must contain exactly one declared type.");
            }
        }

        [Test]
        public void HandwrittenProductionFilesShouldMatchTheirDeclaredTypeName()
        {
            foreach (string sourceFile in GetProductionSourceFiles())
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile), path: sourceFile);
                SyntaxNode typeDeclaration = syntaxTree.GetRoot().DescendantNodes()
                    .FirstOrDefault(node => node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)
                    ?? throw new AssertionException($"{sourceFile} contains no declared type.");
                string declaredName = GetDeclaredTypeName(typeDeclaration);
                string fileName = Path.GetFileNameWithoutExtension(sourceFile);

                Assert.That(
                    declaredName,
                    Is.EqualTo(fileName),
                    $"{sourceFile} must match the declared type name '{declaredName}'.");
            }
        }

        [Test]
        public void ProductionNamespacesShouldMatchTheirFolders()
        {
            string projectDirectory = GetProductionSourceDirectory();

            foreach (string sourceFile in GetProductionSourceFiles())
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile), path: sourceFile);
                string? namespaceName = syntaxTree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();

                if (namespaceName is null)
                {
                    continue;
                }

                string relativeDirectory = Path.GetRelativePath(projectDirectory, Path.GetDirectoryName(sourceFile)!);
                string expectedSuffix = relativeDirectory == "." ? string.Empty : "." + relativeDirectory.Replace(Path.DirectorySeparatorChar, '.');

                Assert.That(
                    namespaceName,
                    Does.EndWith(expectedSuffix),
                    $"{sourceFile} namespace '{namespaceName}' must match folder '{relativeDirectory}'.");
            }
        }

        [Test]
        public void ProductionFilesShouldUseBlockNamespaces()
        {
            foreach (string sourceFile in GetProductionSourceFiles())
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile), path: sourceFile);

                Assert.That(
                    syntaxTree.GetRoot().DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>(),
                    Is.Empty,
                    $"{sourceFile} must use a block namespace.");
            }
        }

        [Test]
        public void ProductionFilesShouldNotUseRegions()
        {
            foreach (string sourceFile in GetProductionSourceFiles())
            {
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourceFile), path: sourceFile);

                Assert.That(
                    syntaxTree.GetRoot().DescendantTrivia().Where(trivia => trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)),
                    Is.Empty,
                    $"{sourceFile} must not contain regions.");
            }
        }

        private static string GetDeclaredTypeName(SyntaxNode typeDeclaration)
        {
            return typeDeclaration switch
            {
                BaseTypeDeclarationSyntax declaration => declaration.Identifier.ValueText,
                DelegateDeclarationSyntax declaration => declaration.Identifier.ValueText,
                _ => throw new AssertionException("Unsupported type declaration.")
            };
        }

        private static IEnumerable<string> GetProductionSourceFiles()
        {
            string projectDirectory = GetProductionSourceDirectory();

            return Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(file => !ExcludedDirectories.Any(directory => file.Split(Path.DirectorySeparatorChar).Contains(directory, StringComparer.Ordinal)))
                .OrderBy(file => file, StringComparer.Ordinal);
        }
    }
}
