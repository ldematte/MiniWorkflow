using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWorkflow
{
    public static class Collections
    {
        public static V GetOrElseUpdate<K, V>(this Dictionary<K, V> self, K key, Func<V> create)
        {
            V val;
            if (!self.TryGetValue(key, out val))
            {
                val = create();
                self.Add(key, val);
            }
            return val;
        }
    }
}
