using System.Text;

namespace CurseTheBeast.Utils;


public static class MyEncoding
{
    public static readonly Encoding GBK = CodePagesEncodingProvider.Instance.GetEncoding("GBK")!;
    // f**k UTF8-BOM
    public static readonly UTF8Encoding UTF8 = new UTF8Encoding(false);
}
