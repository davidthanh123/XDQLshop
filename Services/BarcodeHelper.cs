using System.Text;

namespace ShopQuanLy.Services;

public static class BarcodeHelper
{
    private static readonly string[] Patterns = new string[]
    {
        "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213", // 0-9
        "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132", // 10-19
        "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211", // 20-29
        "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313", // 30-39
        "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331", // 40-49
        "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111", // 50-59
        "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214", // 60-69
        "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111", // 70-79
        "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141", // 80-89
        "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141", // 90-99
        "114131", "311141", "411131", "211412", "211214", "211232", "2331112"                                 // 100-106
    };

    public static string GenerateSvg(string text, int height = 50, int moduleWidth = 2)
    {
        if (string.IsNullOrWhiteSpace(text)) text = "000000";
        var clean = new string(text.Where(c => c >= 32 && c <= 126).ToArray());
        if (clean.Length == 0) clean = "000000";

        var codes = new List<int> { 104 }; // Start B
        int checksum = 104;
        for (int i = 0; i < clean.Length; i++)
        {
            int val = clean[i] - 32;
            codes.Add(val);
            checksum += val * (i + 1);
        }
        codes.Add(checksum % 103);
        codes.Add(106); // Stop

        var sb = new StringBuilder();
        int x = 10;
        foreach (int code in codes)
        {
            string pat = Patterns[code];
            bool bar = true;
            foreach (char ch in pat)
            {
                int w = (ch - '0') * moduleWidth;
                if (bar)
                {
                    sb.Append($"<rect x=\"{x}\" y=\"0\" width=\"{w}\" height=\"{height}\" fill=\"#000\"/>");
                }
                x += w;
                bar = !bar;
            }
        }
        x += 10;

        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {x} {height + 18}\" width=\"100%\" height=\"{height + 18}\">" +
               sb.ToString() +
               $"<text x=\"{x / 2}\" y=\"{height + 14}\" text-anchor=\"middle\" font-family=\"monospace\" font-size=\"12\" fill=\"#333\">{clean}</text>" +
               "</svg>";
    }
}
