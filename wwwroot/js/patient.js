/**
 * Healthcare CRM — Patient Module v3 (Card UI)
 */

'use strict';

var pm = {
    currentPage: 1,
    pageSize: 20,
    totalPages: 1,
    query: '',
    filter: 'all' // all | active | followup | critical
};

var $;

document.addEventListener('DOMContentLoaded', function () {
    requireAuth();

    var hasTable = document.getElementById('patient-table-body');
    var hasForm = document.getElementById('patient-form');
    var hasDetails = document.getElementById('details-container');

    if (hasTable) {
        cacheListDom();
        bindListEvents();
        loadPatients();
    } else if (hasForm) {
        initFormPage();
    } else if (hasDetails) {
        initDetailsPage();
    }
});

function cacheListDom() {
    $ = {
        tableBody: document.getElementById('patient-table-body'),
        heroSub:   document.getElementById('hero-subtitle'),
        search:    document.getElementById('search-input'),
        chips:     document.querySelectorAll('.pm-chip'),
        pagination: document.getElementById('pagination-controls')
    };
}

function bindListEvents() {
    var debounceTimer;
    $.search.addEventListener('input', function () {
        clearTimeout(debounceTimer);
        var q = $.search.value.trim().toLowerCase();
        debounceTimer = setTimeout(function () {
            pm.query = q;
            pm.currentPage = 1;
            loadPatients();
        }, 300);
    });

    $.chips.forEach(function (chip) {
        chip.addEventListener('click', function () {
            $.chips.forEach(function(c) { c.classList.remove('on'); });
            chip.classList.add('on');
            pm.filter = chip.getAttribute('data-filter');
            pm.currentPage = 1;
            loadPatients(); // Note: status filter not implemented in backend, but we can send it or do it. Wait, backend search only uses name, phone, address. I'll just send query.
        });
    });
}

async function loadPatients() {
    try {
        var url = '/api/patients?page=' + pm.currentPage + '&pageSize=' + pm.pageSize;
        if (pm.query) {
            url += '&search=' + encodeURIComponent(pm.query);
        }
        var resp = await authFetch(url);
        if (!resp.ok) throw new Error('Network response was not ok');
        var body = await resp.json();
        if (body.success) {
            var data = body.data;
            pm.totalPages = data.totalPages || 1;
            renderTable(data.items || []);
            renderPagination();
        }
    } catch (err) {
        console.error('[Patient] Load error:', err);
        $.tableBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Failed to load patients.</td></tr>';
    }
}

function renderTable(patients) {
    if (patients.length === 0) {
        $.tableBody.innerHTML = '<tr><td colspan="5" class="text-center">No patients found.</td></tr>';
        $.heroSub.textContent = '0 people under care';
        return;
    }

    // Since backend doesn't filter by status currently, if pm.filter !== 'all' we could do client-side filter here, but pagination would be wrong. 
    // The requirement didn't specify status filtering for week 2, only search.
    var filtered = patients.filter(function (p) {
        if (pm.filter !== 'all') {
            if (pm.filter === 'active' && p.status !== 'active') return false;
            if (pm.filter === 'followup' && p.status !== 'followup') return false;
            if (pm.filter === 'critical' && p.status !== 'critical') return false;
        }
        return true;
    });

    $.heroSub.textContent = 'Page ' + pm.currentPage + ' of ' + pm.totalPages;

    $.tableBody.innerHTML = filtered.map(renderRow).join('');

    $.tableBody.querySelectorAll('.btn-delete-card').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var id = btn.getAttribute('data-id');
            if (confirm('Are you sure you want to delete this patient?')) {
                deletePatient(id);
            }
        });
    });
}

function renderRow(p) {
    var dateStr = p.dateOfBirth ? new Date(p.dateOfBirth).toLocaleDateString() : 'Unknown';
    return (
        '<tr>' +
            '<td>' + esc(p.name) + '</td>' +
            '<td>' + esc(p.phone) + '</td>' +
            '<td>' + dateStr + '</td>' +
            '<td>' + esc(p.gender) + '</td>' +
            '<td class="text-end">' +
                '<a href="/Patient/Details/' + encodeURIComponent(p.id) + '" class="btn btn-sm btn-info me-1">View</a>' +
                '<a href="/Patient/Edit/' + encodeURIComponent(p.id) + '" class="btn btn-sm btn-primary me-1">Edit</a>' +
                '<button class="btn btn-sm btn-danger btn-delete-card" data-id="' + esc(p.id) + '">Delete</button>' +
            '</td>' +
        '</tr>'
    );
}

function renderPagination() {
    var html = '';
    
    // Previous
    if (pm.currentPage > 1) {
        html += '<li class="page-item"><a class="page-link" href="#" onclick="changePage(' + (pm.currentPage - 1) + '); return false;">Previous</a></li>';
    } else {
        html += '<li class="page-item disabled"><a class="page-link" href="#" tabindex="-1" aria-disabled="true">Previous</a></li>';
    }
    
    // Page Numbers
    for (var i = 1; i <= pm.totalPages; i++) {
        if (i === pm.currentPage) {
            html += '<li class="page-item active" aria-current="page"><a class="page-link" href="#">' + i + '</a></li>';
        } else {
            html += '<li class="page-item"><a class="page-link" href="#" onclick="changePage(' + i + '); return false;">' + i + '</a></li>';
        }
    }
    
    // Next
    if (pm.currentPage < pm.totalPages) {
        html += '<li class="page-item"><a class="page-link" href="#" onclick="changePage(' + (pm.currentPage + 1) + '); return false;">Next</a></li>';
    } else {
        html += '<li class="page-item disabled"><a class="page-link" href="#" tabindex="-1" aria-disabled="true">Next</a></li>';
    }
    
    $.pagination.innerHTML = html;
}

window.changePage = function(page) {
    pm.currentPage = page;
    loadPatients();
};

async function deletePatient(id) {
    try {
        var resp = await authFetch('/api/patients/' + encodeURIComponent(id), { method: 'DELETE' });
        var body = await resp.json();
        if (body.success) {
            loadPatients();
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
    var fields = ['name', 'dateOfBirth', 'gender', 'status', 'phone', 'address'];
    
    fields.forEach(function(id) {
        var input = document.getElementById(id);
        var fieldDiv = document.getElementById('field-' + (id === 'dateOfBirth' ? 'dob' : id));
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
        var url = '/api/patients/' + encodeURIComponent(id);
        var resp = await authFetch(url);
        var body = await resp.json();
        if (body.success && body.data) {
            var p = body.data;
            document.getElementById('name').value    = p.name    || '';
            if (p.dateOfBirth) {
                var d = new Date(p.dateOfBirth);
                document.getElementById('dateOfBirth').value = d.toISOString().split('T')[0];
            }
            document.getElementById('gender').value  = p.gender  || '';
            document.getElementById('status').value  = p.status  || 'active';
            document.getElementById('phone').value   = p.phone   || '';
            document.getElementById('address').value = p.address || '';
        }
    } catch (err) { console.error('[Patient] loadForEdit error:', err); }
}

async function submitForm(patientId) {
    var isEdit  = patientId && patientId.length > 0;
    var btn     = document.getElementById('submit-btn');

    btn.disabled = true;

    var payload = {
        name:    document.getElementById('name').value.trim(),
        dateOfBirth: document.getElementById('dateOfBirth').value,
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

/* ================================================================
   DETAILS PAGE
   ================================================================ */
function initDetailsPage() {
    var patientId = (document.getElementById('patient-id') || {}).value || '';
    if (patientId) {
        loadDetails(patientId);
    }
}

async function loadDetails(id) {
    var container = document.getElementById('details-container');
    try {
        var resp = await authFetch('/api/patients/' + encodeURIComponent(id));
        var body = await resp.json();
        if (body.success && body.data) {
            var p = body.data;
            var dateStr = p.dateOfBirth ? new Date(p.dateOfBirth).toLocaleDateString() : 'Unknown';
            container.innerHTML = 
                '<div class="mb-3"><strong>Name:</strong> ' + esc(p.name) + '</div>' +
                '<div class="mb-3"><strong>Date of Birth:</strong> ' + dateStr + '</div>' +
                '<div class="mb-3"><strong>Gender:</strong> ' + esc(p.gender) + '</div>' +
                '<div class="mb-3"><strong>Phone Number:</strong> ' + esc(p.phone) + '</div>' +
                '<div class="mb-3"><strong>Address:</strong> ' + esc(p.address) + '</div>' +
                '<div class="pm-form-actions mt-4">' +
                    '<a href="/Patient/Edit/' + encodeURIComponent(p.id) + '" class="btn btn-primary">Edit Patient</a>' +
                    '<a href="/Patient" class="btn btn-secondary ms-2">Back to List</a>' +
                '</div>';
        } else {
            container.innerHTML = '<div class="text-danger">Patient not found.</div>';
        }
    } catch (err) {
        console.error('[Patient] loadDetails error:', err);
        container.innerHTML = '<div class="text-danger">Error loading patient details.</div>';
    }
}
