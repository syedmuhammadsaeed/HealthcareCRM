/**
 * Healthcare CRM — Doctor Module
 */

'use strict';

var dm = {
    allDoctors: [],
    currentPage: 1,
    pageSize: 5,
    totalPages: 1,
    query: '',
    filter: 'all' // all | active | inactive
};

var $;

document.addEventListener('DOMContentLoaded', function () {
    requireAuth();

    var hasTable = document.getElementById('doctor-table-body');
    if (hasTable) {
        cacheListDom();
        bindListEvents();
        loadDoctors();
    }
});

function cacheListDom() {
    $ = {
        tableBody: document.getElementById('doctor-table-body'),
        search:    document.getElementById('search-input'),
        chips:     document.querySelectorAll('#filter-chips button'),
        pagination: document.getElementById('pagination-controls'),
        footerInfo: document.getElementById('footer-info'),
        statTotal:  document.getElementById('stat-total'),
        statActive: document.getElementById('stat-active'),
        statInactive: document.getElementById('stat-inactive')
    };
}

function bindListEvents() {
    var debounceTimer;
    $.search.addEventListener('input', function () {
        clearTimeout(debounceTimer);
        var q = $.search.value.trim().toLowerCase();
        debounceTimer = setTimeout(function () {
            dm.query = q;
            dm.currentPage = 1;
            renderAll();
        }, 300);
    });

    $.chips.forEach(function (chip) {
        chip.addEventListener('click', function () {
            $.chips.forEach(function(c) { c.classList.remove('on'); });
            chip.classList.add('on');
            dm.filter = chip.getAttribute('data-filter');
            dm.currentPage = 1;
            renderAll();
        });
    });
}

async function loadDoctors() {
    try {
        var url = '/api/doctors'; // returns active doctors
        var resp = await authFetch(url);
        if (!resp.ok) throw new Error('Network response was not ok');
        var body = await resp.json();
        if (body.success) {
            dm.allDoctors = body.data || [];
            updateStats();
            renderAll();
        }
    } catch (err) {
        console.error('[Doctor] Load error:', err);
        $.tableBody.innerHTML = '<tr><td colspan="5" style="text-align:center; padding: 20px; color: #C24A3B;">Failed to load doctors.</td></tr>';
    }
}

function updateStats() {
    var total = dm.allDoctors.length;
    var active = dm.allDoctors.filter(d => d.isActive).length;
    var inactive = total - active;
    
    $.statTotal.textContent = total;
    $.statActive.textContent = active;
    $.statInactive.textContent = inactive;
}

function renderAll() {
    // 1. Filter
    var filtered = dm.allDoctors.filter(function(d) {
        if (dm.filter !== 'all') {
            if (dm.filter === 'active' && !d.isActive) return false;
            if (dm.filter === 'inactive' && d.isActive) return false;
        }
        if (dm.query) {
            var searchStr = (d.name + ' ' + d.specialization).toLowerCase();
            if (searchStr.indexOf(dm.query) === -1) return false;
        }
        return true;
    });

    // 2. Paginate
    dm.totalPages = Math.ceil(filtered.length / dm.pageSize) || 1;
    if (dm.currentPage > dm.totalPages) dm.currentPage = dm.totalPages;

    var startIdx = (dm.currentPage - 1) * dm.pageSize;
    var paginated = filtered.slice(startIdx, startIdx + dm.pageSize);

    // 3. Render Table
    if (paginated.length === 0) {
        $.tableBody.innerHTML = '<tr><td colspan="5" style="text-align:center; padding: 40px; color: var(--muted);">No doctors found.</td></tr>';
        $.footerInfo.innerHTML = 'Showing <strong>0</strong> doctors';
    } else {
        $.tableBody.innerHTML = paginated.map(renderRow).join('');
        var endIdx = startIdx + paginated.length;
        $.footerInfo.innerHTML = 'Showing <strong>' + (startIdx + 1) + '–' + endIdx + '</strong> of <strong>' + filtered.length + '</strong> doctors';
    }

    // 4. Render Pagination
    renderPagination();
}

function renderRow(d) {
    var initial = d.name ? d.name.replace('Dr. ', '').charAt(0).toUpperCase() : '?';
    var shortId = d.id ? d.id.substring(d.id.length - 4).toUpperCase() : '';
    
    var statusHtml = d.isActive 
        ? '<span class="status-dot active">Active</span>' 
        : '<span class="status-dot inactive">Inactive</span>';
        
    return (
        '<tr>' +
            '<td><div class="dt-cell"><div class="dt-avatar">' + initial + '</div><div><div class="dt-name">' + esc(d.name) + '</div><div class="dt-sub">DOC-' + shortId + '</div></div></div></td>' +
            '<td><span class="spec-tag">' + esc(d.specialization) + '</span></td>' +
            '<td class="phone">' + esc(d.phone) + '</td>' +
            '<td>' + statusHtml + '</td>' +
            '<td><div class="row-actions"><a href="#">View</a></div></td>' +
        '</tr>'
    );
}

function renderPagination() {
    var html = '';
    
    // Previous
    if (dm.currentPage > 1) {
        html += '<button class="page-btn" id="prev-btn" onclick="changePage(' + (dm.currentPage - 1) + '); return false;">‹ Previous</button>';
    } else {
        html += '<button class="page-btn" id="prev-btn" disabled>‹ Previous</button>';
    }
    
    // Page Numbers
    for (var i = 1; i <= dm.totalPages; i++) {
        if (i === dm.currentPage) {
            html += '<button class="page-btn current" aria-current="page">' + i + '</button>';
        } else {
            html += '<button class="page-btn" onclick="changePage(' + i + '); return false;">' + i + '</button>';
        }
    }
    
    // Next
    if (dm.currentPage < dm.totalPages) {
        html += '<button class="page-btn" id="next-btn" onclick="changePage(' + (dm.currentPage + 1) + '); return false;">Next ›</button>';
    } else {
        html += '<button class="page-btn" id="next-btn" disabled>Next ›</button>';
    }
    
    $.pagination.innerHTML = html;
}

window.changePage = function(page) {
    dm.currentPage = page;
    renderAll();
};

function esc(text) {
    var d = document.createElement('div');
    d.appendChild(document.createTextNode(String(text)));
    return d.innerHTML;
}
