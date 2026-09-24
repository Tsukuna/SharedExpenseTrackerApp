/**
 * api.js — Centralised Axios wrappers for all MVC action endpoints.
 * All mutating calls (create, update, delete, toggle) go through Axios (JSON).
 * Data retrieval is handled server-side via ViewData.
 */

// ── Axios instance ────────────────────────────────────────────────────────────
const _http = axios.create({
    headers: {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest'
    },
    withCredentials: true
});

// Attach CSRF token from the hidden anti-forgery field (if present on the page)
_http.interceptors.request.use(config => {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) config.headers['RequestVerificationToken'] = token;
    return config;
});

// ── Toast on any 4xx/5xx ─────────────────────────────────────────────────────
_http.interceptors.response.use(
    res => res,
    err => {
        // Let the caller handle it; they can call showToast themselves
        return Promise.reject(err);
    }
);

// ── Public API object ─────────────────────────────────────────────────────────
const api = {

    // ── Lists ─────────────────────────────────────────────────────────────────
    getGroupLists(groupId) {
        return _http.get(`/Lists/GroupLists?groupId=${groupId}`);
    },

    createList(groupId, name) {
        return _http.post('/Lists/Create', { groupId, name });
    },

    editList(listId, name) {
        return _http.post('/Lists/Edit', { listId, name });
    },

    deleteList(listId, groupId) {
        return _http.post('/Lists/Delete', { listId, groupId });
    },

    // ── Expenses ──────────────────────────────────────────────────────────────
    createExpense(listId, groupId, name, amount, note = null) {
        return _http.post('/Expenses/Create', { listId, groupId, name, amount, note });
    },

    togglePaid(expenseId, listId, groupId, isPaid) {
        return _http.post('/Expenses/TogglePaid', { expenseId, listId, groupId, isPaid });
    }
};

// ── Global toast helper ───────────────────────────────────────────────────────
/**
 * Show a toast notification.
 * @param {string} message
 * @param {'success'|'error'|'info'} type
 */
function showToast(message, type = 'info') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const colours = {
        success: 'bg-emerald-600',
        error: 'bg-red-600',
        info: 'bg-brand-600'
    };

    const icons = {
        success: '<path stroke-linecap="round" stroke-linejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />',
        error: '<path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />',
        info: '<path stroke-linecap="round" stroke-linejoin="round" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />'
    };

    const toast = document.createElement('div');
    toast.className = `pointer-events-auto flex items-center gap-3 rounded-xl ${colours[type]} px-4 py-3 text-sm font-medium text-white shadow-lg
                       translate-x-2 opacity-0 transition-all duration-300`;
    toast.innerHTML = `
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            ${icons[type]}
        </svg>
        <span>${message}</span>`;

    container.appendChild(toast);

    // Animate in
    requestAnimationFrame(() => {
        toast.classList.remove('translate-x-2', 'opacity-0');
        toast.classList.add('translate-x-0', 'opacity-100');
    });

    // Remove after 3s
    setTimeout(() => {
        toast.classList.add('opacity-0', 'translate-x-2');
        toast.addEventListener('transitionend', () => toast.remove());
    }, 3000);
}
