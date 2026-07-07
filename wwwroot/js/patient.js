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
            pm.totalCount = data.totalCount || 0;
            renderTable(data.items || []);
            renderPagination();
        }
    } catch (err) {
        console.error('[Patient] Load error:', err);
        $.tableBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Failed to load patients.</td></tr>';
    }
}

function renderTable(patients) {
    var infoText = document.getElementById('footer-info');
    var pageInfoText = document.getElementById('page-info-text');

    if (patients.length === 0) {
        $.tableBody.innerHTML = '<tr><td colspan="5" style="text-align:center; padding: 40px; color: var(--muted);">No patients found.</td></tr>';
        if (pageInfoText) pageInfoText.textContent = '0 people under care';
        if (infoText) infoText.innerHTML = 'Showing <strong>0</strong> patients';
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

    if (pageInfoText) {
        pageInfoText.textContent = 'Page ' + pm.currentPage + ' of ' + pm.totalPages;
    }
    if (infoText) {
        var start = (pm.currentPage - 1) * pm.pageSize + 1;
        var end = start + filtered.length - 1;
        if (filtered.length === 0) { start = 0; end = 0; }
        infoText.innerHTML = 'Showing <strong>' + start + '–' + end + '</strong> of <strong>' + (pm.totalCount || filtered.length) + '</strong> patients';
    }

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
    var dateStr = p.dateOfBirth ? new Date(p.dateOfBirth).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : 'Unknown';
    var initial = p.name ? p.name.charAt(0).toUpperCase() : '?';
    var shortId = p.id ? p.id.substring(p.id.length - 4).toUpperCase() : '';
    
    return (
        '<tr>' +
            '<td><div class="pt-cell"><div class="pt-avatar">' + initial + '</div><div><div class="pt-name">' + esc(p.name) + '</div><div class="pt-sub">PT-' + shortId + '</div></div></div></td>' +
            '<td class="phone">' + esc(p.phone) + '</td>' +
            '<td class="dob">' + dateStr + '</td>' +
            '<td>' + esc(p.gender) + '</td>' +
            '<td>' +
              '<div class="row-actions">' +
                '<a href="/Patient/Details/' + encodeURIComponent(p.id) + '">View</a><span class="sep"></span>' +
                '<a href="/Patient/Edit/' + encodeURIComponent(p.id) + '">Edit</a><span class="sep"></span>' +
                '<button class="btn-delete btn-delete-card" data-id="' + esc(p.id) + '">Delete</button>' +
              '</div>' +
            '</td>' +
        '</tr>'
    );
}

function renderPagination() {
    var html = '';
    
    // Previous
    if (pm.currentPage > 1) {
        html += '<button class="page-btn" id="prev-btn" onclick="changePage(' + (pm.currentPage - 1) + '); return false;">‹ Previous</button>';
    } else {
        html += '<button class="page-btn" id="prev-btn" disabled>‹ Previous</button>';
    }
    
    // Page Numbers
    for (var i = 1; i <= pm.totalPages; i++) {
        if (i === pm.currentPage) {
            html += '<button class="page-btn current" aria-current="page">' + i + '</button>';
        } else {
            html += '<button class="page-btn" onclick="changePage(' + i + '); return false;">' + i + '</button>';
        }
    }
    
    // Next
    if (pm.currentPage < pm.totalPages) {
        html += '<button class="page-btn" id="next-btn" onclick="changePage(' + (pm.currentPage + 1) + '); return false;">Next ›</button>';
    } else {
        html += '<button class="page-btn" id="next-btn" disabled>Next ›</button>';
    }
    
    $.pagination.innerHTML = html;
    
    // Also bind page-size select if present
    var pageSizeSelect = document.getElementById('page-size-select');
    if (pageSizeSelect && !pageSizeSelect.hasAttribute('data-bound')) {
        pageSizeSelect.value = pm.pageSize.toString();
        pageSizeSelect.addEventListener('change', function() {
            pm.pageSize = parseInt(this.value);
            pm.currentPage = 1;
            loadPatients();
        });
        pageSizeSelect.setAttribute('data-bound', 'true');
    }
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
    try {
        var resp = await authFetch('/api/patients/' + encodeURIComponent(id));
        var body = await resp.json();
        if (body.success && body.data) {
            var p = body.data;
            
            var profileCard = document.getElementById('patient-profile');
            var statusTag = document.getElementById('patient-status');
            if (profileCard) profileCard.style.display = 'block';
            if (statusTag) statusTag.style.display = 'inline-flex';

            var dateStr = p.dateOfBirth ? new Date(p.dateOfBirth).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : 'Unknown';
            var initial = p.name ? p.name.charAt(0).toUpperCase() : '?';
            var shortId = p.id ? p.id.substring(p.id.length - 4).toUpperCase() : '';

            var domName = document.getElementById('patient-name');
            var domAvatar = document.getElementById('patient-avatar');
            var domId = document.getElementById('patient-id-display');
            var domDob = document.getElementById('patient-dob');
            var domGender = document.getElementById('patient-gender');
            var domPhone = document.getElementById('patient-phone');
            var domAddress = document.getElementById('patient-address');
            
            if (domName) domName.textContent = p.name || 'Unknown';
            if (domAvatar) domAvatar.textContent = initial;
            if (domId) domId.textContent = 'Patient ID · PT-' + shortId;
            if (domDob) domDob.textContent = dateStr;
            if (domGender) domGender.textContent = p.gender || 'Unknown';
            if (domPhone) domPhone.textContent = p.phone || 'Unknown';
            if (domAddress) domAddress.textContent = p.address || 'Unknown';
            
            if (statusTag) {
                var s = p.status || 'active';
                statusTag.textContent = s.charAt(0).toUpperCase() + s.slice(1);
            }

            var btnEdit = document.getElementById('btn-edit-patient');
            if (btnEdit) btnEdit.href = '/Patient/Edit/' + encodeURIComponent(p.id);

            var btnDel = document.getElementById('btn-delete-patient');
            if (btnDel) {
                btnDel.onclick = function() {
                    if (confirm('Are you sure you want to delete this patient?')) {
                        deletePatient(p.id).then(function() {
                            window.location.href = '/Patient';
                        });
                    }
                };
            }

            var payload = parseJwt(getToken());
            var role = getRoleFromJwt(payload);
            if (payload && (role === 'Admin' || role === 'SuperAdmin')) {
                var assignSection = document.getElementById('assign-doctor-section');
                if (assignSection) {
                    assignSection.style.display = 'block';
                    loadDoctorsForAssignment(p);
                }
            }
        } else {
            var container = document.getElementById('details-container');
            if (container) container.innerHTML = '<div style="color:var(--coral); padding:20px;">Patient not found.</div>';
        }
    } catch (err) {
        console.error('[Patient] loadDetails error:', err);
        var container = document.getElementById('details-container');
        if (container) container.innerHTML = '<div style="color:var(--coral); padding:20px;">Error loading patient details.</div>';
    }
}

async function loadDoctorsForAssignment(patient) {
    try {
        var resp = await authFetch('/api/doctors');
        var body = await resp.json();
        var select = document.getElementById('assign-doctor-select');
        
        if (body.success && body.data) {
            var doctors = body.data;
            select.innerHTML = '<option value="">-- Select a Doctor --</option>';
            doctors.forEach(function(d) {
                var spec = d.specialization ? ' - ' + d.specialization : '';
                select.innerHTML += '<option value="' + d.id + '">' + esc(d.name) + spec + '</option>';
            });

            if (patient.assignedDoctorId) {
                var doc = doctors.find(function(x) { return x.id === patient.assignedDoctorId; });
                if (doc) {
                    var info = document.getElementById('assigned-info');
                    if (info) info.style.display = 'block';
                    document.getElementById('assigned-doctor-name').textContent = doc.name + (doc.specialization ? ' (' + doc.specialization + ')' : '');
                    
                    var dateStr = '';
                    if (patient.appointmentDate) {
                        dateStr = new Date(patient.appointmentDate).toLocaleDateString();
                    }
                    if (patient.appointmentTime) {
                        dateStr += ' at ' + patient.appointmentTime;
                    }
                    document.getElementById('assigned-appointment').textContent = dateStr;

                    select.value = patient.assignedDoctorId;
                }
            }
            if (patient.appointmentDate) {
                document.getElementById('assign-date').value = patient.appointmentDate.split('T')[0];
            }
            if (patient.appointmentTime) {
                document.getElementById('assign-time').value = patient.appointmentTime;
            }
        } else {
            select.innerHTML = '<option value="">Failed to load doctors</option>';
        }

        var form = document.getElementById('assign-doctor-form');
        form.onsubmit = async function(e) {
            e.preventDefault();
            var btn = document.getElementById('btn-assign-doctor');
            btn.disabled = true;
            btn.textContent = 'Assigning...';
            var alertBox = document.getElementById('assign-alert');
            alertBox.style.display = 'none';

            var payload = {
                doctorId: select.value,
                appointmentDate: document.getElementById('assign-date').value,
                appointmentTime: document.getElementById('assign-time').value
            };

            try {
                var assignResp = await authFetch('/api/patients/' + encodeURIComponent(patient.id) + '/assign', {
                    method: 'POST',
                    body: JSON.stringify(payload)
                });
                var assignBody = await assignResp.json();
                if (assignBody.success) {
                    alertBox.textContent = 'Assigned successfully!';
                    alertBox.style.background = 'var(--sage-light)';
                    alertBox.style.color = 'var(--forest-dark)';
                    alertBox.style.display = 'block';
                    setTimeout(function() { window.location.reload(); }, 1000);
                } else {
                    alertBox.textContent = assignBody.message || 'Failed to assign.';
                    alertBox.style.background = 'var(--coral-light)';
                    alertBox.style.color = 'var(--error)';
                    alertBox.style.display = 'block';
                }
            } catch (ex) {
                alertBox.textContent = 'Network error.';
                alertBox.style.background = 'var(--coral-light)';
                alertBox.style.color = 'var(--error)';
                alertBox.style.display = 'block';
            } finally {
                btn.disabled = false;
                btn.textContent = 'Assign Patient';
            }
        };

    } catch (err) {
        console.error('[Patient] loadDoctors error:', err);
    }
}
