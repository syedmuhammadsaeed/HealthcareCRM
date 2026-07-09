/**
 * Healthcare CRM — Register Page JavaScript
 * Handles client-side validation, UI interactivity, and new account creation.
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
    var confirmInput         = document.getElementById('confirm');
    var termsInput           = document.getElementById('terms');
    var registerBtn          = document.getElementById('register-btn');
    var registerBtnText      = document.getElementById('register-btn-text');
    var spinner              = document.getElementById('register-spinner');
    var alertContainer       = document.getElementById('alert-container');

    // Plain JS regex
    var EMAIL_REGEX    = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    var PASSWORD_REGEX = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9]).{8,}$/;

    function showAlert(message, type) {
        alertContainer.innerHTML =
            '<div class="alert alert-crm alert-' + type + '">' +
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
        registerBtnText.textContent = loading ? 'Creating account...' : 'Create account';
        spinner.style.display = loading ? 'inline-block' : 'none';
    }

    // ---- Simple field validation ----
    function wireField(input, errorId, validate){
        var error = document.getElementById(errorId);
        input.addEventListener('blur', function() {
            var valid = validate(input.value);
            input.setAttribute('aria-invalid', String(!valid));
            input.classList.toggle('is-valid', valid);
            error.classList.toggle('show', !valid);
        });
        input.addEventListener('input', function() {
            if (input.getAttribute('aria-invalid') === 'true' && validate(input.value)) {
                input.setAttribute('aria-invalid', 'false');
                input.classList.add('is-valid');
                error.classList.remove('show');
            }
        });
    }

    wireField(fullNameInput, 'fullName-error', function(v) { return v.trim().length >= 2; });
    wireField(emailInput, 'email-error', function(v) { return EMAIL_REGEX.test(v.trim()); });

    // ---- Password visibility toggle ----
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

    // ---- Live password strength + requirement checklist ----
    var reqs = {
        len: function(v) { return v.length >= 8; },
        upper: function(v) { return /[A-Z]/.test(v); },
        lower: function(v) { return /[a-z]/.test(v); },
        num: function(v) { return /[0-9]/.test(v); },
        special: function(v) { return /[^A-Za-z0-9]/.test(v); }
    };
    var bars = document.querySelectorAll('.pw-meter .bar');
    var strengthText = document.getElementById('pw-strength-text');
    var barColors = ['#C0392B', '#E08E45', '#D9B23C', '#3E7C4A'];
    var labels = ['Too weak', 'Weak', 'Good', 'Strong'];

    passwordInput.addEventListener('input', function() {
        var val = passwordInput.value;
        var metCount = 0;
        Object.keys(reqs).forEach(function(key) {
            var fn = reqs[key];
            var li = document.querySelector('[data-req="' + key + '"]');
            if (li) {
                var met = fn(val);
                li.classList.toggle('met', met);
                if (met) metCount++;
            }
        });
        var level = val.length === 0 ? 0 : Math.max(1, Math.ceil((metCount / 5) * 4));
        bars.forEach(function(bar, i) {
            bar.style.background = i < level ? barColors[level - 1] : 'var(--line)';
        });
        strengthText.textContent = val.length === 0 ? 'Too weak' : labels[level - 1];
        strengthText.style.color = val.length === 0 ? 'var(--muted)' : barColors[level - 1];
    });

    // ---- Confirm password matching ----
    var confirmError = document.getElementById('confirm-error');
    function checkMatch(){
        if (confirmInput.value.length === 0) {
            confirmInput.setAttribute('aria-invalid', 'false');
            confirmInput.classList.remove('is-valid');
            confirmError.classList.remove('show');
            return;
        }
        var match = confirmInput.value === passwordInput.value;
        confirmInput.setAttribute('aria-invalid', String(!match));
        confirmInput.classList.toggle('is-valid', match);
        confirmError.classList.toggle('show', !match);
    }
    confirmInput.addEventListener('input', checkMatch);
    confirmInput.addEventListener('blur', checkMatch);
    passwordInput.addEventListener('input', function() { if (confirmInput.value) checkMatch(); });

    termsInput.addEventListener('change', function() {
        if (termsInput.checked) {
            document.getElementById('terms-error').classList.remove('show');
        }
    });

    // ---- Role Toggle ----
    var roleCards = document.querySelectorAll('.role-card');
    roleCards.forEach(function(card) {
        card.addEventListener('click', function() {
            roleCards.forEach(function(c) { c.classList.remove('selected'); });
            card.classList.add('selected');
            var rInput = card.querySelector('input');
            if (rInput) {
                rInput.checked = true;
                rInput.dispatchEvent(new Event('change'));
            }
        });
    });

    var roleAdmin = document.getElementById('role-admin');
    var roleDoctor = document.getElementById('role-doctor');
    var doctorFields = document.getElementById('doctor-fields');
    var specializationInput = document.getElementById('specialization');
    var phoneInput = document.getElementById('phone');
    var addressInput = document.getElementById('address');

    function toggleDoctorFields() {
        if (roleDoctor.checked) {
            doctorFields.style.display = 'block';
        } else {
            doctorFields.style.display = 'none';
        }
    }
    roleAdmin.addEventListener('change', toggleDoctorFields);
    roleDoctor.addEventListener('change', toggleDoctorFields);

    // ---- Final Form Submission Validation ----
    function validateFormOnSubmit() {
        var valid = true;

        if (!fullNameInput.value.trim() || fullNameInput.value.trim().length < 2) {
            fullNameInput.setAttribute('aria-invalid', 'true');
            fullNameInput.classList.remove('is-valid');
            document.getElementById('fullName-error').classList.add('show');
            valid = false;
        }
        if (!EMAIL_REGEX.test(emailInput.value.trim())) {
            emailInput.setAttribute('aria-invalid', 'true');
            emailInput.classList.remove('is-valid');
            document.getElementById('email-error').classList.add('show');
            valid = false;
        }
        if (!PASSWORD_REGEX.test(passwordInput.value)) {
            // We can rely on the live strength meter to guide them, but we still block submission
            passwordInput.focus(); 
            valid = false;
        }
        if (!confirmInput.value || confirmInput.value !== passwordInput.value) {
            confirmInput.setAttribute('aria-invalid', 'true');
            confirmInput.classList.remove('is-valid');
            confirmError.classList.add('show');
            valid = false;
        }
        if (!termsInput.checked) {
            document.getElementById('terms-error').classList.add('show');
            valid = false;
        }

        return valid;
    }

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        clearAlert();
        
        if (!validateFormOnSubmit()) return;

        setLoading(true);
        try {
            var selectedRole = document.querySelector('input[name="role"]:checked').value;
            var response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    fullName:        fullNameInput.value.trim(),
                    email:           emailInput.value.trim(),
                    password:        passwordInput.value,
                    confirmPassword: confirmInput.value,
                    role:            selectedRole,
                    specialization:  selectedRole === 'Doctor' ? specializationInput.value.trim() : null,
                    phone:           selectedRole === 'Doctor' ? phoneInput.value.trim() : null,
                    address:         selectedRole === 'Doctor' ? addressInput.value.trim() : null
                })
            });

            var result = await response.json();

            if (result.success) {
                showAlert('Account created successfully! Redirecting to login...', 'success');
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
            if (!result || !result.success) {
                setLoading(false);
            }
        }
    });
})();
