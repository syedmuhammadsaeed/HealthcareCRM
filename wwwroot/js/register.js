/**
 * Healthcare CRM — Register Page JavaScript
 * Handles client-side validation and new account creation.
 */
'use strict';

(function () {
    // Already logged in — go straight to patients
    if (localStorage.getItem('hcrm_token')) {
        window.location.replace('/Patient');
        return;
    }

    var form                 = document.getElementById('register-form');
    var fullNameInput        = document.getElementById('fullName');
    var emailInput           = document.getElementById('email');
    var passwordInput        = document.getElementById('password');
    var confirmPasswordInput = document.getElementById('confirmPassword');
    var registerBtn          = document.getElementById('register-btn');
    var registerBtnText      = document.getElementById('register-btn-text');
    var spinner              = document.getElementById('register-spinner');
    var alertContainer       = document.getElementById('alert-container');

    // Plain JS regex — no Razor escaping issues
    var EMAIL_REGEX    = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    var PASSWORD_REGEX = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9]).{8,}$/;

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
        registerBtn.disabled = loading;
        registerBtnText.textContent = loading ? 'Creating account\u2026' : 'Create Account';
        spinner.classList.toggle('d-none', !loading);
    }

    function validateForm() {
        var valid = true;

        fullNameInput.classList.remove('is-invalid');
        if (!fullNameInput.value.trim() || fullNameInput.value.trim().length < 2) {
            fullNameInput.classList.add('is-invalid');
            valid = false;
        }

        emailInput.classList.remove('is-invalid');
        if (!emailInput.value.trim() || !EMAIL_REGEX.test(emailInput.value.trim())) {
            emailInput.classList.add('is-invalid');
            valid = false;
        }

        passwordInput.classList.remove('is-invalid');
        if (!PASSWORD_REGEX.test(passwordInput.value)) {
            passwordInput.classList.add('is-invalid');
            valid = false;
        }

        confirmPasswordInput.classList.remove('is-invalid');
        if (!confirmPasswordInput.value || passwordInput.value !== confirmPasswordInput.value) {
            confirmPasswordInput.classList.add('is-invalid');
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
            var response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    fullName:        fullNameInput.value.trim(),
                    email:           emailInput.value.trim(),
                    password:        passwordInput.value,
                    confirmPassword: confirmPasswordInput.value
                })
            });

            var result = await response.json();

            if (result.success) {
                showAlert('Account created successfully! Redirecting to login\u2026', 'success');
                setTimeout(function () {
                    window.location.replace('/Account/Login');
                }, 1500);
            } else {
                showAlert(result.message || 'Registration failed. Please try again.', 'danger');
            }
        } catch (err) {
            console.error('Register fetch error:', err);
            showAlert('Unable to connect to the server. Please try again.', 'danger');
        } finally {
            setLoading(false);
        }
    });
})();
