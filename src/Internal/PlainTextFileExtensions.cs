﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PlainTextFileExtensions.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Internal;

static class PlainTextFileExtensions {
    public static bool IsTextFile ( string extension ) {
        for (int i = 0; i < PlainTextExtensions.Length; i++) {
            if (string.Equals ( PlainTextExtensions [ i ], extension, StringComparison.OrdinalIgnoreCase )) {
                return true;
            }
        }
        return false;
    }

    // list forked from https://github.com/sindresorhus/text-extensions/blob/main/text-extensions.json
    // with some personal additions
    static readonly string [] PlainTextExtensions = [
        "ada",
        "adb",
        "ads",
        "applescript",
        "as",
        "asc",
        "ascii",
        "ascx",
        "asm",
        "asmx",
        "asp",
        "aspx",
        "atom",
        "au3",
        "awk",
        "bas",
        "bash",
        "bashrc",
        "bat",
        "bbcolors",
        "bcp",
        "bdsgroup",
        "bdsproj",
        "bib",
        "bowerrc",
        "c",
        "cbl",
        "cc",
        "cfc",
        "cfg",
        "cfm",
        "cfml",
        "cgi",
        "cjs",
        "clj",
        "cljs",
        "cls",
        "cmake",
        "cmd",
        "cnf",
        "cob",
        "code-snippets",
        "coffee",
        "coffeekup",
        "conf",
        "cp",
        "cpp",
        "cpt",
        "cpy",
        "crt",
        "cs",
        "csh",
        "cson",
        "csproj",
        "csr",
        "css",
        "csslintrc",
        "csv",
        "ctl",
        "curlrc",
        "cxx",
        "d",
        "dart",
        "dfm",
        "diff",
        "dof",
        "dpk",
        "dpr",
        "dproj",
        "dtd",
        "eco",
        "editorconfig",
        "ejs",
        "el",
        "elm",
        "emacs",
        "eml",
        "ent",
        "erb",
        "erl",
        "eslintignore",
        "eslintrc",
        "ex",
        "exs",
        "f",
        "f03",
        "f77",
        "f90",
        "f95",
        "fish",
        "for",
        "fpp",
        "frm",
        "fs",
        "fsproj",
        "fsx",
        "ftn",
        "gemrc",
        "gemspec",
        "gitattributes",
        "gitconfig",
        "gitignore",
        "gitkeep",
        "gitmodules",
        "go",
        "gpp",
        "gradle",
        "graphql",
        "groovy",
        "groupproj",
        "grunit",
        "gtmpl",
        "gvimrc",
        "h",
        "haml",
        "hbs",
        "hgignore",
        "hh",
        "hpp",
        "hrl",
        "hs",
        "hta",
        "htaccess",
        "htc",
        "htm",
        "html",
        "htpasswd",
        "hxx",
        "iced",
        "iml",
        "inc",
        "inf",
        "info",
        "ini",
        "ino",
        "int",
        "irbrc",
        "itcl",
        "itermcolors",
        "itk",
        "jade",
        "java",
        "jhtm",
        "jhtml",
        "js",
        "jscsrc",
        "jshintignore",
        "jshintrc",
        "json",
        "json5",
        "jsonld",
        "jsp",
        "jspx",
        "jsx",
        "ksh",
        "less",
        "lhs",
        "lisp",
        "log",
        "ls",
        "lsp",
        "lua",
        "m",
        "m4",
        "mak",
        "map",
        "markdown",
        "master",
        "md",
        "mdown",
        "mdwn",
        "mdx",
        "metadata",
        "mht",
        "mhtml",
        "mjs",
        "mk",
        "mkd",
        "mkdn",
        "mkdown",
        "ml",
        "mli",
        "mm",
        "mxml",
        "nfm",
        "nfo",
        "noon",
        "npmignore",
        "npmrc",
        "nuspec",
        "nvmrc",
        "ops",
        "pas",
        "pasm",
        "patch",
        "pbxproj",
        "pch",
        "pem",
        "pg",
        "php",
        "php3",
        "php4",
        "php5",
        "phpt",
        "phtml",
        "pir",
        "pl",
        "pm",
        "pmc",
        "pod",
        "pot",
        "prettierrc",
        "properties",
        "props",
        "pt",
        "pug",
        "purs",
        "py",
        "pyx",
        "r",
        "rake",
        "rb",
        "rbw",
        "rc",
        "rdoc",
        "rdoc_options",
        "resx",
        "rexx",
        "rhtml",
        "rjs",
        "rlib",
        "ron",
        "rs",
        "rss",
        "rst",
        "rtf",
        "rvmrc",
        "rxml",
        "s",
        "sass",
        "scala",
        "scm",
        "scss",
        "seestyle",
        "sh",
        "shtml",
        "sln",
        "sls",
        "spec",
        "sql",
        "sqlite",
        "sqlproj",
        "srt",
        "ss",
        "sss",
        "st",
        "strings",
        "sty",
        "styl",
        "stylus",
        "sub",
        "sublime-build",
        "sublime-commands",
        "sublime-completions",
        "sublime-keymap",
        "sublime-macro",
        "sublime-menu",
        "sublime-project",
        "sublime-settings",
        "sublime-workspace",
        "sv",
        "svc",
        "svg",
        "swift",
        "t",
        "tcl",
        "tcsh",
        "terminal",
        "tex",
        "text",
        "textile",
        "tg",
        "tk",
        "tmLanguage",
        "tmpl",
        "tmTheme",
        "tpl",
        "ts",
        "tsv",
        "tsx",
        "tt",
        "tt2",
        "ttml",
        "twig",
        "txt",
        "v",
        "vb",
        "vbproj",
        "vbs",
        "vcproj",
        "vcxproj",
        "vh",
        "vhd",
        "vhdl",
        "vim",
        "viminfo",
        "vimrc",
        "vm",
        "vue",
        "webapp",
        "webmanifest",
        "wsc",
        "x-php",
        "xaml",
        "xht",
        "xhtml",
        "xml",
        "xs",
        "xsd",
        "xsl",
        "xslt",
        "y",
        "yaml",
        "yml",
        "zsh",
        "zshrc"
    ];
}
