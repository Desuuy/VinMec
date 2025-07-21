// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.onload = function() {
    document.getElementById("loginForm").scrollIntoView({
        behavior: "smooth",
        block: "start"
    });
};

// Kiểm tra các trường đầu vào và ngăn gửi form nếu có lỗi
(() => {
    'use strict';

    const form = document.getElementById('registrationForm');
    const phoneInput = document.getElementById('sdt');
    const passwordInput = document.getElementById('pass');
    const repasswordInput = document.getElementById('repass');

    // Regex kiểm tra mật khẩu
    // Regex kiểm tra mật khẩu
    const passwordPattern = new RegExp(
        "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$"
    );

    // Kiểm tra số điện thoại khi nhập
    phoneInput.addEventListener('input', () => {
        const phoneValid = /^\d{10,}$/.test(phoneInput.value);
        toggleValidation(phoneInput, phoneValid);
    });

    // Kiểm tra mật khẩu khi nhập
    passwordInput.addEventListener('input', () => {
        const passwordValid = passwordPattern.test(passwordInput.value);
        toggleValidation(passwordInput, passwordValid);
    });

    // Kiểm tra mật khẩu nhập lại
    repasswordInput.addEventListener('input', () => {
        const passwordsMatch = passwordInput.value === repasswordInput.value;
        toggleValidation(repasswordInput, passwordsMatch);
    });

    // Hàm bật/tắt class is-valid/is-invalid
    function toggleValidation(input, isValid) {
        if (isValid) {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        } else {
            input.classList.remove('is-valid');
            input.classList.add('is-invalid');
        }
    }

    // Xử lý sự kiện submit
    form.addEventListener('submit', function(event) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }
        if (!phoneInput.classList.contains('is-valid') || 
            !passwordInput.classList.contains('is-valid') || 
            !repasswordInput.classList.contains('is-valid')) {
            event.preventDefault();
            
        }
        form.classList.add('was-validated');
    }, false);

})();

function hideVerifyForm() {
    document.getElementById('verifyForm').style.display = 'none';
    document.getElementById('loginForm').style.display = 'block';
}