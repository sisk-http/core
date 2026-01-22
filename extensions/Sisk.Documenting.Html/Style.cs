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
    public const string EmbedTargetStyles = """
        iframe.sisk-embed-target {
            width: 100%;
            min-height: 400px;
            border: 1px solid #e1e4e8;
            border-radius: 8px;
            margin: 1em 0;
        }

        @media (prefers-color-scheme: dark) {
            iframe.sisk-embed-target {
                border-color: #1e1e1e;
            }
        }
        """;
    public const string EmbedStyles = """
        body.iframe-mode,
        html.iframe-mode {
            padding: 0;
            margin: 0;
        }
        body.iframe-mode .iframe-container {
            max-width: 100%;
            padding: 1.25rem;
            background: var(--bg-primary);
            border-radius: 12px;
            box-shadow: 0 4px 24px rgba(0, 0, 0, 0.15);
        }
        body.iframe-mode .endpoint {
            margin: 0;
        }
        body.iframe-mode .endpoint-description {
            border: none;
            background: transparent;
        }
        body.iframe-mode .endpoint-description[open] > .endpoint-content {
            padding-top: 0;
        }
        body.iframe-mode .endpoint-description > summary {
            display: none;
        }
        body.iframe-mode .endpoint-content {
            padding: 0;
        }
        body.iframe-mode .endpoint-content-header {
            margin-bottom: 1rem;
            padding-bottom: 1rem;
            border-bottom: 1px solid var(--border-color);
        }
        body.iframe-mode .endpoint-content-header h3 {
            font-size: 1.25rem;
            margin: 0;
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
        p, li { line-height: 1.6 }

        :root {
            --font-monospace: ui-monospace,SFMono-Regular,SF Mono,Menlo,Consolas,Liberation Mono,monospace;
            --sidebar-width: 280px;
            --border-color: #e1e4e8;
            --bg-secondary: #f6f8fa;
            --text-primary: #24292f;
            --text-secondary: #57606a;
            --accent-color: #0969da;
        }
        
        html, body { 
            margin: 0;
            background-color: #ffffff;
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

        .search-trigger {
            display: flex;
            align-items: center;
            gap: 8px;
            width: 100%;
            padding: 8px 12px;
            margin-top: 12px;
            background: var(--bg-secondary);
            border: 1px solid var(--border-color);
            border-radius: 6px;
            color: var(--text-secondary);
            font-size: 0.85em;
            cursor: pointer;
            transition: all 0.15s;
        }

        .search-trigger:hover {
            background: #e8ebef;
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
            background: white;
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
            background: white;
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
        }

        .search-result-item .method-badge {
            flex-shrink: 0;
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
            background: #fff3cd;
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
            background: var(--bg-secondary);
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
            background-color: #e8ebef;
            color: var(--text-primary);
            text-decoration: none;
            transition: none;
        }

        .nav-item.active {
            background-color: #dbeafe;
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
        }

        body.iframe-mode .copy-button-wrapper {
            margin-left: auto;
        }
        body.iframe-mode .open-ext-button {
            background: transparent;
            border: none;
            opacity: .5;
            cursor: pointer;
            transition: .25s ease;
            text-decoration: none;
        }
        body.iframe-mode .open-ext-button:hover {
            opacity: 1;
        }
        body.iframe-mode .open-ext-button svg {
            width: 16px;
            height: 16px;
            fill: #1a1a2e;
        }
        @media (prefers-color-scheme: dark) {
            body.iframe-mode .open-ext-button svg {
                fill: #e6edf3;
            }
        }

        /* Main Content */
        main {
            flex: 1;
            padding: 40px 60px;
            padding-left: calc(var(--sidebar-width) + 20px);
            width: 100%;
            max-width: 1400px;
            margin: 0 auto 0 auto;
            background: white;
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
            border: none;
            border-bottom: 1px solid var(--border-color);
            margin: 2rem 0;
        }

        /* Endpoint Card */
        .endpoint-card {
            background: white;
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
            background-color: #1e293b;
            color: #e2e8f0;
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
            background: #dbeafe;
            border: 1px solid #93c5fd;
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
            background: white;
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
            color: #000000cc;
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
            background: white;
            border: 1px solid var(--border-color);
            border-radius: 6px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
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
            background: #333;
            color: #fff;
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

        @media (prefers-color-scheme: dark) {
            .copy-toast {
                background: #e6edf3;
                color: #060606;
            }
        }

        @media (prefers-color-scheme: dark) {
            .endpoint-description > summary > span:nth-child(1) {
                color: #ffffffcc;
            }
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
            font-weight: 600;
            color: var(--accent-color);
        }

        .param-type {
            font-size: 0.85em;
            color: var(--text-secondary);
        }

        .param-required {
            font-size: 0.75em;
            font-weight: 600;
            color: #dc2626;
            background: #fef2f2;
            padding: 2px 6px;
            border-radius: 3px;
        }

        .param-description {
            color: var(--text-secondary);
            font-size: 0.9em;
        }

        .endpoint + .endpoint {
            margin-top: .5em;
        }

        :not(pre) > code {
            background-color: #eff1f3;
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
            background: white;
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
            background: #dcfce7;
            color: #166534;
        }

        .status-code.error {
            background: #fef2f2;
            color: #dc2626;
        }

        .status-code.redirect {
            background: #fef3c7;
            color: #92400e;
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
            background: var(--bg-secondary);
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

        /* Dark Theme */
        @media (prefers-color-scheme: dark) {
            :root {
                --border-color: #1e1e1e;
                --bg-secondary: #0c0c0c;
                --text-primary: #e6edf3;
                --text-secondary: #8b949e;
                --accent-color: #58a6ff;
            }
    
            html, body {
                background-color: #060606;
            }
    
            main {
                background: #060606;
            }
    
            .content-header h1 {
                color: var(--text-primary);
            }

            .copy-button svg {
                filter: invert(.9);
            }
    
            .group-section h2 {
                background: #060606;
            }
            
            .nav-item:hover {
                background-color: #1c1c1c;
            }
                
            .nav-item.active {
                background-color: #0c1829;
                border-left-color: var(--accent-color);
            }
    
            .endpoint-description {
                border-color: var(--border-color);
            }
    
            .endpoint-description > summary {
                background: #060606;
            }
    
            .endpoint-description > summary:hover,
            .endpoint-description[open] > summary {
                background-color: var(--bg-secondary);
            }
    
            .endpoint-content {
                background: #060606;
            }
    
            pre {
                background-color: #0c0c0c;
                color: #e6edf3;
            }
    
            :not(pre) > code {
                background-color: #1e1e1e;
                color: #e6edf3;
            }
    
            .param-name {
                color: var(--accent-color);
            }
    
            .param-required {
                background: #3d1f1f;
                color: #f85149;
            }
    
            .status-code.success {
                background: #1a3d1a;
                color: #3fb950;
            }
    
            .status-code.error {
                background: #3d1f1f;
                color: #f85149;
            }
    
            .status-code.redirect {
                background: #3d2f1a;
                color: #d29922;
            }
    
            .info-box {
                background: #1f3a5f;
                border-color: #388bfd;
            }
    
            a {
                color: var(--accent-color);
            }
    
            h1 a, h2 a {
                color: var(--text-primary);
            }
    
            .tab-button {
                color: var(--text-secondary);
            }
    
            .tab-button:hover {
                color: var(--text-primary);
                background: #1c1c1c;
            }
    
            .tab-button.active {
                color: var(--accent-color);
            }

            .dropdown-menu {
                background: #0c0c0c;
                box-shadow: 0 4px 12px rgba(0,0,0,0.5);
            }

            .search-trigger {
                background: #1c1c1c;
            }

            .search-trigger:hover {
                background: #1e1e1e;
            }

            .search-trigger kbd {
                background: #060606;
            }

            .search-dialog {
                background: #060606;
                border: 1px solid var(--border-color);
            }

            .search-result-name mark {
                background: #634d00;
            }

            .search-result-group {
                background: #1c1c1c;
            }
        }
        """;
}
