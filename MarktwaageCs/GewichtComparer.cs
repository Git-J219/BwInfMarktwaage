using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarktwaageCs
{
    class GewichtComparer : IComparer<long>
    {
        public int Compare(long x, long y)
        {
            if (x == 0 && y == 0) return 0;
            if (x == 0) return -1;
            if (y == 0) return 1;
            if (y > x) return -1;
            if (x > y) return 1;
            return 0; //x == y
        }
    }
}
