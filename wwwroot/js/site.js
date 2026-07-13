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
 * Removes the JWT token from localStorage and cookies (user logout).
 */
function clearToken() {
    localStorage.removeItem(HCRM_TOKEN_KEY);
    document.cookie = 'hcrm_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
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

/**
 * Parses the payload of a JWT token.
 */
function parseJwt(token) {
    try {
        var base64Url = token.split('.')[1];
        var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        return JSON.parse(jsonPayload);
    } catch (e) {
        return null;
    }
}

/**
 * Extracts the user role from the parsed JWT payload.
 * Handles the fully qualified Microsoft claim URI.
 */
function getRoleFromJwt(payload) {
    if (!payload) return null;
    return payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
}

// ── Navbar Auth State ───────────────────────────────────────────────
// Toggles authenticated vs guest navigation items on every page load.

document.addEventListener('DOMContentLoaded', function () {
    var navAuthenticated = document.getElementById('nav-authenticated');
    var navLogout        = document.getElementById('nav-logout-btn');
    var navGuest         = document.getElementById('nav-guest');

    if (isAuthenticated()) {
        if (navAuthenticated) navAuthenticated.style.display = 'flex';
        if (navLogout)        navLogout.style.display        = 'flex';
        if (navGuest)         navGuest.style.display         = 'none';

        var payload = parseJwt(getToken());
        if (payload && payload.name) {
            var nameEl = document.getElementById('nav-user-name');
            var avatarEl = document.getElementById('nav-user-avatar');
            if (nameEl) nameEl.textContent = payload.name;
            if (avatarEl) {
                var parts = payload.name.trim().split(' ');
                var initials = 'U';
                if (parts.length > 0) {
                    var firstWord = parts[0].replace(/[^a-zA-Z]/g, '');
                    if (firstWord.toLowerCase() === 'dr' && parts.length > 1) {
                        initials = 'DR';
                    } else if (parts.length === 1) {
                        initials = parts[0].substring(0, 2).toUpperCase();
                    } else {
                        initials = (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
                    }
                }
                avatarEl.textContent = initials;
            }
            
            // Show Approvals link if user is SuperAdmin
            var role = getRoleFromJwt(payload);
            var navApprovals = document.getElementById('nav-approvals');
            if (navApprovals && role === 'SuperAdmin') {
                navApprovals.style.display = ''; // Use default styling (e.g. flex or inline-block)
            }
        }
    } else {
        if (navAuthenticated) navAuthenticated.style.display = 'none';
        if (navLogout)        navLogout.style.display        = 'none';
        if (navGuest)         navGuest.style.display         = '';
    }
});
