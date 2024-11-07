# Cascadium

Cascadium is an lightweight pre-processor for the CSS styling language.

This small module can compile CSS with the following features into a legacy CSS file that is more compatible with most browsers. It is written in C#, built to run in any operating system without .NET installed. Also, it's source code is open-source.

Cascadium, unlike other CSS preprocessors, tends to be an extension of the CSS language and not another language. It has some developer-specific quirks, but all of them are aimed at still being "CSS".

Main features:

- Convert nested CSS into plain CSS
- Single line comments
- Minify, compress and merge CSS files
- Custom property converters
- Media query rewriters

## Getting started

You can use the library in your C# project or use the cross-platform tool compatible with absolutely any type of project.

To use the library in your code, you can start by adding the reference to Cascadium:

```
dotnet add package Cascadium.Compiler
```

And use as the example below:

```csharp
static void Main(string[] args)
{
    string xcss = """
        div {
            color: red;

            > span {
                color: blue;
                font-weight: 500;
            }
        }
        """;

    var stylesheet = CascadiumCompiler.Parse(xcss);
    var css = stylesheet.Export();

    Console.WriteLine(css);
}
```

And get the result:

```css
div{color:red}div>span{color:blue;font-weight:500}
```