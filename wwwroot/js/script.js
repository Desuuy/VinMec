document.getElementById('formChuyenNganh').addEventListener('submit', function (event) {
    event.preventDefault(); // Ngăn chặn gửi form mặc định

    const formData = new FormData(this);
    
    // Giả sử bạn có một API endpoint để xử lý gửi dữ liệu
    fetch('/api/chuyennganh', {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        // Thêm dòng mới vào bảng
        const table = document.getElementById('chuyenNganhList');
        const newRow = table.insertRow();
        newRow.innerHTML = `
            <td>${data.MaCN}</td>
            <td>${data.TenCN}</td>
            <td><img src="${data.HinhAnhUrl}" class="image-preview" alt="Hình Ảnh"></td>
            <td>
                <button class="delete" onclick="deleteEntry(${data.MaCN})">Xóa</button>
            </td>
        `;
        // Xóa form
        this.reset();
        document.getElementById('imagePreview').style.display = 'none'; // Ẩn hình ảnh xem trước
    })
    .catch(error => {
        console.error('Error:', error);
    });
});

function deleteEntry(maCN) {
    // Thực hiện logic xóa tại đây
    const table = document.getElementById('chuyenNganhList');
    const rows = table.getElementsByTagName('tr');
    for (let i = 0; i < rows.length; i++) {
        const cells = rows[i].getElementsByTagName('td');
        if (cells.length > 0 && cells[0].innerText == maCN) {
            table.deleteRow(i);
            break;
        }
    }
}
