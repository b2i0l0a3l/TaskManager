const UI = {
    loading: document.getElementById("loading"), 
    error: document.getElementById("error"), 
    toast: document.getElementById("toast"), 

    stats: document.getElementById("stats"), 
    shown: document.getElementById("shown"),

    filters: document.getElementById("filters"), 
    search: document.getElementById("search"), 

    title: document.getElementById("title"),
    desc: document.getElementById("desc"), 
    btnAdd: document.getElementById("btnAdd"), 
    btnReset: document.getElementById("btnReset"), 

    btnSeed: document.getElementById("btnSeed"),
    btnClearAll: document.getElementById("btnClearAll"), 

    list: document.getElementById("list"),

    
};

let db = null;
let tasks = [];
let currentFilter = "all";

function showToast(message) {
    UI.toast.textContent = message;
    UI.toast.style.display = "block";
    setTimeout(() => (UI.toast.style.display = "none"), 2200);
}

function setLoading(isLoading) {
    UI.loading.style.display = isLoading ? "flex" : "none";
    UI.btnAdd.disabled = isLoading;
    UI.btnSeed.disabled = isLoading;
    UI.btnClearAll.disabled = isLoading;
}

function setError(message) {
    UI.error.textContent = message || "";
    UI.error.style.display = message ? "block" : "none";
}
function esc(s) {
    return String(s ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function formatDate(ms) {
    return new Date(ms).toLocaleString();
}
function openDB() {
return new Promise((resolve, reject) => {
    const request = indexedDB.open("TaskDB", 1);
    request.onupgradeneeded = () => {
    const db = request.result;
    const store = db.createObjectStore("tasks", {
        keyPath: "id",
        autoIncrement: true,
    });
    store.createIndex("status", "status", { unique: false });
    store.createIndex("createdAt", "createdAt", { unique: false });
    };
    request.onsuccess = () => resolve(request.result);
    request.onerror = () =>
    reject(request.error || new Error("Failed to open IndexedDB"));
});
}

function tx(storeName, mode = "readonly") {
    const transaction = db.transaction(storeName, mode);
    return transaction.objectStore(storeName);
}

function idbRequestToPromise(request) {
    return new Promise((resolve, reject) => {
        request.onsuccess = () => resolve(request.result);
        request.onerror = () =>
        reject(request.error || new Error("IndexedDB request failed"));
    });
}
async function readAllTasks() {
    const store = tx("tasks", "readonly");
    const all = await idbRequestToPromise(store.getAll());
    all.sort((a, b) => (b.createdAt || 0) - (a.createdAt || 0));
    return all;
    }

async function createTask({ title, description }) {
    const now = Date.now();
    const task = {
        title,
        description,
        status: "pending",
        createdAt: now,
        updatedAt: now,
    };
    const store = tx("tasks", "readwrite");
    const id = await idbRequestToPromise(store.add(task));
    return { ...task, id };
}
async function updateTask(id, patch) {
    const store = tx("tasks", "readwrite");
    const existing = await idbRequestToPromise(store.get(id));
    if (!existing) throw new Error("Task not found");
    const updated = {
        ...existing,
        ...patch,
        updatedAt: Date.now(),
    };
    console.log(existing)
    console.log(patch)
    console.log(updated)
    await idbRequestToPromise(store.put(updated));
    return updated;
}

async function deleteTask(id) {
    const store = tx("tasks", "readwrite");
    await idbRequestToPromise(store.delete(id));
}
async function clearAllTasks() {
    const store = tx("tasks", "readwrite");
    await idbRequestToPromise(store.clear());
}

function validateInput() {
    const title = UI.title.value.trim();
    const description = UI.desc.value.trim();

    if (!title) return { ok: false, message: "❌ Title is required" };
    if (title.length < 3)
        return {
        ok: false,
        message: "❌ Title must be at least 3 characters",
        };
    if (description.length > 300)
        return {
        ok: false,
        message: "❌ Description must be 300 characters or less",
        };

    return { ok: true, title, description };
}

function getVisibleTasks() {
    const q = UI.search.value.trim().toLowerCase();
    return tasks.filter((t) => {
        const matchesFilter =
        currentFilter === "all" ? true : t.status === currentFilter;
        const matchesSearch = !q
        ? true
        : (t.title || "").toLowerCase().includes(q) ||
            (t.description || "").toLowerCase().includes(q);
        return matchesFilter && matchesSearch;
    });
}
function render() {
    const visible = getVisibleTasks();
    const total = tasks.length;
    const pending = tasks.filter((t) => t.status === "pending").length;
    const done = tasks.filter((t) => t.status === "done").length;
    UI.stats.textContent = `Total: ${total.toLocaleString()} • Pending: ${pending.toLocaleString()} • Done: ${done.toLocaleString()}`;
    UI.shown.textContent = `${visible.length.toLocaleString()} shown`;

    if (visible.length === 0) {
        UI.list.innerHTML = `<div class="empty muted">No tasks match your filter/search.</div>`;
        return;
    }

    UI.list.innerHTML = visible
        .map((t) => {
        const isDone = t.status === "done";
        const badge = isDone ? "Done" : "Pending";
        const toggleLabel = isDone ? "Mark Pending ↩️" : "Mark Done ✅";
        const statusClass = isDone ? "done" : "pending";

        return `
            <div class="task ${statusClass}" data-id="${t.id}">
            <div>
                <h3>${esc(t.title)}</h3>
                <p>${esc(t.description || "—")}</p>

                <div class="meta">
                <span class="pill">#${t.id}</span>
                <span class="pill">${badge}</span>
                <span class="pill">Created: ${esc(
                    formatDate(t.createdAt)
                )}</span>
                </div>
            </div>

            <div class="actions">
                <button class="secondary" data-action="toggle">${toggleLabel}</button>
                <button class="danger" data-action="delete">Delete 🗑️</button>
            </div>
            </div>
        `;
        })
        .join("");
}
async function handleAdd() {
    setError("");
    const v = validateInput();
    if (!v.ok) return setError(v.message);
    UI.btnAdd.disabled = true;

    try {
        const created = await createTask({
        title: v.title,
        description: v.description,
        });

        tasks.unshift(created);

        UI.title.value = "";
        UI.desc.value = "";

        render();
        showToast("✅ Task added");
    } catch (e) {
        setError(`❌ Add failed: ${e.message}`);
    } finally {
        UI.btnAdd.disabled = false;
    }
}

async function showPopup(message){
    return await new Promise((res,rej)=>{
    const popup = document.querySelector(".popup");
    popup.style.display = "flex";
    document.querySelector(".Question").textContent = message;
    popup.addEventListener("click", async (e)=>{
            const pop = e.target.closest(".popup");
            if(!pop) {
                pop.style.display = "none";
            }
            const btn = e.target.closest("button");
            if(!btn) return;
            popup.style.display = "none";
            res(btn.dataset.actions);
        })
    });
}

async function handleToggle(id) {
    setError("");
    const idx = tasks.findIndex((t) => t.id === id);
    if (idx === -1) return;
    const old = { ...tasks[idx] };
    const newStatus = old.status === "done" ? "pending" : "done";

    tasks[idx] = {
        ...tasks[idx],
        status: newStatus,
        updatedAt: Date.now(),
    };
    render();
    try {
        await updateTask(id, { status: newStatus });
        showToast(
        newStatus === "done" ? "✅ Marked as done" : "✅ Marked as pending"
        );
    } catch (e) {
        tasks[idx] = old;
        render();
        setError(`❌ Update failed: ${e.message}`);
        showToast("❌ Update failed (reverted)");
    }
}
async function handleDelete(id) {
    setError("");

    const t = tasks.find((x) => x.id === id);
    if (!t) return;
    const result = await showPopup(`Delete this task?\n\n"${t.title}"`);
    if(result === "No" || result === undefined)return;

    const backup = [...tasks];
    tasks = tasks.filter((x) => x.id !== id);
    render();

    try {
        await deleteTask(id);
        showToast("✅ Task deleted");
    } catch (e) {
        tasks = backup;
        render();
        setError(`❌ Delete failed: ${e.message}`);
        showToast("❌ Delete failed (reverted)");
    }
}
async function handleSeed() {
    setError("");
    const samples = [
        {
        title: "Record lesson: Event Delegation",
        description: "Add examples with bubbling and delegation.",
        },
        {
        title: "Build IndexedDB Task Project",
        description: "CRUD + search + filters + modern cards.",
        },
        {
        title: "Prepare quiz questions",
        description: "Mix MCQ and true/false for DOM lessons.",
        },
    ];

    UI.btnSeed.disabled = true;

    try {
        for (const s of samples) {
            const created = await createTask({
                title: s.title,
                description: s.description,
            });
            tasks.unshift(created);
        }

        render();
        showToast("✅ Seeded sample tasks");
    } catch (e) {
        setError(`❌ Seed failed: ${e.message}`);
    } finally {
        UI.btnSeed.disabled = false;
    }
 }

async function handleClearAll() {
    setError("");

    if (await showPopup("Clear ALL tasks?\n\nThis cannot be undone.") === "No") return;

    UI.btnClearAll.disabled = true;

    try {
        await clearAllTasks();
        tasks = [];
        render();
        showToast("✅ All tasks cleared");
    } catch (e) {
        setError(`❌ Clear failed: ${e.message}`);
    } finally {
        UI.btnClearAll.disabled = false;
    }
}
function resetForm() {
    UI.title.value = "";
    UI.desc.value = "";
    setError("");
}

UI.list.addEventListener("click", (event) => {
    const btn = event.target.closest("button");
    if (!btn) return;

    const card = event.target.closest(".task");
    if (!card) return;

    const id = Number(card.dataset.id);
    const action = btn.dataset.action;

    if (action === "toggle") handleToggle(id);
    if (action === "delete") handleDelete(id);
});

UI.filters.addEventListener("click", (event) => {
    const btn = event.target.closest("button");
    if (!btn) return;

    currentFilter = btn.dataset.filter;

    [...UI.filters.querySelectorAll("button")].forEach((b) =>
        b.classList.remove("active")
    );
    btn.classList.add("active");

    render();
    });

UI.search.addEventListener("input", render);

UI.btnAdd.addEventListener("click", handleAdd);
UI.btnReset.addEventListener("click", resetForm);
UI.btnSeed.addEventListener("click", handleSeed);
UI.btnClearAll.addEventListener("click", handleClearAll);

window.addEventListener("keydown", (e) => {
    if (e.ctrlKey && e.key === "Enter") handleAdd();
});

(async function boot() {
    setLoading(true);
    setError("");

    try {
        db = await openDB(); 
        tasks = await readAllTasks(); 
        render();
    } catch (e) {
        setError(`❌ Failed to initialize app: ${e.message}`);
    } finally {
        setLoading(false);
    }
})();