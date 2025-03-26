document.addEventListener('DOMContentLoaded', function() {
    console.log('Navigation script loaded');
    
    // Xử lý dropdown menu người dùng
    const userMenuButton = document.querySelector('.user-menu-button');
    const userDropdownMenu = document.querySelector('.user-dropdown-menu');
    
    if (userMenuButton && userDropdownMenu) {
        console.log('User menu elements found');
        
        userMenuButton.addEventListener('click', function(e) {
            e.stopPropagation();
            console.log('User menu button clicked');
            userDropdownMenu.classList.toggle('show');
        });
        
        // Đóng menu khi click bên ngoài
        document.addEventListener('click', function(e) {
            if (!userMenuButton.contains(e.target) && !userDropdownMenu.contains(e.target)) {
                userDropdownMenu.classList.remove('show');
            }
        });
    } else {
        console.log('User menu elements not found');
    }
}); 