using ChaosFramework.Collections;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input.Layouts
{
    public class Product
    {
        public readonly Vendor vendor;
        public readonly ushort productId;
        public readonly uint fullId;
        readonly LinkedList<string> names;

        public Product(Vendor vendor, ushort productId, string name)
        {
            this.vendor = vendor;
            this.productId = productId;
            names = new LinkedList<string>(name);
            fullId = (uint)(vendor.vendorId << 16) | this.productId;
        }

        public SysCol.IEnumerable<string> GetNames()
        {
            foreach (string name in names)
                yield return name;
        }

        public void AddName(string name) => names.Add(name);

        public override string ToString() => $"{nameof(Product)}{names.MakeString()}";
    }
}
