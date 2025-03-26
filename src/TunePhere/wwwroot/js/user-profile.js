document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded'); // Debug log
    
    // Lấy các phần tử DOM
    const editProfileBtn = document.getElementById('edit-profile-btn');
    const editProfileModal = document.getElementById('edit-profile-modal');
    const closeModal = document.getElementById('close-modal');
    const openEditModal = document.querySelector('.change-photo-btn');
    
    console.log('Edit button:', editProfileBtn); // Debug log
    console.log('Modal:', editProfileModal); // Debug log
    
    // Xử lý nút chỉnh sửa hồ sơ
    if (editProfileBtn && editProfileModal) {
        editProfileBtn.addEventListener('click', function(e) {
            e.preventDefault();
            console.log('Edit button clicked'); // Debug log
            editProfileModal.classList.add('show');
        });
    }
    
    // Xử lý nút thay đổi ảnh
    if (openEditModal && editProfileModal) {
        openEditModal.addEventListener('click', function(e) {
            e.preventDefault();
            console.log('Change photo clicked'); // Debug log
            editProfileModal.classList.add('show');
        });
    }
    
    // Xử lý nút đóng modal
    if (closeModal && editProfileModal) {
        closeModal.addEventListener('click', function() {
            editProfileModal.classList.remove('show');
        });
    }
    
    // Đóng modal khi click bên ngoài
    window.addEventListener('click', function(e) {
        if (editProfileModal && e.target === editProfileModal) {
            editProfileModal.classList.remove('show');
        }
    });
    
    // Xử lý chọn ảnh đại diện
    const chooseProfileImage = document.getElementById('choose-profile-image');
    const profileImageUpload = document.getElementById('profile-image-upload');
    const avatarPreview = document.getElementById('avatar-preview');
    
    if (chooseProfileImage && profileImageUpload) {
        chooseProfileImage.addEventListener('click', function() {
            profileImageUpload.click();
        });
        
        profileImageUpload.addEventListener('change', function() {
            if (this.files && this.files[0]) {
                const reader = new FileReader();
                
                reader.onload = function(e) {
                    // Xóa nội dung hiện tại của preview
                    avatarPreview.innerHTML = '';
                    
                    // Tạo element img mới
                    const img = document.createElement('img');
                    img.src = e.target.result;
                    img.alt = 'Profile Preview';
                    
                    // Thêm vào preview
                    avatarPreview.appendChild(img);
                };
                
                reader.readAsDataURL(this.files[0]);
            }
        });
    }
    
    // Thêm xử lý cho ảnh bìa
    const chooseCoverImage = document.getElementById('choose-cover-image');
    const coverImageUpload = document.getElementById('cover-image-upload');
    const coverPreview = document.getElementById('cover-preview');
    
    if (chooseCoverImage && coverImageUpload) {
        chooseCoverImage.addEventListener('click', function() {
            coverImageUpload.click();
        });
        
        coverImageUpload.addEventListener('change', function() {
            if (this.files && this.files[0]) {
                const reader = new FileReader();
                
                reader.onload = function(e) {
                    // Xóa nội dung hiện tại của preview
                    coverPreview.innerHTML = '';
                    
                    // Tạo element img mới
                    const img = document.createElement('img');
                    img.src = e.target.result;
                    img.alt = 'Cover Preview';
                    
                    // Thêm vào preview
                    coverPreview.appendChild(img);
                };
                
                reader.readAsDataURL(this.files[0]);
            }
        });
    }
    
    // Xử lý tabs
    const tabs = document.querySelectorAll('.tab');
    tabs.forEach(tab => {
        tab.addEventListener('click', function() {
            // Xóa active class từ tất cả tabs
            tabs.forEach(t => t.classList.remove('active'));
            // Thêm active class vào tab được click
            this.classList.add('active');
            
            // Ở đây có thể thêm code để hiển thị nội dung tương ứng
        });
    });
    
    // Hiệu ứng hover cho music cards
    const musicCards = document.querySelectorAll('.music-card');
    musicCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.querySelector('.play-button').style.opacity = '1';
            this.querySelector('.play-button').style.transform = 'translateY(0)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.querySelector('.play-button').style.opacity = '0';
            this.querySelector('.play-button').style.transform = 'translateY(10px)';
        });
    });

}); 

// Đảm bảo dropdown menu hoạt động ở trang profile
document.addEventListener('DOMContentLoaded', function () {
    const userMenuButton = document.querySelector('.user-menu-button');
    const userDropdownMenu = document.querySelector('.user-dropdown-menu');

    if (userMenuButton && userDropdownMenu) {
        console.log('Profile page: User menu elements found');

        userMenuButton.addEventListener('click', function (e) {
            e.stopPropagation();
            console.log('Profile page: User menu button clicked');
            userDropdownMenu.classList.toggle('show');
        });

        // Đóng menu khi click bên ngoài
        document.addEventListener('click', function (e) {
            if (!userMenuButton.contains(e.target) && !userDropdownMenu.contains(e.target)) {
                userDropdownMenu.classList.remove('show');
            }
        });
    }
});