using ChaosFramework.Collections;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input.Layouts
{
    public class Vendor
    {
        public readonly ushort vendorId;
        internal readonly SysCol.Dictionary<ushort, Product> products = new SysCol.Dictionary<ushort, Product>();
        readonly LinkedList<string> names;

        public Vendor(ushort vendorId, string name)
        {
            this.vendorId = vendorId;
            names = new LinkedList<string>(name);
        }

        public SysCol.IEnumerable<string> GetNames()
        {
            foreach (string name in names)
                yield return name;
        }

        public SysCol.IEnumerable<Product> GetProducts()
        {
            foreach (Product product in products.Values)
                yield return product;
        }

        public void AddName(string name) => names.Add(name);

        public override string ToString() => $"{nameof(Vendor)}{names.MakeString()}";
    }
}
