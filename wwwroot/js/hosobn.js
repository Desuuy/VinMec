// scripts.js

// Hàm tìm kiếm bệnh nhân theo số điện thoại
function searchPatient() {
    const phone = document.getElementById("searchInput").value;

    // Gọi API tìm kiếm (giả lập hoặc thực tế)
    fetch(`/api/hosos?phone=${phone}`)
        .then(response => response.json())
        .then(data => {
            // Xóa dữ liệu hiện tại trong bảng
            const tableBody = document.getElementById("patientTableBody");
            tableBody.innerHTML = "";

            // Đổ dữ liệu mới vào bảng
            data.forEach(patient => {
                const row = document.createElement("tr");

                row.innerHTML = `
                    <td>${patient.MaHS}</td>
                    <td>${patient.TenND}</td>
                    <td>${patient.sdt}</td>
                    <td>${patient.DiaChi}</td>
                    <td>${patient.NgaySinh}</td>
                    <td>${patient.GioiTinh}</td>
                    <td>${patient.Email}</td>
                    <td>${patient.MoTaBenh}</td>
                `;
                tableBody.appendChild(row);
            });
        })
        .catch(error => console.error("Error:", error));
}
// Hiển thị modal
function showForm() {
    document.getElementById("patientFormModal").style.display = "block";
}

// Ẩn modal
function hideForm() {
    document.getElementById("patientFormModal").style.display = "none";
}