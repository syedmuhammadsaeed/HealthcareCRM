/**
 * Healthcare CRM — Patient Module v3 (Card UI)
 */

'use strict';

var pm = {
    all: [],
    filtered: [],
    query: '',
    filter: 'all' // all | active | followup | critical
};

var $;

document.addEventListener('DOMContentLoaded', function () {
    requireAuth();

    var hasGrid = document.getElementById('patient-grid');
    var hasForm = document.getElementById('patient-form');

    if (hasGrid) {
        cacheListDom();
        bindListEvents();
        loadPatients();
    } else if (hasForm) {
        initFormPage();
    }
});

function cacheListDom() {
    $ = {
        grid:      document.getElementById('patient-grid'),
        heroSub:   document.getElementById('hero-subtitle'),
        search:    document.getElementById('search-input'),
        chips:     document.querySelectorAll('.pm-chip')
    };
}

function bindListEvents() {
    var debounceTimer;
    $.search.addEventListener('input', function () {
        clearTimeout(debounceTimer);
        var q = $.search.value.trim().toLowerCase();
        debounceTimer = setTimeout(function () {
            pm.query = q;
            filterAndRender();
        }, 300);
    });

    $.chips.forEach(function (chip) {
        chip.addEventListener('click', function () {
            $.chips.forEach(function(c) { c.classList.remove('on'); });
            chip.classList.add('on');
            pm.filter = chip.getAttribute('data-filter');
            filterAndRender();
        });
    });
}

async function loadPatients() {
    try {
        var resp = await authFetch('/api/patients');
        if (!resp.ok) throw new Error('Network response was not ok');
        var body = await resp.json();
        if (body.success) {
            pm.all = body.data || [];

            pm.all.forEach(function(p, i) {
                p.status = p.status || 'active';
                p.city = p.address ? p.address.split(',')[0] : 'Unknown';
            });

            filterAndRender();
        }
    } catch (err) {
        console.error('[Patient] Load error:', err);
        $.grid.innerHTML = '<div style="grid-column:1/-1;text-align:center;padding:40px;color:var(--coral);">Failed to load patients.</div>';
    }
}

function filterAndRender() {
    // 1. Apply Filters
    pm.filtered = pm.all.filter(function (p) {
        // Status Filter
        if (pm.filter !== 'all') {
            if (pm.filter === 'active' && p.status !== 'active') return false;
            if (pm.filter === 'followup' && p.status !== 'followup') return false;
            if (pm.filter === 'critical' && p.status !== 'critical') return false;
        }
        // Text Filter
        if (pm.query) {
            var q = pm.query;
            var match = (p.name || '').toLowerCase().includes(q) ||
                        (p.phone || '').toLowerCase().includes(q) ||
                        (p.city || '').toLowerCase().includes(q);
            if (!match) return false;
        }
        return true;
    });

    // 2. Update Subtitle
    var total = pm.filtered.length;
    var uniqueCities = [...new Set(pm.filtered.map(function(p) { return p.city; }).filter(Boolean))].length;
    var cityText = uniqueCities === 1 ? '1 city' : uniqueCities + ' cities';
    $.heroSub.textContent = total + ' people under care across ' + cityText;

    // 3. Render Cards
    $.grid.innerHTML = pm.filtered.map(renderCard).join('') + renderAddCard();

    // 4. Bind delete buttons
    $.grid.querySelectorAll('.btn-delete-card').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var id = btn.getAttribute('data-id');
            if (confirm('Delete this patient?')) {
                deletePatient(id);
            }
        });
    });
}

function renderCard(p) {
    var initial = ((p.name || '?')[0]).toUpperCase();

    var statusClass = 'pm-status-active';
    var statusText  = 'Active';
    if (p.status === 'followup') { statusClass = 'pm-status-followup'; statusText = 'Follow-up'; }
    if (p.status === 'critical') { statusClass = 'pm-status-critical'; statusText = 'Critical'; }

    var dateStr = p.createdDate ? new Date(p.createdDate).toLocaleDateString() : 'Unknown';

    return (
        '<div class="pm-card">' +
            '<div class="pm-card-top">' +
                '<div class="pm-avatar-lg">' + esc(initial) + '</div>' +
                '<span class="pm-status-tag ' + statusClass + '">' + statusText + '</span>' +
            '</div>' +
            '<h4>' + esc(p.name) + '</h4>' +
            '<div class="pm-meta">' +
                '<span>' + esc(String(p.age)) + ' yrs</span>' +
                '<span>' + esc(p.gender) + '</span>' +
                '<span>' + esc(p.city) + '</span>' +
            '</div>' +
            '<div class="pm-divider"></div>' +
            '<div class="pm-info-row"><span class="pm-k">Phone</span><span>' + esc(p.phone) + '</span></div>' +
            '<div class="pm-info-row"><span class="pm-k">Registered</span><span>' + dateStr + '</span></div>' +
            '<div class="pm-card-actions">' +
                '<a href="/Patient/Edit/' + encodeURIComponent(p.id) + '" class="pm-btn-action primary">Edit</a>' +
                '<button class="pm-btn-action btn-delete-card" data-id="' + esc(p.id) + '">Delete</button>' +
            '</div>' +
        '</div>'
    );
}

function renderAddCard() {
    return (
        '<a href="/Patient/Create" class="pm-card pm-card-add">' +
            '<div class="pm-add-content">' +
                '<div class="pm-add-icon">+</div>' +
                '<div class="pm-add-text">Add new patient</div>' +
            '</div>' +
        '</a>'
    );
}

async function deletePatient(id) {
    try {
        var resp = await authFetch('/api/patients/' + encodeURIComponent(id), { method: 'DELETE' });
        var body = await resp.json();
        if (body.success) {
            pm.all = pm.all.filter(function(p) { return p.id !== id; });
            filterAndRender();
        } else {
            alert('Failed to delete: ' + body.message);
        }
    } catch (err) {
        alert('Network error deleting patient.');
    }
}

function esc(text) {
    var d = document.createElement('div');
    d.appendChild(document.createTextNode(String(text)));
    return d.innerHTML;
}

/* ================================================================
   FORM PAGE
   ================================================================ */
function initFormPage() {
    var patientId = (document.getElementById('patient-id') || {}).value || '';
    var isEdit    = patientId.length > 0;

    if (isEdit) loadForEdit(patientId);

    var form = document.getElementById('patient-form');
    
    // Clear validation on input
    form.addEventListener('input', function(e) {
        var field = e.target.closest('.pm-field');
        if (field) field.classList.remove('invalid');
    });

    form.addEventListener('submit', function (e) {
        e.preventDefault();
        if (!validateForm()) return;
        submitForm(patientId);
    });
}

function validateForm() {
    var isValid = true;
    var fields = ['name', 'age', 'gender', 'status', 'phone', 'address'];
    
    fields.forEach(function(id) {
        var input = document.getElementById(id);
        var fieldDiv = document.getElementById('field-' + id);
        if (input && fieldDiv) {
            if (!input.checkValidity()) {
                fieldDiv.classList.add('invalid');
                isValid = false;
            } else {
                fieldDiv.classList.remove('invalid');
            }
        }
    });
    return isValid;
}

async function loadForEdit(id) {
    try {
        var resp = await authFetch('/api/patients');
        var body = await resp.json();
        if (body.success && Array.isArray(body.data)) {
            var p = body.data.find(function (x) { return x.id === id; });
            if (p) {
                document.getElementById('name').value    = p.name    || '';
                document.getElementById('age').value     = p.age != null ? p.age : '';
                document.getElementById('gender').value  = p.gender  || '';
                document.getElementById('status').value  = p.status  || 'active';
                document.getElementById('phone').value   = p.phone   || '';
                document.getElementById('address').value = p.address || '';
            }
        }
    } catch (err) { console.error('[Patient] loadForEdit error:', err); }
}

async function submitForm(patientId) {
    var isEdit  = patientId && patientId.length > 0;
    var btn     = document.getElementById('submit-btn');

    btn.disabled = true;

    var payload = {
        name:    document.getElementById('name').value.trim(),
        age:     parseInt(document.getElementById('age').value, 10),
        gender:  document.getElementById('gender').value,
        status:  document.getElementById('status').value,
        phone:   document.getElementById('phone').value.trim(),
        address: document.getElementById('address').value.trim()
    };

    try {
        var url    = isEdit ? '/api/patients/' + encodeURIComponent(patientId) : '/api/patients';
        var method = isEdit ? 'PUT' : 'POST';
        var resp   = await authFetch(url, { method: method, body: JSON.stringify(payload) });
        var body   = await resp.json();

        if (body.success) {
            window.location.replace('/Patient');
        } else {
            alert('Failed to save patient.');
        }
    } catch (err) {
        alert('Network error.');
    } finally {
        btn.disabled = false;
    }
}
