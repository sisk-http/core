// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Script.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting.Html;

internal class Scripts {
    public const string ThemeScript = """
        (() => {
            let theme = 'system';
            try { theme = localStorage.getItem('sisk-docs-theme') || 'system'; } catch { }
            document.documentElement.dataset.theme = theme;
        })();
        """;

    public const string DefaultScript = """
        (root, docsUrl) => {
        function setTheme(theme) {
            document.documentElement.dataset.theme = theme;
            try { localStorage.setItem('sisk-docs-theme', theme); } catch { }
            root.querySelectorAll('.theme-switcher button').forEach(b => {
                b.setAttribute('aria-pressed', String(b.dataset.themeValue === theme));
            });
        }

        root.querySelectorAll('.theme-switcher button').forEach(btn => {
            btn.addEventListener('click', () => setTheme(btn.dataset.themeValue));
        });

        if (root === document) setTheme(document.documentElement.dataset.theme || 'system');

        function closeAllDropdowns() {
            root.querySelectorAll('.dropdown-menu.show').forEach(d => d.classList.remove('show'));
        }

        document.addEventListener('click', closeAllDropdowns);

        root.querySelectorAll('.copy-button-wrapper').forEach(wrapper => {
            wrapper.addEventListener('click', (e) => {
                e.stopPropagation();
            });
        });

        root.querySelectorAll('.copy-button').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();

                const menu = btn.nextElementSibling;
                const wasOpen = menu.classList.contains('show');

                closeAllDropdowns();

                if (!wasOpen) {
                    menu.classList.add('show');
                }
            });
        });

        function showToast(message) {
            const existing = root.querySelector('.copy-toast');
            if (existing) existing.remove();
            
            const toast = document.createElement('div');
            toast.className = 'copy-toast';
            toast.textContent = message;
            (root.body || root).appendChild(toast);
            
            requestAnimationFrame(() => toast.classList.add('show'));
            
            setTimeout(() => {
                toast.classList.remove('show');
                setTimeout(() => toast.remove(), 300);
            }, 2000);
        }

        function extractEndpointMarkdown(details) {
            const content = details.querySelector('.endpoint-content');
            if (!content) return '';

            let md = '';
            
            const h3 = content.querySelector('h3');
            if (h3) md += `## ${h3.textContent}\n\n`;

            content.querySelectorAll('.endpoint-main > p').forEach(p => md += `${p.textContent}\n\n`);

            const path = content.querySelector('.endpoint-path');
            if (path) md += `\`\`\`\n${path.querySelector('.method-badge').textContent} ${path.querySelector('code').textContent}\n\`\`\`\n\n`;

            content.querySelectorAll('.params-section').forEach(section => {
                const title = section.querySelector('p');
                if (title) md += `### ${title.textContent}\n\n`;

                section.querySelectorAll('.param-item').forEach(item => {
                    const name = item.querySelector('.param-name');
                    const type = item.querySelector('.param-type');
                    const required = item.querySelector('.param-required');
                    const paramDesc = item.querySelector('.param-description');
                    const statusCode = item.querySelector('.status-code');

                    if (statusCode) {
                        md += `- **${statusCode.textContent}**`;
                    } else if (name) {
                        md += `- **${name.textContent}**`;
                        if (type) md += ` \`${type.textContent}\``;
                        if (required) md += ` *(${required.textContent})*`;
                    }

                    if (paramDesc) md += `: ${paramDesc.textContent}`;
                    md += '\n';

                    const pre = item.querySelector('pre code');
                    if (pre) md += `\n\`\`\`json\n${pre.textContent}\n\`\`\`\n`;
                });
                md += '\n';
            });

            content.querySelectorAll('.example-card').forEach(card => {
                md += `### ${card.querySelector('.example-card-title').textContent}\n\n`;

                card.querySelectorAll(':scope > .tab-content > .example-variant').forEach(variant => {
                    const label = [variant.parentElement.dataset.label, variant.dataset.title].filter(Boolean).join(' - ');
                    const variantDesc = variant.querySelector('.example-description-text');
                    md += label ? `- **${label}**` : '-';
                    if (variantDesc) md += `${label ? ':' : ''} ${variantDesc.textContent.trim()}`;
                    md += '\n';

                    const pre = variant.querySelector('pre code');
                    if (pre) md += `\n\`\`\`\n${pre.textContent}\n\`\`\`\n`;
                });
                md += '\n';
            });

            return md.trim();
        }

        root.querySelectorAll('.dropdown-item').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();

                const action = item.getAttribute('data-action');
                const targetId = item.getAttribute('data-target-id');
                const url = docsUrl + '#' + targetId;

                closeAllDropdowns();

                if (action === 'copy-link') {
                    navigator.clipboard.writeText(url);
                    showToast('Link copied!');
                } else if (action === 'copy-markdown') {
                    const details = item.closest('.endpoint-description');
                    const md = extractEndpointMarkdown(details);
                    navigator.clipboard.writeText(md);
                    showToast('Markdown copied!');
                } else if (action === 'copy-embed') {
                    const endpointName = item.getAttribute('data-endpoint-name');
                    const embedUrl = `${docsUrl.split('?')[0]}?embed-target=${encodeURIComponent(endpointName)}&r=${encodeURIComponent(docsUrl)}`;
                    navigator.clipboard.writeText(`<script src="${embedUrl}"></` + `script>`);
                    showToast('Embed copied!');
                }
            });
        });

        root.querySelectorAll('.nav-group-header').forEach(header => {
            header.addEventListener('click', () => {
                header.parentElement.classList.toggle('open');
            });
        });

        const isEndpointPageMode = root === document;
        let listingScrollY = 0;

        function closeEndpointPage() {
            const wasOpen = document.body.classList.contains('endpoint-page-open');
            document.body.classList.remove('endpoint-page-open');
            root.querySelectorAll('.endpoint-description[open]').forEach(d => {
                d.open = false;
            });
            root.querySelectorAll('.endpoint.current, .group-section.current').forEach(e => e.classList.remove('current'));
            root.querySelectorAll('.nav-item.active').forEach(i => i.classList.remove('active'));
            if (wasOpen) window.scrollTo(0, listingScrollY);
        }

        function openDetailsForHash(hash) {
            const target = hash && hash.length > 1 && root.getElementById(hash.slice(1));
            const details = target && target.closest('.endpoint-description');

            if (!details) {
                if (isEndpointPageMode) closeEndpointPage();
                if (target) target.scrollIntoView({ block: 'start' });
                return;
            }

            if (!isEndpointPageMode) {
                details.open = true;
                return;
            }

            if (!document.body.classList.contains('endpoint-page-open')) {
                listingScrollY = window.scrollY;
            }

            root.querySelectorAll('.endpoint-description[open]').forEach(d => {
                d.open = false;
            });
            root.querySelectorAll('.endpoint.current, .group-section.current').forEach(e => e.classList.remove('current'));

            details.open = true;
            details.closest('.endpoint').classList.add('current');
            details.closest('.group-section')?.classList.add('current');
            document.body.classList.add('endpoint-page-open');
            window.scrollTo(0, 0);

            root.querySelectorAll('.nav-item').forEach(i => {
                i.classList.toggle('active', i.getAttribute('href') === hash);
            });
        }

        root.querySelectorAll('.endpoint-description > summary').forEach(summary => {
            summary.addEventListener('click', (e) => {
                if (!isEndpointPageMode) return;
                e.preventDefault();
                history.pushState(null, '', '#' + summary.id);
                openDetailsForHash('#' + summary.id);
            });
        });

        root.querySelectorAll('.endpoint-back').forEach(back => {
            back.addEventListener('click', (e) => {
                e.preventDefault();
                history.pushState(null, '', window.location.pathname + window.location.search);
                closeEndpointPage();
            });
        });

        root.querySelectorAll('.nav-item').forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                root.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active'));
                item.classList.add('active');

                const href = item.getAttribute('href');
                if (href) {
                    history.pushState(null, '', href);
                    openDetailsForHash(href);
                    closeMobileMenu();
                }
            });
        });

        if (isEndpointPageMode) {
            openDetailsForHash(window.location.hash);

            window.addEventListener('popstate', () => {
                openDetailsForHash(window.location.hash);
                requestAnimationFrame(updateDescriptionToggles);
            });
        }

        root.querySelectorAll('.tab-control').forEach(tabControl => {
            const buttons = tabControl.querySelectorAll(':scope > .tab-header > .tab-button');
            const contents = tabControl.querySelectorAll(':scope > .tab-content');

            buttons.forEach(button => {
                button.addEventListener('click', () => {
                    const tabName = button.getAttribute('data-tab');

                    buttons.forEach(b => b.classList.remove('active'));
                    contents.forEach(c => c.classList.toggle('active', c.getAttribute('data-tab-content') === tabName));

                    button.classList.add('active');
                });
            });
        });

        root.querySelectorAll('.example-variant-select').forEach(select => {
            select.addEventListener('change', () => {
                select.parentElement.querySelectorAll(':scope > .example-variant').forEach(v => {
                    v.classList.toggle('active', v.dataset.variant === select.value);
                });
                updateDescriptionToggles();
            });
        });

        root.querySelectorAll('.example-description-toggle').forEach(toggle => {
            const moreLabel = toggle.textContent;
            toggle.addEventListener('click', () => {
                const expanded = toggle.parentElement.classList.toggle('expanded');
                toggle.textContent = expanded ? toggle.dataset.less : moreLabel;
            });
        });

        function updateDescriptionToggles() {
            root.querySelectorAll('.example-description').forEach(desc => {
                const text = desc.querySelector('.example-description-text');
                if (!text.offsetParent || desc.classList.contains('expanded')) return;
                desc.classList.toggle('overflowing', text.scrollHeight > text.clientHeight + 1);
            });
        }

        root.addEventListener('click', (e) => {
            if (e.target.closest('.tab-button, .nav-item, .endpoint-description > summary, .search-result-item')) {
                requestAnimationFrame(updateDescriptionToggles);
            }
        });
        window.addEventListener('resize', updateDescriptionToggles);
        updateDescriptionToggles();

        const mobileMenuBtn = root.querySelector('.mobile-menu-btn');
        const sidebar = root.querySelector('.sidebar');
        const sidebarOverlay = root.querySelector('.sidebar-overlay');

        function openMobileMenu() {
            if (sidebar) sidebar.classList.add('open');
            if (sidebarOverlay) sidebarOverlay.classList.add('active');
            document.body.style.overflow = 'hidden';
        }

        function closeMobileMenu() {
            if (sidebar) sidebar.classList.remove('open');
            if (sidebarOverlay) sidebarOverlay.classList.remove('active');
            document.body.style.overflow = '';
        }

        if (mobileMenuBtn) {
            mobileMenuBtn.addEventListener('click', () => {
                if (sidebar && sidebar.classList.contains('open')) {
                    closeMobileMenu();
                } else {
                    openMobileMenu();
                }
            });
        }

        if (sidebarOverlay) {
            sidebarOverlay.addEventListener('click', closeMobileMenu);
        }

        const searchOverlay = root.querySelector('.search-overlay');
        const searchInput = root.querySelector('.search-input');
        const searchResults = root.querySelector('.search-results');
        const searchTrigger = root.querySelector('.search-trigger');
        let selectedIndex = -1;
        let currentResults = [];

        const endpointData = [];
        root.querySelectorAll('.endpoint-description').forEach(details => {
            const summary = details.querySelector('summary');
            const method = summary.querySelector('span:first-child')?.textContent || '';
            const path = summary.querySelector('span:nth-child(2)')?.textContent || '';
            const name = summary.querySelector('span:nth-child(3)')?.textContent || '';
            const group = details.closest('.group-section')?.querySelector('h2')?.textContent || '';
            const id = summary.id || '';
            const color = summary.querySelector('span:first-child')?.style.backgroundColor || '';
            endpointData.push({ method, path, name, group, id, color });
        });

        function fuzzyMatch(pattern, text) {
            pattern = pattern.toLowerCase();
            text = text.toLowerCase();
            let patternIdx = 0;
            let score = 0;
            let lastMatchIdx = -1;
            const matches = [];
            
            for (let i = 0; i < text.length && patternIdx < pattern.length; i++) {
                if (text[i] === pattern[patternIdx]) {
                    matches.push(i);
                    if (lastMatchIdx === i - 1) score += 2;
                    else score += 1;
                    if (i === 0 || /[^a-z0-9]/i.test(text[i - 1])) score += 2;
                    lastMatchIdx = i;
                    patternIdx++;
                }
            }
            
            return patternIdx === pattern.length ? { score, matches } : null;
        }

        function multiWordMatch(query, text) {
            const words = query.toLowerCase().split(/\s+/).filter(w => w.length > 0);
            const textLower = text.toLowerCase();
            let totalScore = 0;
            const allMatches = [];
            
            for (const word of words) {
                let wordFound = false;
                
                const substringIdx = textLower.indexOf(word);
                if (substringIdx !== -1) {
                    let wordScore = word.length * 3;
                    if (substringIdx === 0 || /[^a-z0-9]/i.test(text[substringIdx - 1])) wordScore += 5;
                    totalScore += wordScore;
                    for (let i = 0; i < word.length; i++) {
                        allMatches.push(substringIdx + i);
                    }
                    wordFound = true;
                }
                
                if (!wordFound) {
                    const fuzzy = fuzzyMatch(word, text);
                    if (fuzzy) {
                        totalScore += fuzzy.score;
                        allMatches.push(...fuzzy.matches);
                        wordFound = true;
                    }
                }
                
                if (!wordFound) {
                    return null;
                }
            }
            
            return { score: totalScore, matches: [...new Set(allMatches)].sort((a, b) => a - b) };
        }

        function highlightMatches(text, indices) {
            if (!indices || indices.length === 0) return text;
            
            let result = '';
            let lastIdx = 0;
            
            for (let i = 0; i < indices.length; i++) {
                const start = indices[i];
                let end = start;
                
                while (i + 1 < indices.length && indices[i + 1] === end + 1) {
                    end++;
                    i++;
                }
                
                result += text.slice(lastIdx, start);
                result += '<mark>' + text.slice(start, end + 1) + '</mark>';
                lastIdx = end + 1;
            }
            
            result += text.slice(lastIdx);
            return result;
        }

        function search(query) {
            if (!query.trim()) {
                currentResults = [];
                renderResults([]);
                return;
            }

            const results = endpointData.map(item => {
                const nameMatch = multiWordMatch(query, item.name);
                const pathMatch = multiWordMatch(query, item.path);
                const methodMatch = multiWordMatch(query, item.method);
                const groupMatch = multiWordMatch(query, item.group);
                
                const bestScore = Math.max(
                    (nameMatch?.score || 0) * 2,
                    (pathMatch?.score || 0) * 1.5,
                    methodMatch?.score || 0,
                    groupMatch?.score || 0
                );
                
                if (bestScore === 0) return null;
                
                return {
                    ...item,
                    score: bestScore,
                    nameMatches: nameMatch?.matches || [],
                    pathMatches: pathMatch?.matches || []
                };
            }).filter(Boolean).sort((a, b) => b.score - a.score).slice(0, 20);

            currentResults = results;
            selectedIndex = results.length > 0 ? 0 : -1;
            renderResults(results);
        }

        function renderResults(results) {
            if (results.length === 0) {
                const query = searchInput?.value || '';
                searchResults.innerHTML = query.trim() 
                    ? '<div class="search-empty">No results found</div>' 
                    : '<div class="search-empty">Type to search endpoints...</div>';
                return;
            }
            
            searchResults.innerHTML = results.map((item, index) => `
                <a href="#${item.id}" class="search-result-item${index === selectedIndex ? ' selected' : ''}" data-index="${index}">
                    <span class="method-badge" style="display:inline-block;padding:4px 8px;border-radius:4px;font-size:0.75em;font-weight:700;text-transform:uppercase;min-width:50px;text-align:center;background:${item.color}">${item.method}</span>
                    <div class="search-result-info">
                        <div class="search-result-name">${highlightMatches(item.name, item.nameMatches)}</div>
                        <div class="search-result-path">${highlightMatches(item.path, item.pathMatches)}</div>
                    </div>
                    <span class="search-result-group">${item.group}</span>
                </a>
            `).join('');
        }

        function openSearch() {
            if (searchOverlay) {
                searchOverlay.classList.add('open');
                searchInput?.focus();
                searchInput.value = '';
                search('');
            }
        }

        function closeSearch() {
            if (searchOverlay) {
                searchOverlay.classList.remove('open');
                selectedIndex = -1;
            }
        }

        function selectResult(index) {
            if (index >= 0 && index < currentResults.length) {
                selectedIndex = index;
                renderResults(currentResults);
                const selected = searchResults.querySelector('.selected');
                selected?.scrollIntoView({ block: 'nearest' });
            }
        }

        function navigateToSelected() {
            if (selectedIndex >= 0 && currentResults[selectedIndex]) {
                const id = currentResults[selectedIndex].id;
                history.pushState(null, '', '#' + id);
                openDetailsForHash('#' + id);
                closeSearch();
                closeMobileMenu();
            }
        }

        if (searchTrigger) {
            searchTrigger.addEventListener('click', openSearch);
        }

        if (searchOverlay) {
            searchOverlay.addEventListener('click', (e) => {
                if (e.target === searchOverlay) closeSearch();
            });
        }

        if (searchInput) {
            searchInput.addEventListener('input', (e) => search(e.target.value));
            searchInput.addEventListener('keydown', (e) => {
                if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    selectResult(Math.min(selectedIndex + 1, currentResults.length - 1));
                } else if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    selectResult(Math.max(selectedIndex - 1, 0));
                } else if (e.key === 'Enter') {
                    e.preventDefault();
                    navigateToSelected();
                } else if (e.key === 'Escape') {
                    closeSearch();
                }
            });
        }

        if (searchResults) {
            searchResults.addEventListener('click', (e) => {
                const item = e.target.closest('.search-result-item');
                if (item) {
                    e.preventDefault();
                    const index = parseInt(item.dataset.index);
                    selectedIndex = index;
                    navigateToSelected();
                }
            });
        }

        document.addEventListener('keydown', (e) => {
            if (searchOverlay && (e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                openSearch();
            }
            if (e.key === 'Escape' && searchOverlay?.classList.contains('open')) {
                closeSearch();
            }
        });
        // Scroll Spy
        let spyTicking = false;
        const spySections = [];
        
        // Initialize spy sections
        root.querySelectorAll('.nav-item').forEach(item => {
            const href = item.getAttribute('href');
            if (href && href.startsWith('#')) {
                const element = root.querySelector(href);
                if (element) {
                    spySections.push({ item, element });
                }
            }
        });

        function updateActiveSpy() {
            if (document.body.classList.contains('endpoint-page-open')) {
                spyTicking = false;
                return;
            }

            const scrollPos = window.scrollY + 100;
            
            let currentSection = null;
            
            for (const section of spySections) {
                if (section.element.offsetTop <= scrollPos) {
                    currentSection = section;
                }
            }

            if (currentSection) {
                const activeItem = root.querySelector('.nav-item.active');
                if (activeItem !== currentSection.item) {
                    if (activeItem) activeItem.classList.remove('active');
                    currentSection.item.classList.add('active');
                    
                    const group = currentSection.item.closest('.nav-group');
                    if (group && !group.classList.contains('open')) {
                        group.classList.add('open');
                    }
                }
            }
            
            spyTicking = false;
        }

        window.addEventListener('scroll', () => {
            if (!spyTicking) {
                window.requestAnimationFrame(updateActiveSpy);
                spyTicking = true;
            }
        });
        
        updateActiveSpy();
        }
        """;
}
