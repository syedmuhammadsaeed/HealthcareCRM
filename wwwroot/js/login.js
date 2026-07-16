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
        loginBtnText.textContent = loading ? 'Signing in...' : 'Sign in';
        spinner.style.display = loading ? 'inline-block' : 'none';
    }

    function validateForm() {
        var valid = true;

        var emailError = document.getElementById('email-error');
        if (!emailInput.value.trim()) {
            emailInput.setAttribute('aria-invalid', 'true');
            emailError.innerText = 'Email is required.';
            emailError.style.display = 'flex';
            valid = false;
        } else if (!EMAIL_REGEX.test(emailInput.value.trim())) {
            emailInput.setAttribute('aria-invalid', 'true');
            emailError.innerText = 'Please enter a valid email address.';
            emailError.style.display = 'flex';
            valid = false;
        } else {
            emailInput.setAttribute('aria-invalid', 'false');
            emailError.style.display = 'none';
        }

        var pwError = document.getElementById('password-error');
        if (!passwordInput.value) {
            passwordInput.setAttribute('aria-invalid', 'true');
            pwError.innerText = 'Password is required.';
            pwError.style.display = 'flex';
            valid = false;
        } else {
            passwordInput.setAttribute('aria-invalid', 'false');
            pwError.style.display = 'none';
        }

        return valid;
    }

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        clearAlert();
        if (!validateForm()) return;

        setLoading(true);
        try {
            var selectedRole = document.querySelector('input[name="role"]:checked').value;
            var response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    email:    emailInput.value.trim(),
                    password: passwordInput.value,
                    role:     selectedRole
                })
            });

            var result = await response.json();

            if (result.success && result.data && result.data.token) {
                localStorage.setItem('hcrm_token', result.data.token);
                document.cookie = 'hcrm_token=' + result.data.token + '; path=/; SameSite=Lax';
                
                var base64Url = result.data.token.split('.')[1];
                var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
                var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
                    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
                }).join(''));
                var payload = JSON.parse(jsonPayload);
                var actualRole = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

                if (actualRole === 'Doctor') {
                    window.location.replace('/Doctor');
                } else if (actualRole === 'SuperAdmin') {
                    window.location.replace('/SuperAdmin');
                } else if (actualRole === 'Admin') {
                    window.location.replace('/Patient');
                } else {
                    window.location.replace('/Home/Index');
                }
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

    // Inline validation clearance on input (to avoid scolding user while typing)
    [emailInput, passwordInput].forEach(function(input) {
        input.addEventListener('input', function() {
            if (input.getAttribute('aria-invalid') === 'true') {
                input.setAttribute('aria-invalid', 'false');
                var err = document.getElementById(input.id + '-error');
                if (err) err.style.display = 'none';
            }
        });
    });

    // Password visibility toggle
    var toggleBtn = document.querySelector('.toggle-pw');
    if (toggleBtn) {
        toggleBtn.addEventListener('click', function() {
            var isPassword = passwordInput.type === 'password';
            passwordInput.type = isPassword ? 'text' : 'password';
            toggleBtn.textContent = isPassword ? 'Hide' : 'Show';
            toggleBtn.setAttribute('aria-pressed', String(isPassword));
            toggleBtn.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
            passwordInput.focus();
        });
    }

    // Role selector: toggle the selected pill
    var rolePills = document.querySelectorAll('.role-pill');
    rolePills.forEach(function(pill) {
      pill.addEventListener('click', function() {
        rolePills.forEach(function(p) { p.classList.remove('selected'); });
        pill.classList.add('selected');
        var rInput = pill.querySelector('input');
        if (rInput) {
            rInput.checked = true;
            rInput.dispatchEvent(new Event('change'));
        }
      });
    });

})();
