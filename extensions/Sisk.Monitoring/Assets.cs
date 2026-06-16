namespace Sisk.Monitoring;

internal class Assets {
    public const string DefaultStyles = """
        :root {
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

        [data-theme="dark"] {
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

        .meters-grid {
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
            flex-shrink: 0;
        }

        .meter-chart-container {
            position: relative;
            height: 124px;
            border: 1px solid var(--border);
            border-radius: 8px;
            background: var(--surface-secondary);
            overflow: hidden;
        }

        .meter-chart-svg {
            width: 100%;
            height: 100%;
            display: block;
        }

        .meter-chart-axis {
            stroke: var(--border);
            stroke-width: 1;
            vector-effect: non-scaling-stroke;
        }

        .meter-chart-area {
            fill: var(--accent-surface);
        }

        .meter-chart-line {
            fill: none;
            stroke: var(--accent);
            stroke-width: 2;
            stroke-linejoin: round;
            stroke-linecap: round;
            vector-effect: non-scaling-stroke;
        }

        .meter-chart-crosshair {
            stroke: var(--accent);
            stroke-width: 1;
            stroke-dasharray: 3 3;
            opacity: 0;
            vector-effect: non-scaling-stroke;
        }

        .meter-chart-point {
            fill: var(--accent);
            stroke: var(--surface);
            stroke-width: 2;
            opacity: 0;
            vector-effect: non-scaling-stroke;
        }

        .meter-chart-label {
            fill: var(--text-muted);
            font-size: 10px;
            font-family: var(--font-sans);
        }

        .meter-chart-tooltip {
            position: absolute;
            background: var(--surface);
            color: var(--text-primary);
            border: 1px solid var(--border);
            box-shadow: var(--shadow);
            border-radius: 6px;
            font-size: 0.75rem;
            line-height: 1.3;
            white-space: nowrap;
            padding: 0.25rem 0.45rem;
            pointer-events: none;
            opacity: 0;
            transition: opacity 0.12s;
            z-index: 2;
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

        .meter-modal-stats {
            display: grid;
            grid-template-columns: repeat(4, minmax(0, 1fr));
            gap: 0.65rem;
        }

        .meter-stat {
            border: 1px solid var(--border);
            border-radius: 8px;
            background: var(--surface-secondary);
            padding: 0.6rem 0.75rem;
            display: flex;
            flex-direction: column;
            gap: 0.15rem;
        }

        .meter-stat-label {
            font-size: 0.72rem;
            text-transform: uppercase;
            letter-spacing: 0.03em;
            color: var(--text-muted);
            font-weight: 600;
        }

        .meter-stat-value {
            font-size: 1rem;
            color: var(--text-primary);
        }

        .meter-modal-chart {
            border: 1px solid var(--border);
            border-radius: 8px;
            overflow: hidden;
        }

        .meter-modal-chart .meter-chart-container {
            border: none;
            border-radius: 0;
            height: 320px;
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

        .health-chart-swatch.cpu {
            background: var(--chart-cpu);
        }

        .health-chart-swatch.memory {
            background: var(--chart-memory);
        }

        .health-chart-swatch.disk {
            background: var(--chart-disk);
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

            .meter-modal-stats {
                grid-template-columns: repeat(2, minmax(0, 1fr));
            }

            .meter-modal {
                max-height: calc(100vh - 2rem);
                overflow-y: auto;
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
            const numberFormatter = new Intl.NumberFormat('en-US', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
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

            function parseReadings(raw) {
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
                        const value = Number(item?.value ?? 0);
                        if (Number.isNaN(timestamp.getTime())) {
                            return null;
                        }
                        return { timestamp, value: Number.isFinite(value) ? value : 0 };
                    })
                    .filter(item => item !== null);
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

            function calculateStats(readings) {
                const values = readings.map(r => r.value);
                const total = values.reduce((sum, value) => sum + value, 0);
                return {
                    min: values.length ? Math.min(...values) : 0,
                    max: values.length ? Math.max(...values) : 0,
                    avg: values.length ? total / values.length : 0,
                    total
                };
            }

            function createSvgElement(tag) {
                return document.createElementNS('http://www.w3.org/2000/svg', tag);
            }

            function getMeterFromCard(card) {
                const chartContainer = card.querySelector('.meter-chart-container');
                return {
                    id: card.getAttribute('data-meter-id') ?? chartContainer?.getAttribute('data-meter-id') ?? '',
                    label: card.getAttribute('data-meter-label') ?? 'Meter',
                    readings: parseReadings(chartContainer?.getAttribute('data-readings') ?? '[]')
                };
            }

            function renderMeterChart(container, readings, options = {}) {
                const chartHeight = Number(options.height ?? 112);
                const showAxes = options.showAxes === true;
                const interactive = options.interactive !== false;
                const width = Math.max(220, container.clientWidth || 220);
                const padding = {
                    top: 10,
                    right: 10,
                    bottom: showAxes ? 28 : 10,
                    left: showAxes ? 34 : 10
                };
                const plotWidth = Math.max(1, width - padding.left - padding.right);
                const plotHeight = Math.max(1, chartHeight - padding.top - padding.bottom);
                const valueTarget = container.closest('.meter-card')?.querySelector('.meter-current-value');

                container.innerHTML = '';

                if (!readings.length) {
                    container.classList.add('meter-chart-empty');
                    container.textContent = 'No data';
                    if (valueTarget) {
                        valueTarget.dataset.defaultValue = '-';
                        valueTarget.textContent = '-';
                    }
                    return;
                }

                container.classList.remove('meter-chart-empty');

                const values = readings.map(reading => reading.value);
                const minValue = Math.min(0, ...values);
                let maxValue = Math.max(...values);
                if (maxValue <= minValue) {
                    maxValue = minValue + 1;
                }

                const points = readings.map((reading, index) => {
                    const x = padding.left + ((readings.length <= 1 ? 0 : index / (readings.length - 1)) * plotWidth);
                    const yFactor = (reading.value - minValue) / (maxValue - minValue);
                    const y = padding.top + ((1 - yFactor) * plotHeight);
                    return { x, y, reading };
                });

                const totalValue = values.reduce((sum, value) => sum + value, 0);
                const defaultValue = formatNumber(totalValue);
                if (valueTarget) {
                    valueTarget.dataset.defaultValue = defaultValue;
                    valueTarget.textContent = defaultValue;
                }

                const svg = createSvgElement('svg');
                svg.setAttribute('class', 'meter-chart-svg');
                svg.setAttribute('viewBox', `0 0 ${width} ${chartHeight}`);
                svg.setAttribute('preserveAspectRatio', 'none');

                if (showAxes) {
                    const horizontalAxis = createSvgElement('line');
                    horizontalAxis.setAttribute('class', 'meter-chart-axis');
                    horizontalAxis.setAttribute('x1', String(padding.left));
                    horizontalAxis.setAttribute('x2', String(width - padding.right));
                    horizontalAxis.setAttribute('y1', String(chartHeight - padding.bottom));
                    horizontalAxis.setAttribute('y2', String(chartHeight - padding.bottom));
                    svg.appendChild(horizontalAxis);

                    const leftLabel = createSvgElement('text');
                    leftLabel.setAttribute('class', 'meter-chart-label');
                    leftLabel.setAttribute('x', String(padding.left));
                    leftLabel.setAttribute('y', String(chartHeight - 8));
                    leftLabel.textContent = formatDate(readings[0].timestamp);
                    svg.appendChild(leftLabel);

                    const rightLabel = createSvgElement('text');
                    rightLabel.setAttribute('class', 'meter-chart-label');
                    rightLabel.setAttribute('x', String(width - padding.right));
                    rightLabel.setAttribute('y', String(chartHeight - 8));
                    rightLabel.setAttribute('text-anchor', 'end');
                    rightLabel.textContent = formatDate(readings[readings.length - 1].timestamp);
                    svg.appendChild(rightLabel);
                }

                const area = createSvgElement('polygon');
                const areaPoints = [
                    `${padding.left},${padding.top + plotHeight}`,
                    ...points.map(point => `${point.x},${point.y}`),
                    `${padding.left + plotWidth},${padding.top + plotHeight}`
                ];
                area.setAttribute('class', 'meter-chart-area');
                area.setAttribute('points', areaPoints.join(' '));
                svg.appendChild(area);

                const line = createSvgElement('polyline');
                line.setAttribute('class', 'meter-chart-line');
                line.setAttribute('points', points.map(point => `${point.x},${point.y}`).join(' '));
                svg.appendChild(line);

                const crosshair = createSvgElement('line');
                crosshair.setAttribute('class', 'meter-chart-crosshair');
                svg.appendChild(crosshair);

                const marker = createSvgElement('circle');
                marker.setAttribute('class', 'meter-chart-point');
                marker.setAttribute('r', '4');
                svg.appendChild(marker);

                const tooltip = document.createElement('div');
                tooltip.className = 'meter-chart-tooltip';
                container.appendChild(svg);
                container.appendChild(tooltip);

                if (!interactive) {
                    return;
                }

                function hideHover() {
                    crosshair.style.opacity = '0';
                    marker.style.opacity = '0';
                    tooltip.style.opacity = '0';
                    if (valueTarget) {
                        valueTarget.textContent = valueTarget.dataset.defaultValue ?? '-';
                    }
                }

                svg.addEventListener('mousemove', event => {
                    const bounds = svg.getBoundingClientRect();
                    const relativeX = ((event.clientX - bounds.left) / bounds.width) * width;
                    const normalized = (relativeX - padding.left) / plotWidth;
                    const index = Math.max(0, Math.min(points.length - 1, Math.round(normalized * (points.length - 1))));
                    const point = points[index];

                    crosshair.setAttribute('x1', String(point.x));
                    crosshair.setAttribute('x2', String(point.x));
                    crosshair.setAttribute('y1', String(padding.top));
                    crosshair.setAttribute('y2', String(padding.top + plotHeight));
                    crosshair.style.opacity = '1';

                    marker.setAttribute('cx', String(point.x));
                    marker.setAttribute('cy', String(point.y));
                    marker.style.opacity = '1';

                    tooltip.textContent = `${formatDate(point.reading.timestamp)} · ${formatNumber(point.reading.value)}`;
                    tooltip.style.opacity = '1';

                    const tooltipMargin = 8;
                    const tooltipWidth = tooltip.offsetWidth;
                    const tooltipHeight = tooltip.offsetHeight;

                    let tooltipLeft = point.x - (tooltipWidth / 2);
                    tooltipLeft = Math.max(tooltipMargin, Math.min(width - tooltipWidth - tooltipMargin, tooltipLeft));

                    let tooltipTop = point.y - tooltipHeight - 10;
                    if (tooltipTop < tooltipMargin)
                    {
                        tooltipTop = point.y + 10;
                    }
                    tooltipTop = Math.max(tooltipMargin, Math.min(chartHeight - tooltipHeight - tooltipMargin, tooltipTop));

                    tooltip.style.left = `${tooltipLeft}px`;
                    tooltip.style.top = `${tooltipTop}px`;

                    if (valueTarget) {
                        valueTarget.textContent = formatNumber(point.reading.value);
                    }
                });

                svg.addEventListener('mouseleave', hideHover);
            }

            function renderHealthChart(container, readings) {
                const chartHeight = 320;
                const width = Math.max(320, container.clientWidth || 320);
                const padding = { top: 26, right: 18, bottom: 30, left: 42 };
                const plotWidth = Math.max(1, width - padding.left - padding.right);
                const plotHeight = Math.max(1, chartHeight - padding.top - padding.bottom);
                const series = [
                    { key: 'cpu', label: 'CPU', color: 'var(--chart-cpu)' },
                    { key: 'memory', label: 'RAM', color: 'var(--chart-memory)' },
                    { key: 'disk', label: 'Disk', color: 'var(--chart-disk)' }
                ];

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

                [0, 25, 50, 75, 100].forEach(value => {
                    const y = padding.top + ((100 - value) / 100 * plotHeight);

                    const grid = createSvgElement('line');
                    grid.setAttribute('class', value === 0 ? 'health-chart-axis' : 'health-chart-grid');
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
                    label.textContent = `${value}%`;
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
                    points: readings.map((reading, index) => {
                        const x = padding.left + ((readings.length <= 1 ? 0 : index / (readings.length - 1)) * plotWidth);
                        const value = Math.max(0, Math.min(100, Number(reading[item.key] ?? 0)));
                        const y = padding.top + ((100 - value) / 100 * plotHeight);
                        return { x, y, value, reading };
                    })
                }));

                pointsBySeries.forEach(item => {
                    const line = createSvgElement('polyline');
                    line.setAttribute('class', 'health-chart-line');
                    line.setAttribute('stroke', item.color);
                    line.setAttribute('points', item.points.map(point => `${point.x},${point.y}`).join(' '));
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
                    swatch.className = `health-chart-swatch ${item.key}`;
                    const label = document.createElement('span');
                    label.textContent = item.label;
                    entry.appendChild(swatch);
                    entry.appendChild(label);
                    legend.appendChild(entry);
                });

                container.appendChild(svg);
                container.appendChild(legend);

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
                    const normalized = (relativeX - padding.left) / plotWidth;
                    const index = Math.max(0, Math.min(readings.length - 1, Math.round(normalized * (readings.length - 1))));
                    const reading = readings[index];
                    const x = padding.left + ((readings.length <= 1 ? 0 : index / (readings.length - 1)) * plotWidth);

                    crosshair.setAttribute('x1', String(x));
                    crosshair.setAttribute('x2', String(x));
                    crosshair.setAttribute('y1', String(padding.top));
                    crosshair.setAttribute('y2', String(padding.top + plotHeight));
                    crosshair.style.opacity = '1';

                    markers.forEach(item => {
                        const point = item.points[index];
                        item.marker.setAttribute('cx', String(point.x));
                        item.marker.setAttribute('cy', String(point.y));
                        item.marker.style.opacity = '1';
                    });

                    tooltip.innerHTML = [
                        `<strong>${formatDate(reading.timestamp)}</strong>`,
                        ...series.map(item => `${item.label}: ${formatNumber(reading[item.key])}%`)
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

            function setModalStats(modal, readings) {
                const stats = calculateStats(readings);
                modal.querySelector('[data-stat="min"]').textContent = formatNumber(stats.min);
                modal.querySelector('[data-stat="max"]').textContent = formatNumber(stats.max);
                modal.querySelector('[data-stat="avg"]').textContent = formatNumber(stats.avg);
                modal.querySelector('[data-stat="total"]').textContent = formatNumber(stats.total);
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

            function openMeterModal(meter) {
                closeMeterModal();

                const overlay = document.createElement('div');
                overlay.className = 'meter-modal-overlay';
                overlay.setAttribute('data-meter-id', meter.id);
                overlay.innerHTML = `
                    <div class="meter-modal" role="dialog" aria-modal="true" aria-label="${escapeHtml(meter.label)}">
                        <div class="meter-modal-header">
                            <h2>${escapeHtml(meter.label)}</h2>
                            <button type="button" class="toolbar-btn meter-modal-close-btn">Close</button>
                        </div>
                        <div class="meter-modal-stats">
                            <div class="meter-stat"><span class="meter-stat-label">Min</span><strong class="meter-stat-value" data-stat="min"></strong></div>
                            <div class="meter-stat"><span class="meter-stat-label">Max</span><strong class="meter-stat-value" data-stat="max"></strong></div>
                            <div class="meter-stat"><span class="meter-stat-label">Avg</span><strong class="meter-stat-value" data-stat="avg"></strong></div>
                            <div class="meter-stat"><span class="meter-stat-label">Total</span><strong class="meter-stat-value" data-stat="total"></strong></div>
                        </div>
                        <div class="meter-modal-chart">
                            <div class="meter-chart-container" data-meter-id="${escapeHtml(meter.id)}"></div>
                        </div>
                    </div>
                `;

                document.body.appendChild(overlay);

                const modal = overlay.querySelector('.meter-modal');
                const modalChart = overlay.querySelector('.meter-modal-chart .meter-chart-container');

                setModalStats(overlay, meter.readings);
                renderMeterChart(modalChart, meter.readings, {
                    height: 320,
                    showAxes: true,
                    interactive: true
                });

                overlay.addEventListener('click', () => closeMeterModal());
                modal.addEventListener('click', event => event.stopPropagation());
                overlay.querySelector('.meter-modal-close-btn')?.addEventListener('click', () => closeMeterModal());
            }

            function renderMeterCards() {
                document.querySelectorAll('.meter-card').forEach(card => {
                    const meter = getMeterFromCard(card);
                    const chartContainer = card.querySelector('.meter-chart-container');
                    if (!chartContainer) {
                        return;
                    }
                    renderMeterChart(chartContainer, meter.readings, {
                        height: 112,
                        showAxes: false,
                        interactive: true
                    });
                });
            }

            function getOpenModalId() {
                return document.querySelector('.meter-modal-overlay')?.getAttribute('data-meter-id') ?? null;
            }

            function refreshOpenModal(payloadById) {
                const openModalId = getOpenModalId();
                if (!openModalId || !payloadById.has(openModalId)) {
                    return;
                }

                const meterPayload = payloadById.get(openModalId);
                const readings = parseReadings(meterPayload.readings ?? []);
                const overlay = document.querySelector('.meter-modal-overlay');
                const modalChart = overlay?.querySelector('.meter-modal-chart .meter-chart-container');

                if (!overlay || !modalChart) {
                    return;
                }

                setModalStats(overlay, readings);
                renderMeterChart(modalChart, readings, {
                    height: 320,
                    showAxes: true,
                    interactive: true
                });
            }

            function refreshMeters() {
                if (!metersEndpoint) {
                    location.reload();
                    return;
                }

                fetch(metersEndpoint, { cache: 'no-store' })
                    .then(response => response.ok ? response.json() : [])
                    .then(payload => {
                        if (!Array.isArray(payload)) {
                            return;
                        }

                        const payloadById = new Map(payload
                            .filter(item => item && typeof item.id === 'string')
                            .map(item => [item.id, item]));

                        document.querySelectorAll('.meter-card').forEach(card => {
                            const meterId = card.getAttribute('data-meter-id');
                            if (!meterId || !payloadById.has(meterId)) {
                                return;
                            }

                            const meterPayload = payloadById.get(meterId);
                            const chartContainer = card.querySelector('.meter-chart-container');
                            const readings = parseReadings(meterPayload.readings ?? []);

                            if (chartContainer) {
                                chartContainer.setAttribute('data-readings', JSON.stringify(meterPayload.readings ?? []));
                                renderMeterChart(chartContainer, readings, {
                                    height: 112,
                                    showAxes: false,
                                    interactive: true
                                });
                            }
                        });

                        refreshOpenModal(payloadById);
                    })
                    .catch(() => {
                    });
            }

            function refreshHealth() {
                if (!healthEndpoint || !healthChart) {
                    location.reload();
                    return;
                }

                fetch(healthEndpoint, { cache: 'no-store' })
                    .then(response => response.ok ? response.json() : null)
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

            function refreshLog() {
                fetch(location.href)
                    .then(r => r.text())
                    .then(html => {
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(html, 'text/html');
                        const newContent = doc.getElementById('log-content');
                        if (newContent) {
                            logEl.innerHTML = newContent.innerHTML;
                            logEl.className = newContent.className;
                            logEl.classList.add('log-expanded');
                            applyExpandedSizing();
                            if (tailEnabled) scrollToBottom();
                        }
                    });
            }

            function refreshCurrentPage() {
                if (logEl) {
                    refreshLog();
                    return;
                }

                if (isMetersPage && document.querySelector('.meter-card')) {
                    refreshMeters();
                    return;
                }

                if (isHealthPage && healthChart) {
                    refreshHealth();
                    return;
                }

                location.reload();
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

                const expandButton = event.target.closest('.meter-expand-btn');
                if (!expandButton) {
                    return;
                }

                const card = expandButton.closest('.meter-card');
                if (!card) {
                    return;
                }

                const meter = getMeterFromCard(card);
                openMeterModal(meter);
            });

            window.addEventListener('resize', () => {
                applyExpandedSizing();
                renderMeterCards();

                const overlay = document.querySelector('.meter-modal-overlay');
                if (overlay) {
                    const openModalId = overlay.getAttribute('data-meter-id');
                    if (openModalId) {
                        const sourceCard = document.querySelector(`.meter-card[data-meter-id="${openModalId}"]`);
                        if (sourceCard) {
                            const meter = getMeterFromCard(sourceCard);
                            const modalChart = overlay.querySelector('.meter-modal-chart .meter-chart-container');
                            if (modalChart) {
                                renderMeterChart(modalChart, meter.readings, {
                                    height: 320,
                                    showAxes: true,
                                    interactive: true
                                });
                            }
                            setModalStats(overlay, meter.readings);
                        }
                    }
                }
            });

            renderMeterCards();

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

            if (btnToggleRefresh) {
                startAutoRefresh();
            }
        })();
        """;
}
