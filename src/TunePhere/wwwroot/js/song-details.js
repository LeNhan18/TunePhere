// Sử dụng biến toàn cục từ view
let audioPlayer = document.getElementById('audioPlayer');
let isPlaying = false;
let currentSongIndex = 0;
let songs = [];

// Biến và hàm cho tính năng lyrics đồng bộ
let syncedLyrics = [];
let currentLine = 0;
let syncMode = false;
let activeLyricLine = -1;

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

    // Tăng lượt nghe khi trang chi tiết được tải
    incrementPlayCount(window.songData.songId);

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
        // Tắt hiệu ứng đĩa
        toggleVinylEffect(false);
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
        addLyricButton.addEventListener('click', function () {
            const modal = new bootstrap.Modal(document.getElementById('addLyricModal'));
            modal.show();
        });
    } else {
        // Ẩn nút nếu không có quyền
        addLyricButton.style.display = 'none';
    }

    // Xử lý nút lưu lyrics
    const saveLyricBtn = document.getElementById('saveLyricBtn');
    if (saveLyricBtn) {
        saveLyricBtn.addEventListener('click', function () {
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

    // Nếu bài hát tự động phát khi trang tải xong, cũng bật hiệu ứng đĩa
    audioPlayer.addEventListener('play', function () {
        console.log("Sự kiện play được kích hoạt");
        isPlaying = true;
        updatePlayPauseButton();
        toggleVinylEffect(true);
    });

    audioPlayer.addEventListener('pause', function () {
        console.log("Sự kiện pause được kích hoạt");
        isPlaying = false;
        updatePlayPauseButton();
        toggleVinylEffect(false);
    });

    // Thêm hiệu ứng hover đẹp hơn cho đĩa nhạc
    const songImage = document.querySelector('.song-image');
    if (songImage) {
        // Tạo hiệu ứng cản quang khi hover
        songImage.addEventListener('mouseenter', function () {
            if (!isPlaying) {
                this.style.transform = 'scale(1.05)';
                this.style.boxShadow = '0 0 50px rgba(100, 65, 165, 0.4)';
            }
        });

        songImage.addEventListener('mouseleave', function () {
            if (!isPlaying) {
                this.style.transform = 'scale(1)';
                this.style.boxShadow = '0 0 50px rgba(0, 0, 0, 0.8)';
            }
        });
    }
});

// Khai báo biến để theo dõi hiệu ứng đĩa
let vinylEffectActive = false;

// Cập nhật hàm togglePlay() để đơn giản hóa và tránh phát lại từ đầu
function togglePlay() {
    const audioPlayer = document.getElementById('audioPlayer');
    
    try {
        if (audioPlayer.paused || audioPlayer.ended) {
            // Chỉ phát từ vị trí hiện tại, không khởi tạo lại
            isPlaying = true;
            updatePlayPauseButton();
            audioPlayer.play()
                .then(() => {
                    toggleVinylEffect(true);
                })
                .catch(error => {
                    console.error("Lỗi khi phát nhạc:", error);
                    isPlaying = false;
                    updatePlayPauseButton();
                });
        } else {
            audioPlayer.pause();
            isPlaying = false;
            updatePlayPauseButton();
            toggleVinylEffect(false);
        }
    } catch (error) {
        console.error("Lỗi khi chuyển đổi phát/dừng:", error);
        alert("Có lỗi xảy ra khi phát nhạc. Vui lòng thử lại sau.");
        isPlaying = false;
        updatePlayPauseButton();
    }
}

// Thêm hàm mới để xử lý hiệu ứng đĩa xoay
function toggleVinylEffect(active) {
    const songImage = document.querySelector('.song-image');

    if (!songImage) return;

    if (active) {
        songImage.classList.add('playing');
        // Nếu đang phát, đặt lại transform và thêm hiệu ứng glow
        songImage.style.transform = 'scale(1)';
        songImage.style.boxShadow = '0 0 50px rgba(100, 65, 165, 0.3)';
        vinylEffectActive = true;
    } else {
        songImage.classList.remove('playing');
        // Trả lại bóng đổ ban đầu khi dừng
        songImage.style.boxShadow = '0 0 50px rgba(0, 0, 0, 0.8)';
        vinylEffectActive = false;
    }
}

// Sửa hàm playAudio() - chỉ khởi tạo lần đầu, không phát lại từ đầu
function playAudio() {
    const audioPlayer = document.getElementById('audioPlayer');
    const fileUrl = window.songData.fileUrl;
    
    // Cập nhật trạng thái phát ngay khi bắt đầu phát
    isPlaying = true;
    updatePlayPauseButton();
    
    // Kiểm tra xem audio đã được khởi tạo chưa
    if (!audioPlayer.src || audioPlayer.src !== encodeURI(fileUrl)) {
        console.log("Khởi tạo audio mới:", fileUrl);
        
        // Mã hóa URL để xử lý ký tự đặc biệt và khoảng trắng
        const encodedUrl = encodeURI(fileUrl);
        
        // Kiểm tra file có tồn tại không
        fetch(encodedUrl, { method: 'HEAD' })
            .then(response => {
                if (!response.ok) {
                    console.error("Không tìm thấy file:", response.status);
                    alert("Không tìm thấy file âm thanh. Vui lòng kiểm tra lại đường dẫn.");
                    isPlaying = false;
                    updatePlayPauseButton();
                    return;
                }
                
                // Thiết lập MIME type
                const fileExtension = fileUrl.split('.').pop().toLowerCase();
                let mimeType;
                
                switch (fileExtension) {
                    case 'mp3': mimeType = 'audio/mpeg'; break;
                    case 'm4a': mimeType = 'audio/mp4'; break;
                    case 'wav': mimeType = 'audio/wav'; break;
                    case 'ogg': mimeType = 'audio/ogg'; break;
                    case 'aac': mimeType = 'audio/aac'; break;
                    case 'flac': mimeType = 'audio/flac'; break;
                    default: mimeType = 'audio/mpeg';
                }
                
                // Kiểm tra hỗ trợ định dạng
                const canPlayType = audioPlayer.canPlayType(mimeType);
                if (canPlayType === '' || canPlayType === 'no') {
                    console.warn("Trình duyệt không hỗ trợ định dạng file:", mimeType);
                    alert("Trình duyệt của bạn không hỗ trợ định dạng file này. Vui lòng sử dụng trình duyệt khác hoặc chuyển đổi file.");
                    isPlaying = false;
                    updatePlayPauseButton();
                    return;
                }
                
                // Thiết lập audio source và phát
                audioPlayer.src = encodedUrl;
                audioPlayer.type = mimeType;
                audioPlayer.load();
                playWithErrorHandling();
            })
            .catch(error => {
                console.error("Lỗi kiểm tra file:", error.message);
                alert("Lỗi khi kiểm tra file: " + error.message);
                isPlaying = false;
                updatePlayPauseButton();
            });
    } else {
        // Nếu audio đã được khởi tạo, chỉ cần phát
        console.log("Tiếp tục phát từ vị trí hiện tại");
        playWithErrorHandling();
    }
    
    // Hàm giúp phát nhạc với xử lý lỗi
    function playWithErrorHandling() {
        const playPromise = audioPlayer.play();
                
        if (playPromise !== undefined) {
            playPromise.then(() => {
                console.log("Phát nhạc thành công");
                toggleVinylEffect(true);
                
                // Chỉ tăng lượt nghe khi bắt đầu từ đầu
                if (audioPlayer.currentTime < 1) {
                    incrementPlayCount(window.songData.songId);
                }
            }).catch(error => {
                console.error("Lỗi phát nhạc:", error.message);
                
                // Chi tiết hóa lỗi phát nhạc
                let errorMsg = "Không thể phát bài hát. ";
                if (error.name === 'NotSupportedError') {
                    errorMsg += "Định dạng file không được hỗ trợ.";
                } else if (error.name === 'NotAllowedError') {
                    errorMsg += "Trình duyệt không cho phép tự động phát nhạc.";
                } else {
                    errorMsg += error.message;
                }
                
                alert(errorMsg);
                isPlaying = false;
                updatePlayPauseButton();
            });
        }
    }
}

// Hàm tăng lượt nghe
function incrementPlayCount(songId) {
    // Lấy token CSRF từ meta tag
    const token = document.querySelector('meta[name="__RequestVerificationToken"]').content;

    fetch(`/Songs/IncrementPlayCount/${songId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        }
    })
        .then(response => response.json())
        .then(data => {
            console.log('Đã tăng lượt nghe:', data.playCount);

            // Cập nhật số lượt nghe trên giao diện nếu có
            const playCountElements = document.querySelectorAll('.stat-item span');
            playCountElements.forEach(element => {
                if (element.parentElement.querySelector('i.fa-play')) {
                    element.textContent = `${data.playCount} lượt nghe`;
                }
            });
        })
        .catch(error => console.error('Lỗi khi tăng lượt nghe:', error));
}

// Đảm bảo hàm updatePlayPauseButton làm việc chính xác
function updatePlayPauseButton() {
    const button = document.getElementById('playPauseButton');
    
    if (!button) {
        console.error('Không tìm thấy nút playPauseButton');
        return;
    }
    
    const icon = button.querySelector('i');
    
    if (!icon) {
        console.error('Không tìm thấy icon trong nút playPauseButton');
        return;
    }
    
    console.log("Cập nhật trạng thái nút play/pause:", isPlaying ? "Đang phát" : "Đã dừng");
    
    // Xóa và thêm class mới
    icon.className = isPlaying ? 'fas fa-pause' : 'fas fa-play';
}

// Thay thế hoàn toàn hàm nextSong
function nextSong() {
    console.log("==== NEXT SONG ====");
    
    // Ưu tiên artist trước tất cả nếu fromArtist = true
    if (window.artistData && window.artistData.songs && window.artistData.songs.length > 1 && window.artistData.fromArtist) {
        let currentIndex = parseInt(window.artistData.currentIndex);
        if (isNaN(currentIndex)) currentIndex = 0;
        
        let nextIndex = currentIndex + 1;
        if (nextIndex >= window.artistData.songs.length) {
            nextIndex = 0; // Quay lại bài đầu tiên
        }
        
        console.log(`Artist navigation: Moving from index ${currentIndex} to ${nextIndex}`);
        const nextSong = window.artistData.songs[nextIndex];
        
        // Thêm timestamp để tránh cache
        window.location.href = `/Songs/Details/${nextSong.id}?artistId=${window.artistData.artistId}&fromArtist=true&index=${nextIndex}&t=${Date.now()}`;
        return;
    }
    
    // Giữ nguyên logic cho favorites, playlist, album
    if (window.favoritesData && window.favoritesData.songs && window.favoritesData.songs.length > 1) {
        let nextIndex = parseInt(window.favoritesData.currentIndex) + 1;
        if (nextIndex >= window.favoritesData.songs.length) {
            nextIndex = 0;
        }
        navigateToSong(nextIndex, 'favorites');
        return;
    }
    
    if (window.playlistData && window.playlistData.songs && window.playlistData.songs.length > 1) {
        let nextIndex = parseInt(window.playlistData.currentIndex) + 1;
        if (nextIndex >= window.playlistData.songs.length) {
            nextIndex = 0;
        }
        navigateToSong(nextIndex, 'playlist');
        return;
    }
    
    if (window.albumData && window.albumData.songs && window.albumData.songs.length > 1) {
        let nextIndex = parseInt(window.albumData.currentIndex) + 1;
        if (nextIndex >= window.albumData.songs.length) {
            nextIndex = 0;
        }
        navigateToSong(nextIndex, 'album');
        return;
    }
    
    // Nếu không có điều kiện nào được thỏa mãn, thử chuyển theo nghệ sĩ
    if (window.artistData && window.artistData.songs && window.artistData.songs.length > 1) {
        let currentIndex = parseInt(window.artistData.currentIndex);
        if (isNaN(currentIndex)) currentIndex = 0;
        
        let nextIndex = currentIndex + 1;
        if (nextIndex >= window.artistData.songs.length) {
            nextIndex = 0;
        }
        
        console.log(`Fallback artist navigation: Moving from index ${currentIndex} to ${nextIndex}`);
        const nextSong = window.artistData.songs[nextIndex];
        
        window.location.href = `/Songs/Details/${nextSong.id}?artistId=${window.artistData.artistId}&fromArtist=true&index=${nextIndex}&t=${Date.now()}`;
    }
}

// Tương tự sửa hàm previousSong
function previousSong() {
    console.log("==== PREVIOUS SONG ====");
    
    // Ưu tiên artist trước tất cả nếu fromArtist = true
    if (window.artistData && window.artistData.songs && window.artistData.songs.length > 1 && window.artistData.fromArtist) {
        let currentIndex = parseInt(window.artistData.currentIndex);
        if (isNaN(currentIndex)) currentIndex = 0;
        
        let prevIndex = currentIndex - 1;
        if (prevIndex < 0) {
            prevIndex = window.artistData.songs.length - 1; // Quay lại bài cuối cùng
        }
        
        console.log(`Artist navigation: Moving from index ${currentIndex} to ${prevIndex}`);
        const prevSong = window.artistData.songs[prevIndex];
        
        // Thêm timestamp để tránh cache
        window.location.href = `/Songs/Details/${prevSong.id}?artistId=${window.artistData.artistId}&fromArtist=true&index=${prevIndex}&t=${Date.now()}`;
        return;
    }
    
    // Giữ nguyên logic cho favorites, playlist, album
    if (window.favoritesData && window.favoritesData.songs && window.favoritesData.songs.length > 1) {
        let prevIndex = parseInt(window.favoritesData.currentIndex) - 1;
        if (prevIndex < 0) {
            prevIndex = window.favoritesData.songs.length - 1;
        }
        navigateToSong(prevIndex, 'favorites');
        return;
    }
    
    if (window.playlistData && window.playlistData.songs && window.playlistData.songs.length > 1) {
        let prevIndex = parseInt(window.playlistData.currentIndex) - 1;
        if (prevIndex < 0) {
            prevIndex = window.playlistData.songs.length - 1;
        }
        navigateToSong(prevIndex, 'playlist');
        return;
    }
    
    if (window.albumData && window.albumData.songs && window.albumData.songs.length > 1) {
        let prevIndex = parseInt(window.albumData.currentIndex) - 1;
        if (prevIndex < 0) {
            prevIndex = window.albumData.songs.length - 1;
        }
        navigateToSong(prevIndex, 'album');
        return;
    }
    
    // Nếu không có điều kiện nào được thỏa mãn, thử chuyển theo nghệ sĩ
    if (window.artistData && window.artistData.songs && window.artistData.songs.length > 1) {
        let currentIndex = parseInt(window.artistData.currentIndex);
        if (isNaN(currentIndex)) currentIndex = 0;
        
        let prevIndex = currentIndex - 1;
        if (prevIndex < 0) {
            prevIndex = window.artistData.songs.length - 1;
        }
        
        console.log(`Fallback artist navigation: Moving from index ${currentIndex} to ${prevIndex}`);
        const prevSong = window.artistData.songs[prevIndex];
        
        window.location.href = `/Songs/Details/${prevSong.id}?artistId=${window.artistData.artistId}&fromArtist=true&index=${prevIndex}&t=${Date.now()}`;
    }
}

// Hàm điều hướng đến bài hát theo chỉ số
function navigateToSong(index, type = 'playlist') {
    // Xác định dữ liệu dựa trên loại (playlist, album, favorites hoặc artist)
    let data;
    if (type === 'playlist') {
        data = window.playlistData;
    } else if (type === 'artist') {
        data = window.artistData;

    } else if (type === 'album') {
        data = window.albumData;
    } else if (type === 'favorites') {
        data = window.favoritesData;
    }

    if (!data || !data.songs || index < 0 || index >= data.songs.length) {
        console.error(`Không thể chuyển bài hát: dữ liệu ${type} không hợp lệ`, data);
        return;
    }

    const song = data.songs[index];

    // Kiểm tra song và song.id tồn tại
    if (!song || !song.id) {
        console.error("Dữ liệu bài hát không hợp lệ:", song);
        return;
    }

    console.log(`Chuyển đến bài hát ${type}:`, song.title, "ID:", song.id, "Index:", index);

    // Tạo URL với ID bài hát, ID và index tương ứng với loại
    let nextUrl = '';
    if (type === 'playlist') {
        nextUrl = `/Songs/Details/${song.id}?playlistId=${data.playlistId}&index=${index}`;
    } else if (type === 'artist') {
        nextUrl = `/Songs/Details/${song.id}?artistId=${data.artistId}&fromArtist=true&index=${index}`;
    } else if (type === 'album') {
        nextUrl = `/Songs/Details/${song.id}?albumId=${data.albumId}&index=${index}`;
    } else if (type === 'favorites') {
        nextUrl = `/Songs/Details/${song.id}?fromFavorites=true&index=${index}`;
    }

    // Lưu vị trí hiện tại vào sessionStorage
    if (audioPlayer && !isNaN(audioPlayer.currentTime)) {
        sessionStorage.setItem('lastPlaybackTime', audioPlayer.currentTime);
    }

    console.log("Chuyển đến URL:", nextUrl);
    // Chuyển đến trang chi tiết bài hát tiếp theo
    window.location.href = nextUrl;
}

// Thêm vào đoạn khởi tạo DOMContentLoaded để khôi phục vị trí phát
document.addEventListener('DOMContentLoaded', function () {
    // Code hiện tại...

    // Tự động phát và khôi phục vị trí (nếu đang chuyển bài trong playlist hoặc album)
    const lastPlaybackTime = sessionStorage.getItem('lastPlaybackTime');
    if (lastPlaybackTime && (window.playlistData || window.albumData)) {
        audioPlayer.addEventListener('canplay', function onCanPlay() {
            // Chỉ thực hiện một lần
            audioPlayer.removeEventListener('canplay', onCanPlay);

            // Đặt vị trí và phát
            audioPlayer.currentTime = parseFloat(lastPlaybackTime);
            playAudio();

            // Xóa lưu trữ sau khi đã sử dụng
            sessionStorage.removeItem('lastPlaybackTime');
        });
    }

    // Hiển thị thông tin playlist hoặc album nếu có
    if (window.playlistData) {
        console.log(`Đang phát từ playlist: ${window.playlistData.playlistTitle}`);
        console.log(`Bài hát ${window.playlistData.currentIndex + 1}/${window.playlistData.songs.length}`);
    } else if (window.albumData) {
        console.log(`Đang phát từ album: ${window.albumData.albumTitle}`);
        console.log(`Bài hát ${window.albumData.currentIndex + 1}/${window.albumData.songs.length}`);
    }
});

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

// Thêm hàm để force refresh trang
function forceRefresh() {
    window.location.reload(true); // true để force refresh từ server
}

// Cập nhật phần xử lý form submit trong Edit.cshtml
document.addEventListener('DOMContentLoaded', function () {
    const editForm = document.querySelector('form[action*="Edit"]');
    if (editForm) {
        editForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const formData = new FormData(this);

            fetch(this.action, {
                method: 'POST',
                body: formData
            })
                .then(response => {
                    if (response.ok) {
                        // Chuyển về trang Details và force refresh
                        window.location.href = response.url;
                        setTimeout(forceRefresh, 100);
                    } else {
                        alert('Có lỗi xảy ra khi cập nhật lyrics');
                    }
                })
                .catch(error => {
                    console.error('Lỗi:', error);
                    alert('Có lỗi xảy ra khi cập nhật lyrics');
                });
        });
    }
});

document.addEventListener('DOMContentLoaded', function () {
    const addLyricButton = document.getElementById('addLyricButton');
    if (addLyricButton) {
        addLyricButton.addEventListener('click', function () {
            const modal = new bootstrap.Modal(document.getElementById('addLyricModal'));
            modal.show();
        });
    }

    // Xử lý form submit
    const lyricForm = document.querySelector('#addLyricModal form');
    if (lyricForm) {
        lyricForm.addEventListener('submit', function (e) {
            // Cải thiện xử lý form submit để đảm bảo dữ liệu đồng bộ được gửi đi
            const syncedContentInput = document.getElementById('syncedContentInput');

            // Log dữ liệu đồng bộ để debug
            console.log("Submitting form with synced content:", syncedContentInput.value);

            // Kiểm tra nếu nội dung trống
            const content = this.querySelector('textarea[name="Content"]').value;
            if (!content || content.trim() === '') {
                e.preventDefault();
                alert('Vui lòng nhập lời bài hát');
                return false;
            }
        });
    }
});

// Khởi tạo chức năng đồng bộ lyrics
function initLyricSyncing() {
    console.log("Initializing lyric syncing...");
    const startSyncingBtn = document.getElementById('startSyncing');
    const markTimestampBtn = document.getElementById('markTimestamp');
    const resetSyncBtn = document.getElementById('resetSync');
    const syncPreview = document.getElementById('syncPreview');
    const syncedContentInput = document.getElementById('syncedContentInput');
    const lyricContent = document.querySelector('textarea[name="Content"]');

    if (!startSyncingBtn || !markTimestampBtn || !resetSyncBtn || !syncPreview || !lyricContent) {
        console.log("Missing elements for sync feature");
        return;
    }

    // Reset quá trình đồng bộ - cần làm sạch hoàn toàn dữ liệu cũ
    resetSyncBtn.addEventListener('click', function () {
        // Reset mảng dữ liệu đồng bộ
        syncedLyrics = [];
        currentLine = 0;
        syncMode = false;

        // Reset UI
        markTimestampBtn.disabled = true;
        startSyncingBtn.disabled = false;
        audioPlayer.pause();

        // Xóa sạch giao diện hiển thị
        initSyncState(true); // Thêm tham số true để xác định đây là reset hoàn toàn
    });

    // Khởi tạo trạng thái - thêm tham số forceReset
    function initSyncState(forceReset = false) {
        // Nếu là reset hoàn toàn hoặc khởi tạo lần đầu
        if (forceReset) {
            syncedLyrics = []; // Làm trống mảng dữ liệu
            syncedContentInput.value = ''; // Xóa dữ liệu đồng bộ cũ trong input
        }

        currentLine = 0;
        syncMode = false;

        // Phân tách lời bài hát thành từng dòng
        const lines = lyricContent.value.split('\n').filter(line => line.trim() !== '');

        // Tạo preview
        syncPreview.innerHTML = '';
        lines.forEach((line, index) => {
            const lineElement = document.createElement('div');
            lineElement.classList.add('lyric-line');
            lineElement.textContent = line;
            lineElement.dataset.index = index;
            syncPreview.appendChild(lineElement);
        });

        // Chỉ hiển thị dữ liệu đồng bộ cũ nếu không phải là reset hoàn toàn
        if (!forceReset && syncedContentInput.value) {
            try {
                const existingSyncedData = JSON.parse(syncedContentInput.value);
                console.log("Found existing synced data:", existingSyncedData);

                // Hiển thị dữ liệu đồng bộ đã có
                existingSyncedData.forEach((item, index) => {
                    if (index < syncPreview.children.length) {
                        const lineElement = syncPreview.children[index];
                        lineElement.classList.add('synced');
                        lineElement.innerHTML = `<span class="time-marker">[${formatTime(item.time)}]</span> ${item.text}`;
                    }
                });
            } catch (e) {
                console.error("Error parsing synced content:", e);
            }
        }
    }

    // Bắt đầu đồng bộ
    startSyncingBtn.addEventListener('click', function () {
        // Reset mảng dữ liệu đồng bộ khi bắt đầu mới
        syncedLyrics = [];

        // Làm mới UI
        initSyncState(true); // Đảm bảo xóa dữ liệu cũ khi bắt đầu đồng bộ
        syncMode = true;

        // Reset audio player về đầu
        audioPlayer.currentTime = 0;
        audioPlayer.play()
            .then(() => {
                markTimestampBtn.disabled = false;
                startSyncingBtn.disabled = true;
            })
            .catch(e => {
                console.error("Error starting playback:", e);
                alert("Không thể phát nhạc để đồng bộ. Vui lòng thử lại.");
                syncMode = false;
                markTimestampBtn.disabled = true;
                startSyncingBtn.disabled = false;
            });
    });

    // Đánh dấu thời gian cho mỗi dòng
    markTimestampBtn.addEventListener('click', function () {
        if (!syncMode || currentLine >= syncPreview.children.length) {
            return;
        }

        const time = audioPlayer.currentTime;
        const lineElement = syncPreview.children[currentLine];
        const lineText = lineElement.textContent.trim(); // Loại bỏ khoảng trắng thừa

        // Lưu timestamp và text
        syncedLyrics.push({
            time: time,
            text: lineText
        });

        // Highlight dòng hiện tại và di chuyển đến dòng tiếp theo
        lineElement.classList.add('synced');

        // Xóa nội dung cũ trước khi thêm mới
        lineElement.innerHTML = '';

        // Thêm thời gian mới và nội dung
        const timeMarker = document.createElement('span');
        timeMarker.className = 'time-marker';
        timeMarker.textContent = `[${formatTime(time)}]`;

        lineElement.appendChild(timeMarker);
        lineElement.appendChild(document.createTextNode(' ' + lineText));
        lineElement.a

        currentLine++;


        // Nếu đã đồng bộ hết các dòng
        if (currentLine >= syncPreview.children.length) {
            markTimestampBtn.disabled = true;

            // Lưu dữ liệu đồng bộ vào input hidden
            const syncedData = JSON.stringify(syncedLyrics);
            syncedContentInput.value = syncedData;
            console.log("Synced lyrics data saved:", syncedData);

            // Hiển thị thông báo hoàn tất
            const alertDiv = document.createElement('div');
            alertDiv.className = 'alert alert-success mt-2';
            alertDiv.innerHTML = '<i class="fas fa-check-circle"></i> Lời bài hát đã được đồng bộ. Nhấn Cập nhật để lưu.';

            // Xóa thông báo cũ nếu có
            const existingAlert = document.querySelector('#syncedLyrics .alert');
            if (existingAlert) {
                existingAlert.remove();
            }

            // Thêm thông báo mới
            document.querySelector('.lyric-sync-editor').appendChild(alertDiv);

            alert('Đã đồng bộ hoàn tất! Nhấn Cập nhật để lưu.');
        }
    });

    // Khởi tạo ban đầu
    initSyncState();
}

// Cập nhật hàm hiển thị lyrics theo thời gian hiện tại của bài hát
function updateSyncedLyrics() {
    // Thêm offset -0.5 giây để hiển thị sớm hơn và khắc phục delay
    const LYRICS_OFFSET = -0.5; // Offset 0.5 giây để hiển thị sớm hơn
    const currentTime = audioPlayer.currentTime - LYRICS_OFFSET;

    const lyricsContainer = document.querySelector('.lyrics-content');

    if (!window.syncedLyricsData || !lyricsContainer || audioPlayer.paused) {
        return;
    }

    // Tìm dòng hiện tại dựa trên thời gian
    let currentActiveLineIndex = -1;

    for (let i = 0; i < window.syncedLyricsData.length; i++) {
        if (i === window.syncedLyricsData.length - 1 ||
            (currentTime >= window.syncedLyricsData[i].time &&
                currentTime < window.syncedLyricsData[i + 1].time)) {
            currentActiveLineIndex = i;
            break;
        }
    }

    // Nếu dòng active thay đổi
    if (currentActiveLineIndex !== activeLyricLine && currentActiveLineIndex >= 0) {
        activeLyricLine = currentActiveLineIndex;

        // Cập nhật giao diện hiển thị
        const lines = lyricsContainer.querySelectorAll('.lyric-line');
        lines.forEach((line, index) => {
            if (index === activeLyricLine) {
                line.classList.add('active');
                // Cuộn đến dòng hiện tại
                line.scrollIntoView({ behavior: 'smooth', block: 'center' });
            } else {
                line.classList.remove('active');
            }
        });
    }
}

// Thêm vào cuối document.addEventListener('DOMContentLoaded'...)
document.addEventListener('DOMContentLoaded', function () {
    // Code khác...

    // Khởi tạo tính năng đồng bộ lyrics
    initLyricSyncing();

    // Làm mới giao diện lyrics nếu có dữ liệu đồng bộ
    const lyricElement = document.querySelector('.lyric-text');
    if (lyricElement && lyricElement.dataset.syncedContent) {
        try {
            // Parse dữ liệu đồng bộ
            window.syncedLyricsData = JSON.parse(lyricElement.dataset.syncedContent);
            console.log("Loaded synced lyrics data:", window.syncedLyricsData);

            // Định dạng lại hiển thị lyrics
            const lyricsContainer = document.querySelector('.lyrics-content');
            if (lyricsContainer && window.syncedLyricsData.length > 0) {
                lyricsContainer.innerHTML = '';

                window.syncedLyricsData.forEach(line => {
                    const lineElement = document.createElement('div');
                    lineElement.classList.add('lyric-line');
                    lineElement.textContent = line.text;
                    lyricsContainer.appendChild(lineElement);
                });

                // Thêm sự kiện cập nhật lyrics
                audioPlayer.addEventListener('timeupdate', updateSyncedLyrics);
            }
        } catch (e) {
            console.error('Lỗi khi parse dữ liệu lyrics đồng bộ:', e);
        }
    }

    // Hiển thị lyrics đồng bộ
    const syncedLyricsDisplay = document.getElementById('syncedLyricsDisplay');
    if (syncedLyricsDisplay && syncedLyricsDisplay.dataset.syncedContent) {
        try {
            // Parse dữ liệu đồng bộ
            window.syncedLyricsData = JSON.parse(syncedLyricsDisplay.dataset.syncedContent);
            console.log("Loaded synced lyrics data for display:", window.syncedLyricsData);

            // Định dạng hiển thị lyrics
            if (window.syncedLyricsData.length > 0) {
                syncedLyricsDisplay.innerHTML = '';

                window.syncedLyricsData.forEach(line => {
                    const lineElement = document.createElement('div');
                    lineElement.classList.add('lyric-line');

                    // Thêm data-time để debugging
                    lineElement.dataset.time = line.time;

                    // Cắt ngắn text quá dài và thêm dấu ... nếu cần
                    if (line.text.length > 100) {
                        lineElement.title = line.text; // Hiển thị khi hover
                    }

                    lineElement.textContent = line.text;
                    syncedLyricsDisplay.appendChild(lineElement);
                });

                // Thêm sự kiện cập nhật lyrics
                audioPlayer.addEventListener('timeupdate', updateSyncedLyrics);
                console.log("Added timeupdate event listener for synced lyrics");
            }
        } catch (e) {
            console.error('Lỗi khi parse dữ liệu lyrics đồng bộ:', e);
        }
    }

    // Đảm bảo dữ liệu đồng bộ được submit đúng cách
    const addLyricForm = document.querySelector('#addLyricModal form');
    if (addLyricForm) {
        addLyricForm.addEventListener('submit', function (event) {
            // Lấy các giá trị của form
            const formData = new FormData(this);

            // Log tất cả dữ liệu của form để debug
            for (let pair of formData.entries()) {
                console.log(pair[0] + ': ' + pair[1]);
            }

            // Kiểm tra nếu đang ở tab đồng bộ và đã hoàn tất đồng bộ
            const syncedTab = document.querySelector('.nav-link[href="#syncedLyrics"]');
            if (syncedTab && syncedTab.classList.contains('active')) {
                const syncedContentInput = document.getElementById('syncedContentInput');
                if (!syncedContentInput.value) {
                    alert('Vui lòng hoàn tất đồng bộ lyrics trước khi lưu!');
                    event.preventDefault();
                    return false;
                }
            }
        });
    }
});

// Cập nhật submit form để sử dụng fetch API thay vì submit thông thường
document.addEventListener('DOMContentLoaded', function () {
    const editForm = document.querySelector('#addLyricModal form');
    if (editForm) {
        editForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const formData = new FormData(this);

            // Log dữ liệu form trước khi gửi để debug
            console.log("Form action:", this.action);
            for (let pair of formData.entries()) {
                console.log(pair[0] + ': ' + (pair[0] === 'SyncedContent' ? 'Data length: ' + pair[1].length : pair[1]));
            }

            fetch(this.action, {
                method: 'POST',
                body: formData
            })
                .then(response => {
                    console.log("Response status:", response.status);
                    if (response.ok) {
                        // Chuyển về trang Details và force refresh
                        window.location.href = response.url;
                        // Đảm bảo page refresh
                        setTimeout(() => {
                            window.location.reload(true);
                        }, 100);
                    } else {
                        alert('Có lỗi xảy ra khi cập nhật lyrics');
                    }
                })
                .catch(error => {
                    console.error('Lỗi:', error);
                    alert('Có lỗi xảy ra khi cập nhật lyrics');
                });
        });
    }
});

// Xử lý sự kiện nhấn trái tim
document.addEventListener('DOMContentLoaded', function () {
    const heartIcon = document.querySelector('.heart-icon');
    if (heartIcon) {
        heartIcon.addEventListener('click', function () {
            this.classList.toggle('liked');

            // Animation nhịp tim
            this.style.animation = 'heartBeat 0.4s';
            setTimeout(() => {
                this.style.animation = '';
            }, 400);

            // Cập nhật số lượt thích
            const likeCount = document.querySelector('.like-count');
            if (likeCount) {
                const count = parseInt(likeCount.textContent);
                likeCount.textContent = this.classList.contains('liked') ? count + 1 : Math.max(0, count - 1);
            }
        });
    }
});

// Thêm animation cho trái tim
document.head.insertAdjacentHTML('beforeend', `
    <style>
    @keyframes heartBeat {
        0% { transform: scale(1); }
        50% { transform: scale(1.3); }
        100% { transform: scale(1); }
    }
    </style>
`);

// Thêm hàm mới để kiểm tra và chuẩn bị file âm thanh khi trang vừa tải xong
function initAudioPlayer() {
    const audioPlayer = document.getElementById('audioPlayer');
    
    if (window.songData && window.songData.fileUrl) {
        try {
            // Thiết lập nguồn nhưng chưa phát
            audioPlayer.src = window.songData.fileUrl;
            audioPlayer.preload = 'auto';
            
            // Xử lý lỗi
            audioPlayer.onerror = function(e) {
                console.error('Lỗi khi tải file âm thanh:', e);
            };
            
            // Load file nhưng chưa phát
            audioPlayer.load();
            console.log('Đã tải file âm thanh:', window.songData.fileUrl);
        } catch (error) {
            console.error('Lỗi khi khởi tạo audio player:', error);
        }
    } else {
        console.error('Không tìm thấy thông tin file âm thanh');
    }
}

// Khởi tạo audio player khi trang được tải
document.addEventListener('DOMContentLoaded', function() {
    // ... existing code ...
    
    // Khởi tạo audio player
    initAudioPlayer();
});

// Thêm hàm kiểm tra file khi trang tải xong
function validateAudioFile() {
    const fileUrl = window.songData.fileUrl;
    console.log("Kiểm tra file audio:", fileUrl);
    
    fetch(fileUrl, { method: 'HEAD' })
        .then(response => {
            if (!response.ok) {
                console.error(`File không tồn tại: ${response.status} - ${fileUrl}`);
                // Hiển thị cảnh báo nhỏ
                const warningEl = document.createElement('div');
                warningEl.className = 'alert alert-warning';
                warningEl.style.position = 'fixed';
                warningEl.style.top = '10px';
                warningEl.style.right = '10px';
                warningEl.style.zIndex = '9999';
                warningEl.innerHTML = 'Cảnh báo: File âm thanh không tồn tại hoặc không thể truy cập.';
                document.body.appendChild(warningEl);
                setTimeout(() => warningEl.remove(), 5000);
            } else {
                console.log("File audio hợp lệ");
            }
        })
        .catch(error => {
            console.error("Lỗi khi kiểm tra file:", error);
        });
}

// Hàm trợ giúp lấy thông báo lỗi chi tiết
function getAudioErrorMessage(error) {
    if (!error) return "Lỗi không xác định";
    
    switch (error.code) {
        case MediaError.MEDIA_ERR_ABORTED:
            return "Phát nhạc bị hủy.";
        case MediaError.MEDIA_ERR_NETWORK:
            return "Lỗi mạng khi tải file.";
        case MediaError.MEDIA_ERR_DECODE:
            return "Không thể giải mã file âm thanh.";
        case MediaError.MEDIA_ERR_SRC_NOT_SUPPORTED:
            return "Định dạng file không được hỗ trợ.";
        default:
            return "Lỗi không xác định: " + error.code;
    }
}

// Thêm hàm kiểm tra trực tiếp URL file audio
function debugAudioFile() {
    const fileUrl = window.songData.fileUrl;
    const encodedUrl = encodeURI(fileUrl);
    
    console.log("Đường dẫn gốc:", fileUrl);
    console.log("Đường dẫn mã hóa:", encodedUrl);
    
    // Tạo phần tử hiển thị thông tin debug
    const debugInfo = document.createElement('div');
    debugInfo.style.position = 'fixed';
    debugInfo.style.top = '10px';
    debugInfo.style.left = '10px';
    debugInfo.style.backgroundColor = 'rgba(0,0,0,0.8)';
    debugInfo.style.color = 'white';
    debugInfo.style.padding = '10px';
    debugInfo.style.borderRadius = '5px';
    debugInfo.style.zIndex = '9999';
    debugInfo.style.maxWidth = '80%';
    debugInfo.style.wordBreak = 'break-all';
    
    debugInfo.innerHTML = `
        <h4>Thông tin debug:</h4>
        <p>File URL: ${fileUrl}</p>
        <p>Encoded URL: ${encodedUrl}</p>
        <button id="testDirectUrl">Mở trực tiếp file</button>
        <button id="closeDebug">Đóng</button>
    `;
    
    document.body.appendChild(debugInfo);
    
    // Thêm sự kiện mở file trực tiếp
    document.getElementById('testDirectUrl').addEventListener('click', function() {
        window.open(encodedUrl, '_blank');
    });
    
    // Thêm sự kiện đóng debug
    document.getElementById('closeDebug').addEventListener('click', function() {
        debugInfo.remove();
    });
}

// Khởi tạo audio player cải tiến
document.addEventListener('DOMContentLoaded', function() {
    // ... existing code ...
    
    /* 
    // Xóa nút debug để kiểm tra file
    const playerControls = document.querySelector('.player-controls');
    if (playerControls) {
        const debugButton = document.createElement('button');
        debugButton.className = 'control-button';
        debugButton.innerHTML = '<i class="fas fa-bug"></i>';
        debugButton.title = 'Kiểm tra file audio';
        debugButton.addEventListener('click', debugAudioFile);
        playerControls.appendChild(debugButton);
    }
    */
    
    // Xử lý lỗi audio chi tiết hơn
    const audioPlayer = document.getElementById('audioPlayer');
    audioPlayer.addEventListener('error', function(e) {
        const errorCode = e.target.error ? e.target.error.code : 'không xác định';
        console.error("Lỗi audio:", errorCode);
        
        let errorMessage = "Lỗi không xác định";
        if (e.target.error) {
            switch (e.target.error.code) {
                case MediaError.MEDIA_ERR_ABORTED: 
                    errorMessage = "Phát nhạc bị hủy";
                    break;
                case MediaError.MEDIA_ERR_NETWORK:
                    errorMessage = "Lỗi mạng khi tải file";
                    break;
                case MediaError.MEDIA_ERR_DECODE:
                    errorMessage = "Không thể giải mã file âm thanh";
                    break;
                case MediaError.MEDIA_ERR_SRC_NOT_SUPPORTED:
                    errorMessage = "Định dạng file không được hỗ trợ";
                    break;
            }
        }
        
        alert("Lỗi phát nhạc: " + errorMessage);
        isPlaying = false;
        updatePlayPauseButton();
    });
});

// Thêm kiểm tra định dạng khi trang tải xong
document.addEventListener('DOMContentLoaded', function() {
    // ... existing code ...
    
    // Phát hiện các định dạng được hỗ trợ
    const audioPlayer = document.getElementById('audioPlayer');
    const supportedFormats = {
        mp3: audioPlayer.canPlayType('audio/mpeg'),
        m4a: audioPlayer.canPlayType('audio/mp4'),
        wav: audioPlayer.canPlayType('audio/wav'),
        ogg: audioPlayer.canPlayType('audio/ogg'),
        aac: audioPlayer.canPlayType('audio/aac'),
        flac: audioPlayer.canPlayType('audio/flac')
    };
    
    console.log("Các định dạng hỗ trợ:", supportedFormats);
    
    // Kiểm tra nhanh định dạng file hiện tại
    const fileUrl = window.songData.fileUrl;
    const fileExtension = fileUrl.split('.').pop().toLowerCase();
    
    if (fileExtension && supportedFormats[fileExtension] === '') {
        console.warn("Trình duyệt không hỗ trợ định dạng file:", fileExtension);
        
        // Hiển thị cảnh báo cho người dùng
        const warningEl = document.createElement('div');
        warningEl.className = 'alert alert-warning';
        warningEl.style.position = 'fixed';
        warningEl.style.top = '10px';
        warningEl.style.right = '10px';
        warningEl.style.zIndex = '9999';
        warningEl.innerHTML = `Cảnh báo: Trình duyệt của bạn không hỗ trợ định dạng file ${fileExtension.toUpperCase()}. Vui lòng sử dụng trình duyệt khác hoặc chuyển đổi file.`;
        document.body.appendChild(warningEl);
        setTimeout(() => warningEl.remove(), 8000);
    }
});