using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;

namespace VocabT
{
    static class WpfCopyExtensions
    {
        public static T Copy<T> (this T control) where T: Control
        {
            string xaml = XamlWriter.Save(control);
            StringReader stringReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            return (T) XamlReader.Load(xmlReader);
        }
    }
}
