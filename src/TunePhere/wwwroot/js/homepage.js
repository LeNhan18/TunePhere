document.addEventListener('DOMContentLoaded', function () {
    // Xử lý chức năng đóng/mở sidebar
    const toggleBtn = document.getElementById('toggle-sidebar');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('main-content');

    toggleBtn.addEventListener('click', function () {
        sidebar.classList.toggle('collapsed');
        mainContent.classList.toggle('expanded');

        // Đổi icon khi đóng/mở
        const icon = toggleBtn.querySelector('i');
        if (sidebar.classList.contains('collapsed')) {
            icon.classList.remove('fa-chevron-left');
            icon.classList.add('fa-chevron-right');
        } else {
            icon.classList.remove('fa-chevron-right');
            icon.classList.add('fa-chevron-left');
        }
    });

    // Hover effect cho các card
    const musicCards = document.querySelectorAll('.music-card, .artist-card, .chart-card');
    musicCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            const playBtn = this.querySelector('.play-btn');
            if (playBtn) {
                playBtn.style.opacity = '1';
                playBtn.style.transform = 'translateY(0)';
            }
        });

        card.addEventListener('mouseleave', function () {
            const playBtn = this.querySelector('.play-btn');
            if (playBtn) {
                playBtn.style.opacity = '0';
                playBtn.style.transform = 'translateY(10px)';
            }
        });
    });

    // Cải thiện chức năng menu người dùng
    const userMenuButton = document.querySelector('.user-menu-button');
    const userDropdownMenu = document.querySelector('.user-dropdown-menu');
    
    if (userMenuButton && userDropdownMenu) {
        // Khi nhấn vào nút user menu
        userMenuButton.addEventListener('click', function (e) {
            e.stopPropagation();
            userDropdownMenu.classList.toggle('show');
        });
        
        // Chỉ đóng menu khi click ra ngoài menu và nút
        document.addEventListener('click', function (e) {
            if (!userMenuButton.contains(e.target) && !userDropdownMenu.contains(e.target)) {
                userDropdownMenu.classList.remove('show');
            }
        });
    }
});