/**
 * signalr-client.js — SignalR hub connection and live unpaid-count badge updates.
 *
 * Pages that want live updates must set window._signalRConfig before this script runs:
 *   window._signalRConfig = { groupId: <long>, listId: <long> };
 *
 * The nav badge (#global-unpaid-badge / #global-unpaid-count) is always updated.
 * The in-page badge (#unpaid-badge / #unpaid-count) is updated when on a list details page.
 */

(function () {
    'use strict';

    // Only start if SignalR library is loaded
    if (typeof signalR === 'undefined') return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/expense')
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    // ── Event: server broadcasts new unpaid count ──────────────────────────────
    connection.on('UnpaidCountChanged', function (groupId, listId, newCount, newAmount) {
        // Update the page-level badge (List Details page)
        const countEl  = document.getElementById('unpaid-count');
        const badgeEl  = document.getElementById('unpaid-badge');
        if (countEl) {
            countEl.textContent = newCount;
            // Optionally update the amount text too
            if (badgeEl) {
                badgeEl.innerHTML = `<span id="unpaid-count">${newCount}</span> unpaid &bull; ${formatAmount(newAmount)} ks`;
            }
        }

        // Update global nav badge
        const globalCount = document.getElementById('global-unpaid-count');
        const globalBadge = document.getElementById('global-unpaid-badge');
        if (globalCount && globalBadge) {
            globalCount.textContent = newCount;
            if (newCount > 0) {
                globalBadge.classList.remove('hidden');
                globalBadge.classList.add('inline-flex');
            } else {
                globalBadge.classList.add('hidden');
                globalBadge.classList.remove('inline-flex');
            }
        }
    });

    // ── Start connection ────────────────────────────────────────────────────────
    async function startConnection() {
        try {
            await connection.start();

            // Join the scoped group for the current list (if on list details)
            const cfg = window._signalRConfig;
            if (cfg && cfg.groupId && cfg.listId) {
                await connection.invoke('JoinListGroup', cfg.groupId, cfg.listId);
            }
        } catch (err) {
            // Silent retry is handled by withAutomaticReconnect()
            console.warn('[SignalR] Connection failed, will retry:', err);
        }
    }

    startConnection();

    // ── Reconnected: re-join the group ─────────────────────────────────────────
    connection.onreconnected(async () => {
        const cfg = window._signalRConfig;
        if (cfg && cfg.groupId && cfg.listId) {
            try {
                await connection.invoke('JoinListGroup', cfg.groupId, cfg.listId);
            } catch (e) { /* ignore */ }
        }
    });

    // ── Helpers ────────────────────────────────────────────────────────────────
    function formatAmount(val) {
        return Number(val).toLocaleString();
    }
})();
