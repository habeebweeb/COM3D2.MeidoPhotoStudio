using BepInEx.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.Configuration;

public class FaceShapeKeyConfiguration(ConfigFile configFile)
    : ShapeKeyConfiguration(configFile, "Character", "Face", DefaultBlockList)
{
    private static readonly string[] DefaultBlockList =
    [
        "earelf", "earnone", "eyebig", "eyeclose", "eyeclose1_normal", "eyeclose1_tare", "eyeclose1_tsuri", "eyeclose2",
        "eyeclose2_normal", "eyeclose2_tare", "eyeclose2_tsuri", "eyeclose3", "eyeclose5", "eyeclose5_normal",
        "eyeclose5_tare", "eyeclose5_tsuri", "eyeclose6", "eyeclose6_normal", "eyeclose6_tare", "eyeclose6_tsuri",
        "eyeclose7", "eyeclose7_normal", "eyeclose7_tare", "eyeclose7_tsuri", "eyeclose8", "eyeclose8_normal",
        "eyeclose8_tare", "eyeclose8_tsuri", "eyeeditl1_dw", "eyeeditl1_up", "eyeeditl2_dw", "eyeeditl2_up",
        "eyeeditl3_dw", "eyeeditl3_up", "eyeeditl4_dw", "eyeeditl4_up", "eyeeditl5_dw", "eyeeditl5_up", "eyeeditl6_dw",
        "eyeeditl6_up", "eyeeditl7_dw", "eyeeditl7_up", "eyeeditl8_dw", "eyeeditl8_up", "eyeeditr1_dw", "eyeeditr1_up",
        "eyeeditr2_dw", "eyeeditr2_up", "eyeeditr3_dw", "eyeeditr3_up", "eyeeditr4_dw", "eyeeditr4_up", "eyeeditr5_dw",
        "eyeeditr5_up", "eyeeditr6_dw", "eyeeditr6_up", "eyeeditr7_dw", "eyeeditr7_up", "eyeeditr8_dw", "eyeeditr8_up",
        "hitomih", "hitomis", "hoho", "hoho2", "hohol", "hohos", "mayueditin", "mayueditout", "mayuha", "mayuup",
        "mayuv", "mayuvhalf", "mayuw", "moutha", "mouthc", "mouthdw", "mouthfera", "mouthferar", "mouthhe", "mouthi",
        "mouths", "mouthup", "mouthuphalf", "namida", "nosefook", "shape", "shapehoho", "shapehohopushr", "shapeslim",
        "shock", "tangopen", "tangout", "tangup", "tear1", "tear2", "tear3", "toothoff", "yodare",
    ];
}
