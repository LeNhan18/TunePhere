// Sử dụng biến toàn cục từ view
let audioPlayer = document.getElementById('audioPlayer');
let isPlaying = false;
let currentSongIndex = 0;
let songs = [];

// Khởi tạo khi trang tải xong
document.addEventListener('DOMContentLoaded', function () {
    // Thiết lập nguồn audio
    audioPlayer.src = window.songData.fileUrl;
    
    // Khởi tạo dữ liệu bài hát
    songs.push({
        id: 1,
        title: window.songData.title,
        artist: window.songData.artist,
        audioUrl: window.songData.fileUrl,
        imageUrl: window.songData.imageUrl
    });
    
    // Tự động phát khi trang tải xong
    playAudio();
    
    // Xử lý tua nhạc
    const seekBar = document.getElementById('seekBar');
    seekBar.addEventListener('input', () => {
        const time = (seekBar.value / 100) * audioPlayer.duration;
        audioPlayer.currentTime = time;
    });
    
    // Xử lý âm lượng
    const volumeBar = document.getElementById('volumeBar');
    volumeBar.addEventListener('input', () => {
        audioPlayer.volume = volumeBar.value / 100;
        updateVolumeIcon();
    });
    
    // Cập nhật tiến trình
    audioPlayer.addEventListener('timeupdate', () => {
        const percent = (audioPlayer.currentTime / audioPlayer.duration) * 100;
        seekBar.value = percent;
        seekBar.style.background = `linear-gradient(to right, #1ed760 ${percent}%, #535353 ${percent}%)`;

        document.getElementById('currentTime').textContent = formatTime(audioPlayer.currentTime);
        document.getElementById('duration').textContent = formatTime(audioPlayer.duration);
    });
    
    // Xử lý khi bài hát kết thúc
    audioPlayer.addEventListener('ended', () => {
        isPlaying = false;
        updatePlayPauseButton();
    });
    
    // Xử lý hiển thị nút thêm lời bài hát chỉ khi người dùng là admin hoặc người sở hữu
    const addLyricButton = document.getElementById('addLyricButton');
    
    // Thêm đoạn log để debug
    console.log("User is admin:", window.songData.userIsAdmin);
    console.log("User is owner:", window.songData.userIsOwner);
    
    // Kiểm tra quyền truy cập
    const userIsAdmin = window.songData.userIsAdmin === true;
    const userIsOwner = window.songData.userIsOwner === true;
    
    // Chỉ hiển thị nút khi người dùng có quyền
    if (userIsAdmin || userIsOwner) {
        // Thêm sự kiện click
        addLyricButton.addEventListener('click', function() {
            // Hiển thị modal
            new bootstrap.Modal(document.getElementById('addLyricModal')).show();
        });
    } else {
        // Ẩn nút nếu không có quyền
        addLyricButton.style.display = 'none';
    }
    
    // Xử lý nút lưu lyrics
    const saveLyricBtn = document.getElementById('saveLyricBtn');
    if (saveLyricBtn) {
        saveLyricBtn.addEventListener('click', function() {
            const songId = document.getElementById('lyricSongId').value;
            const content = document.getElementById('lyricContent').value;
            
            if (!content || content.trim() === '') {
                alert('Vui lòng nhập lời bài hát');
                return;
            }
            
            // Hiển thị loading
            saveLyricBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
            saveLyricBtn.disabled = true;
            
            // Sử dụng FormData thay vì tạo form thủ công để giữ nguyên xuống dòng
            const formData = new FormData();
            formData.append('SongId', songId);
            formData.append('Content', content);
            formData.append('Language', 'vi');
            
            // Lấy token CSRF
            const token = document.querySelector('meta[name="__RequestVerificationToken"]').content;
            formData.append('__RequestVerificationToken', token);
            
            // Gửi request bằng fetch
            fetch('/Lyrics/AddLyric', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': token
                }
            })
            .then(response => {
                if (response.ok) {
                    window.location.reload(); // Tải lại trang sau khi lưu
                } else {
                    alert('Có lỗi xảy ra khi lưu lời bài hát');
                    saveLyricBtn.innerHTML = 'Lưu lại';
                    saveLyricBtn.disabled = false;
                }
            })
            .catch(error => {
                console.error('Lỗi:', error);
                alert('Có lỗi xảy ra khi lưu lời bài hát');
                saveLyricBtn.innerHTML = 'Lưu lại';
                saveLyricBtn.disabled = false;
            });
        });
    }

    console.log("Lyrics data:", Model.Lyrics); // Kiểm tra dữ liệu lyrics
});

// Phát nhạc
function playAudio() {
    audioPlayer.play()
        .then(() => {
            isPlaying = true;
            updatePlayPauseButton();
        })
        .catch(error => {
            console.error('Lỗi phát nhạc:', error);
        });
}

// Xử lý play/pause
function togglePlay() {
    if (isPlaying) {
        audioPlayer.pause();
        isPlaying = false;
    } else {
        audioPlayer.play()
            .then(() => {
                isPlaying = true;
            })
            .catch(error => {
                console.error('Lỗi phát nhạc:', error);
            });
    }
    
    updatePlayPauseButton();
}

// Cập nhật nút play/pause
function updatePlayPauseButton() {
    const button = document.getElementById('playPauseButton');
    const icon = button.querySelector('i');
    
    if (isPlaying) {
        icon.className = 'fas fa-pause';
    } else {
        icon.className = 'fas fa-play';
    }
}

// Phát bài tiếp theo (không có hành động vì chỉ có 1 bài)
function nextSong() {
    // Giữ nguyên bài hiện tại vì đây là trang chi tiết 1 bài
    console.log("Không có bài tiếp theo");
}

// Phát bài trước (không có hành động vì chỉ có 1 bài)
function previousSong() {
    // Giữ nguyên bài hiện tại vì đây là trang chi tiết 1 bài
    console.log("Không có bài trước");
}

// Xử lý tắt/bật âm thanh
function toggleMute() {
    const volumeButton = document.getElementById('volumeButton');
    const icon = volumeButton.querySelector('i');
    const volumeBar = document.getElementById('volumeBar');
    
    if (audioPlayer.volume > 0) {
        audioPlayer.volume = 0;
        volumeBar.value = 0;
        icon.className = 'fas fa-volume-mute';
    } else {
        audioPlayer.volume = 1;
        volumeBar.value = 100;
        icon.className = 'fas fa-volume-up';
    }
}

// Cập nhật icon âm lượng
function updateVolumeIcon() {
    const volumeButton = document.getElementById('volumeButton');
    const icon = volumeButton.querySelector('i');
    
    if (audioPlayer.volume === 0) {
        icon.className = 'fas fa-volume-mute';
    } else {
        icon.className = 'fas fa-volume-up';
    }
}

// Hàm format thời gian
function formatTime(seconds) {
    if (isNaN(seconds)) return '0:00';

    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = Math.floor(seconds % 60);
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
}

// Hiển thị video YouTube
function showVideo(url) {
    if (!url) return;

    let videoId = "";
    if (url.includes("v=")) {
        videoId = url.split("v=")[1];
        const ampersandPosition = videoId.indexOf('&');
        if (ampersandPosition !== -1) {
            videoId = videoId.substring(0, ampersandPosition);
        }
    } else if (url.includes("youtu.be/")) {
        videoId = url.split("youtu.be/")[1];
    }

    const embedUrl = `https://www.youtube.com/embed/${videoId}`;
    document.getElementById('videoFrame').src = embedUrl;
    new bootstrap.Modal(document.getElementById('videoModal')).show();
}

// Xử lý nút thêm lời bài hát
document.addEventListener('DOMContentLoaded', function() {
    const addLyricButton = document.getElementById('addLyricButton');
    if (addLyricButton) {
        // Kiểm tra quyền truy cập
        const userIsAdmin = window.songData.userIsAdmin === true;
        const userIsOwner = window.songData.userIsOwner === true;
        
        // Chỉ hiển thị nút khi người dùng có quyền
        if (userIsAdmin || userIsOwner) {
            // Thêm sự kiện click
            addLyricButton.addEventListener('click', function() {
                // Hiển thị modal
                new bootstrap.Modal(document.getElementById('addLyricModal')).show();
            });
        } else {
            // Ẩn nút nếu không có quyền
            addLyricButton.style.display = 'none';
        }
    }
    
    // Xử lý nút lưu lyrics
    const saveLyricBtn = document.getElementById('saveLyricBtn');
    if (saveLyricBtn) {
        saveLyricBtn.addEventListener('click', function() {
            const songId = document.getElementById('lyricSongId').value;
            const content = document.getElementById('lyricContent').value;
            
            if (!content || content.trim() === '') {
                alert('Vui lòng nhập lời bài hát');
                return;
            }
            
            // Hiển thị loading
            saveLyricBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
            saveLyricBtn.disabled = true;
            
            // Sử dụng FormData thay vì tạo form thủ công để giữ nguyên xuống dòng
            const formData = new FormData();
            formData.append('SongId', songId);
            formData.append('Content', content);
            formData.append('Language', 'vi');
            
            // Lấy token CSRF
            const token = document.querySelector('meta[name="__RequestVerificationToken"]').content;
            formData.append('__RequestVerificationToken', token);
            
            // Gửi request bằng fetch
            fetch('/Lyrics/AddLyric', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': token
                }
            })
            .then(response => {
                if (response.ok) {
                    window.location.reload(); // Tải lại trang sau khi lưu
                } else {
                    alert('Có lỗi xảy ra khi lưu lời bài hát');
                    saveLyricBtn.innerHTML = 'Lưu lại';
                    saveLyricBtn.disabled = false;
                }
            })
            .catch(error => {
                console.error('Lỗi:', error);
                alert('Có lỗi xảy ra khi lưu lời bài hát');
                saveLyricBtn.innerHTML = 'Lưu lại';
                saveLyricBtn.disabled = false;
            });
        });
    }
});