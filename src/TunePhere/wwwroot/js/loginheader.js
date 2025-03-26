function animateThenRedirect(url) {
    console.log("Hàm được gọi với URL:", url);
    const container = document.querySelector(".container");

    if (url.includes('Register')) {
        container.classList.add("sign-up-mode");
    } else {
        container.classList.remove("sign-up-mode");
    }

    console.log("Animation started");

    setTimeout(function () {
        window.location.href = url;
    }, 1000);

    return false;
}
if (localStorage.getItem('pageTransitionTarget')) {
    // Ẩn content trước khi tải
    document.documentElement.style.visibility = 'hidden';

    // Thêm style cho lớp che phủ
    const style = document.createElement('style');
    style.textContent = `
                .page-transition-overlay {
                    position: fixed;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    background: linear-gradient(-45deg, #000000, #130f40, #8e2de2, #4a00e0);
                    background-size: 400% 400%;
                    z-index: 9999;
                    pointer-events: none;
                    opacity: 1;
                }
            `;
    document.head.appendChild(style);

    // Thêm lớp che phủ ngay lập tức
    document.addEventListener('DOMContentLoaded', function () {
        document.documentElement.style.visibility = '';
    });
}