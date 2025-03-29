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

    // Khởi tạo chức năng đồng bộ lyrics nếu có form
    if (document.getElementById('lyricForm')) {
        initLyricSyncing();
    }
});

// Khởi tạo các biến
let playButton = document.querySelector('.play-btn');
let vinylDisc = document.querySelector('.vinyl-disc');
let currentLyricText = document.querySelector('.current-lyric .lyric-text');

// Hàm chuyển đổi phát/dừng
function togglePlay() {
    if (audioPlayer.paused) {
        audioPlayer.play()
            .then(() => {
                playButton.innerHTML = '<i class="fas fa-pause"></i>';
                vinylDisc.classList.add('playing');
            })
            .catch(error => {
                console.error('Error playing audio:', error);
                alert('Không thể phát nhạc. Vui lòng thử lại.');
            });
    } else {
        audioPlayer.pause();
        playButton.innerHTML = '<i class="fas fa-play"></i>';
        vinylDisc.classList.remove('playing');
    }
}

// Hàm cập nhật lyrics
function updateLyrics(currentTime) {
    const lines = document.querySelectorAll('.lyric-line');
    let activeLineIndex = -1;

    // Tìm dòng lyrics hiện tại
    lines.forEach((line, index) => {
        const timestamp = parseFloat(line.dataset.time);
        if (timestamp <= currentTime) {
            activeLineIndex = index;
        }
    });

    // Cập nhật trạng thái các dòng
    lines.forEach((line, index) => {
        line.classList.remove('active', 'prev', 'next');
        
        if (index === activeLineIndex) {
            line.classList.add('active');
            // Hiển thị lyrics đang phát
            if (currentLyricText) {
                const lyricContent = line.querySelector('.lyric-text').textContent;
                currentLyricText.textContent = lyricContent;
                currentLyricText.style.opacity = '1';
            }
            // Scroll đến dòng đang active
            line.scrollIntoView({ behavior: 'smooth', block: 'center' });
        } else if (index === activeLineIndex - 1) {
            line.classList.add('prev');
        } else if (index === activeLineIndex + 1) {
            line.classList.add('next');
        }
    });

    // Nếu không có dòng nào active, ẩn lyrics đang phát
    if (activeLineIndex === -1 && currentLyricText) {
        currentLyricText.style.opacity = '0';
    }
}

// Cập nhật thời gian và lyrics khi audio đang phát
audioPlayer.addEventListener('timeupdate', () => {
    updateLyrics(audioPlayer.currentTime);
});

// Xử lý khi audio bắt đầu phát
audioPlayer.addEventListener('play', () => {
    playButton.innerHTML = '<i class="fas fa-pause"></i>';
    vinylDisc.classList.add('playing');
    updateLyrics(audioPlayer.currentTime);
});

// Xử lý khi audio tạm dừng
audioPlayer.addEventListener('pause', () => {
    playButton.innerHTML = '<i class="fas fa-play"></i>';
    vinylDisc.classList.remove('playing');
    if (currentLyricText) {
        currentLyricText.style.opacity = '0';
    }
});

// Xử lý khi audio kết thúc
audioPlayer.addEventListener('ended', () => {
    playButton.innerHTML = '<i class="fas fa-play"></i>';
    vinylDisc.classList.remove('playing');
    const lines = document.querySelectorAll('.lyric-line');
    lines.forEach(line => {
        line.classList.remove('active', 'prev', 'next');
    });
    if (currentLyricText) {
        currentLyricText.style.opacity = '0';
    }
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
    window.submitLyricForm = function() {
        const form = document.getElementById('lyricForm');
        const formData = new FormData(form);
        const token = document.querySelector('meta[name="__RequestVerificationToken"]').content;
        const songId = document.getElementById('lyricSongId').value;
        const content = document.getElementById('lyricContent').value;
        const syncedContent = document.getElementById('syncedContentInput').value;

        // Kiểm tra nội dung
        if (!content || content.trim() === '') {
            alert('Vui lòng nhập lời bài hát');
            return;
        }

        // Thêm dữ liệu vào FormData
        formData.set('SongId', songId);
        formData.set('Content', content);
        formData.set('Language', 'vi');
        
        // Thêm dữ liệu đồng bộ nếu có
        if (syncedContent) {
            formData.set('SyncedContent', syncedContent);
            console.log("Synced content being sent:", syncedContent);
        }

        // Log toàn bộ dữ liệu sẽ gửi
        console.log("Form data being sent:");
        for (let pair of formData.entries()) {
            console.log(pair[0] + ': ' + pair[1]);
        }

        // Hiển thị loading
        const submitBtn = document.querySelector('#lyricForm button[type="submit"]');
        if (submitBtn) {
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
            submitBtn.disabled = true;
        }

        fetch('/Lyrics/AddLyric', {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': token
            }
        })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error('Server response: ' + text);
                });
            }
            return response.text();
        })
        .then(result => {
            console.log("Save successful:", result);
            window.location.reload();
        })
        .catch(error => {
            console.error('Error saving lyrics:', error);
            alert('Có lỗi xảy ra khi lưu lời bài hát: ' + error.message);
            if (submitBtn) {
                submitBtn.innerHTML = 'Lưu lại';
                submitBtn.disabled = false;
            }
        });
    };
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
        console.error("Missing elements for sync feature");
        return;
    }

    let syncedLyrics = [];
    let currentLine = 0;
    let syncMode = false;

    // Reset quá trình đồng bộ
    function resetSync() {
        syncedLyrics = [];
        currentLine = 0;
        syncMode = false;
        markTimestampBtn.disabled = true;
        startSyncingBtn.disabled = false;
        audioPlayer.pause();
        audioPlayer.currentTime = 0;
        initSyncState(true);
    }

    resetSyncBtn.addEventListener('click', resetSync);

    // Khởi tạo trạng thái đồng bộ
    function initSyncState(forceReset = false) {
        if (forceReset) {
            syncedLyrics = [];
            syncedContentInput.value = '';
        }

        // Phân tách lời bài hát thành từng dòng
        const lines = lyricContent.value.split('\n').filter(line => line.trim() !== '');
        
        // Tạo preview
        syncPreview.innerHTML = '';
        lines.forEach((line, index) => {
            const lineElement = document.createElement('div');
            lineElement.classList.add('lyric-line');
            lineElement.innerHTML = `<span class="lyric-text">${line}</span>`;
            lineElement.dataset.index = index;
            syncPreview.appendChild(lineElement);
        });

        // Hiển thị dữ liệu đồng bộ đã có
        if (!forceReset && syncedContentInput.value) {
            try {
                const existingSyncedData = JSON.parse(syncedContentInput.value);
                existingSyncedData.forEach((item, index) => {
                    if (index < syncPreview.children.length) {
                        const lineElement = syncPreview.children[index];
                        lineElement.classList.add('synced');
                        lineElement.innerHTML = `
                            <span class="time-marker">[${formatTime(item.time)}]</span>
                            <span class="lyric-text">${item.text}</span>
                        `;
                    }
                });
            } catch (e) {
                console.error("Error parsing synced content:", e);
            }
        }
    }

    // Bắt đầu đồng bộ
    startSyncingBtn.addEventListener('click', async function() {
        try {
            // Reset trạng thái
            resetSync();
            syncMode = true;
            
            // Đảm bảo audio đã được tải
            if (audioPlayer.readyState === 0) {
                await new Promise((resolve, reject) => {
                    audioPlayer.addEventListener('loadeddata', resolve, { once: true });
                    audioPlayer.addEventListener('error', reject, { once: true });
                });
            }
            
            // Phát nhạc
            await audioPlayer.play();
            markTimestampBtn.disabled = false;
            startSyncingBtn.disabled = true;
            
        } catch (e) {
            console.error("Error starting playback:", e);
            alert("Không thể phát nhạc để đồng bộ. Vui lòng thử lại.");
            resetSync();
        }
    });

    // Đánh dấu thời gian
    markTimestampBtn.addEventListener('click', function() {
        if (!syncMode || currentLine >= syncPreview.children.length) return;

        const time = audioPlayer.currentTime;
        const lineElement = syncPreview.children[currentLine];
        const lineText = lineElement.querySelector('.lyric-text')?.textContent.trim() || '';

        if (!lineText) {
            console.error("Cannot find lyric text for line", currentLine);
            return;
        }

        // Lưu timestamp và text
        const syncedLine = {
            time: time,
            text: lineText
        };
        syncedLyrics.push(syncedLine);

        // Cập nhật giao diện
        lineElement.classList.add('synced');
        lineElement.innerHTML = `
            <span class="time-marker">[${formatTime(time)}]</span>
            <span class="lyric-text">${lineText}</span>
        `;

        // Lưu dữ liệu vào input hidden
        syncedContentInput.value = JSON.stringify(syncedLyrics);
        console.log("Added synced line:", syncedLine);
        console.log("Current synced lyrics:", syncedLyrics);

        currentLine++;

        // Kiểm tra hoàn thành
        if (currentLine >= syncPreview.children.length) {
            markTimestampBtn.disabled = true;
            audioPlayer.pause();
            
            // Kiểm tra dữ liệu đồng bộ
            try {
                const parsedData = JSON.parse(syncedContentInput.value);
                if (Array.isArray(parsedData) && parsedData.length > 0) {
                    alert('Đã đồng bộ hoàn tất! Nhấn Cập nhật để lưu.');
                } else {
                    throw new Error('Invalid synced data format');
                }
            } catch (e) {
                console.error("Error validating synced data:", e);
                alert('Có lỗi với dữ liệu đồng bộ. Vui lòng thử lại.');
                resetSync();
            }
        }
    });

    // Khởi tạo ban đầu
    initSyncState();
}

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

// Xử lý khi click vào đĩa nhạc
vinylDisc.addEventListener('click', function() {
    if (this.classList.contains('can-play')) {
        togglePlay();
    }
});