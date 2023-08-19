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

You can specify compilation options under the `CSSCompilerOptions` parameter, which:

- `bool Pretty` produces an pretty, formatted and indented output.
- `bool UseVarShortcut` specifies if the compiler should rewrite `--variable` to `var(--variable)` in the value context, example:

    ```css
    :root {
        --red: #FF1100;        
    }

    div {
        color: --red;      
    }
    ```

    Should be rewrited to:

    ```css
    div {
        color: var(--red);
    }
    ```

    And it will NOT rewrite if the variable is inside an string:

    ```scss
    div {
        color: --red;
        background: url('--red');
    }

    // becomes:
    div {
        color: var(--red);
        background: url('--red');
    }
    ```

## Considerations

- Nesting `@` blocks is not supported.
- Properties and values is trimmed.
- The last `;` in the rule is removed.