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

// Cập nhật nút play/pause - đảm bảo nút hiển thị đúng
function updatePlayPauseButton() {
    const button = document.getElementById('playPauseButton');
    if (!button) return;
    
    const icon = button.querySelector('i');
    if (!icon) return;
    
    // Log để debug
    console.log("Cập nhật trạng thái nút, isPlaying =", isPlaying);
    
    if (isPlaying) {
        icon.className = 'fas fa-pause'; // Hiển thị nút tạm dừng khi đang phát
    } else {
        icon.className = 'fas fa-play';  // Hiển thị nút phát khi đang dừng
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

// Thêm sự kiện lắng nghe trạng thái audio
document.addEventListener('DOMContentLoaded', function() {
    // ... existing code ...
    
    // Thêm các sự kiện lắng nghe âm thanh
    audioPlayer.addEventListener('play', function() {
        isPlaying = true;
        updatePlayPauseButton();
    });
    
    audioPlayer.addEventListener('pause', function() {
        isPlaying = false;
        updatePlayPauseButton();
    });
    
    // Thêm lắng nghe sự kiện khi tương tác với thanh trình phát
    const playPauseButton = document.getElementById('playPauseButton');
    if (playPauseButton) {
        playPauseButton.addEventListener('click', function() {
            // Thêm thời gian chờ ngắn để đảm bảo DOM cập nhật sau khi xử lý
            setTimeout(updatePlayPauseButton, 50);
        });
    }
    
    // ... existing code ...
});

// Thêm hàm để force refresh trang
function forceRefresh() {
    window.location.reload(true); // true để force refresh từ server
}

// Cập nhật phần xử lý form submit trong Edit.cshtml
document.addEventListener('DOMContentLoaded', function() {
    const editForm = document.querySelector('form[action*="Edit"]');
    if (editForm) {
        editForm.addEventListener('submit', function(e) {
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

document.addEventListener('DOMContentLoaded', function() {
    const addLyricButton = document.getElementById('addLyricButton');
    if (addLyricButton) {
        addLyricButton.addEventListener('click', function() {
            const modal = new bootstrap.Modal(document.getElementById('addLyricModal'));
            modal.show();
        });
    }

    // Xử lý form submit
    const lyricForm = document.querySelector('#addLyricModal form');
    if (lyricForm) {
        lyricForm.addEventListener('submit', function(e) {
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
    resetSyncBtn.addEventListener('click', function() {
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
    startSyncingBtn.addEventListener('click', function() {
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
    markTimestampBtn.addEventListener('click', function() {
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
document.addEventListener('DOMContentLoaded', function() {
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
        addLyricForm.addEventListener('submit', function(event) {
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
document.addEventListener('DOMContentLoaded', function() {
    const editForm = document.querySelector('#addLyricModal form');
    if (editForm) {
        editForm.addEventListener('submit', function(e) {
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