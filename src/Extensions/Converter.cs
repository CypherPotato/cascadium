using Cascadium.Converters;
using Cascadium.Entity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascadium.Extensions;

internal class Converter
{
    public static void ConvertAll(CssStylesheet css, CascadiumOptions options)
    {
        foreach (var rule in css._rules)
        {
            foreach (KeyValuePair<string, string> declaration in rule._declarations.ToArray())
            {
                foreach (CSSConverter converter in options.Converters)
                {
                    if (converter.CanConvert(declaration.Key, declaration.Value))
                    {
                        NameValueCollection output = new NameValueCollection();
                        converter.Convert(declaration.Value, output);
                        rule._declarations.Remove(declaration.Key);

                        foreach (string nprop in output)
                        {
                            string? value = output[nprop];
                            if (string.IsNullOrEmpty(value)) continue;
                            rule._declarations[nprop] = value;
                        }
                    }
                }
            }
        }
        foreach (var subcss in css._stylesheets)
        {
            ConvertAll(subcss, options);
        }
    }
}
