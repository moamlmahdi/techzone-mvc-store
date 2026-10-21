// ==============================================================
// TechZone Store - Standalone Interactive App Logic (HTML/CSS/JS)
// ==============================================================

const initialProducts = [
    {
        id: 1,
        name: "لابتوب MacBook Pro 16 M3 Max",
        category: "حواسيب محمولة",
        brand: "Apple",
        price: 12499.00,
        stock: 15,
        image: "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=800&q=80",
        desc: "معالج Apple M3 Max بـ 16 نواة، ذاكرة موحدة 48GB RAM، تخزين 1TB SSD، شاشة Liquid Retina XDR مقاس 16.2 بوصة."
    },
    {
        id: 2,
        name: "هاتف Samsung Galaxy S24 Ultra 5G",
        category: "هواتف ذكية",
        brand: "Samsung",
        price: 4899.00,
        stock: 28,
        image: "https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?auto=format&fit=crop&w=800&q=80",
        desc: "شاشة Dynamic AMOLED 2X مقاس 6.8 بوصة، إطار تيتانيوم، كاميرا 200MP مع ميزات Galaxy AI وقلم S-Pen."
    },
    {
        id: 3,
        name: "سماعة رأس لاسلكية Sony WH-1000XM5",
        category: "صوتيات وملحقات",
        brand: "Sony",
        price: 1399.00,
        stock: 42,
        image: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=80",
        desc: "سماعة رأس احترافية بأفضل تقنية عزل ضوضاء نشط (ANC)، بطارية تدوم حتى 30 ساعة، وصوت Hi-Res."
    },
    {
        id: 4,
        name: "شاشة الألعاب والتصميم LG UltraGear OLED 34",
        category: "شاشات العرض",
        brand: "LG",
        price: 3650.00,
        stock: 9,
        image: "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=800&q=80",
        desc: "شاشة منحنية UltraWide مقاس 34 بوصة بتقنية OLED، معدل تحديث 240Hz وزمن استجابة 0.03ms."
    }
];

let products = JSON.parse(localStorage.getItem("tz_products")) || initialProducts;
let users = JSON.parse(localStorage.getItem("tz_users")) || [
    { fullName: "مدير النظام (Admin)", username: "admin", email: "admin@techzone.sa", pass: "Admin@123", role: "Admin" },
    { fullName: "أحمد محمد (مستخدم)", username: "user", email: "user@techzone.sa", pass: "User@123", role: "User" }
];
let currentUser = JSON.parse(localStorage.getItem("tz_current_user")) || null;
let productModalInstance = null;

document.addEventListener("DOMContentLoaded", () => {
    productModalInstance = new bootstrap.Modal(document.getElementById("productModal"));
    updateAuthUI();
    renderProducts();
});

function notify(msg) {
    const box = document.getElementById("notificationBox");
    document.getElementById("notificationText").textContent = msg;
    box.classList.remove("d-none");
    setTimeout(() => box.classList.add("d-none"), 4500);
}

function showSection(name) {
    document.querySelectorAll(".spa-section").forEach(s => s.classList.remove("active"));
    document.getElementById(`section-${name}`).classList.add("active");

    document.querySelectorAll(".navbar-nav .nav-link").forEach(l => l.classList.remove("active"));
    const activeLink = document.getElementById(`nav-${name}`);
    if (activeLink) activeLink.classList.add("active");
    window.scrollTo({ top: 0, behavior: "smooth" });
}

function updateAuthUI() {
    const area = document.getElementById("authHeaderArea");
    const navAdd = document.getElementById("nav-add-item");
    const btnAddTop = document.getElementById("btnAddProductTop");
    const hint = document.getElementById("rolePermissionHint");

    const isAdmin = currentUser && currentUser.role === "Admin";

    if (isAdmin) {
        navAdd.classList.remove("d-none");
        btnAddTop.classList.remove("d-none");
        hint.innerHTML = `<span class="text-success fw-bold"><i class="bi bi-shield-lock-fill me-1"></i>أنت مسجل كـ Admin: يمكنك الإضافة والتعديل والحذف.</span>`;
    } else {
        navAdd.classList.add("d-none");
        btnAddTop.classList.add("d-none");
        hint.innerHTML = `<span class="text-secondary"><i class="bi bi-eye-fill text-info me-1"></i>الصلاحية الحالية: <strong>قراءة فقط (User Read-Only)</strong>. سجل دخولك كـ Admin لتفعيل الإضافة والتعديل والحذف.</span>`;
    }

    if (currentUser) {
        area.innerHTML = `
            <div class="d-flex align-items-center gap-2 bg-dark bg-opacity-50 px-3 py-1 rounded-pill border border-secondary">
                <span class="text-white small fw-bold">${currentUser.fullName}</span>
                <span class="badge ${isAdmin ? 'bg-warning text-dark' : 'bg-info text-dark'}">${isAdmin ? 'Admin' : 'User (قراءة فقط)'}</span>
            </div>
            <button class="btn btn-outline-light btn-sm rounded-pill px-3" onclick="logout()">خروج</button>
        `;
    } else {
        area.innerHTML = `
            <button class="btn btn-outline-light btn-sm rounded-pill px-3" onclick="showSection('login')">تسجيل الدخول (Login)</button>
            <button class="btn btn-info btn-sm rounded-pill px-3 fw-bold text-dark" onclick="showSection('register')">إنشاء حساب (User)</button>
        `;
    }
    renderProducts();
}

function renderProducts() {
    const container = document.getElementById("productsContainer");
    const search = (document.getElementById("searchBox")?.value || "").toLowerCase();
    const cat = document.getElementById("categoryFilter")?.value || "";
    const isAdmin = currentUser && currentUser.role === "Admin";

    const filtered = products.filter(p => {
        const matchesSearch = p.name.toLowerCase().includes(search) || p.brand.toLowerCase().includes(search) || p.desc.toLowerCase().includes(search);
        const matchesCat = !cat || p.category === cat;
        return matchesSearch && matchesCat;
    });

    if (filtered.length === 0) {
        container.innerHTML = `<div class="col-12 text-center py-5 text-muted">لا توجد منتجات مطابقة للبحث.</div>`;
        return;
    }

    container.innerHTML = filtered.map(p => `
        <div class="col-md-6 col-lg-4">
            <div class="card h-100 border-0 shadow-sm rounded-4 overflow-hidden tz-card-hover">
                <div class="position-relative bg-white text-center p-3 border-bottom" style="height:210px;">
                    <img src="${p.image}" class="img-fluid h-100 object-fit-contain" onerror="this.src='https://placehold.co/600x400/1e293b/38bdf8?text=TechZone'" />
                    <span class="badge bg-dark position-absolute top-0 start-0 m-3 rounded-pill">${p.category}</span>
                    <span class="badge ${p.stock > 0 ? 'bg-success' : 'bg-danger'} position-absolute top-0 end-0 m-3 rounded-pill">الكمية: ${p.stock}</span>
                </div>
                <div class="card-body d-flex flex-column p-4">
                    <small class="text-primary fw-bold">${p.brand}</small>
                    <h5 class="fw-bold mt-1 mb-2">${p.name}</h5>
                    <p class="text-secondary small flex-grow-1">${p.desc}</p>
                    <div class="d-flex justify-content-between align-items-center pt-3 border-top mt-2">
                        <span class="fs-5 fw-extrabold text-success">${Number(p.price).toLocaleString()} ر.س</span>
                        <span class="badge bg-light text-secondary border">ID #${p.id}</span>
                    </div>
                    ${isAdmin ? `
                        <div class="d-flex gap-2 mt-3 pt-3 border-top">
                            <button class="btn btn-warning btn-sm flex-grow-1 rounded-pill fw-bold" onclick="openProductModal(${p.id})">
                                <i class="bi bi-pencil-square me-1"></i> تعديل
                            </button>
                            <button class="btn btn-outline-danger btn-sm flex-grow-1 rounded-pill fw-bold" onclick="deleteProduct(${p.id})">
                                <i class="bi bi-trash3-fill me-1"></i> حذف
                            </button>
                        </div>
                    ` : ''}
                </div>
            </div>
        </div>
    `).join("");
}

function handleLogin(e) {
    e.preventDefault();
    const u = document.getElementById("spaLoginUser").value.trim();
    const p = document.getElementById("spaLoginPass").value.trim();
    quickLogin(u, p);
}

function quickLogin(u, p) {
    const found = users.find(x => (x.username.toLowerCase() === u.toLowerCase() || x.email.toLowerCase() === u.toLowerCase()) && x.pass === p);
    if (!found) {
        alert("بيانات الدخول غير صحيحة!");
        return;
    }
    currentUser = found;
    localStorage.setItem("tz_current_user", JSON.stringify(currentUser));
    updateAuthUI();
    notify(`مرحباً ${found.fullName}! تم تسجيل الدخول بصلاحية (${found.role}).`);
    showSection("products");
}

function handleRegister(e) {
    e.preventDefault();
    const fullName = document.getElementById("regFullName").value.trim();
    const username = document.getElementById("regUsername").value.trim();
    const email = document.getElementById("regEmail").value.trim();
    const pass = document.getElementById("regPassword").value.trim();

    if (users.some(x => x.username.toLowerCase() === username.toLowerCase() || x.email.toLowerCase() === email.toLowerCase())) {
        alert("اسم المستخدم أو البريد مسجل مسبقاً!");
        return;
    }

    const newUser = { fullName, username, email, pass, role: "User" };
    users.push(newUser);
    localStorage.setItem("tz_users", JSON.stringify(users));
    currentUser = newUser;
    localStorage.setItem("tz_current_user", JSON.stringify(currentUser));
    updateAuthUI();
    notify(`تم إنشاء حسابك بنجاح يا ${fullName} بصلاحية (User - قراءة فقط).`);
    showSection("products");
}

function logout() {
    currentUser = null;
    localStorage.removeItem("tz_current_user");
    updateAuthUI();
    notify("تم تسجيل الخروج بنجاح.");
}

function openProductModal(id = null) {
    if (!currentUser || currentUser.role !== "Admin") {
        alert("غير مصرح! هذه العملية مخصصة للـ Admin فقط.");
        return;
    }
    if (id) {
        const p = products.find(x => x.id === id);
        document.getElementById("productModalTitle").textContent = "تعديل بيانات المنتج (Admin)";
        document.getElementById("prodId").value = p.id;
        document.getElementById("prodName").value = p.name;
        document.getElementById("prodCategory").value = p.category;
        document.getElementById("prodBrand").value = p.brand;
        document.getElementById("prodPrice").value = p.price;
        document.getElementById("prodStock").value = p.stock;
        document.getElementById("prodImage").value = p.image;
        document.getElementById("prodDesc").value = p.desc;
    } else {
        document.getElementById("productModalTitle").textContent = "إضافة منتج جديد (Admin)";
        document.getElementById("prodId").value = "";
        document.getElementById("prodName").value = "";
        document.getElementById("prodCategory").value = "حواسيب محمولة";
        document.getElementById("prodBrand").value = "";
        document.getElementById("prodPrice").value = "";
        document.getElementById("prodStock").value = 10;
        document.getElementById("prodImage").value = "https://images.unsplash.com/photo-1526738549149-8e07eca6c147?auto=format&fit=crop&w=800&q=80";
        document.getElementById("prodDesc").value = "";
    }
    productModalInstance.show();
}

function saveProduct(e) {
    e.preventDefault();
    const id = document.getElementById("prodId").value;
    const item = {
        id: id ? Number(id) : (products.length ? Math.max(...products.map(x => x.id)) + 1 : 1),
        name: document.getElementById("prodName").value.trim(),
        category: document.getElementById("prodCategory").value,
        brand: document.getElementById("prodBrand").value.trim(),
        price: Number(document.getElementById("prodPrice").value),
        stock: Number(document.getElementById("prodStock").value),
        image: document.getElementById("prodImage").value.trim() || "https://placehold.co/600x400/1e293b/38bdf8?text=TechZone",
        desc: document.getElementById("prodDesc").value.trim()
    };

    if (id) {
        const idx = products.findIndex(x => x.id === Number(id));
        products[idx] = item;
        notify(`تم تعديل المنتج «${item.name}» بنجاح!`);
    } else {
        products.unshift(item);
        notify(`تمت إضافة المنتج «${item.name}» بنجاح!`);
    }

    localStorage.setItem("tz_products", JSON.stringify(products));
    productModalInstance.hide();
    renderProducts();
}

function deleteProduct(id) {
    if (!currentUser || currentUser.role !== "Admin") return;
    const p = products.find(x => x.id === id);
    if (confirm(`هل أنت متأكد من حذف المنتج «${p.name}»؟`)) {
        products = products.filter(x => x.id !== id);
        localStorage.setItem("tz_products", JSON.stringify(products));
        renderProducts();
        notify(`تم حذف المنتج «${p.name}» نهائياً.`);
    }
}
