using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public class EnumHelper
    {

        public static string GetDescription(Enum _enum)
        {
            Type type = _enum.GetType();
            MemberInfo[] info = type.GetMember(_enum.ToString());

            if (info != null && info.Length > 0)
            {
                object[] attributes = info[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                {
                    return ((DescriptionAttribute)attributes[0]).Description;
                }
            }

            return _enum.ToString();
        }

        public static string GetDescriptionByName(Type type, string name)
        {
            MemberInfo[] info = type.GetMember(name);

            if (info != null && info.Length > 0)
            {
                object[] attributes = info[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                {
                    return ((DescriptionAttribute)attributes[0]).Description;
                }
            }

            return name;
        }
    }
}
