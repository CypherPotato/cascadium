# Cascadium

Cascadium is a lightweight CSS preprocessor for CSS.

This small project can compile CSS with superpowers into a flat CSS file that is more compatible with a larger number of browsers. The project was written in C# and can run on any operating system without the installation of .NET.

Cascadium, unlike other preprocessors, tends to be an extension of CSS and not another markup or programming language. It has some developer-specific features, but all of them are focused on still being "CSS."

Main features:

- Convert nested CSS into flat CSS
- Single-line comments
- Minification, compression, and merging of CSS files
- Custom property converters
- Rewrite media queries

##  1. <a name='TableofContents'></a>Table of Contents

<!-- vscode-markdown-toc -->
* 1. [Table of Contents](#TableofContents)
* 2. [Getting Started](#GettingStarted)
* 3. [Introduction](#Introduction)
	* 3.1. [Syntax](#Syntax)
* 4. [Compiler Settings](#CompilerSettings)
	* 4.1. [KeepNestingSpace](#KeepNestingSpace)
	* 4.2. [Pretty](#Pretty)
	* 4.3. [UseVarShortcut](#UseVarShortcut)
	* 4.4. [Merge](#Merge)
* 5. [CLI Settings](#CLISettings)
	* 5.1. [Configuration File](#ConfigurationFile)
	* 5.2. [Watch](#Watch)
	* 5.3. [InputDirectories](#InputDirectories)
	* 5.4. [InputFiles](#InputFiles)
	* 5.5. [Exclude](#Exclude)
	* 5.6. [Extensions](#Extensions)
	* 5.7. [OutputFile](#OutputFile)
	* 5.8. [FilenameTag](#FilenameTag)
	* 5.9. [AtRulesRewrites](#AtRulesRewrites)
* 6. [Converters](#Converters)

<!-- vscode-markdown-toc-config
	numbering=true
	autoSave=true
	/vscode-markdown-toc-config -->
<!-- /vscode-markdown-toc -->

##  2. <a name='GettingStarted'></a>Getting Started

You can use the library in your C# project or use the command-line tool compatible with any type of project.

To use the library in your code, you can start by adding the reference to Cascadium:

```csharp
dotnet add package Cascadium.Compiler
```

And use it as in the example below:

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

# Documentation

##  3. <a name='Introduction'></a>Introduction

The goal of **Cascadium** is to be as faithful to the original CSS as possible. Therefore, you will not find a similar experience to what SASS or LESS offers: there will be no preprocessor variables, no mixins, loops, etc. Cascadium should be seen as an extension of CSS, not as a language like SASS or LESS.

Creating entire projects with SCSS is possible, with pure CSS as well, so it will be with Cascadium too. The main advantage of Cascadium is being flexible enough to be integrated into any type of project, as its generator can produce CSS files equivalent to the typed Cascadium code.

Currently, Cascadium can be used in two ways:

- CLI: through the Cascadium tool binaries, which allows usage in any project of any programming language.
- .NET Package: the .NET library of Cascadium, which allows the use of the compiler in a managed development environment.

###  3.1. <a name='Syntax'></a>Syntax

As mentioned, Cascadium tries to maintain a syntax as faithful as possible to the original CSS. The following code is written in Cascadium, and the compiled result is displayed below:

```scss
.card {
    width: fit-content;
    padding: 16px;
    border: 1px solid black;

    > .card-title {
        font-weight: 500;
    }

    > .card-body {
        border-top: 1px solid gainsboro;
    }

    // mobile
    @media (min-width: 768px) {
        width: 100%;
    }
}
```

It compiles to the following CSS:

```css
.card {
    width: fit-content;
    padding: 16px;
    border: 1px solid black;
}

.card > .card-title {
    font-weight: 500;
}

.card > .card-body {
    border-top: 1px solid gainsboro;
}

@media (min-width: 768px) {
    .card {
        width: 100%;
    }
}
```

The example above addresses the main use of Cascadium: nesting. You can intersperse selectors with the `&` operator before the selector's content:

```scss
// Cascadium
div {
    color: red;

    & .blue-div {
        color: blue;
    }
}

// CSS
div {
    color: red;
}

div.blue-div {
    color: blue;
}
```

##  4. <a name='CompilerSettings'></a>Compiler Settings

###  4.1. <a name='KeepNestingSpace'></a>KeepNestingSpace

Syntax:

- CLI:
    ```
    --p:keepnestingspace <true|false>
    ```

- JSON:
    ```json
    {
        "KeepNestingSpace": true
    }
    ```

The space between the `&` and the selector's content is optional in Cascadium. In other preprocessors, this space is concatenated directly with the result of the parent selector. This behavior can be disabled by setting `KeepNestingSpace` to `false`. See the example:

```scss
div {
    color: red;

    & .blue-div {
        color: blue;
    }
}

// KeepNestingSpace = false (default)
div.blue-div {
    color: blue;
}

// KeepNestingSpace = true
div .blue-div {
    color: blue;
}
```

----

----

###  4.2. <a name='Pretty'></a>Pretty

Syntax:

- CLI:
    ```
    --p:pretty <true|false>
    ```

- JSON:
    ```json
    {
        "Pretty": true
    }
    ```

This determines whether the compilation result should be formatted and pleasant ("pretty") or minified.

----

###  4.3. <a name='UseVarShortcut'></a>UseVarShortcut

Syntax:

- CLI:
    ```
    --p:usevarshortcuts <true|false>
    ```

- JSON:
    ```json
    {
        "UseVarShortcut": true
    }
    ```

> Note that there is a typo in these properties. In the CLI, it ends with an **s** at the end. In JSON, it does not.

This property allows you to enable or disable the variable shortcut. This property converts the following code:

```scss
div {
    color: --primary-color;
    background url(--url);
}
```

Into:

```css
div {
    color: var(--primary-color);
    background url(var(--url));
}
```

----

###  4.4. <a name='Merge'></a>Merge

Syntax:

- CLI:
    ```
    --p:merge <none|selectors|atrules|declarations|all>
    ```

This property performs a merge compilation on the compilation result. Merging is an additional and optional step that can help reduce the size of the final CSS output when there is a lot of code repetition.

An unmerged code:

```scss
// duplicate declarations
div {
    color: red;
    color: blue;
}

// identical properties, but with
// different selectors
span {
    color: red;
}
p {
    color: red;
}

// duplicate at-rules
@page {
    body {
        size: A4;
    }
}
@page {
    body {
        size: auto;
    }
}
```

Is merged into:

```css
div {
    color: blue;
}

span, p {
    color: red;
}

@page {
    body {
        size: auto;
    }
}
```

This feature is experimental, and the merged result may not be equivalent to the normal compilation result. Use this property with caution.

##  5. <a name='CLISettings'></a>CLI Settings

###  5.1. <a name='ConfigurationFile'></a>Configuration File

Cascadium, through the CLI tool, looks for a configuration file in the current directory where it is being executed. The sought file must be named `cascadium.json`, `cascadium.json5`, `cssconfig.json`, or `cssconfig.json5`.

Note that [JSON5](https://json5.org/) is supported by this tool. Plain JSON is also supported. The JSON properties are defined in the format:

```json
{
    "pretty": false,
    "inputDirectories": [
        "./style"
    ],
    "outputFile": "./dist/output.css"
}
```

See the compiler settings section for more information about properties for the compiler. All properties are case-insensitive. In addition to these, there are properties exclusive to the CLI tool:

You can also start the CLI tool and point to a configuration file path with the `--config` option:

```
cascadium -c myconfig.json
cascadium --config myconfig.json
```

----

###  5.2. <a name='Watch'></a>Watch

The Cascadium compiler has a function to monitor all compilation files in the current directory. You can start the watch by adding the `watch` option to the first command of the tool:

```
cascadium watch
```

First, it will obtain the shortest path of all entries specified, either by the CLI or by the configuration file (if any), and will monitor all `.xcss` files and additional extensions with `-x` or from the configuration file.

For every modified, deleted, or created file, the compiler will recompile only the affected files and not those already compiled. The output of the compiler is specified in `OutputFile`.

It is not possible to use `Merge` with `Watch`.

----

###  5.3. <a name='InputDirectories'></a>InputDirectories

Syntax:

- CLI:
    ```
    -d ./styles ./css
    --dir .
    ```

- JSON:
    ```json
    {
        "InputDirectories": [
            "./css",
            "./vendor"
        ]
    }
    ```

Specifies one or more input folders for the compiler. Cascadium will search for CSS files in the specified directories in a recursive search (it will look into the subfolders of the directories). By default, it will look for all files with the `.xcss` extension in the directory, but you can specify other extensions in the `extensions` property.

The directories are always relative to the configuration file's directory. Absolute paths are also allowed.

The Cascadium CLI compiler organizes the compilation priority by the largest number of "components" in a path. For example, files found in `/css` are compiled before files found in `/css/components/button`.

The resulting file (`outputFile`) is never recompiled.

----

###  5.4. <a name='InputFiles'></a>InputFiles

Syntax:

- CLI:
    ```
    -f file1.css file2.css file3.css [...]
    --file file.css [...]
    ```

- JSON:
    ```json
    {
        "InputFiles": [
            "./css/some-file.xcss",
            "./css/globals.css"
        ]
    }
    ```

Specifies one or more input files for the compiler. This property includes any file, with the full or relative path to the configuration file, regardless of the file extension.

The files specified here are always compiled before `InputDirectories`.

----

###  5.5. <a name='Exclude'></a>Exclude

Syntax:

- CLI:
    ```
    -e node_modules
    --exclude vendor
    ```

- JSON:
    ```json
    {
        "ExcludePatterns": [
            "[\\\\/]\\.", // matches . at the beginning
            "node_modules"
        ]
    }
    ```

This property excludes files from the compiler. Each string is a case-insensitive regular expression that is applied to the absolute path of each file that will be compiled.

----

###  5.6. <a name='Extensions'></a>Extensions

Syntax:

- CLI:
    ```
    -x .css .scss
    --extension .less
    ```

- JSON:
    ```json
    {
        "Extensions": [
            ".css",
            ".scss"
        ]
    }
    ```

This property specifies one or more extensions that the compiler should look for when using `InputDirectories`. The `.xcss` extension is always included by default.

The `Exclude` configuration still tests the extensions included by this property.

----

###  5.7. <a name='OutputFile'></a>OutputFile

Syntax:

- CLI:
    ```
    -o app.scs
    --outfile app.css
    ```

- JSON:
    ```json
    {
        "OutputFile": "./dist/app.css"
    }
    ```

Specifies the output file for the compiler. This file concatenates all compiled files, following the compilation order explained in `InputDirectories`.

If this parameter is not specified, the compiler will write the output to standard output (stdout).

----

###  5.8. <a name='FilenameTag'></a>FilenameTag

Syntax:

- CLI:
    ```
    --p:filenametag <full|relative>
    ```

- JSON:
    ```json
    {
        "FilenameTag": "full" | "relative" | "none"
    }
    ```

This option inserts a comment at the beginning of each part of the generated output file containing the name of the original file from which that section was generated. This function is useful for debugging and for identifying the file that generated each part of the output file.

This property is not compatible with `Merge` and provides better results when used with `Pretty = true`.

----

###  5.9. <a name='AtRulesRewrites'></a>AtRulesRewrites

Syntax:

- JSON:
    ```json
    {
        "AtRulesRewrites": {
            "media tablet": "media only screen and (max-width: 1100px)",
            "media mobile": "media only screen and (max-width: 700px)"
        }
    }
    ```

This property allows you to replace the name of an at-rule with another. For example:

```scss
div {
    color: red;

    @media mobile {
        color: blue;
    }
}
```

It compiles to:

```css
div {
    color: red;
}

@media only screen and (max-width: 700px) {
    div {
        color: blue;
    }
}
```

##  6. <a name='Converters'></a>Converters

Converters are tools that allow the conversion of properties and values to emit other CSS. This feature works best in a managed .NET environment but can also be partially used with the CLI.

Note that using converters will make your project even more dependent on Cascadium.

The example below compiles the property `$size` to emit two properties: `width` and `height`:

```csharp
static void Main(string[] args)
{
    string xcss = """
        div {
            $size: 10px 20px;
        }
        """;

    var stylesheet = CascadiumCompiler.Parse(xcss, new CascadiumOptions()
    {
        Pretty = true,
        Converters = [new CssSizeConverter()]
    });
    var css = stylesheet.Export();

    Console.WriteLine(css);
}

public class CssSizeConverter : CSSConverter
{
    public override bool CanConvert(string propertyName, string value)
    {
        return propertyName == "$size";
    }

    public override void Convert(string? value, NameValueCollection outputDeclarations)
    {
        // use SafeSplit to split a CSS value without
        // cutting strings and CSS expressions
        string[] values = SafeSplit(value);

        if (values.Length == 1)
        {
            outputDeclarations.Add("width", values[0]);
            outputDeclarations.Add("height", values[0]);
        }
        else if (values.Length >= 2)
        {
            outputDeclarations.Add("width", values[0]);
            outputDeclarations.Add("height", values[1]);
        }
    }
}
```

The compilation result is:

```css
div {
    width: 10px;
    height: 20px;
}
```

You can also define converters in the JSON configuration file:

```json
{
    "Converters": [
        {
            "MatchProperty": "$size",
            "ArgumentCount": 1,
            "Output": {
                "width": "$1"
            }
        },
        {
            "MatchProperty": "$size",
            "ArgumentCount": 2,
            "Output": {
                "width": "$1",
                "height": "$2"
            }
        },
        {
            "MatchProperty": "border-radius",
            "Output": {
                "-webkit-border-radius": "$*",
                "-moz-border-radius": "$*",
                "-ms-border-radius": "$*",
                "border-radius": "$*"
            }
        }
    ]
}
```