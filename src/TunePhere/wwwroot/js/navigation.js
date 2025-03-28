document.addEventListener('DOMContentLoaded', function () {
    console.log('Navigation script loaded');

    // Đợi một chút để đảm bảo tất cả DOM đã sẵn sàng
    setTimeout(function () {
        setupUserMenu();
    }, 100);

    function setupUserMenu() {
        // Xử lý dropdown menu người dùng
        const userMenuButton = document.querySelector('.user-menu-button');
        const userDropdownMenu = document.querySelector('.user-dropdown-menu');

        if (userMenuButton && userDropdownMenu) {
            console.log('User menu elements found');

            // Đảm bảo chỉ thêm event listener một lần
            userMenuButton.removeEventListener('click', handleMenuClick);
            userMenuButton.addEventListener('click', handleMenuClick);

            // Đảm bảo chỉ thêm document click event một lần
            document.removeEventListener('click', handleDocumentClick);
            document.addEventListener('click', handleDocumentClick);

            function handleMenuClick(e) {
                e.preventDefault();
                e.stopPropagation();
                console.log('User menu button clicked');
                userDropdownMenu.classList.toggle('show');
            }

            function handleDocumentClick(e) {
                if (!userMenuButton.contains(e.target) && !userDropdownMenu.contains(e.target)) {
                    userDropdownMenu.classList.remove('show');
                }
            }
        } else {
            console.log('User menu elements not found');
        }
    }
}); 

