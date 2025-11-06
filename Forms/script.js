const apiUrl = "https://localhost:7042/api/students";
const modalElement = document.getElementById("studentModal");
const studentModal = new bootstrap.Modal(modalElement);
const form = document.getElementById("studentForm");
const modalTitle = document.getElementById("modalTitle");

let isEdit = false;
let editId = null;

function toTitleCase(input) {
    if (!input) return input;
    input = input.toLowerCase(); // tüm harfler küçük
    return input.charAt(0).toUpperCase() + input.slice(1); // ilk harf büyük
}

// Open modal for create
document.getElementById("openCreateModal").addEventListener("click", () => openModal(false));

function openModal(edit = false, student = null) {
    isEdit = edit;
    form.reset();
    form.classList.remove('was-validated');

    if (edit && student) {
        modalTitle.textContent = "Update Student";
        document.getElementById("studentId").value = student.id;
        document.getElementById("firstName").value = student.firstName;
        document.getElementById("lastName").value = student.lastName;
        document.getElementById("studentNumber").value = student.studentNumber;
        document.getElementById("grade").value = student.grade;
        document.getElementById("email").value = student.email || "";
        editId = student.id;
    } else {
        modalTitle.textContent = "Add Student";
        editId = null;
    }

    studentModal.show();
}

async function loadStudents() {
    const res = await fetch(apiUrl);
    const data = await res.json();
    const tbody = document.querySelector("#studentTable tbody");

    tbody.innerHTML = "";

    data.forEach(s => {
        const row = `
        <tr>
            <td>${s.id}</td>
            <td>${s.firstName}</td>
            <td>${s.lastName}</td>
            <td>${s.studentNumber}</td>
            <td>${s.grade}</td>
            <td>${s.email || ""}</td>
            <td>
                <button class="btn btn-sm btn-warning me-1" onclick='openModal(true, ${JSON.stringify(s).replace(/"/g, '&quot;')})'>Edit</button>
                <button class="btn btn-sm btn-danger" onclick="deleteStudent(${s.id})">Delete</button>
            </td>
        </tr>`;
        tbody.innerHTML += row;
    });
}

async function deleteStudent(id) {
    if (!confirm("Are you sure you want to delete this student?")) return;
    await fetch(`${apiUrl}/${id}`, { method: "DELETE" });
    loadStudents();
}



// Save student (POST or PUT)
form.addEventListener("submit", async (e) => {
    e.preventDefault();
    form.classList.add('was-validated');

    if (!form.checkValidity()) return;

    const student = {
        firstName: toTitleCase(document.getElementById("firstName").value.trim()),
        lastName: toTitleCase(document.getElementById("lastName").value.trim()),
        studentNumber: document.getElementById("studentNumber").value.trim(),
        grade: parseInt(document.getElementById("grade").value),
        email: document.getElementById("email").value.trim() || null
    };

    const payload = isEdit
        ? { id: editId, ...student }
        : student;

    try {
        const res = await fetch(isEdit ? `${apiUrl}/${editId}` : apiUrl, {
            method: isEdit ? "PUT" : "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            studentModal.hide();
            loadStudents();
        } else {
            const err = await res.text();
            alert("Error: " + err);
        }
    } catch (error) {
        alert("Connection error: " + error);
    }
});
// Search function
document.getElementById("searchInput").addEventListener("input", function () {
    const filter = this.value.toLowerCase();
    const rows = document.querySelectorAll("#studentTable tbody tr");

    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(filter) ? "" : "none";
    });
});
loadStudents();