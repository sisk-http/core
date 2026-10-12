// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Style.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting.Html;

internal class Style {
    public const string EmbedStyles = """
        :host {
            display: block;
            margin: 1em 0;
            font-size: 16px;
            font-family: -apple-system,BlinkMacSystemFont,"Segoe UI","Noto Sans",Helvetica,Arial,sans-serif,"Apple Color Emoji","Segoe UI Emoji";
            color: var(--text-primary);
        }
        .sisk-embed {
            padding: 1.25rem;
            background: var(--bg-primary);
            border: 1px solid var(--border-color);
            border-radius: 12px;
        }
        .sisk-embed .endpoint {
            margin: 0;
        }
        .sisk-embed .endpoint-description {
            border: none;
            background: transparent;
        }
        .sisk-embed .endpoint-description[open] > .endpoint-content {
            padding-top: 0;
        }
        .sisk-embed .endpoint-description > summary {
            display: none;
        }
        .sisk-embed .endpoint-content {
            padding: 0;
        }
        .sisk-embed .endpoint-content-header {
            margin-bottom: 1rem;
            padding-bottom: 1rem;
            border-bottom: 1px solid var(--border-color);
        }
        .sisk-embed .endpoint-content-header h3 {
            font-size: 1.25rem;
            margin: 0;
        }
        .sisk-embed .endpoint-layout.has-examples {
            display: block;
        }
        .sisk-embed .endpoint-layout.has-examples .endpoint-examples {
            position: static;
            max-height: none;
            overflow: visible;
            margin-top: 1.5rem;
        }

        .sisk-api-embed {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
            font-size: 14px;
            line-height: 1.6;
            color: #1a1a2e;
            background: #ffffff;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            overflow: hidden;
            margin: 1rem 0;
        }
        .sisk-api-embed .endpoint {
            margin: 0;
        }
        .sisk-api-embed .endpoint-description {
            border: none;
            background: transparent;
        }
        .sisk-api-embed .endpoint-description > summary {
            display: none;
        }
        .sisk-api-embed .endpoint-content {
            padding: 1.25rem;
        }
        .sisk-api-embed .endpoint-content-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
            padding-bottom: 0.75rem;
            border-bottom: 1px solid #e2e8f0;
        }
        .sisk-api-embed .endpoint-content-header h3 {
            font-size: 1.1rem;
            font-weight: 600;
            margin: 0;
            color: #1a1a2e;
        }
        .sisk-api-embed pre {
            background: #f8fafc;
            border: 1px solid #e2e8f0;
            border-radius: 6px;
            padding: 0.875rem;
            overflow-x: auto;
            margin: 0.75rem 0;
        }
        .sisk-api-embed code {
            font-family: 'JetBrains Mono', 'Fira Code', 'Consolas', monospace;
            font-size: 0.85em;
        }
        .sisk-api-embed .params-section {
            margin-top: 1rem;
        }
        .sisk-api-embed .params-section > p {
            font-weight: 600;
            margin-bottom: 0.5rem;
            color: #374151;
        }
        .sisk-api-embed .params-list {
            list-style: none;
            padding: 0;
            margin: 0;
        }
        .sisk-api-embed .param-item {
            padding: 0.625rem 0;
            border-bottom: 1px solid #f1f5f9;
        }
        .sisk-api-embed .param-item:last-child {
            border-bottom: none;
        }
        .sisk-api-embed .param-header {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            flex-wrap: wrap;
        }
        .sisk-api-embed .param-name {
            font-weight: 600;
            color: #1e40af;
            font-family: 'JetBrains Mono', 'Fira Code', monospace;
            font-size: 0.9em;
        }
        .sisk-api-embed .param-type {
            font-size: 0.8em;
            color: #6b7280;
            background: #f3f4f6;
            padding: 0.125rem 0.375rem;
            border-radius: 4px;
        }
        .sisk-api-embed .param-required {
            font-size: 0.75em;
            color: #dc2626;
            font-weight: 500;
        }
        .sisk-api-embed .param-description {
            margin-top: 0.25rem;
            color: #4b5563;
            font-size: 0.9em;
        }
        .sisk-api-embed .parameter-examples {
            margin-top: 1.25rem;
        }
        .sisk-api-embed .parameter-examples-title {
            margin: 0 0 0.75rem;
            font-size: 1rem;
            font-weight: 600;
            color: #1a1a2e;
        }
        .sisk-api-embed .parameter-example {
            margin-top: 0.5rem;
            border: 1px solid #d8dee6;
            border-radius: 8px;
            overflow: hidden;
            background: #ffffff;
        }
        .sisk-api-embed .parameter-example > summary {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1rem;
            min-height: 3.5rem;
            padding: .5rem;
            cursor: pointer;
            list-style: none;
            font-size: 1rem;
            font-weight: 400;
            line-height: 1.2;
            color: #1a1a2e;
            background: #f6f8fa;
            transition: background-color 0.15s;
        }
        .sisk-api-embed .parameter-example > summary::-webkit-details-marker {
            display: none;
        }
        .sisk-api-embed .parameter-example > summary::marker {
            content: "";
        }
        .sisk-api-embed .parameter-example > summary::after {
            content: "";
            width: 0.4rem;
            height: 0.4rem;
            flex: 0 0 auto;
            border-right: 2px solid currentColor;
            border-bottom: 2px solid currentColor;
            opacity: 0.55;
            transform: rotate(-45deg);
            transition: transform 0.15s ease;
        }
        .sisk-api-embed .parameter-example[open] > summary::after {
            transform: rotate(45deg);
        }
        .sisk-api-embed .parameter-example > summary:hover {
            background: #eef1f4;
        }
        .sisk-api-embed .parameter-example > summary:focus-visible {
            outline: 2px solid #0969da;
            outline-offset: -2px;
        }
        .sisk-api-embed .parameter-example[open] > summary {
            border-bottom: 1px solid #d8dee6;
        }
        .sisk-api-embed .parameter-example-body {
            padding: 1rem;
        }
        .sisk-api-embed .parameter-example-description {
            color: #4b5563;
            font-size: 1rem;
        }
        .sisk-api-embed .parameter-example-description > :first-child {
            margin-top: 0;
        }
        .sisk-api-embed .parameter-example-description > :last-child {
            margin-bottom: 0;
        }
        .sisk-api-embed .parameter-example-body > pre {
            margin: 1rem 0 0;
            padding: 1rem;
            border: none;
            border-radius: 8px;
            background: #f6f8fa;
            color: #1a1a2e;
        }
        .sisk-api-embed .parameter-example-body > pre:first-child {
            margin-top: 0;
        }
        .sisk-api-embed .status-code {
            display: inline-block;
            padding: 0.125rem 0.5rem;
            border-radius: 4px;
            font-weight: 600;
            font-size: 0.85em;
            margin-right: 0.5rem;
        }
        .sisk-api-embed .status-code.success {
            background: #dcfce7;
            color: #166534;
        }
        .sisk-api-embed .status-code.error {
            background: #fee2e2;
            color: #991b1b;
        }
        .sisk-api-embed .status-code.redirect {
            background: #fef3c7;
            color: #92400e;
        }
        """;

    public const string DefaultStyles =
        """
        * { box-sizing: border-box; }
        p, li { line-height: 1.6; font-size: 1rem }

        :root {
            color-scheme: light;
            --font-monospace: ui-monospace,SFMono-Regular,SF Mono,Menlo,Consolas,Liberation Mono,monospace;
            --sidebar-width: 280px;
            --bg-primary: #ffffff;
            --bg-secondary: #f6f8fa;
            --bg-hover: #e8ebef;
            --border-color: #e1e4e8;
            --text-primary: #24292f;
            --text-secondary: #57606a;
            --accent-color: #0969da;
            --accent-bg: #dbeafe;
            --method-text: #000000cc;
            --code-bg: #1e293b;
            --code-text: #e2e8f0;
            --inline-code-bg: #eff1f3;
            --danger-bg: #fef2f2;
            --danger-text: #dc2626;
            --success-bg: #dcfce7;
            --success-text: #166534;
            --warning-bg: #fef3c7;
            --warning-text: #92400e;
            --shadow-color: rgba(0, 0, 0, 0.1);
            --control-bg: var(--bg-secondary);
            --control-hover-bg: #e8ebef;
            --dialog-border: transparent;
            --mark-bg: #fff3cd;
            --info-bg: #dbeafe;
            --info-border: #93c5fd;
            --menu-bg: #ffffff;
            --toast-bg: #333333;
            --toast-text: #ffffff;
            --summary-hover-bg: #eef1f4;
        }

        @media (prefers-color-scheme: dark) {
            :root:not([data-theme="light"]) {
                color-scheme: dark;
                --bg-primary: #060606;
                --bg-secondary: #0c0c0c;
                --bg-hover: #1c1c1c;
                --border-color: #1e1e1e;
                --text-primary: #e6edf3;
                --text-secondary: #8b949e;
                --accent-color: #58a6ff;
                --accent-bg: #0c1829;
                --method-text: #ffffffcc;
                --code-bg: #0c0c0c;
                --code-text: #e6edf3;
                --inline-code-bg: #1e1e1e;
                --danger-bg: #3d1f1f;
                --danger-text: #f85149;
                --success-bg: #1a3d1a;
                --success-text: #3fb950;
                --warning-bg: #3d2f1a;
                --warning-text: #d29922;
                --shadow-color: rgba(0, 0, 0, 0.5);
                --control-bg: #1c1c1c;
                --control-hover-bg: #1e1e1e;
                --dialog-border: var(--border-color);
                --mark-bg: #634d00;
                --info-bg: #1f3a5f;
                --info-border: #388bfd;
                --menu-bg: #0c0c0c;
                --toast-bg: #e6edf3;
                --toast-text: #060606;
                --summary-hover-bg: #161616;
            }
        }

        :root[data-theme="dark"] {
            color-scheme: dark;
            --bg-primary: #060606;
            --bg-secondary: #0c0c0c;
            --bg-hover: #1c1c1c;
            --border-color: #1e1e1e;
            --text-primary: #e6edf3;
            --text-secondary: #8b949e;
            --accent-color: #58a6ff;
            --accent-bg: #0c1829;
            --method-text: #ffffffcc;
            --code-bg: #0c0c0c;
            --code-text: #e6edf3;
            --inline-code-bg: #1e1e1e;
            --danger-bg: #3d1f1f;
            --danger-text: #f85149;
            --success-bg: #1a3d1a;
            --success-text: #3fb950;
            --warning-bg: #3d2f1a;
            --warning-text: #d29922;
            --shadow-color: rgba(0, 0, 0, 0.5);
            --control-bg: #1c1c1c;
            --control-hover-bg: #1e1e1e;
            --dialog-border: var(--border-color);
            --mark-bg: #634d00;
            --info-bg: #1f3a5f;
            --info-border: #388bfd;
            --menu-bg: #0c0c0c;
            --toast-bg: #e6edf3;
            --toast-text: #060606;
            --summary-hover-bg: #161616;
        }
        
        html, body { 
            margin: 0;
            background-color: var(--bg-primary);
            font-size: 16px;
            font-family: -apple-system,BlinkMacSystemFont,"Segoe UI","Noto Sans",Helvetica,Arial,sans-serif,"Apple Color Emoji","Segoe UI Emoji";
            color: var(--text-primary);
        }

        .page-wrapper {
            display: flex;
            min-height: 100vh;
        }

        /* Sidebar Navigation */
        .sidebar {
            width: var(--sidebar-width);
            min-width: var(--sidebar-width);
            background-color: var(--bg-secondary);
            border-right: 1px solid var(--border-color);
            padding: 20px 0;
            position: fixed;
            height: 100vh;
            overflow-y: auto;
            z-index: 100;
        }

        .sidebar-header {
            padding: 0 20px 20px 20px;
            border-bottom: 1px solid var(--border-color);
            margin-bottom: 10px;
        }

        .sidebar-title {
            display: flex;
            align-items: flex-start;
            justify-content: space-between;
            gap: 12px;
        }

        .sidebar-title > div:first-child {
            min-width: 0;
        }

        /* Theme Switcher */
        .theme-switcher {
            display: inline-flex;
            flex-shrink: 0;
            gap: 2px;
            padding: 2px;
            border: 1px solid var(--border-color);
            border-radius: 6px;
            background: var(--bg-primary);
        }

        main > .theme-switcher {
            float: right;
        }

        .theme-switcher button {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 24px;
            height: 24px;
            padding: 0;
            border: none;
            border-radius: 4px;
            background: transparent;
            color: var(--text-secondary);
            cursor: pointer;
            transition: background-color 0.15s, color 0.15s;
        }

        .theme-switcher button:hover {
            color: var(--text-primary);
        }

        .theme-switcher button[aria-pressed="true"] {
            background: var(--bg-hover);
            color: var(--text-primary);
        }

        .theme-switcher button:focus-visible {
            outline: 2px solid var(--accent-color);
            outline-offset: -2px;
        }

        .theme-switcher svg {
            width: 14px;
            height: 14px;
        }

        .search-trigger {
            display: flex;
            align-items: center;
            gap: 8px;
            width: 100%;
            padding: 8px 12px;
            margin-top: 12px;
            background: var(--control-bg);
            border: 1px solid var(--border-color);
            border-radius: 6px;
            color: var(--text-secondary);
            font-size: 0.85em;
            cursor: pointer;
            transition: all 0.15s;
        }

        .search-trigger:hover {
            background: var(--control-hover-bg);
            border-color: var(--accent-color);
        }

        .search-trigger svg {
            width: 14px;
            height: 14px;
            fill: currentColor;
            flex-shrink: 0;
        }

        .search-trigger span {
            flex: 1;
            text-align: left;
        }

        .search-trigger kbd {
            font-size: 0.75em;
            padding: 2px 6px;
            background: var(--bg-primary);
            border: 1px solid var(--border-color);
            border-radius: 4px;
            font-family: inherit;
        }

        .search-overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.5);
            z-index: 9999;
            justify-content: center;
            padding-top: 10vh;
        }

        .search-overlay.open {
            display: flex;
        }

        .search-dialog {
            width: 100%;
            max-width: 600px;
            max-height: 70vh;
            background: var(--bg-primary);
            border: 1px solid var(--dialog-border);
            border-radius: 10px;
            box-shadow: 0 8px 40px rgba(0, 0, 0, 0.25);
            overflow: hidden;
            display: flex;
            flex-direction: column;
        }

        .search-input-wrapper {
            display: flex;
            align-items: center;
            gap: 12px;
            padding: 16px;
            border-bottom: 1px solid var(--border-color);
        }

        .search-input-wrapper svg {
            width: 20px;
            height: 20px;
            fill: var(--text-secondary);
            flex-shrink: 0;
        }

        .search-input {
            flex: 1;
            border: none;
            font-size: 1rem;
            outline: none;
            background: transparent;
            color: var(--text-primary);
        }

        .search-input::placeholder {
            color: var(--text-secondary);
        }

        .search-results {
            overflow-y: auto;
            flex: 1;
            max-height: 50vh;
        }

        .search-result-item {
            display: flex;
            align-items: center;
            gap: 12px;
            padding: 12px 16px;
            cursor: pointer;
            transition: background 0.1s;
            text-decoration: none;
            color: inherit;
        }

        .search-result-item:hover,
        .search-result-item.selected {
            background: var(--bg-secondary);
            text-decoration: none;
        }

        .search-result-item .method-badge {
            flex-shrink: 0;
            color: var(--method-text);
        }

        .search-result-info {
            flex: 1;
            min-width: 0;
        }

        .search-result-name {
            font-weight: 500;
            color: var(--text-primary);
            margin-bottom: 2px;
        }

        .search-result-name mark {
            background: var(--mark-bg);
            color: inherit;
            border-radius: 2px;
            padding: 0;
        }

        .search-result-path {
            font-size: 0.85em;
            color: var(--text-secondary);
            font-family: var(--font-monospace);
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .search-result-group {
            font-size: 0.75em;
            color: var(--text-secondary);
            padding: 2px 8px;
            background: var(--control-bg);
            border-radius: 4px;
            flex-shrink: 0;
        }

        .search-empty {
            padding: 32px 16px;
            text-align: center;
            color: var(--text-secondary);
        }

        .sidebar-header h1 {
            font-size: 1.1em;
            margin: 0 0 5px 0;
            padding: 0;
            border: none;
            color: var(--text-primary);
        }

        .sidebar-header .version {
            font-size: 0.85em;
            color: var(--text-secondary);
        }

        .nav-section {
            padding: 10px 0;
        }

        .nav-section-title {
            font-size: 0.75em;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            color: var(--text-secondary);
            padding: 8px 20px;
            margin: 0;
        }

        .nav-group {
            margin-bottom: 5px;
        }

        .nav-group-header {
            display: flex;
            align-items: center;
            padding: 8px 20px;
            cursor: pointer;
            font-size: 0.9em;
            font-weight: 500;
            color: var(--text-primary);
            opacity: .7;
            transition: 0.15s;
        }

        .nav-group-header:hover {
            opacity: 1;
            transition: none;
        }

        .nav-group-header:hover .arrow {
            opacity: 1;
        }

        .nav-group-header .arrow {
            margin-right: 8px;
            width: 16px;
            height: 16px;
            transition: transform 0.2s;
            flex-shrink: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            opacity: .7;
        }

        .nav-group-header .arrow svg {
            width: 100%;
            height: 100%;
            fill: currentColor;
        }

        .nav-group.open .arrow {
            transform: rotate(90deg);
        }

        .nav-items {
            display: none;
            padding-left: 20px;
        }

        .nav-group.open .nav-items {
            display: block;
        }

        .nav-item {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 6px 12px;
            width: fit-content;
            font-size: 0.875em;
            color: var(--text-secondary);
            text-decoration: none;
            border-radius: 6px;
            transition: all 0.15s;
        }

        .nav-item + .nav-item {
            margin-top: 2px;
        }

        .nav-item:hover {
            background-color: var(--bg-hover);
            color: var(--text-primary);
            text-decoration: none;
            transition: none;
        }

        .nav-item.active {
            background-color: var(--accent-bg);
            color: var(--accent-color);
        }

        .nav-item .method-badge {
            display: inline-block;
            padding: 2px 6px;
            border-radius: 3px;
            font-size: 0.7em;
            font-weight: 700;
            text-transform: uppercase;
            min-width: 45px;
            text-align: center;
            color: var(--method-text);
        }

        .sisk-embed .copy-button-wrapper {
            margin-left: auto;
        }
        .sisk-embed .open-ext-button {
            background: transparent;
            border: none;
            opacity: .5;
            cursor: pointer;
            transition: .25s ease;
            text-decoration: none;
        }
        .sisk-embed .open-ext-button:hover {
            opacity: 1;
        }
        .sisk-embed .open-ext-button svg {
            width: 16px;
            height: 16px;
            fill: var(--text-primary);
        }

        /* Main Content */
        main {
            flex: 1;
            padding: 40px 60px;
            padding-left: calc(var(--sidebar-width) + 20px);
            width: 100%;
            max-width: 1400px;
            margin: 0 auto 0 auto;
            background: var(--bg-primary);
        }

        .content-header {
            margin-bottom: 30px;
            padding-bottom: 20px;
            border-bottom: 1px solid var(--border-color);
        }

        .content-header .breadcrumb {
            font-size: 0.85em;
            color: var(--accent-color);
            margin-bottom: 10px;
        }

        .content-header h1 {
            font-size: 2em;
            margin: 0 0 10px 0;
            padding: 0;
            border: none;
            color: var(--text-primary);
        }

        .content-header .description {
            color: var(--text-secondary);
            font-size: 1.1em;
            margin: 0;
        }
        
        h1, h2, h3 {
            margin-top: 2rem;
            margin-bottom: 1rem;
            font-weight: 600;
            line-height: 1.25;
        }
        
        h2 {
            font-size: 1.5em;
            padding-bottom: .3em;
            border-bottom: 1px solid var(--border-color);
            margin-top: 2.5rem;
        }

        h3 {
            font-size: 1.25em;
        }
        
        h1 a,
        h2 a {
            opacity: 0;   
            color: var(--text-primary);
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
            border: none;
            border-bottom: 1px solid var(--border-color);
            margin: 2rem 0;
        }

        /* Endpoint Card */
        .endpoint-card {
            background: var(--bg-primary);
            border: 1px solid var(--border-color);
            border-radius: 8px;
            margin-bottom: 20px;
            overflow: hidden;
        }

        .endpoint-header {
            display: flex;
            align-items: center;
            gap: 12px;
            padding: 16px 20px;
            background: var(--bg-secondary);
            border-bottom: 1px solid var(--border-color);
        }

        .endpoint-header .method-badge {
            display: inline-block;
            padding: 6px 12px;
            border-radius: 4px;
            font-weight: 700;
            font-size: 0.8em;
            text-transform: uppercase;
            min-width: 60px;
            text-align: center;
        }

        .endpoint-header .path {
            font-family: var(--font-monospace);
            font-size: 0.95em;
            color: var(--text-primary);
            flex: 1;
        }

        .endpoint-body {
            padding: 20px;
        }
        
        pre {
            background-color: var(--code-bg);
            color: var(--code-text);
            padding: 1rem;
            overflow: auto;
            font-size: 0.875em;
            line-height: 1.6;
            border-radius: 8px;
            max-height: 500px;
            margin: 1rem 0;
        }

        pre code {
            color: inherit;
            background: none;
            padding: 0;
        }

        .muted {
            color: var(--text-secondary);
        }
        
        .at {
            color: #9a6700;
            font-size: 0.85em;
            font-weight: 500;
        }
        
        a {
            color: var(--accent-color);
            text-decoration: none;
        }
        
        a:hover {
            text-decoration: underline;
        }

        /* Info Box */
        .info-box {
            display: flex;
            align-items: flex-start;
            gap: 12px;
            padding: 16px;
            background: var(--info-bg);
            border: 1px solid var(--info-border);
            border-radius: 8px;
            margin: 1rem 0;
        }

        .info-box .icon {
            color: var(--accent-color);
            font-size: 1.2em;
        }

        .info-box .content {
            flex: 1;
        }

        /* Endpoint Description (details/summary) */
        .endpoint-description {
            border: 1px solid var(--border-color);
            border-radius: 8px;
            margin-bottom: 12px;
            overflow: hidden;
        }

        .endpoint-description > summary {
            display: flex;
            align-items: center;
            gap: 12px;
            width: 100%;
            padding: 14px 16px;
            background: var(--bg-primary);
            cursor: pointer;
            list-style: none;
            transition: background-color 0.15s;
        }

        .endpoint-description > summary::-webkit-details-marker {
            display: none;
        }

        .endpoint-description > summary:hover {
            background-color: var(--bg-secondary);
        }

        .endpoint-description[open] > summary {
            border-bottom: 1px solid var(--border-color);
            background-color: var(--bg-secondary);
        }

        .endpoint-description > summary > span:nth-child(1) {
            display: inline-block;
            padding: 4px 8px;
            min-width: 55px;
            text-align: center;
            border-radius: 4px;
            font-weight: 700;
            font-size: 0.75em;
            color: var(--method-text);
            text-transform: uppercase;
            user-select: none;
        }

        /* Copy Button & Dropdown */
        .copy-button-wrapper {
            margin-left: auto;
            position: relative;
            display: flex;
            align-items: center;
            gap: 3px;
        }

        .copy-button-wrapper > a,
        .copy-button-wrapper > button {
            display: inline-flex;
            padding: 4px;
        }
        
        .copy-button {
            background: transparent;
            border: none;
            color: var(--text-primary);
            opacity: .5;
            cursor: pointer;
            transition: .25s ease;
        }

        .copy-button svg {
            width: 16px;
            height: 16px;
        }
        
        .copy-button:hover {
            opacity: 1;
        }
        
        .dropdown-menu {
            display: none;
            position: absolute;
            top: 100%;
            right: 0;
            margin-top: 8px;
            background: var(--menu-bg);
            border: 1px solid var(--border-color);
            border-radius: 6px;
            box-shadow: 0 4px 12px var(--shadow-color);
            z-index: 1000;
            min-width: 160px;
            padding: 4px 0;
            flex-direction: column;
        }
        
        .dropdown-menu.show {
            display: flex;
        }
        
        .dropdown-item {
            display: block;
            width: 100%;
            text-align: left;
            padding: 8px 16px;
            border: none;
            background: transparent;
            cursor: pointer;
            font-size: 0.9em;
            color: var(--text-primary);
            transition: background 0.15s;
            font-family: inherit;
        }
        
        .dropdown-item:hover {
            background: var(--bg-secondary);
        }

        /* Toast Notification */
        .copy-toast {
            position: fixed;
            bottom: 24px;
            left: 50%;
            transform: translateX(-50%) translateY(100px);
            background: var(--toast-bg);
            color: var(--toast-text);
            padding: 12px 24px;
            border-radius: 8px;
            font-size: 0.9em;
            z-index: 10000;
            opacity: 0;
            transition: transform 0.3s ease, opacity 0.3s ease;
            pointer-events: none;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        }

        .copy-toast.show {
            transform: translateX(-50%) translateY(0);
            opacity: 1;
        }

        .endpoint-description > summary > span:nth-child(2) {
            font-family: var(--font-monospace);
            font-size: 0.9em;
            color: var(--text-primary);
        }

        .endpoint-description > summary > span:nth-child(3) {
            user-select: none;
            color: var(--text-secondary);
            font-size: 0.9em;
        }

        .endpoint-description .endpoint-content {
            padding: 20px;
            background: var(--bg-primary);
        }

        .endpoint-breadcrumb {
            display: none;
            align-items: center;
            gap: 10px;
            margin-bottom: 12px;
            font-size: 0.9em;
            color: var(--text-secondary);
        }

        .endpoint-breadcrumb > span::before {
            content: "/";
            margin-right: 10px;
            opacity: .5;
        }

        .endpoint-back {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            color: var(--text-secondary);
        }

        .endpoint-back:hover {
            color: var(--accent-color);
            text-decoration: none;
        }

        .endpoint-back svg {
            width: 16px;
            height: 16px;
        }

        .endpoint-content-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 16px;
            margin-bottom: 1rem;
        }

        .endpoint-content-header h3 {
            margin: 0;
            font-size: 1.3em;
        }

        .endpoint-path {
            display: flex;
            align-items: center;
            gap: 10px;
            margin: 1rem 0 1.5rem;
            padding: 6px;
            border: 1px solid var(--border-color);
            border-radius: 8px;
            background: var(--bg-secondary);
        }

        .endpoint-path .method-badge {
            flex-shrink: 0;
            padding: 4px 10px;
            border-radius: 5px;
            font-size: 0.8em;
            font-weight: 700;
            color: var(--method-text);
        }

        .endpoint-path code {
            min-width: 0;
            padding: 0;
            background: none;
            font-size: 0.95em;
            overflow-wrap: anywhere;
        }

        /* Endpoint layout: documentation column and request/response examples column */
        .endpoint-layout.has-examples .endpoint-examples {
            display: flex;
            flex-direction: column;
            gap: 20px;
            margin-top: 1.5rem;
        }

        @media (min-width: 1240px) {
            .endpoint-layout.has-examples {
                display: grid;
                grid-template-columns: minmax(0, 1fr) minmax(0, 0.85fr);
                gap: 48px;
                align-items: start;
            }

            .endpoint-layout.has-examples .endpoint-examples {
                position: sticky;
                top: 24px;
                max-height: calc(100vh - 48px);
                overflow-y: auto;
                margin-top: 0;
            }

            .endpoint-main > p:first-child {
                margin-top: 0;
            }
        }

        .example-card {
            flex-shrink: 0;
            margin: 0;
            border: 1px solid var(--border-color);
            border-radius: 10px;
            background: var(--bg-secondary);
            overflow: hidden;
        }

        .example-card > .example-card-header {
            display: flex;
            align-items: center;
            flex-wrap: wrap;
            gap: 2px;
            padding: 0 12px;
            border-bottom: 1px solid var(--border-color);
        }

        .example-card-title {
            margin-right: auto;
            padding: 10px 8px 10px 4px;
            font-size: 0.85em;
            font-weight: 600;
            color: var(--text-primary);
        }

        .example-card > .example-card-header > .tab-button {
            padding: 10px 8px;
            margin-bottom: -1px;
            font-family: var(--font-monospace);
        }

        .example-card > .example-card-header > .tab-button:hover {
            background: transparent;
        }

        .example-card > .example-card-header > .tab-button.success { color: var(--success-text); }
        .example-card > .example-card-header > .tab-button.redirect { color: var(--warning-text); }
        .example-card > .example-card-header > .tab-button.error { color: var(--danger-text); }

        .example-card > .example-card-header > .tab-button.active {
            border-bottom-color: currentColor;
        }

        .example-card > .example-card-header > .tab-button:not(.active) {
            opacity: .65;
        }

        .example-card > .tab-content {
            background: var(--bg-primary);
        }

        .example-description {
            padding: 12px 16px;
            color: var(--text-secondary);
            font-size: 0.9em;
        }

        .example-description-text {
            max-height: 7.5em;
            overflow: hidden;
            line-height: 1.5;
        }

        .example-description.overflowing:not(.expanded) > .example-description-text {
            -webkit-mask-image: linear-gradient(to bottom, #000 60%, transparent);
            mask-image: linear-gradient(to bottom, #000 60%, transparent);
        }

        .example-description.expanded > .example-description-text {
            max-height: none;
        }

        .example-description-text > :first-child { margin-top: 0; }
        .example-description-text > :last-child { margin-bottom: 0; }

        .example-description-toggle {
            display: none;
            margin-top: 6px;
            padding: 0;
            border: 0;
            background: none;
            color: var(--accent-color);
            font: inherit;
            font-size: 0.9em;
            cursor: pointer;
        }

        .example-description-toggle:hover {
            text-decoration: underline;
        }

        .example-description.overflowing > .example-description-toggle,
        .example-description.expanded > .example-description-toggle {
            display: inline-block;
        }

        .example-variant {
            display: none;
        }

        .example-variant.active {
            display: block;
        }

        .example-variant-select {
            display: block;
            width: calc(100% - 32px);
            margin: 12px 16px 0;
            padding: 6px 8px;
            border: 1px solid var(--border-color);
            border-radius: 6px;
            background: var(--bg-secondary);
            color: var(--text-primary);
            font: inherit;
            font-size: 0.85em;
        }

        .example-card > .tab-content > pre,
        .example-card .tab-control .tab-content > pre {
            margin: 0;
            border-radius: 0;
            max-height: 60vh;
        }

        .example-card > .tab-content > .tab-control {
            margin-top: 0;
        }

        .example-card > .tab-content > .tab-control > .tab-header {
            padding: 0 8px;
        }

        .example-description + pre,
        .example-description + .tab-control {
            border-top: 1px solid var(--border-color);
        }

        /* Endpoint page mode */
        body.endpoint-page-open main {
            max-width: 1680px;
        }

        body.endpoint-page-open main > *:not(.group-section):not(.theme-switcher),
        body.endpoint-page-open .group-section:not(.current),
        body.endpoint-page-open .group-section > h2,
        body.endpoint-page-open .group-section > .endpoint:not(.current),
        body.endpoint-page-open .endpoint.current .endpoint-description > summary {
            display: none;
        }

        body.endpoint-page-open .group-section {
            margin: 0;
        }

        body.endpoint-page-open .endpoint.current .endpoint-description {
            border: none;
            border-radius: 0;
            margin: 0;
            overflow: visible;
        }

        body.endpoint-page-open .endpoint.current .endpoint-content {
            padding: 0;
        }

        body.endpoint-page-open .endpoint-breadcrumb {
            display: flex;
        }

        body.endpoint-page-open .endpoint-content-header {
            align-items: flex-start;
        }

        body.endpoint-page-open .endpoint-content-header h3 {
            font-size: 2em;
            letter-spacing: -0.01em;
        }

        body.endpoint-page-open .endpoint-main > p:first-child,
        body.endpoint-page-open .endpoint-main > p:first-child + p {
            font-size: 1.1em;
            color: var(--text-secondary);
        }

        /* Parameters Section */
        .params-section {
            margin: 1.5rem 0;
        }

        .params-section > p {
            font-weight: 600;
            font-size: 1em;
            margin-bottom: 0.75rem;
            color: var(--text-primary);
        }

        .params-list {
            list-style: none;
            padding: 0;
            margin: 0;
        }

        .param-item {
            padding: 12px 0;
            border-bottom: 1px solid var(--border-color);
        }

        .param-item:last-child {
            border-bottom: none;
        }

        .param-header {
            display: flex;
            align-items: center;
            gap: 10px;
            margin-bottom: 6px;
        }

        .param-name {
            font-family: var(--font-monospace);
            font-weight: 400;
            color: var(--accent-color);
            background: var(--bg-secondary);
            padding: 0 8px;
            font-size: 1rem;
            border-radius: 8px;
        }

        .param-type {
            font-size: 0.85em;
            color: var(--text-secondary);
        }

        .param-required {
            font-size: 0.75em;
            font-weight: 600;
            color: var(--danger-text);
            background: var(--danger-bg);
            padding: 2px 6px;
            border-radius: 3px;
        }

        .param-description {
            color: var(--text-secondary);
            font-size: 0.9em;
            margin-top: -.8em;
        }

        .parameter-examples {
            margin-top: 20px;
        }

        .parameter-examples-title {
            margin: 0 0 12px;
            font-size: 1em;
            font-weight: 600;
            color: var(--text-primary);
        }

        .parameter-example {
            margin-top: 8px;
            border: 1px solid var(--border-color);
            border-radius: 8px;
            overflow: hidden;
            background: var(--bg-primary);
        }

        .parameter-example + .parameter-example {
            margin-top: 2px;
        }

        .parameter-example > summary {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1rem;
            padding: .5rem;
            cursor: pointer;
            list-style: none;
            font-size: .9rem;
            font-weight: 400;
            line-height: 1.2;
            color: var(--text-primary);
            background: var(--bg-secondary);
            transition: background-color 0.15s;
        }

        .parameter-example > summary::-webkit-details-marker {
            display: none;
        }

        .parameter-example > summary::marker {
            content: "";
        }

        .parameter-example > summary::after {
            content: "";
            width: 7px;
            height: 7px;
            flex: 0 0 auto;
            border-right: 2px solid currentColor;
            border-bottom: 2px solid currentColor;
            opacity: 0.55;
            transform: rotate(-45deg);
            transition: transform 0.15s ease;
        }

        .parameter-example[open] > summary::after {
            transform: rotate(45deg);
        }

        .parameter-example > summary:hover {
            background: var(--summary-hover-bg);
        }

        .parameter-example > summary:focus-visible {
            outline: 2px solid var(--accent-color);
            outline-offset: -2px;
        }

        .parameter-example[open] > summary {
            border-bottom: 1px solid var(--border-color);
        }

        .parameter-example-body {
            padding: 16px;
        }

        .parameter-example-description {
            color: var(--text-secondary);
            font-size: 1em;
        }

        .parameter-example-description > :first-child {
            margin-top: 0;
        }

        .parameter-example-description > :last-child {
            margin-bottom: 0;
        }

        .parameter-example-body > pre {
            margin: 16px 0 0;
            padding: 16px;
            border-radius: 8px;
            background: var(--bg-secondary);
            color: var(--text-primary);
        }

        .parameter-example-body > pre:first-child {
            margin-top: 0;
        }

        .endpoint + .endpoint {
            margin-top: .5em;
        }

        :not(pre) > code {
            background-color: var(--inline-code-bg);
            padding: .2em .4em;
            font-family: var(--font-monospace);
            font-size: 0.875em;
            border-radius: 4px;
        }

        .item-description {
            padding: 10px 0;
        }

        .item-description + .item-description {
            border-top: 1px solid var(--border-color);
        }

        .ml1 { margin-left: .25em; }
        .ml2 { margin-left: .5em; }
        .ml3 { margin-left: .75em; }

        /* Group Section */
        .group-section {
            margin-bottom: 3rem;
        }

        .group-section h2 {
            position: sticky;
            top: 0;
            background: var(--bg-primary);
            padding: 15px 0;
            margin: 0 0 1rem 0;
            z-index: 10;
        }

        /* Response Status Codes */
        .status-code {
            display: inline-block;
            padding: 2px 8px;
            border-radius: 4px;
            font-family: var(--font-monospace);
            font-size: 0.85em;
            font-weight: 600;
        }

        .status-code.success {
            background: var(--success-bg);
            color: var(--success-text);
        }

        .status-code.error {
            background: var(--danger-bg);
            color: var(--danger-text);
        }

        .status-code.redirect {
            background: var(--warning-bg);
            color: var(--warning-text);
        }

        /* Tab Control */
        .tab-control {
            margin-top: 1rem;
        }

        .tab-header {
            display: flex;
            gap: 0;
            border-bottom: 1px solid var(--border-color);
        }

        .tab-button {
            padding: 8px 16px;
            background: transparent;
            border: none;
            border-bottom: 2px solid transparent;
            cursor: pointer;
            font-size: 0.85em;
            font-weight: 500;
            color: var(--text-secondary);
            transition: all 0.15s;
        }

        .tab-button:hover {
            color: var(--text-primary);
            background: var(--bg-hover);
        }

        .tab-button.active {
            color: var(--accent-color);
            border-bottom-color: var(--accent-color);
        }

        .tab-content {
            display: none;
            padding-top: 0;
        }

        .tab-content.active {
            display: block;
        }

        .tab-content pre {
            margin-top: 0.75rem;
            margin-bottom: 0;
        }

        /* Mobile Responsiveness */
        @media (max-width: 900px) {
            :root {
                --sidebar-width: 260px;
            }
    
            .sidebar {
                position: fixed;
                left: 0;
                top: 0;
                transform: translateX(-100%);
                transition: transform 0.3s ease;
                box-shadow: 2px 0 10px rgba(0,0,0,0.1);
                z-index: 1000;
            }
    
            .sidebar.open {
                transform: translateX(0);
            }
    
            .sidebar-overlay {
                display: none;
                position: fixed;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background: rgba(0,0,0,0.5);
                z-index: 999;
            }
    
            .sidebar-overlay.active {
                display: block;
            }
    
            main {
                margin-left: 0;
                max-width: 100%;
                padding: 20px 16px;
            }
    
            .content-header {
                margin-bottom: 20px;
                padding-bottom: 15px;
            }
    
            .content-header h1 {
                font-size: 1.5em;
            }
    
            .mobile-menu-btn {
                display: flex;
                align-items: center;
                justify-content: center;
                position: fixed;
                bottom: 20px;
                right: 20px;
                width: 56px;
                height: 56px;
                background: var(--accent-color);
                color: white;
                border: none;
                border-radius: 50%;
                font-size: 24px;
                cursor: pointer;
                box-shadow: 0 4px 12px rgba(0,0,0,0.3);
                z-index: 998;
            }
    
            .mobile-menu-btn:hover {
                background: #0558b8;
            }
    
            .endpoint-description > summary {
                flex-wrap: wrap;
                gap: 8px;
                padding: 12px;
            }
    
            .endpoint-description > summary > span:nth-child(1) {
                min-width: 50px;
                font-size: 0.7em;
            }
    
            .endpoint-description > summary > span:nth-child(2) {
                font-size: 0.8em;
                word-break: break-all;
            }
    
            .endpoint-description > summary > span:nth-child(3) {
                width: 100%;
                margin-top: 4px;
                font-size: 0.85em;
            }
    
            .endpoint-content {
                padding: 15px;
            }
    
            .endpoint-content h3 {
                font-size: 1.1em;
            }

            body.endpoint-page-open .endpoint-content-header h3 {
                font-size: 1.5em;
            }
    
            pre {
                font-size: 0.8em;
                padding: 12px;
                max-height: 350px;
            }
    
            .params-section > p {
                font-size: 0.95em;
            }
    
            .param-header {
                flex-wrap: wrap;
                gap: 6px;
            }
    
            .param-name {
                font-size: 0.9em;
            }
    
            .tab-header {
                overflow-x: auto;
                -webkit-overflow-scrolling: touch;
            }
    
            .tab-button {
                padding: 8px 12px;
                font-size: 0.8em;
                white-space: nowrap;
            }
    
            .group-section h2 {
                font-size: 1.2em;
                padding: 10px 0;
            }
    
            h2 {
                font-size: 1.3em;
            }
        }

        @media (max-width: 480px) {
            main {
                padding: 15px 12px;
            }
    
            .content-header h1 {
                font-size: 1.3em;
            }
    
            .endpoint-description > summary {
                padding: 10px;
            }
    
            .endpoint-content {
                padding: 12px;
            }

            .parameter-example > summary,
            .parameter-example-body {
                padding: 12px;
            }
    
            pre {
                font-size: 0.75em;
                padding: 10px;
                border-radius: 6px;
            }
    
            .status-code {
                font-size: 0.8em;
                padding: 2px 6px;
            }
        }

        /* Desktop - hide mobile elements */
        @media (min-width: 901px) {
            .mobile-menu-btn {
                display: none;
            }
    
            .sidebar-overlay {
                display: none !important;
            }
        }
        """;
}
