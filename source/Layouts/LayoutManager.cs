using ChaosUtil.Reflection;
using ChaosUtil.Serialization.Text;
using System;
using System.Reflection;
using System.Xml;
using Culture = System.Globalization.CultureInfo;
using NumberStyles = System.Globalization.NumberStyles;
using SysCol = System.Collections.Generic;

namespace ChaosFramework.Input.Layouts
{
    public class LayoutManager
    {
        internal const string LAYOUT_RESOURCE_START = "HID_Layout_";
        public static readonly string MAPPED_USAGES_NAMESPACE;

        static LayoutManager()
        {
            string tmp = typeof(MappedUsages.GamePad).FullName;
            MAPPED_USAGES_NAMESPACE = tmp.Remove(tmp.Length - nameof(MappedUsages.GamePad).Length);
        }

        readonly SysCol.Dictionary<uint, Vendor> vendors = new SysCol.Dictionary<uint, Vendor>();
        readonly SysCol.Dictionary<uint, HidLayout> knownDeviceLayouts = new SysCol.Dictionary<uint, HidLayout>();
        readonly SysCol.Dictionary<string, HidLayout> knownLayouts = new SysCol.Dictionary<string, HidLayout>();

        public LayoutManager()
        {
            foreach (PropertyInfo resource in typeof(Properties.Resources).GetProperties(BindingFlags.NonPublic | BindingFlags.Static))
                if (resource.PropertyType == typeof(byte[]))
                    if (resource.Name.StartsWith(LAYOUT_RESOURCE_START))
                        using (System.IO.Stream str = new System.IO.MemoryStream((byte[])resource.GetValue(null)))
                            LoadLayout(resource.Name.Remove(0, LAYOUT_RESOURCE_START.Length), str);
        }

        public HidLayout GetLayout(string mappingName)
        {
            HidLayout mapping;
            if (knownLayouts.TryGetValue(mappingName, out mapping))
                return mapping;

            return null;
        }

        public HidLayout GetLayout(ushort vendorID, ushort productID)
            => GetLayout((uint)(vendorID << 16) | productID);

        public HidLayout GetLayout(uint productIdentifier)
        {
            HidLayout mapping;
            if (knownDeviceLayouts.TryGetValue(productIdentifier, out mapping))
                return mapping;

            return null;
        }

        public void LoadLayout(string layoutName, System.IO.Stream stream)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(stream);

            XmlNode rootNode = xml.SelectSingleNode("/Layout");
            if (rootNode != null)
            {
                XmlAttribute attrLayoutType = rootNode.Attributes["type"];
                if (attrLayoutType != null)
                {
                    Type enumType;
                    if (AssemblyManager.TryGetTypeByFullName(MAPPED_USAGES_NAMESPACE + attrLayoutType.Value, out enumType))
                    {
                        Delegate parser;
                        if (Parse.TryGetParser(enumType, out parser))
                        {
                            Type layoutType = typeof(HidLayout<>).MakeGenericType(enumType);
                            if (layoutType != null)
                            {
                                const string ADDER_NAME = nameof(HidLayout<MappedUsages.GamePad>.AddAxis);
                                MethodInfo adder = layoutType.GetMethod(ADDER_NAME, BindingFlags.NonPublic | BindingFlags.Instance);
                                if (adder != null)
                                {
                                    HidLayout layout = (HidLayout)System.Activator.CreateInstance(layoutType, new[] { layoutName });
                                    if (layout != null)
                                    {
                                        knownLayouts[layoutName] = layout;
                                        foreach (XmlNode axisNode in rootNode.SelectNodes("Axis"))
                                        {
                                            XmlAttribute attrHid = axisNode.Attributes["hidUsageID"];
                                            if (attrHid != null)
                                            {
                                                uint hidUsageID;
                                                if (uint.TryParse(
                                                    attrHid.Value.Replace(" ", string.Empty),
                                                    NumberStyles.HexNumber,
                                                    Culture.InvariantCulture,
                                                    out hidUsageID
                                                    ))
                                                {
                                                    XmlAttribute attrSub = axisNode.Attributes["subIndex"];
                                                    if (attrSub != null)
                                                    {
                                                        uint subIndex;
                                                        if (uint.TryParse(
                                                            attrSub.Value,
                                                            NumberStyles.HexNumber,
                                                            Culture.InvariantCulture,
                                                            out subIndex
                                                            ))
                                                        {
                                                            XmlAttribute attrUsage = axisNode.Attributes["usage"];
                                                            if (attrUsage != null)
                                                            {
                                                                object[] parserArgs = { attrUsage.Value, null };
                                                                if ((bool)parser.DynamicInvoke(parserArgs))
                                                                {
                                                                    adder.Invoke(layout, new[] { parserArgs[1], hidUsageID, subIndex });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LoadUsages(System.IO.Stream stream)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(stream);

            XmlNode rootNode = xml.SelectSingleNode("KnownDevices");
            if (rootNode != null)
            {
                foreach (XmlNode vendorNode in rootNode.SelectNodes("Vendor"))
                {
                    XmlAttribute attrVendorId = vendorNode.Attributes["id"];
                    if (attrVendorId != null)
                    {
                        ushort vendorId;
                        if (ushort.TryParse(attrVendorId.Value, NumberStyles.HexNumber, Culture.InvariantCulture, out vendorId))
                        {
                            string vendorName = null;
                            XmlAttribute attrVendorName = vendorNode.Attributes["name"];
                            if (attrVendorName != null)
                                vendorName = attrVendorName.Value;

                            Vendor vendor;
                            if (vendors.TryGetValue(vendorId, out vendor))
                            {
                                if (vendorName != null)
                                    vendor.AddName(vendorName);
                            }
                            else
                                vendors[vendorId] = vendor = new Vendor(vendorId, vendorName);

                            foreach (XmlNode productNode in vendorNode.SelectNodes("Device"))
                            {
                                XmlAttribute attrProductId = productNode.Attributes["id"];
                                if (attrProductId != null)
                                {
                                    ushort productId;
                                    if (ushort.TryParse(
                                        attrProductId.Value,
                                        NumberStyles.HexNumber,
                                        Culture.InvariantCulture,
                                        out productId
                                        ))
                                    {
                                        XmlAttribute attrLayout = productNode.Attributes["layout"];
                                        if (attrLayout != null)
                                        {
                                            HidLayout layout;
                                            if (knownLayouts.TryGetValue(attrLayout.Value, out layout))
                                            {
                                                string productName = null;
                                                XmlAttribute attrProductName = productNode.Attributes["name"];
                                                if (attrProductName != null)
                                                    productName = attrProductName.Value;

                                                Product product;
                                                if (!vendor.products.TryGetValue(productId, out product))
                                                    vendor.products[productId] = product = new Product(vendor, productId, productName);

                                                knownDeviceLayouts[product.fullId] = layout;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
