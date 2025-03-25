document.addEventListener('DOMContentLoaded', function () {
    console.log("DOM loaded");
    
    // Lấy container
    const container = document.querySelector(".container");
    
    // Kiểm tra xem có phải lần đầu truy cập hay không
    if (sessionStorage.getItem('skipContainerAnimation')) {
        // Đang trong quá trình chuyển trang, bỏ qua animation phóng to
        container.classList.add('no-animation');
        // Xóa dấu hiệu này sau khi đã xử lý
        sessionStorage.removeItem('skipContainerAnimation');
    } else {
        // Lần đầu truy cập, thêm animation phóng to
        container.classList.add('first-load');
    }
    
    // Xử lý lớp che phủ nếu đang trong quá trình chuyển trang
    if (localStorage.getItem('pageTransitionTarget')) {
        // Tạo lớp che phủ
        if (!document.querySelector('.page-transition-overlay')) {
            const overlay = document.createElement('div');
            overlay.className = 'page-transition-overlay active';
            document.body.appendChild(overlay);
        } else {
            document.querySelector('.page-transition-overlay').classList.add('active');
        }
        
        // Thêm class preload vào body để ngăn transitions
        document.body.classList.add('preload');
    }
    
    // Xác định các nút trong từng trang
    const loginButtons = document.querySelectorAll('button[onclick*="Login"]');
    const registerButtons = document.querySelectorAll('button[onclick*="Register"]');
    
    // Log để debug
    console.log("Login buttons:", loginButtons.length);
    console.log("Register buttons:", registerButtons.length);
    
    // Kiểm tra xem các nút có tồn tại không
    if (loginButtons.length > 0 || registerButtons.length > 0) {
        console.log("Buttons found, animation should work");
    }
    
    // Lấy các phần tử cần thiết
    const sign_in_btn = document.querySelector("#sign-in-btn");
    const sign_up_btn = document.querySelector("#sign-up-btn");

    // Xử lý trạng thái từ localStorage ngay lập tức
    const currentTarget = localStorage.getItem('pageTransitionTarget');
    
    if (currentTarget === 'register' && window.location.href.includes('Register')) {
        container.classList.add("sign-up-mode");
    } else if (currentTarget === 'login' && window.location.href.includes('Login')) {
        container.classList.remove("sign-up-mode");
    }
    
    // Đánh dấu container đã sẵn sàng
    container.classList.add('ready');
    
    // Sau khi trang đã tải xong, xóa lớp preload và ẩn lớp che phủ
    setTimeout(function() {
        document.body.classList.add('loaded');
        document.body.classList.remove('preload');
        
        // Ẩn từ từ lớp che phủ
        if (document.querySelector('.page-transition-overlay')) {
            document.querySelector('.page-transition-overlay').classList.remove('active');
            
            // Xóa lớp che phủ sau khi đã ẩn
            setTimeout(() => {
                if (document.querySelector('.page-transition-overlay')) {
                    document.querySelector('.page-transition-overlay').remove();
                }
                // Xóa trạng thái chuyển trang
                localStorage.removeItem('pageTransitionTarget');
            }, 300);
        }
    }, 100);

    // Phần xử lý nút chuyển chế độ (nếu ở cùng một trang)
    if (sign_up_btn && !sign_up_btn.hasAttribute('onclick')) {
        sign_up_btn.addEventListener("click", function (e) {
            e.preventDefault();
            console.log("Đang chuyển sang chế độ đăng ký"); 
            container.classList.add("sign-up-mode");
        });
    }

    if (sign_in_btn && !sign_in_btn.hasAttribute('onclick')) {
        sign_in_btn.addEventListener("click", function (e) {
            e.preventDefault();
            console.log("Đang chuyển sang chế độ đăng nhập"); 
            container.classList.remove("sign-up-mode");
        });
    }

    // Kiểm tra xem container có tồn tại hay không
    console.log("Container:", container);
    console.log("Sign up button:", sign_up_btn);
    console.log("Sign in button:", sign_in_btn);
});

    // Animation cho nốt nhạc bay
    function createMusicNotes() {
        const notes = ['♪', '♫', '♬', '♩'];
        const container = document.querySelector('.music-notes');
        
        setInterval(() => {
            const note = document.createElement('span');
            note.className = 'music-note';
            note.textContent = notes[Math.floor(Math.random() * notes.length)];
            note.style.left = Math.random() * 100 + 'vw';
            note.style.animationDuration = Math.random() * 3 + 2 + 's';
            note.style.opacity = Math.random();
            note.style.fontSize = Math.random() * 20 + 10 + 'px';
            
            container.appendChild(note);
            
            // Xóa nốt nhạc sau khi animation kết thúc
            setTimeout(() => {
                note.remove();
            }, 5000);
        }, 300);
    }

    createMusicNotes();

    // Animation cho equalizer
    function animateEqualizer() {
        const bars = document.querySelectorAll('.bar');
        bars.forEach(bar => {
            const height = Math.random() * 20 + 5;
            bar.style.height = height + 'px';
        });
    }

    setInterval(animateEqualizer, 100);

    // Animation cho particles
    function createParticles() {
        const particleContainer = document.querySelector(".particle");
        for (let i = 0; i < 10; i++) {
            const span = document.createElement("span");
            particleContainer.appendChild(span);
        }
    }

    createParticles();
    
    // Hiển thị validation errors
    const validationSummaries = document.querySelectorAll('.validation-summary-errors');
    validationSummaries.forEach(summary => {
        if (summary.querySelector('ul li')) {
            summary.style.display = 'block';
            summary.style.color = '#ff5555';
            summary.style.backgroundColor = 'rgba(255, 0, 0, 0.1)';
            summary.style.padding = '10px';
            summary.style.borderRadius = '5px';
            summary.style.marginBottom = '15px';
            summary.style.fontSize = '0.9rem';
        }
    });
});

// Định nghĩa lại hàm trong file JS để đảm bảo nó luôn có sẵn
function animateThenRedirect(url) {
    // Tạo lớp che phủ để tránh chớp nháy
    if (!document.querySelector('.page-transition-overlay')) {
        const overlay = document.createElement('div');
        overlay.className = 'page-transition-overlay';
        document.body.appendChild(overlay);
        
        // Cho phép render trước khi thêm class
        setTimeout(() => overlay.classList.add('active'), 10);
    } else {
        document.querySelector('.page-transition-overlay').classList.add('active');
    }
    
    const container = document.querySelector(".container");
    
    // Lưu trạng thái
    if (url.includes('Register')) {
        localStorage.setItem('formState', 'register');
        localStorage.setItem('pageTransitionTarget', 'register');
    } else {
        localStorage.setItem('formState', 'login');
        localStorage.setItem('pageTransitionTarget', 'login');
    }
    
    // Đánh dấu rằng đây là chuyển trang, không cần animation phóng to
    sessionStorage.setItem('skipContainerAnimation', 'true');
    
    if (url.includes('Register')) {
        container.classList.add("sign-up-mode");
    } else {
        container.classList.remove("sign-up-mode");
    }
    
    // Tăng thời gian đợi
    setTimeout(function() {
        window.location.href = url;
    }, 300);
    
    return false;
}