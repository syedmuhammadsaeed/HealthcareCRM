/**
 * Healthcare CRM — Login Page JavaScript
 * Handles client-side validation and JWT-based authentication.
 */
'use strict';

(function () {
    // Already logged in — go straight to patients
    if (localStorage.getItem('hcrm_token')) {
        window.location.replace('/Patient');
        return;
    }

    var form           = document.getElementById('login-form');
    var emailInput     = document.getElementById('email');
    var passwordInput  = document.getElementById('password');
    var loginBtn       = document.getElementById('login-btn');
    var loginBtnText   = document.getElementById('login-btn-text');
    var spinner        = document.getElementById('login-spinner');
    var alertContainer = document.getElementById('alert-container');

    // Plain JS regex — no Razor escaping issues
    var EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    function showAlert(message, type) {
        alertContainer.innerHTML =
            '<div class="alert alert-crm alert-' + type + ' mb-3">' +
                escapeHtml(message) +
            '</div>';
    }

    function clearAlert() { alertContainer.innerHTML = ''; }

    function escapeHtml(text) {
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(String(text)));
        return div.innerHTML;
    }

    function setLoading(loading) {
        loginBtn.disabled = loading;
        loginBtnText.textContent = loading ? 'Signing in\u2026' : 'Sign In';
        spinner.classList.toggle('d-none', !loading);
    }

    function validateForm() {
        var valid = true;

        emailInput.classList.remove('is-invalid');
        if (!emailInput.value.trim() || !EMAIL_REGEX.test(emailInput.value.trim())) {
            emailInput.classList.add('is-invalid');
            valid = false;
        }

        passwordInput.classList.remove('is-invalid');
        if (!passwordInput.value) {
            passwordInput.classList.add('is-invalid');
            valid = false;
        }

        return valid;
    }

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        clearAlert();
        if (!validateForm()) return;

        setLoading(true);
        try {
            var response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    email:    emailInput.value.trim(),
                    password: passwordInput.value
                })
            });

            var result = await response.json();

            if (result.success && result.data && result.data.token) {
                localStorage.setItem('hcrm_token', result.data.token);
                window.location.replace('/Patient');
            } else {
                showAlert(result.message || 'Login failed. Please check your credentials.', 'danger');
            }
        } catch (err) {
            console.error('Login fetch error:', err);
            showAlert('Unable to connect to the server. Please try again.', 'danger');
        } finally {
            setLoading(false);
        }
    });
})();
