using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class BaseEntity
    {

        public IEnumerable<int> StringToIntList(string data)
        {
            if (!String.IsNullOrEmpty(data))
            {
                var splited = data.Split(',');
                return splited.Select(s => int.Parse(s));
            }
            return new List<int>();
        }

    }
}
