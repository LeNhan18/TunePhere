let audioPlayer = document.getElementById('audioPlayer');
if (!audioPlayer) {
    audioPlayer = new Audio();
    audioPlayer.id = 'audioPlayer';
}

document.addEventListener('DOMContentLoaded', function() {
    // Thêm sự kiện click cho tất cả các biểu tượng trái tim
    document.querySelectorAll('.heart-icon').forEach(heartIcon => {
        heartIcon.addEventListener('click', function(e) {
            e.preventDefault();
            const songId = this.getAttribute('data-song-id');
            
            // Gửi request để toggle favorite
            fetch(`/Songs/ToggleFavorite/${songId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': document.getElementById('RequestVerificationToken').value
                }
            })
            .then(response => response.json())
            .then(data => {
                // Animation nhịp tim
                this.style.animation = 'heartBeat 0.4s';
                setTimeout(() => {
                    this.style.animation = '';
                }, 400);
                
                if (!data.liked) {
                    // Nếu đã unlike, xóa dòng khỏi bảng
                    const row = this.closest('tr');
                    row.style.animation = 'fadeOut 0.5s';
                    
                    setTimeout(() => {
                        row.remove();
                        
                        // Cập nhật lại số thứ tự
                        const rows = document.querySelectorAll('.song-item');
                        rows.forEach((row, idx) => {
                            row.querySelector('th').textContent = idx + 1;
                        });
                        
                        // Kiểm tra nếu danh sách trống
                        if (rows.length === 0) {
                            location.reload(); // Tải lại trang để hiển thị trạng thái trống
                        }
                    }, 500);
                }
            })
            .catch(error => console.error('Error:', error));
        });
    });
    
    // Đảm bảo event không chồng chéo
    document.querySelectorAll('.play-overlay').forEach(playBtn => {
        playBtn.addEventListener('click', function(e) {
            e.stopPropagation(); // Ngăn sự kiện nổi bọt lên
        });
    });
    
    document.querySelectorAll('.heart-icon').forEach(heartIcon => {
        heartIcon.addEventListener('click', function(e) {
            e.stopPropagation(); // Ngăn sự kiện nổi bọt lên
        });
    });
});

function playFavoriteSong(button) {
    const row = button.closest('tr');
    const audioUrl = row.getAttribute('data-audio-url');
    const imageUrl = row.getAttribute('data-image-url');
    const title = row.getAttribute('data-title');
    const artist = row.getAttribute('data-artist');

    if (!audioUrl) {
        console.error('Không tìm thấy URL âm thanh');
        return;
    }

    // Cập nhật nguồn audio
    audioPlayer.src = audioUrl;
    audioPlayer.play();

    // Cập nhật giao diện
    document.querySelectorAll('.song-item').forEach(item => {
        item.classList.remove('playing');
        const playIcon = item.querySelector('.play-overlay i');
        if (playIcon) {
            playIcon.className = 'fas fa-play';
        }
    });

    row.classList.add('playing');
    const playOverlay = row.querySelector('.play-overlay i');
    if (playOverlay) {
        playOverlay.className = 'fas fa-pause';
    }

    // Hiển thị thanh player nếu có
    const playerBar = document.getElementById('nowPlayingBar');
    if (playerBar) {
        playerBar.style.display = 'flex';

        // Cập nhật thông tin bài hát đang phát
        const nowPlayingTitle = document.getElementById('nowPlayingTitle');
        const nowPlayingArtist = document.getElementById('nowPlayingArtist');
        const nowPlayingImage = document.getElementById('nowPlayingImage');
        
        if (nowPlayingTitle) nowPlayingTitle.textContent = title;
        if (nowPlayingArtist) nowPlayingArtist.textContent = artist;
        if (nowPlayingImage) nowPlayingImage.src = imageUrl;

        // Đảm bảo nút play hiển thị pause
        const playPauseBtn = document.getElementById('playPauseButton');
        if (playPauseBtn) {
            const playPauseIcon = playPauseBtn.querySelector('i');
            if (playPauseIcon) {
                playPauseIcon.className = 'fas fa-pause';
            }
        }
    }
}

// Thêm animation cho hiệu ứng xóa
const style = document.createElement('style');
style.textContent = `
@keyframes fadeOut {
    from { opacity: 1; }
    to { opacity: 0; }
}
`;
document.head.appendChild(style);

// Xác nhận xóa bài hát khỏi danh sách yêu thích
function confirmRemoveFavorite(id, title) {
    if (confirm(`Bạn có chắc chắn muốn xóa bài hát "${title}" khỏi danh sách yêu thích không?`)) {
        window.location.href = `/UserFavoriteSongs/Remove/${id}`;
    }
}