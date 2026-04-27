namespace ChatNetworking.utils
{
    public static class TextUtils
    {
        public static string SimpleEncode(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            char[] chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                // C# requires an explicit cast back to char
                chars[i] = (char)(chars[i] + 3);
            }
            return new string(chars);
        }

        public static string SimpleDecode(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            char[] chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                // C# requires an explicit cast back to char
                chars[i] = (char)(chars[i] - 3);
            }
            return new string(chars);
        }
    }
}