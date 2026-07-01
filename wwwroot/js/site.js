/**
 * Healthcare CRM – Global JavaScript Utilities
 * Provides JWT token management, auth guard, and authenticated fetch wrapper.
 * Loaded on every page via _Layout.cshtml.
 */

'use strict';

var HCRM_TOKEN_KEY = 'hcrm_token';

// ── Token Management ────────────────────────────────────────────────

/**
 * Retrieves the stored JWT token from localStorage.
 * @returns {string|null} The JWT token, or null if absent.
 */
function getToken() {
    return localStorage.getItem(HCRM_TOKEN_KEY);
}

/**
 * Persists a JWT token in localStorage.
 * @param {string} token - The JWT string to store.
 */
function setToken(token) {
    localStorage.setItem(HCRM_TOKEN_KEY, token);
}

/**
 * Removes the JWT token from localStorage (user logout).
 */
function clearToken() {
    localStorage.removeItem(HCRM_TOKEN_KEY);
}

/**
 * Returns true when a JWT token is present in localStorage.
 * @returns {boolean}
 */
function isAuthenticated() {
    return !!localStorage.getItem(HCRM_TOKEN_KEY);
}

// ── Auth Guard ──────────────────────────────────────────────────────

/**
 * Redirects to the login page if the user is not authenticated.
 * Call at the top of any protected page's script block.
 */
function requireAuth() {
    if (!isAuthenticated()) {
        window.location.replace('/Account/Login');
    }
}

/**
 * Clears the JWT token and redirects to the login page.
 * Bound to the Logout button in _Layout.cshtml.
 */
function logout() {
    clearToken();
    window.location.replace('/Account/Login');
}

// ── Authenticated Fetch ─────────────────────────────────────────────

/**
 * Drop-in replacement for fetch() that automatically attaches the
 * Authorization: Bearer header and handles 401 responses.
 *
 * @param {string}      url     - Request URL.
 * @param {RequestInit} options - Standard fetch options (optional).
 * @returns {Promise<Response>}
 */
async function authFetch(url, options) {
    options = options || {};
    options.headers = Object.assign({}, options.headers || {});

    var token = getToken();
    if (token) {
        options.headers['Authorization'] = 'Bearer ' + token;
    }

    if (!options.headers['Content-Type']) {
        options.headers['Content-Type'] = 'application/json';
    }

    var response = await fetch(url, options);

    if (response.status === 401) {
        clearToken();
        window.location.replace('/Account/Login');
    }

    return response;
}

// ── Navbar Auth State ───────────────────────────────────────────────
// Toggles authenticated vs guest navigation items on every page load.

document.addEventListener('DOMContentLoaded', function () {
    var navAuthenticated = document.getElementById('nav-authenticated');
    var navGuest         = document.getElementById('nav-guest');

    if (isAuthenticated()) {
        if (navAuthenticated) navAuthenticated.style.display = 'flex';
        if (navGuest)         navGuest.style.display         = 'none';
    } else {
        if (navAuthenticated) navAuthenticated.style.display = 'none';
        if (navGuest)         navGuest.style.display         = '';
    }
});
