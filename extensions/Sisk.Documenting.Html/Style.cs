namespace Sisk.Documenting.Html;
internal class Style {
    public const string DefaultStyles =
"""
* { box-sizing: border-box; }
p, li { line-height: 1.6 }

:root {
    --font-monospace: ui-monospace,SFMono-Regular,SF Mono,Menlo,Consolas,Liberation Mono,monospace;
}
        
html, body { 
    margin: 0;
    background-color: white;
    font-size: 16px;
    font-family: -apple-system,BlinkMacSystemFont,"Segoe UI","Noto Sans",Helvetica,Arial,sans-serif,"Apple Color Emoji","Segoe UI Emoji"
}

main {
    background: white;
    max-width: 1200px;
    width: 90vw;
    margin: 30px auto 0 auto;
    padding: 20px 40px;
    border-radius: 14px;
    border: 1px solid #d1d9e0;
}
        
h1, h2, h3 {
    margin-top: 2.5rem;
    margin-bottom: 1rem;
    font-weight: 600;
    line-height: 1.25;
}
        
h1, h2 {
    padding-bottom: .3em;
    border-bottom: 2px solid #d1d9e0b3;
}
        
h1 {                    
    font-size: 2em;                
}
        
h2 {
    font-size: 1.5em;
}
        
h1 a,
h2 a {
    opacity: 0;   
    color: #000;
    text-decoration: none !important;
    user-select: none;
}
        
h1:hover a,
h2:hover a {
    opacity: 0.3;
}
        
h1 a:hover,
h2 a:hover {
    opacity: 1;
}
        
hr {
    border-top: none;
    border-bottom: 3px solid #d1d9e0;
}
        
pre {
    background-color: #f6f8fa;
    color: black;
    padding: 1rem;
    overflow: auto;
    font-size: 85%;
    line-height: 1.45;
    border-radius: 6px;
}

.muted {
    color: #656d76;
}
        
.at {
    color: #9a6700;
}
        
a {
    color: #0969da;
    text-decoration: none;
}
        
a:hover {
    text-decoration: underline;
}

.endpoint-description > summary {
    display: block;
    width: 100%;
    padding: .5em;
    border-radius: 6px;
    border: 1px solid #d1d9e0;
    cursor: pointer;
}

.endpoint-description > summary:hover {
    background-color: #f6f8fa;
}

.endpoint-description > summary > span:nth-child(1) {
    display: inline-block;
    padding: 4px 0;
    width: 70px;
    text-align: center;
    border-radius: 3px;
    font-weight: 700;
    font-size: .8em;
    color: #000000bb;
    text-transform: uppercase;
    user-select: none;
}

.endpoint-description > summary > span:nth-child(2) {
    font-family: var(--font-monospace);
    font-size: 14px;
    margin-left: 10px;
}

.endpoint-description > summary > span:nth-child(3) {
    user-select: none;
}

.endpoint-description h3 {
    margin: 1em 0 1.25em 0;
}

.endpoint + .endpoint {
    margin-top: .25em;
}

:not(pre) > code {
    background-color: #eff1f3;
    padding: .2em .4em;
    font-family: var(--font-monospace);
    font-size: 14px;
    border-radius: 4px;
}

.item-description + .item-description {
    padding-top: 1em;uj
    border-top: 1px solid #d1d9e0;
}

.ml1 { margin-left: .25em; }
.ml2 { margin-left: .5em; }
.ml3 { margin-left: .75em; }
""";
}
