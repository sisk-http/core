namespace Sisk.Monitoring;

internal class Assets {
    public const string ThemeBootstrapScript = """
        (function() {
            const storageKey = 'sisk-monitoring-theme';
            const options = ['light', 'dark', 'system'];
            const media = window.matchMedia('(prefers-color-scheme: dark)');
            let preference = 'system';

            try {
                preference = localStorage.getItem(storageKey) ?? 'system';
            } catch {
            }

            if (!options.includes(preference)) {
                preference = 'system';
            }

            function applyTheme() {
                const resolved = preference === 'system' ? (media.matches ? 'dark' : 'light') : preference;
                document.documentElement.setAttribute('data-theme', resolved);
                document.querySelectorAll('[data-theme-option]').forEach(button => {
                    button.setAttribute('aria-pressed', button.getAttribute('data-theme-option') === preference ? 'true' : 'false');
                });
            }

            media.addEventListener('change', applyTheme);
            document.addEventListener('DOMContentLoaded', applyTheme);
            document.addEventListener('click', event => {
                const button = event.target instanceof Element ? event.target.closest('[data-theme-option]') : null;
                if (!button) {
                    return;
                }

                preference = button.getAttribute('data-theme-option') ?? 'system';
                try {
                    localStorage.setItem(storageKey, preference);
                } catch {
                }
                applyTheme();
            });

            applyTheme();
        })();
        """;

    public const string DefaultStyles = """
        :root {
            color-scheme: light;
            --background: #fff;
            --surface: #ffffff;
            --surface-secondary: #f1f3f4;
            --border: #dadce0;
            --text-primary: #202124;
            --text-secondary: #5f6368;
            --accent: #1a73e8;
            --accent-hover: #1557b0;
            --danger: #d93025;
            --overlay: rgba(0,0,0,.10);
            --text-muted: var(--text-secondary);
            --accent-surface: color-mix(in srgb, var(--accent) 12%, var(--surface));
            --accent-surface-hover: color-mix(in srgb, var(--accent) 18%, var(--surface));
            --danger-surface: color-mix(in srgb, var(--danger) 14%, var(--surface));
            --warning: #8a6d00;
            --warning-surface: color-mix(in srgb, var(--warning) 14%, var(--surface));
            --selected-text: var(--surface);
            --focus-ring: color-mix(in srgb, var(--accent) 65%, transparent);
            --shadow: 0 1px 2px rgba(0,0,0,.08);
            --chart-cpu: var(--accent);
            --chart-memory: #188038;
            --chart-disk: #b06000;
            --chart-http-success: #188038;
            --chart-http-redirect: var(--accent);
            --chart-http-client-error: #b06000;
            --chart-http-server-error: var(--danger);
            --log-token-0-text: #1a73e8;
            --log-token-1-text: #188038;
            --log-token-2-text: #b06000;
            --log-token-3-text: #d93025;
            --log-token-4-text: #8430ce;
            --log-token-5-text: #00796b;
            --log-token-6-text: #c2185b;
            --log-token-7-text: #5f6368;
            --log-token-0-bg: color-mix(in srgb, var(--log-token-0-text) 14%, var(--surface));
            --log-token-1-bg: color-mix(in srgb, var(--log-token-1-text) 14%, var(--surface));
            --log-token-2-bg: color-mix(in srgb, var(--log-token-2-text) 14%, var(--surface));
            --log-token-3-bg: color-mix(in srgb, var(--log-token-3-text) 14%, var(--surface));
            --log-token-4-bg: color-mix(in srgb, var(--log-token-4-text) 14%, var(--surface));
            --log-token-5-bg: color-mix(in srgb, var(--log-token-5-text) 14%, var(--surface));
            --log-token-6-bg: color-mix(in srgb, var(--log-token-6-text) 14%, var(--surface));
            --log-token-7-bg: color-mix(in srgb, var(--log-token-7-text) 14%, var(--surface));
            --font-mono: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
            --font-sans: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
            --sidebar-width: 260px;
        }

        :root[data-theme="dark"] {
            color-scheme: dark;
            --background: #202124;
            --surface: #292a2d;
            --surface-secondary: #35363a;
            --border: #5f6368;
            --text-primary: #e8eaed;
            --text-secondary: #9aa0a6;
            --accent: #8ab4f8;
            --accent-hover: #aecbfa;
            --danger: #f28b82;
            --overlay: rgba(255,255,255,.08);
            --warning: #fdd663;
            --selected-text: #202124;
            --shadow: 0 1px 2px rgba(0,0,0,.40);
            --chart-cpu: var(--accent);
            --chart-memory: #81c995;
            --chart-disk: #fdd663;
            --chart-http-success: #81c995;
            --chart-http-client-error: #fdd663;
            --chart-http-server-error: var(--danger);
            --log-token-0-text: #8ab4f8;
            --log-token-1-text: #81c995;
            --log-token-2-text: #fdd663;
            --log-token-3-text: #f28b82;
            --log-token-4-text: #d7aefb;
            --log-token-5-text: #78d9c6;
            --log-token-6-text: #ffb1c8;
            --log-token-7-text: #c4c7c5;
        }

        *, *::before, *::after {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        html, body {
            height: 100%;
            font-family: var(--font-sans);
            font-size: 14px;
            line-height: 1.6;
            color: var(--text-primary);
            background: var(--background);
            transition: background-color 0.15s, border-color 0.15s, color 0.15s;
        }

        .page-wrapper {
            display: flex;
            min-height: 100vh;
        }

        /* Sidebar */
        .sidebar {
            display: flex;
            flex-direction: column;
            width: var(--sidebar-width);
            background: var(--surface);
            padding: 1.5rem 0;
            position: fixed;
            top: 0;
            left: 0;
            bottom: 0;
            overflow-y: auto;
            z-index: 10;
        }

        .sidebar-header {
            padding: 0 1.25rem 1.25rem;
            border-bottom: 1px solid var(--border);
            margin-bottom: 1rem;
        }

        .sidebar-header h1 {
            font-size: 1.1rem;
            font-weight: 700;
            margin-bottom: 0.15rem;
        }

        .sidebar-header .version {
            font-size: 0.75rem;
            color: var(--text-muted);
        }

        .nav-section {
            padding: 0 0.75rem;
        }

        .nav-section-title {
            font-size: 0.7rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            color: var(--text-muted);
            padding: 0.5rem 0.5rem 0.25rem;
        }

        .nav-group-title {
            font-size: 0.68rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.04em;
            color: var(--text-muted);
            padding: 0.55rem 0.75rem 0.2rem;
        }

        .nav-item {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.4rem 0.75rem;
            border-radius: 66px;
            color: var(--text-secondary);
            text-decoration: none;
            font-size: 0.85rem;
            margin-bottom: 3px;
        }

        .nav-item:hover {
            background: var(--accent-surface-hover);
            color: var(--accent-hover);
        }

        .nav-item.active {
            background: var(--accent-surface);
            color: var(--accent);
            font-weight: 600;
        }

        .nav-icon {
            width: 16px;
            height: 16px;
            flex-shrink: 0;
        }

        .nav-icon svg {
            width: 100%;
            height: 100%;
        }

        .theme-switcher {
            display: flex;
            gap: 0.25rem;
            margin: auto 1.25rem 0;
            padding: 0.25rem;
            border: 1px solid var(--border);
            border-radius: 99px;
            background: var(--surface-secondary);
            align-self: flex-start;
        }

        .theme-option {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 30px;
            height: 26px;
            border: none;
            border-radius: 99px;
            background: transparent;
            color: var(--text-secondary);
            cursor: pointer;
            transition: background-color 0.15s, color 0.15s;
        }

        .theme-option svg {
            width: 15px;
            height: 15px;
        }

        .theme-option:hover {
            color: var(--accent-hover);
        }

        .theme-option:focus-visible {
            outline: none;
            box-shadow: 0 0 0 3px var(--focus-ring);
        }

        .theme-option[aria-pressed="true"] {
            background: var(--surface);
            color: var(--accent);
            box-shadow: var(--shadow);
        }

        /* Main content */
        main {
            flex: 1;
            margin: 0 auto;
            padding: 2rem 2.5rem;
            padding-left: 70px;
            max-width: 1000px;
            min-width: 0;
        }

        .content-header {
            margin-bottom: 2rem;
        }

        .content-header h1 {
            font-size: 1.5rem;
            font-weight: 700;
            margin-bottom: 0.25rem;
        }

        .content-header .description {
            color: var(--text-secondary);
            font-size: 0.9rem;
        }

        /* Cards grid */
        .cards-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
            gap: 1rem;
            margin-bottom: 2rem;
        }

        .card {
            background: var(--surface);
            border: 1px solid var(--border);
            border-radius: 8px;
            padding: 1.25rem;
            box-shadow: var(--shadow);
        }

        .card-label {
            font-size: 0.75rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.03em;
            color: var(--text-muted);
            margin-bottom: 0.5rem;
        }

        .card-value {
            font-size: 1.75rem;
            font-weight: 700;
            color: var(--text-primary);
            line-height: 1.2;
        }

        /* Section */
        .section {
            margin-bottom: 2rem;
        }

        .section h2 {
            font-size: 1.1rem;
            font-weight: 600;
            margin-bottom: 1rem;
            padding-bottom: 0.5rem;
            border-bottom: 1px solid var(--border);
        }

        .group-block {
            margin-bottom: 1rem;
        }

        .group-block:last-child {
            margin-bottom: 0;
        }

        .group-title {
            font-size: 0.85rem;
            font-weight: 600;
            color: var(--accent);
            margin-bottom: 0.6rem;
        }

        .group-block .cards-grid,
        .group-block .stream-list {
            margin-bottom: 0;
        }

        /* Log stream list */
        .stream-list {
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
        }

        .stream-item {
            display: flex;
            align-items: center;
            justify-content: space-between;
            background: var(--surface);
            border: 1px solid var(--border);
            border-radius: 8px;
            padding: 1rem 1.25rem;
            text-decoration: none;
            color: var(--text-primary);
            box-shadow: var(--shadow);
            transition: background-color 0.15s, border-color 0.15s, color 0.15s;
        }

        .stream-item:hover {
            border-color: var(--accent-hover);
            background: var(--accent-surface);
        }

        .stream-item-label {
            font-weight: 600;
            font-size: 0.9rem;
        }

        .stream-item-badge {
            font-size: 0.7rem;
            padding: 0.2rem 0.6rem;
            border-radius: 99px;
            background: var(--accent-surface);
            color: var(--accent);
            font-weight: 600;
        }

        .stream-arrow {
            width: 18px;
            height: 18px;
            color: var(--text-muted);
        }

        .stream-arrow svg {
            width: 100%;
            height: 100%;
        }

        /* Log viewer */
        .log-viewer-header {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            margin-bottom: 1.5rem;
        }

        .log-viewer-header h1 {
            font-size: 1.25rem;
            font-weight: 700;
        }

        .back-link {
            display: inline-flex;
            align-items: center;
            gap: 0.3rem;
            color: var(--accent);
            text-decoration: none;
            font-size: 0.85rem;
            font-weight: 500;
        }

        .back-link:hover {
            text-decoration: underline;
        }

        .back-link svg {
            width: 16px;
            height: 16px;
        }

        .log-content {
            background: var(--surface);
            border: 1px solid var(--border);
            border-radius: 8px;
            font-family: var(--font-mono);
            font-size: 0.8rem;
            line-height: 1.7;
            white-space: pre-wrap;
            word-break: break-word;
            overflow-x: auto;
            max-height: 75vh;
            overflow-y: auto;
            box-shadow: var(--shadow);
            color: var(--text-primary);
        }

        .log-line {
            position: relative;
            white-space: pre-wrap;
            word-break: break-word;
            padding: 0.15rem 2.75rem 0.15rem 1em;
            border-bottom: 1px solid var(--border);
        }

        .log-line:hover {
            background-color: var(--surface-secondary);
        }

        .log-copy-btn {
            position: absolute;
            top: 50%;
            right: 0.35rem;
            transform: translateY(-50%);
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 1.65rem;
            height: 1.65rem;
            border: 1px solid var(--border);
            border-radius: 6px;
            background: var(--surface);
            color: var(--text-secondary);
            box-shadow: var(--shadow);
            cursor: pointer;
            opacity: 0;
            pointer-events: none;
            z-index: 1;
            transition: background-color 0.15s, border-color 0.15s, color 0.15s;
        }

        .log-copy-btn svg {
            width: 0.95rem;
            height: 0.95rem;
        }

        .log-line:hover .log-copy-btn,
        .log-line:focus-within .log-copy-btn {
            opacity: 1;
            pointer-events: auto;
        }

        .log-copy-btn:hover,
        .log-copy-btn:focus-visible {
            border-color: var(--accent-hover);
            color: var(--accent-hover);
            outline: none;
            box-shadow: 0 0 0 3px var(--focus-ring);
        }

        .log-copy-btn.copied {
            border-color: var(--accent);
            color: var(--accent);
        }

        .log-line:last-child {
            border-bottom: none;
        }

        .log-token-date {
            color: var(--warning);
            font-weight: 600;
        }

        .log-token-tag {
            border-radius: 4px;
            padding: 0 0.2rem;
            font-weight: 600;
            color: var(--log-token-color);
            background: var(--log-token-background);
        }

        .log-token-tone-0 {
            --log-token-color: var(--log-token-0-text);
            --log-token-background: var(--log-token-0-bg);
        }

        .log-token-tone-1 {
            --log-token-color: var(--log-token-1-text);
            --log-token-background: var(--log-token-1-bg);
        }

        .log-token-tone-2 {
            --log-token-color: var(--log-token-2-text);
            --log-token-background: var(--log-token-2-bg);
        }

        .log-token-tone-3 {
            --log-token-color: var(--log-token-3-text);
            --log-token-background: var(--log-token-3-bg);
        }

        .log-token-tone-4 {
            --log-token-color: var(--log-token-4-text);
            --log-token-background: var(--log-token-4-bg);
        }

        .log-token-tone-5 {
            --log-token-color: var(--log-token-5-text);
            --log-token-background: var(--log-token-5-bg);
        }

        .log-token-tone-6 {
            --log-token-color: var(--log-token-6-text);
            --log-token-background: var(--log-token-6-bg);
        }

        .log-token-tone-7 {
            --log-token-color: var(--log-token-7-text);
            --log-token-background: var(--log-token-7-bg);
        }

        .log-token-number {
            color: var(--text-secondary);
            font-weight: 600;
        }

        body.log-expanded-page main {
            margin-left: var(--sidebar-width);
            margin-right: 0;
            max-width: none;
            width: calc(100% - var(--sidebar-width));
            height: 100dvh;
        }

        .log-content.log-expanded {
            max-height: none;
        }

        .log-content.log-empty {
            color: var(--text-muted);
            font-style: italic;
            text-align: center;
            padding: 3rem 1rem;
        }

        .log-toolbar {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 0.75rem;
            gap: 1rem;
            flex-wrap: wrap;
        }

        .log-toolbar-actions {
            display: flex;
            gap: 0.4rem;
        }

        .page-toolbar {
            display: flex;
            justify-content: flex-end;
            margin: -0.5rem 0 1.25rem;
        }

        .toolbar-btn {
            padding: 0.35rem 0.75rem;
            border: 1px solid var(--border);
            border-radius: 6px;
            background: var(--surface);
            color: var(--text-secondary);
            font-size: 0.8rem;
            font-family: var(--font-sans);
            cursor: pointer;
            text-decoration: none;
            transition: background-color 0.15s, border-color 0.15s, color 0.15s;
        }

        .toolbar-btn:hover {
            background: var(--accent-surface);
            border-color: var(--accent-hover);
            color: var(--accent-hover);
        }

        .toolbar-btn:focus-visible {
            border-color: var(--accent);
            color: var(--accent);
            outline: none;
            box-shadow: 0 0 0 3px var(--focus-ring);
        }

        .toolbar-btn.active {
            background: var(--accent);
            border-color: var(--accent);
            color: var(--selected-text);
        }

        .toolbar-btn:disabled,
        .toolbar-btn[aria-disabled="true"] {
            background: var(--surface-secondary);
            border-color: var(--border);
            color: var(--text-secondary);
            cursor: not-allowed;
        }

        .log-meta {
            display: flex;
            align-items: center;
            gap: 1rem;
            font-size: 0.8rem;
            color: var(--text-secondary);
        }

        .log-meta-item {
            display: flex;
            align-items: center;
            gap: 0.3rem;
        }

        .empty-state {
            text-align: center;
            padding: 3rem 1rem;
            color: var(--text-muted);
        }

        .empty-state p {
            font-size: 0.9rem;
        }

        /* Progress bar */
        .progress-bar {
            position: relative;
            height: 24px;
            background: var(--surface-secondary);
            border-radius: 12px;
            overflow: hidden;
            margin-top: 0.75rem;
            border: 1px solid var(--border);
        }

        .progress-fill {
            height: 100%;
            border-radius: 12px;
            transition: width 0.3s;
        }

        .progress-label {
            position: absolute;
            inset: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 0.75rem;
            font-weight: 600;
            color: var(--text-primary);
        }

        .http-stats-grid {
            display: grid;
            grid-template-columns: repeat(4, minmax(0, 1fr));
            gap: 0.75rem;
            margin-bottom: 1rem;
        }

        .http-stat-card {
            padding: 0.9rem 1rem;
        }

        .http-stat-card .card-value {
            font-size: 1.35rem;
            font-variant-numeric: tabular-nums;
        }

        .http-charts,
        .http-tables {
            display: grid;
            grid-template-columns: repeat(2, minmax(0, 1fr));
            gap: 1rem;
            margin-bottom: 1rem;
        }

        .http-chart-block .health-chart-container {
            height: 240px;
        }

        .http-table-block {
            min-width: 0;
        }

        .http-table-wrapper {
            border: 1px solid var(--border);
            border-radius: 8px;
            background: var(--surface);
            box-shadow: var(--shadow);
            overflow-x: auto;
        }

        .http-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.8rem;
            font-variant-numeric: tabular-nums;
        }

        .http-table th,
        .http-table td {
            padding: 0.5rem 0.75rem;
            text-align: right;
            white-space: nowrap;
            border-bottom: 1px solid var(--border);
        }

        .http-table th {
            font-size: 0.7rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.03em;
            color: var(--text-muted);
            background: var(--surface-secondary);
        }

        .http-table th:first-child,
        .http-table td:first-child {
            text-align: left;
            max-width: 260px;
            overflow: hidden;
            text-overflow: ellipsis;
            font-family: var(--font-mono);
        }

        .http-table tr:last-child td {
            border-bottom: none;
        }

        .http-table-empty {
            padding: 1.25rem;
            text-align: center;
            font-size: 0.8rem;
            color: var(--text-muted);
        }

        .issues-table td {
            white-space: normal;
            vertical-align: middle;
        }

        .issues-table th:first-child,
        .issues-table td:first-child {
            max-width: none;
            font-family: var(--font-sans);
        }

        .issue-title {
            color: var(--text-primary);
            font-size: 0.85rem;
            font-weight: 600;
            text-decoration: none;
            word-break: break-word;
        }

        .issue-title:hover {
            color: var(--accent);
        }

        .issue-meta {
            margin-top: 0.15rem;
            color: var(--text-muted);
            font-size: 0.75rem;
            font-weight: 400;
            word-break: break-word;
        }

        .issue-location {
            font-family: var(--font-mono);
        }

        .issue-trend {
            display: block;
            width: 120px;
            height: 28px;
            margin-left: auto;
        }

        .card-value .issue-trend {
            margin-left: 0;
        }

        .issue-trend rect {
            fill: var(--danger);
        }

        .issue-trend rect.empty {
            fill: var(--border);
        }

        .issue-occurrence {
            margin-bottom: 0.75rem;
        }

        .issue-occurrence summary {
            cursor: pointer;
            font-size: 0.85rem;
            font-weight: 600;
        }

        .issue-occurrence summary .issue-meta {
            margin-left: 0.6rem;
        }

        .issue-stack {
            margin: 0.75rem 0;
            padding: 0.75rem 1rem;
            max-height: 40vh;
        }

        .issue-section {
            margin-bottom: 0.75rem;
        }

        .meters-grid {
            grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
            margin-bottom: 0;
        }

        .meter-card {
            padding: 1rem;
            display: flex;
            flex-direction: column;
            gap: 0.6rem;
        }

        .meter-card-header {
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            justify-content: space-between;
            gap: 0.6rem;
        }

        .meter-card-header .card-label {
            margin-bottom: 0;
        }

        .meter-current-value {
            font-size: 1.3rem;
            margin-bottom: 0.1rem;
        }

        .meter-expand-btn {
            display: inline-flex;
            flex-shrink: 0;
            padding: 0.3rem;
            border-color: transparent;
            background: transparent;
        }

        .meter-expand-btn svg {
            width: 1rem;
            height: 1rem;
        }

        .meter-card-actions {
            display: flex;
            align-items: center;
            margin-left: auto;
            gap: 0.25rem;
        }

        .meter-card-actions .usage-history-periods {
            width: auto;
            gap: 0;
            padding: 2px;
            border-radius: 6px;
            background: var(--surface-secondary);
        }

        .meter-card-actions .usage-history-periods .toolbar-btn {
            flex: none;
            padding: 0.15rem 0.5rem;
            border-color: transparent;
            border-radius: 4px;
            background: transparent;
            font-size: 0.75rem;
            font-variant-numeric: tabular-nums;
        }

        .meter-card-actions .usage-history-periods .toolbar-btn:hover {
            color: var(--text-primary);
        }

        .meter-card-actions .usage-history-periods .toolbar-btn.active {
            background: var(--surface);
            color: var(--text-primary);
            font-weight: 600;
            box-shadow: var(--shadow);
        }

        .meter-summary {
            display: flex;
            flex-wrap: wrap;
            align-items: baseline;
            gap: 0.3rem 1rem;
            font-size: 0.8rem;
            color: var(--text-secondary);
        }

        .meter-summary strong {
            color: var(--text-primary);
            font-variant-numeric: tabular-nums;
        }

        .meter-chart-container {
            position: relative;
            height: 200px;
            overflow: hidden;
        }

        .meter-chart-empty {
            display: flex;
            align-items: center;
            justify-content: center;
            color: var(--text-muted);
            font-size: 0.8rem;
        }

        .meter-modal-overlay {
            position: fixed;
            inset: 0;
            background: var(--overlay);
            z-index: 50;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 1rem;
        }

        .meter-modal {
            width: min(920px, 100%);
            background: var(--surface);
            border: 1px solid var(--border);
            box-shadow: var(--shadow);
            border-radius: 10px;
            padding: 1rem;
            display: flex;
            flex-direction: column;
            gap: 1rem;
            max-height: calc(100vh - 2rem);
            overflow-y: auto;
        }

        .meter-modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 0.75rem;
        }

        .meter-modal-header h2 {
            font-size: 1.2rem;
            font-weight: 700;
        }

        .meter-modal-chart {
            border: 1px solid var(--border);
            border-radius: 8px;
            overflow: hidden;
        }

        .meter-modal-chart .meter-chart-container {
            height: 320px;
        }

        .usage-history-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1rem;
            margin-bottom: 0.75rem;
        }

        .usage-history-header h2 {
            margin: 0;
        }

        .usage-history-periods {
            display: flex;
            gap: 0.4rem;
        }

        .health-chart-container {
            position: relative;
            height: 320px;
            border: 1px solid var(--border);
            border-radius: 8px;
            background: var(--surface);
            overflow: hidden;
            box-shadow: var(--shadow);
        }

        .health-chart-svg {
            width: 100%;
            height: 100%;
            display: block;
        }

        .health-chart-axis,
        .health-chart-grid {
            stroke: var(--border);
            stroke-width: 1;
            vector-effect: non-scaling-stroke;
        }

        .health-chart-grid {
            opacity: 0.55;
        }

        .health-chart-line {
            fill: none;
            stroke-width: 2;
            stroke-linejoin: round;
            stroke-linecap: round;
            vector-effect: non-scaling-stroke;
        }

        .health-chart-crosshair {
            stroke: var(--accent);
            stroke-width: 1;
            stroke-dasharray: 3 3;
            opacity: 0;
            vector-effect: non-scaling-stroke;
        }

        .health-chart-point {
            stroke: var(--surface);
            stroke-width: 2;
            opacity: 0;
            vector-effect: non-scaling-stroke;
        }

        .health-chart-label {
            fill: var(--text-muted);
            font-size: 10px;
            font-family: var(--font-sans);
        }

        .health-chart-tooltip {
            position: absolute;
            background: var(--surface);
            color: var(--text-primary);
            border: 1px solid var(--border);
            box-shadow: var(--shadow);
            border-radius: 6px;
            font-size: 0.75rem;
            line-height: 1.35;
            white-space: nowrap;
            padding: 0.3rem 0.45rem;
            pointer-events: none;
            opacity: 0;
            transition: opacity 0.12s;
            z-index: 2;
        }

        .health-chart-legend {
            position: absolute;
            top: 0.75rem;
            right: 0.75rem;
            display: flex;
            gap: 0.75rem;
            flex-wrap: wrap;
            padding: 0.35rem 0.5rem;
            border: 1px solid var(--border);
            border-radius: 6px;
            background: var(--surface);
            font-size: 0.72rem;
            color: var(--text-secondary);
        }

        .health-chart-legend-item {
            display: inline-flex;
            align-items: center;
            gap: 0.3rem;
        }

        .health-chart-swatch {
            width: 0.65rem;
            height: 0.65rem;
            border-radius: 999px;
        }

        /* Mobile */
        .mobile-menu-btn {
            display: none;
            position: fixed;
            bottom: 1rem;
            right: 1rem;
            z-index: 20;
            width: 48px;
            height: 48px;
            border-radius: 50%;
            border: none;
            background: var(--accent);
            color: var(--selected-text);
            font-size: 1.25rem;
            cursor: pointer;
            box-shadow: var(--shadow);
            transition: background-color 0.15s, border-color 0.15s, color 0.15s;
        }

        .mobile-menu-btn:hover {
            background: var(--accent-hover);
        }

        .mobile-menu-btn:focus-visible {
            outline: none;
            box-shadow: 0 0 0 3px var(--focus-ring);
        }

        .sidebar-overlay {
            display: none;
            position: fixed;
            inset: 0;
            background: var(--overlay);
            z-index: 9;
        }

        @media (max-width: 768px) {
            .sidebar {
                transform: translateX(-100%);
                transition: transform 0.15s;
            }

            .sidebar.open {
                transform: translateX(0);
            }

            .sidebar-overlay.show {
                display: block;
            }

            main {
                margin: 0;
                padding: 1.5rem 1rem;
            }

            body.log-expanded-page main {
                margin-left: 0;
                margin-right: 0;
                width: 100%;
            }

            .mobile-menu-btn {
                display: flex;
                align-items: center;
                justify-content: center;
            }

            .cards-grid {
                grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
            }

            .http-stats-grid {
                grid-template-columns: repeat(2, minmax(0, 1fr));
            }

            .http-charts,
            .http-tables {
                grid-template-columns: 1fr;
            }

            .theme-switcher {
                margin-top: 1.5rem;
            }

            .usage-history-header {
                align-items: flex-start;
                flex-direction: column;
            }

            .usage-history-periods {
                width: 100%;
            }

            .usage-history-periods .toolbar-btn {
                flex: 1;
            }

            .meter-modal-chart .meter-chart-container {
                height: 250px;
            }
        }
        """;

    public const string DefaultScript = """
        document.querySelector('.mobile-menu-btn')?.addEventListener('click', () => {
            document.querySelector('.sidebar')?.classList.toggle('open');
            document.querySelector('.sidebar-overlay')?.classList.toggle('show');
        });
        document.querySelector('.sidebar-overlay')?.addEventListener('click', () => {
            document.querySelector('.sidebar')?.classList.remove('open');
            document.querySelector('.sidebar-overlay')?.classList.remove('show');
        });

        (function() {
            const logEl = document.getElementById('log-content');
            const btnTail = document.getElementById('btn-tail');
            const btnRefresh = document.getElementById('btn-refresh');
            const btnToggleRefresh = document.getElementById('btn-toggle-refresh');
            const isMetersPage = /\/meters\/?$/.test(location.pathname);
            const isHealthPage = /\/health\/?$/.test(location.pathname);
            const autoRefreshIntervalMs = isHealthPage ? 2000 : 10000;
            const metersGrid = document.querySelector('[data-meters-endpoint]');
            const metersEndpoint = metersGrid?.getAttribute('data-meters-endpoint') ?? null;
            const healthChart = document.querySelector('[data-health-endpoint]');
            const healthEndpoint = healthChart?.getAttribute('data-health-endpoint') ?? null;
            const healthPeriodButtons = Array.from(document.querySelectorAll('[data-health-period]'));
            const httpEndpoint = document.querySelector('[data-http-endpoint]')?.getAttribute('data-http-endpoint') ?? null;
            const httpPeriodButtons = Array.from(document.querySelectorAll('[data-http-period]'));
            const countersEndpoint = document.querySelector('[data-counters-endpoint]')?.getAttribute('data-counters-endpoint') ?? null;
            const logEndpoint = logEl?.getAttribute('data-logstream-endpoint') ?? null;
            const numberFormatter = new Intl.NumberFormat('en-US', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
            const integerFormatter = new Intl.NumberFormat('en-US');
            const dateFormatter = new Intl.DateTimeFormat(undefined, {
                month: '2-digit',
                day: '2-digit',
                hour: '2-digit',
                minute: '2-digit'
            });

            let tailEnabled = true;
            let autoRefreshInterval = null;
            let refreshEnabled = true;
            let tailManuallyChanged = false;
            let refreshManuallyChanged = false;
            let selectedHealthPeriod = '1h';
            let selectedHttpPeriod = '1h';
            let httpPayload = null;
            const meterPayloads = new Map();
            let renderedLogLineCount = 0;
            let renderedLogLastLine = null;

            function formatNumber(value) {
                const numeric = Number(value);
                if (!Number.isFinite(numeric) || numeric === 0) return '-';
                if (numeric > 0 && numeric < 0.01) return '~ 0.01';
                return numberFormatter.format(numeric);
            }

            function formatDate(value) {
                const date = value instanceof Date ? value : new Date(value);
                if (Number.isNaN(date.getTime())) return '-';
                return dateFormatter.format(date);
            }

            function parseHealthReadings(raw) {
                let parsed = raw;
                if (typeof raw === 'string') {
                    try {
                        parsed = JSON.parse(raw);
                    } catch {
                        parsed = [];
                    }
                }
                if (!Array.isArray(parsed)) {
                    return [];
                }
                return parsed
                    .map(item => {
                        const timestamp = new Date(item?.timestamp);
                        if (Number.isNaN(timestamp.getTime())) {
                            return null;
                        }
                        return {
                            timestamp,
                            cpu: Number(item?.cpu ?? 0),
                            disk: Number(item?.disk ?? 0),
                            memory: Number(item?.memory ?? 0)
                        };
                    })
                    .filter(item => item !== null)
                    .map(item => ({
                        timestamp: item.timestamp,
                        cpu: Number.isFinite(item.cpu) ? item.cpu : 0,
                        disk: Number.isFinite(item.disk) ? item.disk : 0,
                        memory: Number.isFinite(item.memory) ? item.memory : 0
                    }));
            }

            function createSvgElement(tag) {
                return document.createElementNS('http://www.w3.org/2000/svg', tag);
            }

            function renderHealthChart(container, readings, options = {}) {
                const chartHeight = options.height ?? 320;
                const width = Math.max(320, container.clientWidth || 320);
                const series = options.series ?? [
                    { key: 'cpu', label: 'CPU', color: 'var(--chart-cpu)' },
                    { key: 'memory', label: 'RAM', color: 'var(--chart-memory)' },
                    { key: 'disk', label: 'Disk', color: 'var(--chart-disk)' }
                ];
                const scaleMin = options.scaleMin ?? 0;
                const scaleMax = options.scaleMax ?? 100;
                const formatValue = options.formatValue ?? (value => `${formatNumber(value)}%`);
                const formatAxisValue = options.formatAxisValue ?? (value => `${value}%`);
                const axisTicks = [0, 0.25, 0.5, 0.75, 1].map(fraction => ({
                    fraction,
                    text: formatAxisValue(scaleMin + (fraction * (scaleMax - scaleMin)))
                }));
                const padding = {
                    top: 26,
                    right: 18,
                    bottom: 30,
                    left: Math.max(42, 14 + (6 * Math.max(...axisTicks.map(tick => tick.text.length))))
                };
                const plotWidth = Math.max(1, width - padding.left - padding.right);
                const plotHeight = Math.max(1, chartHeight - padding.top - padding.bottom);
                const firstTimestamp = readings[0]?.timestamp.getTime() ?? 0;
                const lastTimestamp = readings[readings.length - 1]?.timestamp.getTime() ?? firstTimestamp;
                const timestampRange = Math.max(1, lastTimestamp - firstTimestamp);

                container.innerHTML = '';

                if (!readings.length) {
                    container.classList.add('meter-chart-empty');
                    container.textContent = 'No data';
                    return;
                }

                container.classList.remove('meter-chart-empty');

                const svg = createSvgElement('svg');
                svg.setAttribute('class', 'health-chart-svg');
                svg.setAttribute('viewBox', `0 0 ${width} ${chartHeight}`);
                svg.setAttribute('preserveAspectRatio', 'none');

                axisTicks.forEach(({ fraction, text }) => {
                    const y = padding.top + ((1 - fraction) * plotHeight);

                    const grid = createSvgElement('line');
                    grid.setAttribute('class', fraction === 0 ? 'health-chart-axis' : 'health-chart-grid');
                    grid.setAttribute('x1', String(padding.left));
                    grid.setAttribute('x2', String(width - padding.right));
                    grid.setAttribute('y1', String(y));
                    grid.setAttribute('y2', String(y));
                    svg.appendChild(grid);

                    const label = createSvgElement('text');
                    label.setAttribute('class', 'health-chart-label');
                    label.setAttribute('x', String(padding.left - 8));
                    label.setAttribute('y', String(y + 3));
                    label.setAttribute('text-anchor', 'end');
                    label.textContent = text;
                    svg.appendChild(label);
                });

                const firstLabel = createSvgElement('text');
                firstLabel.setAttribute('class', 'health-chart-label');
                firstLabel.setAttribute('x', String(padding.left));
                firstLabel.setAttribute('y', String(chartHeight - 8));
                firstLabel.textContent = formatDate(readings[0].timestamp);
                svg.appendChild(firstLabel);

                const lastLabel = createSvgElement('text');
                lastLabel.setAttribute('class', 'health-chart-label');
                lastLabel.setAttribute('x', String(width - padding.right));
                lastLabel.setAttribute('y', String(chartHeight - 8));
                lastLabel.setAttribute('text-anchor', 'end');
                lastLabel.textContent = formatDate(readings[readings.length - 1].timestamp);
                svg.appendChild(lastLabel);

                const pointsBySeries = series.map(item => ({
                    ...item,
                    points: readings.map(reading => {
                        const x = padding.left + ((reading.timestamp.getTime() - firstTimestamp) / timestampRange * plotWidth);
                        const raw = reading[item.key];
                        const value = raw === null || raw === undefined ? null : Math.max(scaleMin, Math.min(scaleMax, Number(raw)));
                        const y = padding.top + ((scaleMax - (value ?? scaleMin)) / (scaleMax - scaleMin) * plotHeight);
                        return { x, y, value, reading };
                    })
                }));

                pointsBySeries.forEach(item => {
                    const line = createSvgElement('polyline');
                    line.setAttribute('class', 'health-chart-line');
                    line.setAttribute('stroke', item.color);
                    line.setAttribute('points', item.points
                        .filter(point => point.value !== null)
                        .map(point => `${point.x},${point.y}`)
                        .join(' '));
                    svg.appendChild(line);
                });

                const crosshair = createSvgElement('line');
                crosshair.setAttribute('class', 'health-chart-crosshair');
                svg.appendChild(crosshair);

                const markers = pointsBySeries.map(item => {
                    const marker = createSvgElement('circle');
                    marker.setAttribute('class', 'health-chart-point');
                    marker.setAttribute('fill', item.color);
                    marker.setAttribute('r', '4');
                    svg.appendChild(marker);
                    return { ...item, marker };
                });

                const legend = document.createElement('div');
                legend.className = 'health-chart-legend';
                series.forEach(item => {
                    const entry = document.createElement('span');
                    entry.className = 'health-chart-legend-item';
                    const swatch = document.createElement('span');
                    swatch.className = 'health-chart-swatch';
                    swatch.style.background = item.color;
                    const label = document.createElement('span');
                    label.textContent = item.label;
                    entry.appendChild(swatch);
                    entry.appendChild(label);
                    legend.appendChild(entry);
                });

                container.appendChild(svg);
                if (options.legend !== false) {
                    container.appendChild(legend);
                }

                const tooltip = document.createElement('div');
                tooltip.className = 'health-chart-tooltip';
                container.appendChild(tooltip);

                function hideHealthHover() {
                    crosshair.style.opacity = '0';
                    tooltip.style.opacity = '0';
                    markers.forEach(item => {
                        item.marker.style.opacity = '0';
                    });
                }

                svg.addEventListener('mousemove', event => {
                    const bounds = svg.getBoundingClientRect();
                    const relativeX = ((event.clientX - bounds.left) / bounds.width) * width;
                    const index = pointsBySeries[0].points.reduce((nearestIndex, point, pointIndex, points) =>
                        Math.abs(point.x - relativeX) < Math.abs(points[nearestIndex].x - relativeX) ? pointIndex : nearestIndex, 0);
                    const reading = readings[index];
                    const x = pointsBySeries[0].points[index].x;

                    crosshair.setAttribute('x1', String(x));
                    crosshair.setAttribute('x2', String(x));
                    crosshair.setAttribute('y1', String(padding.top));
                    crosshair.setAttribute('y2', String(padding.top + plotHeight));
                    crosshair.style.opacity = '1';

                    markers.forEach(item => {
                        const point = item.points[index];
                        item.marker.setAttribute('cx', String(point.x));
                        item.marker.setAttribute('cy', String(point.y));
                        item.marker.style.opacity = point.value === null ? '0' : '1';
                    });

                    tooltip.innerHTML = [
                        `<strong>${formatDate(reading.timestamp)}</strong>`,
                        ...series.map(item => `${escapeHtml(item.label)}: ${escapeHtml(formatValue(reading[item.key]))}`)
                    ].join('<br>');
                    tooltip.style.opacity = '1';

                    const tooltipMargin = 8;
                    const tooltipWidth = tooltip.offsetWidth;
                    const tooltipHeight = tooltip.offsetHeight;

                    let tooltipLeft = x - (tooltipWidth / 2);
                    tooltipLeft = Math.max(tooltipMargin, Math.min(width - tooltipWidth - tooltipMargin, tooltipLeft));

                    let tooltipTop = padding.top + 12;
                    if (event.clientY - bounds.top < chartHeight / 2) {
                        tooltipTop = chartHeight - padding.bottom - tooltipHeight - 10;
                    }
                    tooltipTop = Math.max(tooltipMargin, Math.min(chartHeight - tooltipHeight - tooltipMargin, tooltipTop));

                    tooltip.style.left = `${tooltipLeft}px`;
                    tooltip.style.top = `${tooltipTop}px`;
                });

                svg.addEventListener('mouseleave', hideHealthHover);
            }

            function parseHttpReadings(raw) {
                if (!Array.isArray(raw)) {
                    return [];
                }
                return raw
                    .map(item => {
                        const timestamp = new Date(item?.timestamp);
                        if (Number.isNaN(timestamp.getTime())) {
                            return null;
                        }
                        return {
                            timestamp,
                            requests: Number(item?.totals?.requests) || 0,
                            success: Number(item?.totals?.success) || 0,
                            redirect: Number(item?.totals?.redirect) || 0,
                            clientError: Number(item?.totals?.clientError) || 0,
                            serverError: Number(item?.totals?.serverError) || 0,
                            p50: Number(item?.latency?.p50) || 0,
                            p95: Number(item?.latency?.p95) || 0,
                            p99: Number(item?.latency?.p99) || 0
                        };
                    })
                    .filter(item => item !== null);
            }

            function niceScaleMax(rawMax) {
                const magnitude = 10 ** Math.floor(Math.log10(rawMax));
                return Math.ceil(rawMax / (magnitude / 2)) * (magnitude / 2);
            }

            function renderHttpChart(container, readings, series, formatValue) {
                const rawMax = Math.max(1, ...readings.flatMap(reading => series.map(item => Number(reading[item.key] ?? 0))));
                const scaleMax = Math.max(4, niceScaleMax(rawMax));

                renderHealthChart(container, readings, {
                    series,
                    scaleMax,
                    height: 240,
                    formatValue,
                    formatAxisValue: value => String(Math.round(value))
                });
            }

            function renderHttpCharts() {
                const readings = httpPayload ? parseHttpReadings(httpPayload.readings) : [];
                const responsesChart = document.querySelector('[data-http-chart="responses"]');
                const latencyChart = document.querySelector('[data-http-chart="latency"]');

                if (responsesChart) {
                    renderHttpChart(responsesChart, readings, [
                        { key: 'success', label: '2xx', color: 'var(--chart-http-success)' },
                        { key: 'redirect', label: '3xx', color: 'var(--chart-http-redirect)' },
                        { key: 'clientError', label: '4xx', color: 'var(--chart-http-client-error)' },
                        { key: 'serverError', label: '5xx', color: 'var(--chart-http-server-error)' }
                    ], value => formatInteger(value));
                }

                if (latencyChart) {
                    renderHttpChart(latencyChart, readings.filter(reading => reading.requests > 0), [
                        { key: 'p50', label: 'p50', color: 'var(--chart-http-success)' },
                        { key: 'p95', label: 'p95', color: 'var(--chart-http-client-error)' },
                        { key: 'p99', label: 'p99', color: 'var(--chart-http-server-error)' }
                    ], value => formatMilliseconds(value));
                }
            }

            function formatMilliseconds(value) {
                const numeric = Number(value);
                if (!Number.isFinite(numeric) || numeric <= 0) return '-';
                if (numeric >= 1000) return `${(numeric / 1000).toFixed(2)} s`;
                if (numeric >= 100) return `${Math.round(numeric)} ms`;
                return `${numeric.toFixed(1)} ms`;
            }

            function closeMeterModal() {
                document.querySelector('.meter-modal-overlay')?.remove();
            }

            function escapeHtml(value) {
                return String(value)
                    .replaceAll('&', '&amp;')
                    .replaceAll('<', '&lt;')
                    .replaceAll('>', '&gt;')
                    .replaceAll('"', '&quot;')
                    .replaceAll("'", '&#039;');
            }

            function openMeterModal(card) {
                closeMeterModal();

                const label = escapeHtml(card.getAttribute('data-meter-label') ?? 'Meter');
                const overlay = document.createElement('div');
                overlay.className = 'meter-modal-overlay';
                overlay.setAttribute('data-meter-id', card.getAttribute('data-meter-id') ?? '');
                overlay.innerHTML = `
                    <div class="meter-modal" role="dialog" aria-modal="true" aria-label="${label}">
                        <div class="meter-modal-header">
                            <h2>${label}</h2>
                            <div class="meter-card-actions">
                                ${card.querySelector('.usage-history-periods')?.outerHTML ?? ''}
                                <button type="button" class="toolbar-btn meter-modal-close-btn">Close</button>
                            </div>
                        </div>
                        <div class="meter-summary"></div>
                        <div class="meter-modal-chart">
                            <div class="meter-chart-container"></div>
                        </div>
                        <div class="http-table-wrapper" data-meter-stats></div>
                    </div>
                `;

                document.body.appendChild(overlay);
                renderMeter(card);

                overlay.addEventListener('click', event => {
                    if (event.target === overlay) {
                        closeMeterModal();
                    }
                });
                overlay.querySelector('.meter-modal-close-btn')?.addEventListener('click', () => closeMeterModal());
            }

            function renderMeter(card) {
                const payload = meterPayloads.get(card.getAttribute('data-meter-id'));
                if (!payload) {
                    return;
                }

                const { minimum: scaleMin, maximum: scaleMax, ticks } = payload.axis;
                const texts = new Map(ticks.map((text, index) => [scaleMin + ((index / 4) * (scaleMax - scaleMin)), text]));
                payload.series.forEach(item => texts.set(item.summary, item.summaryText));
                payload.readings.forEach(reading => reading.values.forEach((value, index) => texts.set(value, reading.texts[index])));

                const formatValue = value => texts.get(value) ?? '-';

                const series = payload.series.map((item, index) => ({
                    key: `s${index}`,
                    label: item.name || payload.label,
                    color: `var(--log-token-${index % 8}-text)`,
                    summary: item.summary
                }));
                const readings = payload.readings.map(reading => ({
                    timestamp: new Date(reading.timestamp),
                    ...Object.fromEntries(reading.values.map((value, index) => [`s${index}`, value]))
                }));

                const summary = series.length <= 1
                    ? `<span class="card-value meter-current-value">${escapeHtml(formatValue(series[0]?.summary))}</span>`
                    : series.map(item => `
                        <span class="health-chart-legend-item">
                            <span class="health-chart-swatch" style="background: ${item.color}"></span>
                            <span>${escapeHtml(item.label)}</span>
                            <strong>${escapeHtml(formatValue(item.summary))}</strong>
                        </span>`).join('');

                const overlay = document.querySelector('.meter-modal-overlay');
                const targets = overlay?.getAttribute('data-meter-id') === payload.id ? [card, overlay] : [card];
                const selectedPeriod = card.getAttribute('data-selected-period');

                targets.forEach(target => {
                    const chart = target.querySelector('.meter-chart-container');

                    target.querySelectorAll('[data-meter-period]').forEach(button => {
                        const selected = button.getAttribute('data-meter-period') === selectedPeriod;
                        button.classList.toggle('active', selected);
                        button.setAttribute('aria-pressed', selected ? 'true' : 'false');
                    });

                    target.querySelector('.meter-summary').innerHTML = summary;
                    renderHealthChart(chart, readings, {
                        series,
                        scaleMin,
                        scaleMax,
                        height: chart.clientHeight,
                        legend: false,
                        formatValue,
                        formatAxisValue: formatValue
                    });
                });

                if (targets.length > 1) {
                    const summaryLabel = { Sum: 'Total', Average: 'Average', Maximum: 'Peak', Last: 'Latest' }[payload.aggregation];
                    const rows = series.map(item => {
                        const seriesValues = readings.map(reading => reading[item.key]).filter(value => value !== null);
                        const cells = [
                            item.label,
                            formatValue(item.summary),
                            formatValue(seriesValues.length ? Math.min(...seriesValues) : null),
                            formatValue(seriesValues.length ? Math.max(...seriesValues) : null)
                        ];

                        return `<tr>${cells.map(cell => `<td>${escapeHtml(cell)}</td>`).join('')}</tr>`;
                    }).join('');

                    overlay.querySelector('[data-meter-stats]').innerHTML =
                        `<table class="http-table"><thead><tr><th>Series</th><th>${summaryLabel}</th><th>Min</th><th>Max</th></tr></thead><tbody>${rows}</tbody></table>`;
                }
            }

            function refreshMeters() {
                if (!metersEndpoint) {
                    return;
                }

                const cards = Array.from(document.querySelectorAll('.meter-card'));
                const separator = metersEndpoint.includes('?') ? '&' : '?';

                new Set(cards.map(card => card.getAttribute('data-selected-period'))).forEach(period => {
                    fetchJson(`${metersEndpoint}${separator}period=${encodeURIComponent(period)}`)
                        .then(payload => {
                            if (!Array.isArray(payload)) {
                                return;
                            }

                            payload.forEach(item => {
                                const card = cards.find(candidate =>
                                    candidate.getAttribute('data-meter-id') === item.id
                                    && candidate.getAttribute('data-selected-period') === item.period);

                                if (card) {
                                    meterPayloads.set(item.id, item);
                                    renderMeter(card);
                                }
                            });
                        });
                });
            }

            function refreshHealth() {
                if (!healthEndpoint || !healthChart) {
                    return;
                }

                const separator = healthEndpoint.includes('?') ? '&' : '?';
                fetchJson(`${healthEndpoint}${separator}period=${encodeURIComponent(selectedHealthPeriod)}`)
                    .then(payload => {
                        if (!payload || !Array.isArray(payload.readings)) {
                            return;
                        }
                        healthChart.setAttribute('data-health-readings', JSON.stringify(payload.readings));
                        renderHealthChart(healthChart, parseHealthReadings(payload.readings));
                    })
                    .catch(() => {
                    });
            }

            healthPeriodButtons.forEach(button => {
                button.addEventListener('click', () => {
                    selectedHealthPeriod = button.getAttribute('data-health-period') ?? '1h';
                    healthPeriodButtons.forEach(item => {
                        const selected = item === button;
                        item.classList.toggle('active', selected);
                        item.setAttribute('aria-pressed', selected ? 'true' : 'false');
                    });
                    refreshHealth();
                });
            });

            function fetchJson(url) {
                // Resolve the endpoint against the document base and strips any embedded credentials:
                // when the dashboard URL itself carries user:pass, relative fetch() calls would throw.
                let target = url;
                try {
                    const resolved = new URL(url, location.href);
                    resolved.username = '';
                    resolved.password = '';
                    target = resolved.origin + resolved.pathname + resolved.search;
                } catch {
                }

                return fetch(target, { cache: 'no-store' })
                    .then(response => response.ok ? response.json() : null)
                    .catch(() => null);
            }

            function formatInteger(value) {
                const numeric = Number(value);
                return Number.isFinite(numeric) ? integerFormatter.format(numeric) : '-';
            }

            function applyCounterValues(counters) {
                if (!Array.isArray(counters)) {
                    return;
                }

                const byId = new Map(counters
                    .filter(counter => counter && typeof counter.id === 'string')
                    .map(counter => [counter.id, Number(counter.value ?? 0)]));

                document.querySelectorAll('.card[data-counter-id]').forEach(card => {
                    const counterId = card.getAttribute('data-counter-id');
                    if (!byId.has(counterId)) {
                        return;
                    }

                    const valueElement = card.querySelector('.card-value');
                    if (valueElement) {
                        valueElement.textContent = formatNumber(byId.get(counterId));
                    }
                });
            }

            function refreshCounters() {
                if (!countersEndpoint) {
                    return;
                }

                fetchJson(countersEndpoint)
                    .then(payload => {
                        applyCounterValues(payload);
                    });
            }

            function renderHttpStats(http) {
                const values = {
                    requests: formatInteger(http.totals?.requests ?? 0),
                    rpm: Number(http.requestsPerMinute ?? 0).toFixed(1),
                    errorRate: `${Number(http.errorRate ?? 0).toFixed(2)}%`,
                    inFlight: formatInteger(http.inFlight ?? 0),
                    p50: formatMilliseconds(http.latency?.p50),
                    p95: formatMilliseconds(http.latency?.p95),
                    p99: formatMilliseconds(http.latency?.p99),
                    max: formatMilliseconds(http.latency?.max)
                };

                document.querySelectorAll('[data-http-stat]').forEach(element => {
                    element.textContent = values[element.getAttribute('data-http-stat')] ?? '-';
                });
            }

            function renderHttpTable(key, routes, columns) {
                const wrapper = document.querySelector(`[data-http-table="${key}"]`);
                if (!wrapper) {
                    return;
                }

                if (!routes.length) {
                    wrapper.innerHTML = '<p class="http-table-empty">No requests recorded.</p>';
                    return;
                }

                const header = columns.map(column => `<th>${column.label}</th>`).join('');
                const rows = routes.map(route => `<tr>${columns.map(column => {
                    const value = escapeHtml(column.value(route));
                    return column.label === 'Route' ? `<td title="${value}">${value}</td>` : `<td>${value}</td>`;
                }).join('')}</tr>`).join('');

                wrapper.innerHTML = `<table class="http-table"><thead><tr>${header}</tr></thead><tbody>${rows}</tbody></table>`;
            }

            function renderHttpTables(http) {
                const routes = Array.isArray(http.routes) ? http.routes : [];
                const routeColumn = { label: 'Route', value: route => route.route };
                const requestsColumn = { label: 'Requests', value: route => formatInteger(route.totals.requests) };

                renderHttpTable('slowest', [...routes]
                    .sort((a, b) => b.latency.p95 - a.latency.p95)
                    .slice(0, 10), [
                    routeColumn,
                    requestsColumn,
                    { label: 'p50', value: route => formatMilliseconds(route.latency.p50) },
                    { label: 'p95', value: route => formatMilliseconds(route.latency.p95) },
                    { label: 'Max', value: route => formatMilliseconds(route.latency.max) }
                ]);

                renderHttpTable('errors', routes
                    .filter(route => route.totals.clientError + route.totals.serverError > 0)
                    .sort((a, b) => (b.totals.serverError - a.totals.serverError) || (b.totals.clientError - a.totals.clientError))
                    .slice(0, 10), [
                    routeColumn,
                    requestsColumn,
                    { label: '4xx', value: route => formatInteger(route.totals.clientError) },
                    { label: '5xx', value: route => formatInteger(route.totals.serverError) },
                    { label: '5xx %', value: route => `${(route.totals.serverError * 100 / route.totals.requests).toFixed(1)}%` }
                ]);
            }

            function refreshDashboard() {
                if (!httpEndpoint) {
                    return;
                }

                const separator = httpEndpoint.includes('?') ? '&' : '?';
                fetchJson(`${httpEndpoint}${separator}period=${encodeURIComponent(selectedHttpPeriod)}`)
                    .then(payload => {
                        if (!payload) {
                            return;
                        }

                        if (payload.http) {
                            httpPayload = payload.http;
                            renderHttpStats(httpPayload);
                            renderHttpCharts();
                            renderHttpTables(httpPayload);
                        }

                        const recentIssues = document.querySelector('[data-recent-issues]');
                        if (recentIssues) {
                            recentIssues.innerHTML = payload.issues
                                || '<div class="http-table-wrapper"><p class="http-table-empty">No unresolved issues.</p></div>';
                        }

                        applyCounterValues(payload.counters);
                    });
            }

            httpPeriodButtons.forEach(button => {
                button.addEventListener('click', () => {
                    selectedHttpPeriod = button.getAttribute('data-http-period') ?? '1h';
                    httpPeriodButtons.forEach(item => {
                        const selected = item === button;
                        item.classList.toggle('active', selected);
                        item.setAttribute('aria-pressed', selected ? 'true' : 'false');
                    });
                    refreshDashboard();
                });
            });

            const logDateTokenRegex = /\b(\d{4}[-\/]\d{1,2}[-\/]\d{1,2}(?:[ T]\d{1,2}:\d{2}(?::\d{2})?(?:\s?(?:Z|[+-]\d{2}:?\d{2}|[+-]?\d{4}))?)?|\d{1,2}\/(?:\d{1,2}|[A-Za-z]{3,9})\/\d{2,4}(?:\s+\d{1,2}:\d{2}(?::\d{2})?)?(?:\s+[+-]?\d{4})?)\b/gi;
            const logDateTokenTestRegex = new RegExp(logDateTokenRegex.source, 'i');
            const logTimeTokenRegex = /^\d{1,2}:\d{2}(?::\d{2})?(?:[\.,]\d+)?(?:\s?(?:AM|PM|Z|[+-]\d{2}:?\d{2}|[+-]?\d{4}))?$/i;
            const logNumberTokenRegex = /(?<![A-Za-z])[-+]?\d+(?:[\.,]\d+)?(?![A-Za-z])/g;
            const logBracketTokenRegex = /\[[^\]\r\n]+\]/g;
            const copyIconSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M6.9998 6V3C6.9998 2.44772 7.44752 2 7.9998 2H19.9998C20.5521 2 20.9998 2.44772 20.9998 3V17C20.9998 17.5523 20.5521 18 19.9998 18H16.9998V20.9991C16.9998 21.5519 16.5499 22 15.993 22H4.00666C3.45059 22 3 21.5554 3 20.9991L3.0026 7.00087C3.0027 6.44811 3.45264 6 4.00942 6H6.9998ZM5.00242 8L5.00019 20H14.9998V8H5.00242ZM8.9998 6H16.9998V16H18.9998V4H8.9998V6Z"></path></svg>';

            function bracketToneClass(token) {
                let hash = 2166136261;
                for (let i = 0; i < token.length; i++) {
                    hash ^= token.charCodeAt(i);
                    hash = Math.imul(hash, 16777619);
                }
                return `log-token-tag log-token-tone-${(hash >>> 0) % 8}`;
            }

            function isDateOrTimeToken(token) {
                const value = token.replace(/^[\[\] ]+/, '').replace(/[\[\] ]+$/, '');
                return logDateTokenTestRegex.test(value) || logTimeTokenRegex.test(value);
            }

            function appendNumberTokens(segment, parts) {
                let currentIndex = 0;

                for (const numberMatch of segment.matchAll(logNumberTokenRegex)) {
                    const index = numberMatch.index ?? 0;
                    if (index > currentIndex) {
                        parts.push(escapeHtml(segment.slice(currentIndex, index)));
                    }
                    parts.push(`<span class="log-token-number">${escapeHtml(numberMatch[0])}</span>`);
                    currentIndex = index + numberMatch[0].length;
                }

                if (currentIndex < segment.length) {
                    parts.push(escapeHtml(segment.slice(currentIndex)));
                }
            }

            function appendDateAndNumberTokens(segment, parts) {
                let currentIndex = 0;

                for (const dateMatch of segment.matchAll(logDateTokenRegex)) {
                    const index = dateMatch.index ?? 0;
                    if (index > currentIndex) {
                        appendNumberTokens(segment.slice(currentIndex, index), parts);
                    }
                    parts.push(`<span class="log-token-date">${escapeHtml(dateMatch[0])}</span>`);
                    currentIndex = index + dateMatch[0].length;
                }

                if (currentIndex < segment.length) {
                    appendNumberTokens(segment.slice(currentIndex), parts);
                }
            }

            function renderLogTokens(line) {
                const parts = [];
                let currentIndex = 0;

                for (const bracketMatch of line.matchAll(logBracketTokenRegex)) {
                    const index = bracketMatch.index ?? 0;
                    if (index > currentIndex) {
                        appendDateAndNumberTokens(line.slice(currentIndex, index), parts);
                    }

                    const token = bracketMatch[0];
                    if (isDateOrTimeToken(token)) {
                        parts.push(`<span class="log-token-date">${escapeHtml(token)}</span>`);
                    } else {
                        parts.push(`<span class="${bracketToneClass(token)}">${escapeHtml(token)}</span>`);
                    }
                    currentIndex = index + token.length;
                }

                if (currentIndex < line.length) {
                    appendDateAndNumberTokens(line.slice(currentIndex), parts);
                }

                return parts.join('');
            }

            function createLogLine(line) {
                const lineElement = document.createElement('div');
                lineElement.className = 'log-line';
                lineElement.setAttribute('data-log-text', line);

                if (line.length === 0) {
                    lineElement.appendChild(document.createTextNode('\u00a0'));
                } else {
                    lineElement.innerHTML = renderLogTokens(line);
                }

                const copyButton = document.createElement('button');
                copyButton.type = 'button';
                copyButton.className = 'log-copy-btn';
                copyButton.setAttribute('aria-label', 'Copy log line');
                copyButton.title = 'Copy';
                copyButton.innerHTML = copyIconSvg;
                lineElement.appendChild(copyButton);

                return lineElement;
            }

            function renderLogLines(lines) {
                if (!logEl) {
                    return;
                }

                if (!Array.isArray(lines) || lines.length === 0) {
                    if (renderedLogLineCount !== 0) {
                        logEl.replaceChildren(document.createTextNode('No log entries yet.'));
                        logEl.classList.add('log-empty');
                        renderedLogLineCount = 0;
                        renderedLogLastLine = null;
                    }
                    return;
                }

                const newestLine = lines[lines.length - 1];
                const bufferOverflowed = lines.length === renderedLogLineCount && newestLine !== renderedLogLastLine;
                if (renderedLogLineCount === 0 || lines.length < renderedLogLineCount || bufferOverflowed) {
                    logEl.replaceChildren();
                    renderedLogLineCount = 0;
                }

                logEl.classList.remove('log-empty');
                const fragment = document.createDocumentFragment();
                for (let i = renderedLogLineCount; i < lines.length; i++) {
                    fragment.appendChild(createLogLine(lines[i]));
                }
                logEl.appendChild(fragment);
                renderedLogLineCount = lines.length;
                renderedLogLastLine = newestLine;

                if (tailEnabled) {
                    scrollToBottom();
                }
            }

            function refreshLogStream() {
                if (!logEndpoint) {
                    return;
                }

                fetchJson(logEndpoint)
                    .then(payload => {
                        if (payload && Array.isArray(payload.lines)) {
                            renderLogLines(payload.lines);
                        }
                    });
            }

            function setRefreshButtonState() {
                if (!btnToggleRefresh) return;
                btnToggleRefresh.textContent = refreshEnabled ? 'Stop refresh' : 'Start refresh';
                btnToggleRefresh.classList.toggle('active', refreshEnabled);
            }

            function scrollToBottom() {
                if (!logEl) return;
                logEl.scrollTop = logEl.scrollHeight;
            }

            function isScrolledToBottom() {
                if (!logEl) return true;
                return logEl.scrollHeight - logEl.scrollTop - logEl.clientHeight <= 2;
            }

            function applyExpandedSizing() {
                if (!logEl) return;
                if (!logEl.classList.contains('log-expanded')) {
                    logEl.style.removeProperty('height');
                    logEl.style.removeProperty('max-height');
                    document.body.classList.remove('log-expanded-page');
                    return;
                }

                document.body.classList.add('log-expanded-page');

                const top = logEl.getBoundingClientRect().top;
                const viewportBottomPadding = 24;
                const availableHeight = window.innerHeight - top - viewportBottomPadding;
                const targetHeight = Math.max(240, availableHeight);

                logEl.style.height = `${targetHeight}px`;
                logEl.style.maxHeight = `${targetHeight}px`;
            }

            function refreshCurrentPage() {
                if (logEl) {
                    refreshLogStream();
                    return;
                }

                if (httpEndpoint) {
                    refreshDashboard();
                    refreshMeters();
                    return;
                }

                if (isMetersPage && metersEndpoint) {
                    refreshMeters();
                    return;
                }

                if (isHealthPage && healthEndpoint) {
                    refreshHealth();
                    return;
                }

                if (countersEndpoint) {
                    refreshCounters();
                }
            }

            function startAutoRefresh() {
                if (autoRefreshInterval) {
                    clearInterval(autoRefreshInterval);
                }
                refreshEnabled = true;
                setRefreshButtonState();
                autoRefreshInterval = setInterval(refreshCurrentPage, autoRefreshIntervalMs);
            }

            function stopAutoRefresh() {
                if (autoRefreshInterval) {
                    clearInterval(autoRefreshInterval);
                }
                autoRefreshInterval = null;
                refreshEnabled = false;
                setRefreshButtonState();
            }

            btnTail?.addEventListener('click', () => {
                tailManuallyChanged = true;
                tailEnabled = !tailEnabled;
                btnTail.classList.toggle('active', tailEnabled);
                if (tailEnabled) scrollToBottom();
            });

            btnRefresh?.addEventListener('click', () => {
                refreshCurrentPage();
            });

            btnToggleRefresh?.addEventListener('click', () => {
                refreshManuallyChanged = true;
                if (refreshEnabled) {
                    stopAutoRefresh();
                } else {
                    startAutoRefresh();
                }
            });

            logEl?.addEventListener('scroll', () => {
                if (isScrolledToBottom()) {
                    if (!tailManuallyChanged) {
                        tailEnabled = true;
                        btnTail?.classList.add('active');
                    }
                    if (!refreshManuallyChanged && !refreshEnabled) {
                        startAutoRefresh();
                    }
                    return;
                }

                if (!tailManuallyChanged) {
                    tailEnabled = false;
                    btnTail?.classList.remove('active');
                }
                if (!refreshManuallyChanged && refreshEnabled) {
                    stopAutoRefresh();
                }
            });

            function copyText(text) {
                if (navigator.clipboard?.writeText) {
                    return navigator.clipboard.writeText(text);
                }

                const textArea = document.createElement('textarea');
                textArea.value = text;
                textArea.setAttribute('readonly', '');
                textArea.style.position = 'fixed';
                textArea.style.left = '-9999px';
                document.body.appendChild(textArea);
                textArea.select();

                try {
                    document.execCommand('copy');
                    return Promise.resolve();
                } finally {
                    textArea.remove();
                }
            }

            document.addEventListener('click', event => {
                if (!(event.target instanceof Element)) {
                    return;
                }

                const copyButton = event.target.closest('.log-copy-btn');
                if (copyButton) {
                    const logLine = copyButton.closest('.log-line');
                    const logText = logLine?.getAttribute('data-log-text') ?? '';

                    copyText(logText)
                        .then(() => {
                            copyButton.classList.add('copied');
                            copyButton.setAttribute('title', 'Copied');
                            window.setTimeout(() => {
                                copyButton.classList.remove('copied');
                                copyButton.setAttribute('title', 'Copy');
                            }, 900);
                        })
                        .catch(() => {
                            copyButton.setAttribute('title', 'Copy failed');
                        });
                    return;
                }

                const periodButton = event.target.closest('[data-meter-period]');
                if (periodButton) {
                    const meterId = periodButton.closest('[data-meter-id]')?.getAttribute('data-meter-id');
                    const periodCard = Array.from(document.querySelectorAll('.meter-card'))
                        .find(item => item.getAttribute('data-meter-id') === meterId);

                    if (periodCard) {
                        periodCard.setAttribute('data-selected-period', periodButton.getAttribute('data-meter-period'));
                        renderMeter(periodCard);
                        refreshMeters();
                    }
                    return;
                }

                const expandButton = event.target.closest('.meter-expand-btn');
                if (!expandButton) {
                    return;
                }

                const card = expandButton.closest('.meter-card');
                if (!card) {
                    return;
                }

                openMeterModal(card);
            });

            window.addEventListener('resize', () => {
                applyExpandedSizing();
                document.querySelectorAll('.meter-card').forEach(renderMeter);

                if (httpEndpoint) {
                    renderHttpCharts();
                }
            });

            if (healthChart) {
                renderHealthChart(healthChart, parseHealthReadings(healthChart.getAttribute('data-health-readings') ?? '[]'));
            }

            if (logEl) {
                logEl.classList.add('log-expanded');
                applyExpandedSizing();
                btnTail?.classList.add('active');
                if (tailEnabled) {
                    scrollToBottom();
                }
            }

            refreshCurrentPage();

            if (btnToggleRefresh) {
                startAutoRefresh();
            }
        })();
        """;
}
