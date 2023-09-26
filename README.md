<p align="center">
  <img width="100%" height="auto" src="./.github/banner.png">
</p>

------

This small module can compile CSS with the following features into a legacy CSS file that is more compatible with most browsers.

It can compile CSS with **nesting** and **single-line comments** into legacy CSS code, also minifies it.

It can compile:

```scss
@import url("https://foo.bar/");

div {
    display: block;
    width: 400px; // it does support comments
    /* native multi-line comments too */

    height: 300px;

    > a {
        align-self: center;

        & :hover {
            color: red;
        }
    }

    & .test {
        font-size: 20px;
    }
}

@media (max-width: 700px) {
    div.test {
        width: 100%;
    }
}
```

Into:

```css
@import url("https://foo.bar/");

div {
    display: block;
    width: 400px;
    height: 300px
}

div.test {
    font-size: 20px
}

div > a {
    align-self: center
}

div > a:hover {
    color: red
}

@media (max-width: 700px) {
    div.test {
        width: 100%
    }
}
```

Only with:

```c#
string css = SimpleCSS.SimpleCSSCompiler.Compile(prettyCss);
```

It's all you need.

## Specification

The syntax is very similar to what other CSS preprocessors already deliver, the difference is that in the concatenation operator of the "&" selector, the space between the operator and the selector is ignored, and is immediately included next to the parent selector, example:

```scss
div {
    & :hover {
        color: blue;
    }
}
```

Will translate to:

```css
div:hover {
    color: blue
}
```

If you want an space between `div` and `:hover`, just remove the `&` symbol before the selector.

Also, it propagates the selectors when they are separate to the same rule, for example:

```scss
div,
span {
    & :hover,
    & :active {
        color: red;
    }
}
```

Will translate to:

```css
div:hover,
span:hover,
div:active,
span:active {
    color: red
}
```

The compiler doesn't know what you're typing, it compiles based on the tokens you type. Using the `@` operator will automatically start a new style sheet inside the body, and concatenate to the parent style:

```scss
div {
    color: red;
}

@blablabla {
    div {
        color: blue;

        & .yellow {
            color: yellow;
        }
    }
}
```

Compiles to:

```css
div {
    color: red
}

@blablabla {
    div {
        color: blue
    }

    div.yellow {
        color: yellow
    }
}
```

## Additional options



## Converters

You can define custom converters which converts properties and values into new ones.

Example:

```cs
using SimpleCSS;

static void Main(string[] args)
{
    string css = """
        div {
            size: 100px 400px;
        }
        """;

    string pretty = SimpleCSSCompiler.Compile(css, new SimpleCSS.CSSCompilerOptions()
    {
        Converters = new()
        {
            new CssSizeConverter()
        }
    });

    Console.WriteLine(pretty);
}

public class CssSizeConverter : CSSConverter
{
    public override bool CanConvert(string propertyName, string value)
    {
        // determines if the property should be converted
        return propertyName == "size";
    }

    public override void Convert(string? value, NameValueCollection outputDeclarations)
    {
        // get values and remove the default value
        string[] values = this.SafeSplit(value);
        value = null;

        // output the new values
        outputDeclarations.Set("width", values[0]);
        outputDeclarations.Set("height", values[1]);
    }
}
```

And get the converted output:

```css
div {
    width: 100px;
    height: 400px;
}
```

> Note: don't use `string.Split()` to split your values. Use the base `base.SafeSplit()` to split values into expressions, which it supports splitting string and expression literals.

## Considerations

- Nesting `@` blocks is not supported. Nesting inside `@` blocks is supported.
- Properties and values have their values trimmed. Empty values are not included in the output.
- The last `;` in the rule is removed. (except on pretty print mode)