/**
 * Healthcare CRM — Doctor Module
 */

'use strict';

var $;

document.addEventListener('DOMContentLoaded', function () {
    requireAuth();

    var hasTable = document.getElementById('doctor-table-body');

    if (hasTable) {
        cacheListDom();
        loadDoctors();
    }
});

function cacheListDom() {
    $ = {
        tableBody: document.getElementById('doctor-table-body'),
        heroSub:   document.getElementById('hero-subtitle')
    };
}

async function loadDoctors() {
    try {
        var url = '/api/doctors';
        var resp = await authFetch(url);
        if (!resp.ok) throw new Error('Network response was not ok');
        var body = await resp.json();
        if (body.success) {
            var data = body.data;
            renderTable(data || []);
        }
    } catch (err) {
        console.error('[Doctor] Load error:', err);
        $.tableBody.innerHTML = '<tr><td colspan="4" class="text-center text-danger">Failed to load doctors.</td></tr>';
    }
}

function renderTable(doctors) {
    if (doctors.length === 0) {
        $.tableBody.innerHTML = '<tr><td colspan="4" class="text-center">No active doctors found.</td></tr>';
        $.heroSub.textContent = '0 active doctors';
        return;
    }

    $.heroSub.textContent = doctors.length + ' active doctors';

    $.tableBody.innerHTML = doctors.map(renderRow).join('');
}

function renderRow(d) {
    var statusSpan = d.isActive 
        ? '<span class="badge bg-success">Active</span>' 
        : '<span class="badge bg-secondary">Inactive</span>';
        
    return (
        '<tr>' +
            '<td>' + esc(d.name) + '</td>' +
            '<td>' + esc(d.specialization) + '</td>' +
            '<td>' + esc(d.phone) + '</td>' +
            '<td>' + statusSpan + '</td>' +
        '</tr>'
    );
}

function esc(text) {
    var d = document.createElement('div');
    d.appendChild(document.createTextNode(String(text)));
    return d.innerHTML;
}
