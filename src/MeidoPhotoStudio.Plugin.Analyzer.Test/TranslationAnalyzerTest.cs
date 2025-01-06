using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace MeidoPhotoStudio.Plugin.Analyzer.Test;

public class TranslationAnalyzerTest
{
    private const string TranslationSource =
        """
        public static class Translation
        {
            public static string Get(string category, string key) =>
                $"{category}: {key}";
        }
        """;

    private const string TranslationFile =
        /*lang=json,strict*/
        """
        {
          "buttons": {
            "open": "Open File",
            "close": "Close File"
          },
          "toggles": {
            "visibiliity": "Toggle Visibility",
            "apple": "Apples"
          },
          "coolButtons": {
            "open": "Open Cool File"
          },
          "coolToggles": {
            "visibility": "Toogle Cool Visibility"
          }
        }
        """;

    private const string OtherTranslationFile =
        /*lang=json,strict*/
        """
        {
          "sliders": {
            "red": "Red",
            "green": "Green",
            "blue": "Blue"
          }
        }
        """;

    [Fact]
    public async Task CorrectMethodTest() =>
        await new CSharpAnalyzerTest<TranslationAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                AdditionalFiles = { ("translation.ui.json", TranslationFile), },
            },
            TestCode =
                """
                public class Test
                {
                    public static void Main() =>
                        Get("Nice", "World");

                    private static void Get(string key, string value)
                    {
                    }
                }
                """,
        }.RunAsync();

    [Fact]
    public async Task ValidTranslationsTest() =>
        await new CSharpAnalyzerTest<TranslationAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { TranslationSource },
                AdditionalFiles = { ("translation.ui.json", TranslationFile), },
            },
            TestCode =
                """
                public class Test
                {
                    public static void Main() =>
                        Translation.Get("buttons", "open");
                }
                """,
        }.RunAsync();

    [Fact]
    public async Task NoCategoryTest() =>
        await new CSharpAnalyzerTest<TranslationAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { TranslationSource },
                AdditionalFiles = { ("translation.ui.json", TranslationFile), },
            },
            TestCode =
                """
                public class Test
                {
                    public static void Main() =>
                        Translation.Get({|MPS0001:"oops"|}, "open");
                }
                """,
        }.RunAsync();

    [Fact]
    public async Task NoKeyTest() =>
        await new CSharpAnalyzerTest<TranslationAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { TranslationSource },
                AdditionalFiles = { ("translation.ui.json", TranslationFile), },
            },
            TestCode =
                """
                public class Test
                {
                    public static void Main() =>
                        Translation.Get("buttons", {|MPS0002:"oops"|});
                }
                """,
        }.RunAsync();

    [Fact]
    public async Task ConstantTest() =>
        await new CSharpAnalyzerTest<TranslationAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { TranslationSource },
                AdditionalFiles = { ("translation.ui.json", TranslationFile), },
            },
            TestCode =
                """
                public class Test
                {
                    private const string Category = "buttons";

                    public static void Main() =>
                        Translation.Get(Category, "open");
                }
                """,
        }.RunAsync();

    [Fact]
    public async Task LocalConstantTest() =>
        await new CSharpAnalyzerTest<TranslationAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { TranslationSource },
                AdditionalFiles = { ("translation.ui.json", TranslationFile), },
            },
            TestCode =
                """
                public class Test
                {
                    public static void Main()
                    {
                        const string category = "buttons";

                        Translation.Get(category, "open");
                    }
                }
                """,
        }.RunAsync();

    [Fact]
    public async Task ConstantInterpolationTest() =>
        await new CSharpAnalyzerTest<TranslationAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { TranslationSource },
                AdditionalFiles = { ("translation.ui.json", TranslationFile), },
            },
            TestCode =
                """
                public class Test
                {
                    private const string RootCategory = "cool";
                    private const string ButtonCategory = $"{RootCategory}Buttons";
                    private const string ToggleCategory = $"{RootCategory}Toggles";

                    public static void Main()
                    {
                        Translation.Get(ButtonCategory, "open");
                        Translation.Get(ToggleCategory, "visibility");
                    }
                }
                """,
        }.RunAsync();

    [Fact]
    public async Task CasingTest() =>
        await new CSharpAnalyzerTest<TranslationAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { TranslationSource },
                AdditionalFiles = { ("translation.ui.json", TranslationFile), },
            },
            TestCode =
                """
                public class Test
                {
                    public static void Main() =>
                        Translation.Get({|MPS0001:"Buttons"|}, "open");
                }
                """,
        }.RunAsync();

    [Fact]
    public async Task MultipleTranslationFilesTest() =>
        await new CSharpAnalyzerTest<TranslationAnalyzer, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { TranslationSource },
                AdditionalFiles =
                {
                    ("translation.ui.json", TranslationFile),
                    ("translation.other.json", OtherTranslationFile),
                },
            },
            TestCode =
                """
                public class Test
                {
                    public static void Main()
                    {
                        Translation.Get("buttons", "open");
                        Translation.Get("sliders", {|MPS0002:"yellow"|});
                    }
                }
                """,
        }.RunAsync();
}
