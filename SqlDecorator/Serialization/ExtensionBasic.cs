using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CommonInfra
{

    [Description("Basic extansion")]   
    public static class ExtensionBasic
    {
       public static string fixNil(this string str)
        {
            if (str == null) str = string.Empty;
            if (str.Contains('\0'))
                str=str.Replace('\0', ' ');
            return str;
        }

        public static string Flip(this string str)
        {
            if (str != null)
            {
                string Bmsg = "";
                for (int index = str.Length; index > 0; index--)
                    Bmsg = Bmsg + str[index - 1];

                return Bmsg;
            }
            else
                return "";
        }

        public static string Fix(this string str)
        {
            if (str != null)
                return str;
            else
                return "";

        }

        public static string Fix(this string str, int length)
        {
            if (str != null)
                return str.PadRight(length).Substring(0, length);
            else
                return "".PadRight(length);

        }

        public static string PadRightFix(this string str, int length, char pad)
        {
            if (str != null)
                return str.PadRight(length, pad).Substring(0, length);
            else
                return "".PadRight(length, pad);

        }

        public static string PadLeftFix(this string str, int length, char pad)
        {
            if (str != null)
                return str.PadLeft(length, pad).Substring(0, length);
            else
                return "".PadLeft(length, pad);

        }

        public static string PadLeftFix(this int val, int length)
        {
            return val.ToString().PadLeftFix(length,'0');
        }

        public static string PadRightFix(this int val, int length)
        {
            return val.ToString().PadRightFix(length,'0');
        }

        public static string Lasts(this string str, int index,char padLeftChar)
        {
            if (str != null)
            {
                str = str.PadLeft(index, padLeftChar);
                return str.Substring(str.Length - index, index);
            }
            else
                return "".PadLeft(index, padLeftChar);

        }

        public static byte[] Concat(this byte[] rhb, byte[] lhb)
        {
            return rhb.Concat<byte>(lhb).ToArray<byte>();
        }

        public static string GetValue(this Dictionary<int, string> dict, int key)
        {
            string answer = "";
            dict.TryGetValue(key, out answer);
            return answer;
        }

        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }
          
        public static string VisualHeb(this string text)
        {
            string result="";
            string hebstr = "";
            string engstr = "";
            char c;
            bool modeHeb  = false;
            bool hebstart = false;

            for( int i = 0 ; i < text.Length ;i++)
            {
                c = text[i];

                if (c >= 'א' && c <= 'ת')
                {
                    modeHeb = true;
                    if (i == 0) hebstart = true;
                }

                if (c >= 'a' && c <= 'z')
                    modeHeb = false;

                if (c >= 'A' && c <= 'Z')
                    modeHeb = false;

                if (c >= '0' && c <= '9')
                    modeHeb = false;

                if (c == ' ')
                    modeHeb = hebstart;
                    

                if (modeHeb)
                {
                    if (c == '>') c = '<';
                    else
                        if (c == '<') c = '>';

                    if (c == ']') c = '[';
                    else
                        if (c == '[') c = ']';

                    if (c == '(') c = ')';
                    else
                        if (c == ')') c = '(';

                    hebstr = c + hebstr ;

                    if (engstr != "")
                    {
                        if (hebstart)
                            result = engstr + result;
                        else
                            result = result + engstr;

                        engstr = "";
                    }
                }
                else
                {
                    engstr = engstr + c;

                    if (hebstr != "")
                    {
                        if (hebstart)
                            result = hebstr + result;
                        else
                            result = result + hebstr;

                        hebstr = "";
                    }
                }
            }

            if (hebstart)
                result = hebstr + engstr + result;
            else
                result = result + hebstr + engstr;

            return result;
        }

        public static string ToString(this string text)
        {
          if (text==null) 
              return "null";
            else
              return text.ToString();
        }

        public static string Replace(this string text,string str,int startIndex,int lengh)
        {
            string original = text;
            string source = str;

            if (text == null) text = "";
            if (source == null) source = "";

            original = original.PadRight(startIndex + lengh, ' ');
            source = source.PadRight(lengh, ' ');
            
            return original.Substring(0, startIndex) + source.Substring(0, lengh) + original.Substring(startIndex + lengh);
        }
     
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            string description = null;

            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            // we're only getting the first description we find
                            // others will be ignored
                            description = ((DescriptionAttribute)descriptionAttributes[0]).Description;
                        }

                        break;
                    }
                }
            }

            return description;
        }


        public static bool IsFullFileName(this string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return false;
            if (filename.Substring(0, 1) == "\\") return true;
            if (filename.Contains(':')) return true;            
            return false;
        }
    }
}
