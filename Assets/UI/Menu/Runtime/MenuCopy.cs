using System.Text.RegularExpressions;

namespace KhiVanChiTu.UI.Menu
{
    public static class MenuCopy
    {
        public static string Plain(string value)=>Regex.Replace(value??"",@"<[^>]*>","").Trim();
        public static string Value(string value){value=Plain(value);int colon=value.IndexOf(':');return colon>=0?value.Substring(colon+1).Trim():value;}
    }
}
