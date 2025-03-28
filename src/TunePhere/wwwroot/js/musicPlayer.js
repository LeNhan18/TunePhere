// Quản lý trình phát nhạc
const MusicPlayer = {
    player: null,
    currentSong: null,
    playlist: [],
    currentIndex: 0,
    isPlaying: false,

    init: function() {
        this.player = document.getElementById('audio-player');
        this.bindEvents();
        this.loadState();
    },

    bindEvents: function() {
        // Nút phát/tạm dừng
        document.getElementById('play-button').addEventListener('click', () => {
            this.togglePlay();
        });

        // Nút trước
        document.getElementById('prev-button').addEventListener('click', () => {
            this.playPrevious();
        });

        // Nút tiếp theo
        document.getElementById('next-button').addEventListener('click', () => {
            this.playNext();
        });

        // Thanh tiến trình
        const progressBar = document.querySelector('.progress-bar');
        progressBar.addEventListener('click', (e) => {
            const percent = e.offsetX / progressBar.offsetWidth;
            this.player.currentTime = percent * this.player.duration;
            this.updateProgress();
        });

        // Thanh âm lượng
        document.getElementById('volume-slider').addEventListener('input', (e) => {
            const volume = e.target.value / 100;
            this.player.volume = volume;
            localStorage.setItem('musicPlayerVolume', volume);
        });

        // Cập nhật tiến trình
        this.player.addEventListener('timeupdate', () => {
            this.updateProgress();
        });

        // Khi bài hát kết thúc
        this.player.addEventListener('ended', () => {
            this.playNext();
        });

        // Lưu trạng thái khi rời trang
        window.addEventListener('beforeunload', () => {
            this.saveState();
        });

        // Xử lý các liên kết trong trang
        this.handlePageNavigation();
    },

    handlePageNavigation: function() {
        // Chặn hành vi mặc định của các liên kết và sử dụng AJAX để tải nội dung
        document.addEventListener('click', (e) => {
            // Chỉ xử lý các liên kết trong trang
            if (e.target.tagName === 'A' && e.target.href && 
                e.target.href.startsWith(window.location.origin) && 
                !e.target.getAttribute('data-no-ajax')) {
                
                e.preventDefault();
                const url = e.target.href;
                
                // Lưu trạng thái hiện tại
                this.saveState();
                
                // Tải nội dung trang mới bằng AJAX
                fetch(url)
                    .then(response => response.text())
                    .then(html => {
                        // Trích xuất nội dung chính từ HTML
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(html, 'text/html');
                        const content = doc.getElementById('page-content').innerHTML;
                        
                        // Cập nhật nội dung trang
                        document.getElementById('page-content').innerHTML = content;
                        
                        // Cập nhật URL
                        history.pushState({}, '', url);
                        
                        // Khởi chạy lại các script trong nội dung mới
                        this.executeScripts();
                    })
                    .catch(error => {
                        console.error('Error loading page:', error);
                        window.location.href = url; // Fallback to normal navigation
                    });
            }
        });

        // Xử lý nút back/forward của trình duyệt
        window.addEventListener('popstate', () => {
            fetch(window.location.href)
                .then(response => response.text())
                .then(html => {
                    const parser = new DOMParser();
                    const doc = parser.parseFromString(html, 'text/html');
                    const content = doc.getElementById('page-content').innerHTML;
                    document.getElementById('page-content').innerHTML = content;
                    this.executeScripts();
                });
        });
    },

    executeScripts: function() {
        // Tìm và thực thi các script trong nội dung mới
        const scripts = document.getElementById('page-content').querySelectorAll('script');
        scripts.forEach(oldScript => {
            const newScript = document.createElement('script');
            Array.from(oldScript.attributes).forEach(attr => {
                newScript.setAttribute(attr.name, attr.value);
            });
            newScript.appendChild(document.createTextNode(oldScript.innerHTML));
            oldScript.parentNode.replaceChild(newScript, oldScript);
        });
    },

    playSong: function(song) {
        if (!song) return;

        this.currentSong = song;
        this.player.src = song.fileUrl;
        this.player.load();
        this.player.play();
        this.isPlaying = true;

        // Cập nhật UI
        document.getElementById('player-thumbnail').src = song.imageUrl;
        document.getElementById('player-title').textContent = song.title;
        document.getElementById('player-artist').textContent = song.artistName;
        document.getElementById('play-button').innerHTML = '<i class="fas fa-pause"></i>';

        // Lưu bài hát hiện tại vào localStorage
        localStorage.setItem('currentSong', JSON.stringify(song));
    },

    togglePlay: function() {
        if (!this.currentSong) {
            if (this.playlist.length > 0) {
                this.playSong(this.playlist[0]);
            }
            return;
        }

        if (this.isPlaying) {
            this.player.pause();
            document.getElementById('play-button').innerHTML = '<i class="fas fa-play"></i>';
        } else {
            this.player.play();
            document.getElementById('play-button').innerHTML = '<i class="fas fa-pause"></i>';
        }
        this.isPlaying = !this.isPlaying;
        localStorage.setItem('isPlaying', this.isPlaying);
    },

    playNext: function() {
        if (this.playlist.length === 0) return;
        
        this.currentIndex = (this.currentIndex + 1) % this.playlist.length;
        this.playSong(this.playlist[this.currentIndex]);
    },

    playPrevious: function() {
        if (this.playlist.length === 0) return;
        
        this.currentIndex = (this.currentIndex - 1 + this.playlist.length) % this.playlist.length;
        this.playSong(this.playlist[this.currentIndex]);
    },

    updateProgress: function() {
        if (!this.player.duration) return;
        
        const progress = (this.player.currentTime / this.player.duration) * 100;
        document.getElementById('progress').style.width = `${progress}%`;
        
        // Cập nhật thời gian
        document.getElementById('current-time').textContent = this.formatTime(this.player.currentTime);
        document.getElementById('duration').textContent = this.formatTime(this.player.duration);
    },

    formatTime: function(seconds) {
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs < 10 ? '0' : ''}${secs}`;
    },

    addToPlaylist: function(song) {
        // Kiểm tra xem bài hát đã có trong playlist chưa
        const exists = this.playlist.some(item => item.songId === song.songId);
        if (!exists) {
            this.playlist.push(song);
            localStorage.setItem('playlist', JSON.stringify(this.playlist));
        }
    },

    setPlaylist: function(songs, startIndex = 0) {
        this.playlist = songs;
        this.currentIndex = startIndex;
        localStorage.setItem('playlist', JSON.stringify(this.playlist));
        localStorage.setItem('currentIndex', this.currentIndex);
        
        if (songs.length > 0) {
            this.playSong(songs[startIndex]);
        }
    },

    saveState: function() {
        if (this.currentSong) {
            localStorage.setItem('currentSong', JSON.stringify(this.currentSong));
            localStorage.setItem('currentTime', this.player.currentTime);
            localStorage.setItem('isPlaying', this.isPlaying);
            localStorage.setItem('playlist', JSON.stringify(this.playlist));
            localStorage.setItem('currentIndex', this.currentIndex);
        }
    },

    loadState: function() {
        // Khôi phục âm lượng
        const volume = localStorage.getItem('musicPlayerVolume');
        if (volume !== null) {
            this.player.volume = parseFloat(volume);
            document.getElementById('volume-slider').value = volume * 100;
        }

        // Khôi phục playlist
        const playlistJson = localStorage.getItem('playlist');
        if (playlistJson) {
            this.playlist = JSON.parse(playlistJson);
        }

        // Khôi phục vị trí hiện tại
        const indexStr = localStorage.getItem('currentIndex');
        if (indexStr !== null) {
            this.currentIndex = parseInt(indexStr);
        }

        // Khôi phục bài hát hiện tại
        const songJson = localStorage.getItem('currentSong');
        if (songJson) {
            const song = JSON.parse(songJson);
            this.currentSong = song;
            
            // Cập nhật UI
            document.getElementById('player-thumbnail').src = song.imageUrl;
            document.getElementById('player-title').textContent = song.title;
            document.getElementById('player-artist').textContent = song.artistName;
            
            // Cài đặt nguồn audio
            this.player.src = song.fileUrl;
            
            // Khôi phục vị trí phát
            const currentTime = localStorage.getItem('currentTime');
            if (currentTime !== null) {
                this.player.currentTime = parseFloat(currentTime);
            }
            
            // Khôi phục trạng thái phát
            const isPlaying = localStorage.getItem('isPlaying') === 'true';
            this.isPlaying = isPlaying;
            
            if (isPlaying) {
                this.player.play();
                document.getElementById('play-button').innerHTML = '<i class="fas fa-pause"></i>';
            } else {
                document.getElementById('play-button').innerHTML = '<i class="fas fa-play"></i>';
            }
        }
    }
};

// Khởi tạo trình phát nhạc khi trang được tải
document.addEventListener('DOMContentLoaded', function() {
    MusicPlayer.init();
});