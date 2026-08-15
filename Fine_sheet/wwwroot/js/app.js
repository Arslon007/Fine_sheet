const API = '';

// ==================== NAVIGATION ====================

function showPage(name) {
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.getElementById('page-' + name).classList.add('active');

    document.querySelectorAll('nav button').forEach(b => b.classList.remove('active'));
    event.target.classList.add('active');

    if (name === 'fines') { populateEmployeeSelects(); loadFines(); }
    if (name === 'bonuses') { populateEmployeeSelects(); loadBonuses(); }
    if (name === 'salary') { populateEmployeeSelects(); }
    if (name === 'employees') loadEmployees();
}

// ==================== TOAST ====================

function toast(msg, type = 'success') {
    const el = document.getElementById('toast');
    el.textContent = msg;
    el.className = 'toast ' + type + ' show';
    setTimeout(() => el.classList.remove('show'), 3000);
}

// ==================== API HELPERS ====================

async function api(url, options) {
    const res = await fetch(API + url, options);
    if (!res.ok) {
        const text = await res.text();
        let msg;
        try { msg = JSON.parse(text).message || text; } catch { msg = text; }
        throw new Error(msg);
    }
    const text = await res.text();
    return text ? JSON.parse(text) : null;
}

function formatMoney(v) {
    return Number(v).toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' \u0440\u0443\u0431.';
}

function formatDate(d) {
    return new Date(d).toLocaleDateString('ru-RU');
}

// ==================== EMPLOYEES ====================

let employeesCache = [];

async function loadEmployees() {
    try {
        employeesCache = await api('/api/employee');
        renderEmployees();
    } catch (e) {
        toast(e.message, 'error');
    }
}

function renderEmployees() {
    const tbody = document.getElementById('employees-table');
    const empty = document.getElementById('employees-empty');

    if (!employeesCache.length) {
        tbody.innerHTML = '';
        empty.style.display = '';
        return;
    }
    empty.style.display = 'none';
    tbody.innerHTML = employeesCache.map(e => `
        <tr>
            <td>${e.id}</td>
            <td>${e.fullName}</td>
            <td>${e.position}</td>
            <td>${formatMoney(e.salary)}</td>
        </tr>
    `).join('');
}

async function addEmployee(ev) {
    ev.preventDefault();
    const body = {
        fullName: document.getElementById('emp-name').value.trim(),
        position: document.getElementById('emp-position').value,
        salary: parseFloat(document.getElementById('emp-salary').value)
    };
    try {
        const res = await api('/api/employee', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        toast(res.message);
        document.getElementById('form-employee').reset();
        loadEmployees();
    } catch (e) {
        toast(e.message, 'error');
    }
}

// ==================== EMPLOYEE SELECTS ====================

function populateEmployeeSelects() {
    const selects = [
        'fine-employee', 'fines-view-employee',
        'bonus-employee', 'bonuses-view-employee',
        'salary-employee'
    ];
    selects.forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        const current = el.value;
        el.innerHTML = '<option value="">Выберите сотрудника...</option>' +
            employeesCache.map(e => `<option value="${e.id}">${e.fullName}</option>`).join('');
        if (current) el.value = current;
    });
}

// ==================== FINES ====================

async function addFine(ev) {
    ev.preventDefault();
    const body = {
        employeeId: parseInt(document.getElementById('fine-employee').value),
        reason: document.getElementById('fine-reason').value.trim(),
        amount: parseFloat(document.getElementById('fine-amount').value)
    };
    try {
        const res = await api('/api/fine', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        toast(res.message);
        document.getElementById('form-fine').reset();
        populateEmployeeSelects();
        loadFines();
    } catch (e) {
        toast(e.message, 'error');
    }
}

async function loadFines() {
    const empId = document.getElementById('fines-view-employee').value;
    const tbody = document.getElementById('fines-table');
    const empty = document.getElementById('fines-empty');
    const total = document.getElementById('fines-total');

    if (!empId) {
        tbody.innerHTML = '';
        empty.style.display = '';
        empty.textContent = 'Выберите сотрудника';
        total.style.display = 'none';
        return;
    }
    try {
        const [fines, totals] = await Promise.all([
            api('/api/fine/employee/' + empId),
            api('/api/fine/employee/' + empId + '/total')
        ]);
        if (!fines.length) {
            tbody.innerHTML = '';
            empty.style.display = '';
            empty.textContent = 'Нет штрафов';
            total.style.display = 'none';
            return;
        }
        empty.style.display = 'none';
        tbody.innerHTML = fines.map(f => `
            <tr>
                <td>${formatDate(f.date)}</td>
                <td>${f.reason}</td>
                <td style="color:#c62828;font-weight:600">-${formatMoney(f.amount)}</td>
            </tr>
        `).join('');
        total.style.display = '';
        total.innerHTML = `\u0418\u0442\u043e\u0433\u043e \u0448\u0442\u0440\u0430\u0444\u043e\u0432: <span style="color:#c62828">${formatMoney(totals.totalFines)}</span>`;
    } catch (e) {
        toast(e.message, 'error');
    }
}

// ==================== BONUSES ====================

async function addBonus(ev) {
    ev.preventDefault();
    const body = {
        employeeId: parseInt(document.getElementById('bonus-employee').value),
        reason: document.getElementById('bonus-reason').value.trim(),
        amount: parseFloat(document.getElementById('bonus-amount').value)
    };
    try {
        const res = await api('/api/bonus', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        toast(res.message);
        document.getElementById('form-bonus').reset();
        populateEmployeeSelects();
        loadBonuses();
    } catch (e) {
        toast(e.message, 'error');
    }
}

async function loadBonuses() {
    const empId = document.getElementById('bonuses-view-employee').value;
    const tbody = document.getElementById('bonuses-table');
    const empty = document.getElementById('bonuses-empty');
    const total = document.getElementById('bonuses-total');

    if (!empId) {
        tbody.innerHTML = '';
        empty.style.display = '';
        empty.textContent = 'Выберите сотрудника';
        total.style.display = 'none';
        return;
    }
    try {
        const [bonuses, totals] = await Promise.all([
            api('/api/bonus/employee/' + empId),
            api('/api/bonus/employee/' + empId + '/total')
        ]);
        if (!bonuses.length) {
            tbody.innerHTML = '';
            empty.style.display = '';
            empty.textContent = 'Нет бонусов';
            total.style.display = 'none';
            return;
        }
        empty.style.display = 'none';
        tbody.innerHTML = bonuses.map(b => `
            <tr>
                <td>${formatDate(b.date)}</td>
                <td>${b.reason}</td>
                <td style="color:#2e7d32;font-weight:600">+${formatMoney(b.amount)}</td>
            </tr>
        `).join('');
        total.style.display = '';
        total.innerHTML = `\u0418\u0442\u043e\u0433\u043e \u0431\u043e\u043d\u0443\u0441\u043e\u0432: <span style="color:#2e7d32">${formatMoney(totals.totalBonuses)}</span>`;
    } catch (e) {
        toast(e.message, 'error');
    }
}

// ==================== SALARY ====================

async function loadSalary() {
    const empId = document.getElementById('salary-employee').value;
    const result = document.getElementById('salary-result');
    const empty = document.getElementById('salary-empty');

    if (!empId) {
        result.style.display = 'none';
        empty.style.display = '';
        return;
    }
    try {
        const s = await api('/api/salary/' + empId);
        document.getElementById('sal-name').textContent = s.fullName;
        document.getElementById('sal-position').textContent = s.position;
        document.getElementById('sal-base').textContent = formatMoney(s.baseSalary);
        document.getElementById('sal-bonuses').textContent = '+' + formatMoney(s.totalBonuses);
        document.getElementById('sal-fines').textContent = '-' + formatMoney(s.totalFines);
        document.getElementById('sal-final').textContent = formatMoney(s.finalSalary);
        result.style.display = '';
        empty.style.display = 'none';
    } catch (e) {
        toast(e.message, 'error');
    }
}

// ==================== INIT ====================

document.addEventListener('DOMContentLoaded', loadEmployees);
