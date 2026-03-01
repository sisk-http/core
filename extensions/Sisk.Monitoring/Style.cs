namespace Sisk.Monitoring;

internal class Style
{
    public const string DefaultStyles = """
        :root {
            --bg-primary: #ffffff;
            --bg-secondary: #f2f2f2;
            --bg-tertiary: #e6e6e6;
            --text-primary: #1f1f1f;
            --text-secondary: #5f5f5f;
            --text-muted: #9a9a9a;
            --border-color: #d6d6d6;
            --accent: #4f4f4f;
            --accent-light: #4f4f4f20;
            --success: #707070;
            --warning: #8a8a8a;
            --danger: #5a5a5a;
            --card-shadow: 0 1px 3px rgba(0,0,0,0.08);
            --font-mono: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
            --font-sans: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
            --sidebar-width: 260px;
        }

        @media (prefers-color-scheme: dark) {
            :root {
                --bg-primary: #1c1c1c;
                --bg-secondary: #121212;
                --bg-tertiary: #2a2a2a;
                --text-primary: #e5e5e5;
                --text-secondary: #b5b5b5;
                --text-muted: #8b8b8b;
                --border-color: #3a3a3a;
                --accent: #c4c4c4;
                --accent-light: #c4c4c420;
                --success: #9a9a9a;
                --warning: #a8a8a8;
                --danger: #8a8a8a;
                --card-shadow: 0 1px 3px rgba(0,0,0,0.3);
            }
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
            background: var(--bg-secondary);
        }

        .page-wrapper {
            display: flex;
            min-height: 100vh;
        }

        /* Sidebar */
        .sidebar {
            width: var(--sidebar-width);
            background: var(--bg-primary);
            border-right: 1px solid var(--border-color);
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
            border-bottom: 1px solid var(--border-color);
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

        .nav-item {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.4rem 0.75rem;
            border-radius: 6px;
            color: var(--text-secondary);
            text-decoration: none;
            font-size: 0.85rem;
            transition: background 0.15s;
        }

        .nav-item:hover {
            background: var(--accent-light);
            color: var(--accent);
        }

        .nav-item.active {
            background: var(--accent-light);
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
            margin-left: var(--sidebar-width);
            padding: 2rem 2.5rem;
            max-width: 960px;
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
            grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
            gap: 1rem;
            margin-bottom: 2rem;
        }

        .card {
            background: var(--bg-primary);
            border: 1px solid var(--border-color);
            border-radius: 8px;
            padding: 1.25rem;
            box-shadow: var(--card-shadow);
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
            border-bottom: 1px solid var(--border-color);
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
            background: var(--bg-primary);
            border: 1px solid var(--border-color);
            border-radius: 8px;
            padding: 1rem 1.25rem;
            text-decoration: none;
            color: var(--text-primary);
            box-shadow: var(--card-shadow);
            transition: border-color 0.15s;
        }

        .stream-item:hover {
            border-color: var(--accent);
        }

        .stream-item-label {
            font-weight: 600;
            font-size: 0.9rem;
        }

        .stream-item-badge {
            font-size: 0.7rem;
            padding: 0.2rem 0.6rem;
            border-radius: 99px;
            background: var(--accent-light);
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
            background: var(--bg-primary);
            border: 1px solid var(--border-color);
            border-radius: 8px;
            padding: 1rem;
            font-family: var(--font-mono);
            font-size: 0.8rem;
            line-height: 1.7;
            white-space: pre-wrap;
            word-break: break-word;
            overflow-x: auto;
            max-height: 75vh;
            overflow-y: auto;
            box-shadow: var(--card-shadow);
            color: var(--text-primary);
        }

        .log-line {
            white-space: pre-wrap;
            word-break: break-word;
            padding: 0.15rem 0;
            border-bottom: 1px solid color-mix(in srgb, var(--border-color) 50%, transparent);
        }

        .log-line:last-child {
            border-bottom: none;
        }

        .log-token-date {
            color: #b87700;
            font-weight: 600;
        }

        .log-token-tag {
            border-radius: 4px;
            padding: 0 0.2rem;
            font-weight: 600;
        }

        .log-token-number {
            color: var(--text-secondary);
            font-weight: 600;
        }

        body.log-expanded-page main {
            max-width: none;
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
            border: 1px solid var(--border-color);
            border-radius: 6px;
            background: var(--bg-primary);
            color: var(--text-secondary);
            font-size: 0.8rem;
            font-family: var(--font-sans);
            cursor: pointer;
            transition: background 0.15s, border-color 0.15s, color 0.15s;
        }

        .toolbar-btn:hover {
            border-color: var(--accent);
            color: var(--accent);
        }

        .toolbar-btn.active {
            background: var(--accent);
            border-color: var(--accent);
            color: #fff;
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
            background: var(--bg-tertiary);
            border-radius: 12px;
            overflow: hidden;
            margin-top: 0.75rem;
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
            mix-blend-mode: difference;
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
            color: #fff;
            font-size: 1.25rem;
            cursor: pointer;
            box-shadow: 0 2px 8px rgba(0,0,0,0.2);
        }

        .sidebar-overlay {
            display: none;
            position: fixed;
            inset: 0;
            background: rgba(0,0,0,0.4);
            z-index: 9;
        }

        @media (max-width: 768px) {
            .sidebar {
                transform: translateX(-100%);
                transition: transform 0.2s;
            }

            .sidebar.open {
                transform: translateX(0);
            }

            .sidebar-overlay.show {
                display: block;
            }

            main {
                margin-left: 0;
                padding: 1.5rem 1rem;
            }

            .mobile-menu-btn {
                display: flex;
                align-items: center;
                justify-content: center;
            }

            .cards-grid {
                grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
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
            const btnExpand = document.getElementById('btn-expand');

            let tailEnabled = false;
            let autoRefreshInterval = null;
            let refreshEnabled = true;

            function setRefreshButtonState() {
                if (!btnToggleRefresh) return;
                btnToggleRefresh.textContent = refreshEnabled ? 'Stop refresh' : 'Start refresh';
                btnToggleRefresh.classList.toggle('active', refreshEnabled);
            }

            function scrollToBottom() {
                if (!logEl) return;
                logEl.scrollTop = logEl.scrollHeight;
            }

            function applyExpandedSizing() {
                if (!logEl) return;
                if (!logEl.classList.contains('log-expanded')) {
                    logEl.style.removeProperty('height');
                    logEl.style.removeProperty('max-height');
                    logEl.style.removeProperty('width');
                    logEl.style.removeProperty('max-width');
                    document.body.classList.remove('log-expanded-page');
                    return;
                }

                document.body.classList.add('log-expanded-page');

                const top = logEl.getBoundingClientRect().top;
                const viewportBottomPadding = 24;
                const availableHeight = window.innerHeight - top - viewportBottomPadding;
                const targetHeight = Math.max(240, availableHeight);

                const left = logEl.getBoundingClientRect().left;
                const viewportRightPadding = 24;
                const availableWidth = window.innerWidth - left - viewportRightPadding;
                const targetWidth = Math.max(560, availableWidth);

                logEl.style.height = `${targetHeight}px`;
                logEl.style.maxHeight = `${targetHeight}px`;
                logEl.style.width = `${targetWidth}px`;
                logEl.style.maxWidth = `${targetWidth}px`;
            }

            function refreshLog() {
                fetch(location.href)
                    .then(r => r.text())
                    .then(html => {
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(html, 'text/html');
                        const newContent = doc.getElementById('log-content');
                        if (newContent) {
                            const wasExpanded = logEl.classList.contains('log-expanded');
                            logEl.innerHTML = newContent.innerHTML;
                            logEl.className = newContent.className;
                            if (wasExpanded) {
                                logEl.classList.add('log-expanded');
                                btnExpand?.classList.add('active');
                                applyExpandedSizing();
                            }
                            if (tailEnabled) scrollToBottom();
                        }
                    });
            }

            function refreshCurrentPage() {
                if (logEl) {
                    refreshLog();
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
                autoRefreshInterval = setInterval(refreshCurrentPage, 10000);
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
                tailEnabled = !tailEnabled;
                btnTail.classList.toggle('active', tailEnabled);
                if (tailEnabled) scrollToBottom();
            });

            btnRefresh?.addEventListener('click', () => {
                refreshCurrentPage();
            });

            btnToggleRefresh?.addEventListener('click', () => {
                if (refreshEnabled) {
                    stopAutoRefresh();
                } else {
                    startAutoRefresh();
                }
            });

            btnExpand?.addEventListener('click', () => {
                if (!logEl) return;
                logEl.classList.toggle('log-expanded');
                btnExpand.classList.toggle('active');
                applyExpandedSizing();
            });

            window.addEventListener('resize', () => {
                applyExpandedSizing();
            });

            if (btnToggleRefresh) {
                startAutoRefresh();
            }
        })();
        """;
}
