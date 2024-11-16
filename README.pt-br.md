# Cascadium

Cascadium é um pré-processador de CSS leve para CSS.

Esse pequeno projeto consegue compilar CSS com super poderes para um arquivo CSS plano que é mais compatível com um maior número de navegadores. O projeto foi escrito em C# e consegue ser executado em qualquer sistema operacional sem a instalação do .NET.

O Cascadium, ao contrário de outros pré-processadores, tende a ser uma extensão do CSS e não outra linguagem de marcação ou programação. Ele tem algumas características específicas do desenvolvedor, mas todas elas são focadas em ainda ser "CSS".

Principais recursos:

- Converter CSS aninhado em CSS plano
- Comentários de linha única
- Minificação, compressão e mesclagem de arquivos CSS
- Conversores de propriedades personalizados
- Reescrever queries de mídia

##  1. <a name='Tabeladecontedos'></a>Tabela de conteúdos

<!-- vscode-markdown-toc -->
* 1. [Tabela de conteúdos](#Tabeladecontedos)
* 2. [Primeiros passos](#Primeirospassos)
* 3. [Introdução](#Introduo)
	* 3.1. [Sintaxe](#Sintaxe)
* 4. [Configurações do compilador](#Configuraesdocompilador)
	* 4.1. [KeepNestingSpace](#KeepNestingSpace)
	* 4.2. [Pretty](#Pretty)
	* 4.3. [UseVarShortcut](#UseVarShortcut)
	* 4.4. [Merge](#Merge)
* 5. [Configurações do CLI](#ConfiguraesdoCLI)
	* 5.1. [Arquivo de configuração](#Arquivodeconfigurao)
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

##  2. <a name='Primeirospassos'></a>Primeiros passos

Você pode usar a biblioteca em seu projeto C# ou usar a ferramenta de linha de comando compatível com qualquer tipo de projeto.

Para usar a biblioteca em seu código, você pode começar adicionando a referência para o Cascadium:

```csharp
dotnet add package Cascadium.Compiler
```

E usar como o exemplo abaixo:

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

E obter o resultado:

```css
div{color:red}div>span{color:blue;font-weight:500}
```

# Documentação

##  3. <a name='Introduo'></a>Introdução

O objetivo do **Cascadium** é ser o mais fiel ao CSS original. Portanto, você não irá encontrar uma experiência similar com o que é o SASS ou LESS: não terá variáveis de pré-processador, não terá mixins, loops, etc. Cascadium deve ser visto como uma extensão do CSS, e não como uma linguagem como SASS ou LESS.

Criar projetos inteiros com SCSS é possível, com CSS puro também, então com Cascadium também será. A principal vantagem do Cascadium é ser flexível o suficiente para ser integrado à qualquer tipo de projeto, pois seu gerador consegue gerar arquivos CSS equivalentes ao código Cascadium digitado.

Atualmente, o Cascadium pode ser usado por duas formas:

- CLI: através dos binários de ferramenta do Cascadium, que possibilita o uso em qualquer projeto de qualquer linguagem de programação.
- Pacote .NET: a biblioteca do .NET do Cascadium, o que possibilita o uso do compilador em um ambiente gerenciado de desenvolvimento.

###  3.1. <a name='Sintaxe'></a>Sintaxe

Assim como mencionado, o Cascadium tenta manter uma sintaxe mais fiel possível ao CSS original. O código a seguir é escrito em Cascadium e o resultado compilado é exibido em seguida:

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

É compilado para o seguinte CSS:

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

O exemplo acima aborda o principal uso do Cascadium: nesting. Você pode intercalar seletores com o operador `&` antes do conteúdo do seletor:

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

##  4. <a name='Configuraesdocompilador'></a>Configurações do compilador

###  4.1. <a name='KeepNestingSpace'></a>KeepNestingSpace

Sintaxe:

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

O espaço entre o `&` e o conteúdo do seletor é opcional no Cascadium. Em outros pré-processadores, esse espaço é concatenado diretamente com o resultado do seletor pai. Esse comportamento pode ser desativado com `KeepNestingSpace` para `false`. Veja o exemplo:

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

###  4.2. <a name='Pretty'></a>Pretty

Sintaxe:

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

Implica se o resultado da compilação deve ser formatado e agradável ("pretty") ou minificado.

----

###  4.3. <a name='UseVarShortcut'></a>UseVarShortcut

Sintaxe:

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

> Note que existe um erro de digitação nessas propriedades. No CLI, ela termina com um **s** no final. No JSON, não.

Essa propriedade permite ativar o atalho de variáveis ou não. Essa propriedade converte o seguinte código:

```scss
div {
    color: --primary-color;
    background url(--url);
}
```

Em:

```css
div {
    color: var(--primary-color);
    background url(var(--url));
}
```

----

###  4.4. <a name='Merge'></a>Merge

Sintaxe:

- CLI:
    ```
    --p:merge <none|selectors|atrules|declarations|all>
    ```

Essa propriedade executa uma compilação de mesclagem no resultado da compilação. A mesclagem é uma etapa adicional e opcional que pode ajudar a reduzir o tamanho do resultado de saída do CSS final quando existem muita repetição de código.

Um código não mesclado:

```scss
// declarações duplicadas
div {
    color: red;
    color: blue;
}

// propriedades idênticas, mas com
// seletores diferentes
span {
    color: red;
}
p {
    color: red;
}

// at-rules duplicadas
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

É mesclado para:

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

Esse recurso é experimental e é possível que o resultado mesclado não seja equivalente ao resultado de compilação normal. Use com cautela essa propriedade.

##  5. <a name='ConfiguraesdoCLI'></a>Configurações do CLI

###  5.1. <a name='Arquivodeconfigurao'></a>Arquivo de configuração

O Cascadium, através da ferramenta de CLI, busca por um arquivo de configuração no diretório atual onde está sendo executado. Esse arquivo procurado deve ter o nome de `cascadium.json`, `cascadium.json5`, `cssconfig.json` ou `cssconfig.json5`.

Note que, [JSON5](https://json5.org/) é suportado por essa ferramenta. JSON plano também. As propriedades JSON são definidas no formato:

```json
{
    "pretty": false,
    "inputDirectories": [
        "./style"
    ],
    "outputFile": "./dist/output.css",
}
```

Veja na sessão de configurações do compilador para mais informações sobre propriedades para o compilador. Todas as propriedades tem um nome insensível à caso. Além dessas, existem propriedades exclusivas da ferramenta de CLI:

Você também pode iniciar a ferramenta de CLI e apontar o caminho para um arquivo de configuração com a opção `--config`:

```
cascadium -c myconfig.json
cascadium --config myconfig.json
```

----

###  5.2. <a name='Watch'></a>Watch

O compilador Cascadium possui uma função de monitorar por todos os arquivos de compilação no diretório atual. Você pode iniciar o watch adicionando a opção `watch` no primeiro comando da ferramenta:

```
cascadium watch
```

Primeiramente, ele irá obter o menor caminho de todas as entradas que forem especificadas, seja pelo CLI ou pelo arquivo de configuração (se houver), e irá monitorar todos os arquivos `.xcss` e extensões adicionais com `-x` ou do arquivo de configurações.

Para todo arquivo modificado, excluído ou criado, o compilador irá recompilar somente os arquivos afetados e não os já compilados. A saída do compilador é especificada em `OutputFile`.

Não é possível usar `Merge` com o `Watch`.

----

###  5.3. <a name='InputDirectories'></a>InputDirectories

Sintaxe:

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
            "./vendor
        ]
    }
    ```

Especifica uma ou mais pastas de entrada para o compilador. Cascadium irá buscar arquivos CSS nos diretórios especificados em uma busca recursiva (irá olhar as sub-pastas dos diretórios). Por padrão, irá buscar todos os arquivos na extensão `.xcss` que estejam no diretório, mas você consegue especificar outras extensões na propriedade `extensions`.

Os diretórios sempre são relativos ao diretório do arquivo de configuração. Caminhos absolutos também são permitidos.

O compilador CLI do Cascadium organiza a prioridade de compilação pela maior quantia de "componentes" em um caminho. Por exemplo: arquivos encontrados em `/css` são compilados antes que os arquivos encontrados em `/css/components/button`.

O arquivo resultado (`outputFile`) nunca é recompilado.

----

###  5.4. <a name='InputFiles'></a>InputFiles

Sintaxe:

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

Especifica um ou mais arquivos de entrada para o compilador. Essa propriedade inclui qualquer arquivo, com o caminho completo ou relativo ao arquivo de configuração, indiferente da extensão do arquivo.

Os arquivos especificados aqui sempre são compilados antes que `InputDirectories`.

----

###  5.5. <a name='Exclude'></a>Exclude

Sintaxe:

- CLI:
    ```
    -e node_modules
    --exclude vendor
    ```

- JSON:
    ```json
    {
        "ExcludePatterns": [
            "[\\\\/]\\.", // matches . on begin
            "node_modules"
        ]
    }
    ```

Essa propriedade exclui arquivos do compilador. Cada string é uma expressão regular com ignore-case que é aplicada no caminho absoluto para cada arquivo que será compilado.

----

###  5.6. <a name='Extensions'></a>Extensions

Sintaxe:

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

Essa propriedade especifica uma ou mais extensões que o compilador deve buscar ao usar com `InputDirectories`. A extensão `.xcss` é sempre incluída por padrão.

A configuração `Exclude` ainda testa as extensões incluídas por essa propriedade.

----

###  5.7. <a name='OutputFile'></a>OutputFile

Sintaxe:

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

Especifica o arquivo de saída do compilador. Esse arquivo concatena todos os arquivos compilados, seguindo a ordem de compilação explicada em `InputDirectories`.

Se este parâmetro não for especificado, o compilador irá escrever a saída na saída padrão (stdout).

----

###  5.8. <a name='FilenameTag'></a>FilenameTag

Sintaxe:

- CLI:
    ```
    --p:filenametag <full|relative>
    ```

- JSON:
    ```json
    {
        "FilenameTag": "full" | "relative"
    }
    ```

Essa opção insere um comentário no início de cada parte do arquivo de saída gerado contendo o nome do arquivo original de onde aquele trecho foi gerado. Essa função é útil para depuração e para identificar o arquivo que gerou cada parte do arquivo de saída.

Essa propriedade não é compatível com `Merge` e fornece resultados melhores quando usado com `Pretty = true`.

----

###  5.9. <a name='AtRulesRewrites'></a>AtRulesRewrites

Sintaxe:

- JSON:
    ```json
    {
        "AtRulesRewrites": {
            "media tablet": "media only screen and (max-width: 1100px)",
            "media mobile": "media only screen and (max-width: 700px)"
        }
    }
    ```

Essa propriedade permite substituir o nome de um at-rule por outro. Exemplo:

```scss
div {
    color: red;

    @media mobile {
        color: blue;
    }
}
```

É compilado para:

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

Conversores são ferramentas que permitem a conversão de propriedades e valores para emitir outro CSS. Esse recurso funciona melhor em um ambiente gerenciado do .NET, mas também pode ser parcialmente usado com CLI.

Note que, ao usar conversores, seu projeto ficará ainda mais dependente do Cascadium.

O exemplo abaixo compila a propriedade `$size` para emitir duas propriedades: `width` e `height`:

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
        // use o SafeSplit para dividir um valor CSS sem
        // recortar strings e expressões CSS
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

O resultado da compilação é:

```css
div {
    width: 10px;
    height: 20px;
}
```

Você também pode definir converters no arquivo de configuração JSON:

```json
{
    "Converters": [
        {
            "MatchProperty": "$size",
            "ArgumentCount": 1,
            "Output": {
                "width": "$1",
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