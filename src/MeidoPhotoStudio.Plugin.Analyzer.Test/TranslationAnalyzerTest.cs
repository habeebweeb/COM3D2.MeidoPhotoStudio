using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace MeidoPhotoStudio.Plugin.Analyzer.Test;

public class TranslationAnalyzerTest
{
    private const string TranslationSource =
        """
        public partial class Test
        {
            private static readonly Translation Translation = new();
        }

        public class Translation
        {
            public string this[string category, string key] =>
                string.Empty;
        }

        public class LocalizableGUIContent(Translation translation, string category, string key)
        {
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
                public partial class Test
                {
                    public static void Main()
                    {
                        _ = Translation["buttons", "open"];
                        _ = new LocalizableGUIContent(Translation, "buttons", "open");
                    }
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
                public partial class Test
                {
                    public static void Main()
                    {
                        _ = Translation[{|MPS0001:"oops"|}, "open"];
                        _ = new LocalizableGUIContent(Translation, {|MPS0001:"oops"|}, "open");
                    }
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
                public partial class Test
                {
                    public static void Main()
                    {
                        _ = Translation["buttons", {|MPS0002:"oops"|}];
                        _ = new LocalizableGUIContent(Translation, "buttons", {|MPS0002:"oops"|});
                    }
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
                public partial class Test
                {
                    private const string Category = "buttons";

                    public static void Main()
                    {
                        _ = Translation[Category, "open"];
                        _ = new LocalizableGUIContent(Translation, Category, "open");
                    }
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
                public partial class Test
                {
                    public static void Main()
                    {
                        const string category = "buttons";

                        _ = Translation[category, "open"];
                        _ = new LocalizableGUIContent(Translation, category, "open");
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
                public partial class Test
                {
                    private const string RootCategory = "cool";
                    private const string ButtonCategory = $"{RootCategory}Buttons";
                    private const string ToggleCategory = $"{RootCategory}Toggles";

                    public static void Main()
                    {
                        _ = Translation[ButtonCategory, "open"];
                        _ = Translation[ToggleCategory, "visibility"];
                        _ = new LocalizableGUIContent(Translation, ButtonCategory, "open");
                        _ = new LocalizableGUIContent(Translation, ToggleCategory, "visibility");
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
                public partial class Test
                {
                    public static void Main()
                    {
                        _ = Translation[{|MPS0001:"Buttons"|}, "open"];
                        _ = new LocalizableGUIContent(Translation, {|MPS0001:"Buttons"|}, "open");
                    }
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
                public partial class Test
                {
                    public static void Main()
                    {
                        _ = Translation["buttons", "open"];
                        _ = Translation["sliders", {|MPS0002:"yellow"|}];
                        _ = new LocalizableGUIContent(Translation, "buttons", "open");
                        _ = new LocalizableGUIContent(Translation, "sliders", {|MPS0002:"yellow"|});
                    }
                }
                """,
        }.RunAsync();
}
