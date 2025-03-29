// Xử lý hiển thị/ẩn modal
function showAddSongModal() {
    document.getElementById('addSongModal').style.display = 'block';
}

function closeAddSongModal() {
    document.getElementById('addSongModal').style.display = 'none';
}

// Xử lý xóa bài hát khỏi playlist
function removeSong(songId) {
    if (confirm('Bạn có chắc chắn muốn xóa bài hát này khỏi playlist?')) {
        $.post('/PlaylistSongs/RemoveSongFromPlaylist', {
            playlistId: playlistId,
            songId: songId
        })
        .done(function(response) {
            if (response.success) {
                location.reload();
            } else {
                alert(response.message);
            }
        })
        .fail(function() {
            alert('Có lỗi xảy ra khi xóa bài hát');
        });
    }
}

// Xử lý thêm bài hát vào playlist
function addSong(songId) {
    $.post('/PlaylistSongs/AddSongToPlaylist', {
        playlistId: playlistId,
        songId: songId
    })
    .done(function(response) {
        if (response.success) {
            location.reload();
        } else {
            alert(response.message);
        }
    })
    .fail(function() {
        alert('Có lỗi xảy ra khi thêm bài hát');
    });
}

// Xử lý tìm kiếm bài hát
let searchTimeout;
$(document).ready(function() {
    $('#songSearch').on('input', function() {
        clearTimeout(searchTimeout);
        const query = $(this).val().trim();
        
        if (query.length < 2) {
            $('#searchResults').empty();
            return;
        }

        searchTimeout = setTimeout(function() {
            $.get('/Songs/Search', { query: query })
            .done(function(songs) {
                $('#searchResults').empty();
                if (songs.length === 0) {
                    $('#searchResults').html('<div class="no-results">Không tìm thấy bài hát nào</div>');
                    return;
                }
                songs.forEach(function(song) {
                    const songElement = `
                        <div class="search-result-item" onclick="addSong(${song.songId})">
                            <img src="${song.imageUrl || '/images/default-song.jpg'}" alt="${song.title}" />
                            <div class="search-result-info">
                                <div class="search-result-title">${song.title}</div>
                                <div class="search-result-artist">${song.artistName}</div>
                            </div>
                        </div>
                    `;
                    $('#searchResults').append(songElement);
                });
            })
            .fail(function() {
                $('#searchResults').html('<div class="error-message">Có lỗi xảy ra khi tìm kiếm</div>');
            });
        }, 300);
    });

    // Đóng modal khi click bên ngoài
    window.onclick = function(event) {
        const modal = document.getElementById('addSongModal');
        if (event.target == modal) {
            closeAddSongModal();
        }
    }
});

// Xử lý phát nhạc
let currentPlayingElement = null;

function playSong(songElement) {
    const songId = songElement.dataset.songId;
    const songUrl = songElement.dataset.songUrl;
    const songTitle = songElement.dataset.songTitle;
    const songArtist = songElement.dataset.songArtist;
    const songImage = songElement.dataset.songImage;

    const audioPlayer = document.getElementById('audioPlayer');
    const audioSource = audioPlayer.querySelector('source');

    // Nếu đang phát bài hát này thì dừng lại
    if (currentPlayingElement === songElement && !audioPlayer.paused) {
        audioPlayer.pause();
        songElement.querySelector('i').classList.remove('fa-pause');
        songElement.querySelector('i').classList.add('fa-play');
        currentPlayingElement = null;
        return;
    }

    // Reset icon của bài hát đang phát trước đó (nếu có)
    if (currentPlayingElement) {
        currentPlayingElement.querySelector('i').classList.remove('fa-pause');
        currentPlayingElement.querySelector('i').classList.add('fa-play');
    }

    // Cập nhật source và phát nhạc
    audioSource.src = songUrl;
    audioPlayer.load();
    audioPlayer.play()
        .then(() => {
            // Cập nhật icon và lưu element đang phát
            songElement.querySelector('i').classList.remove('fa-play');
            songElement.querySelector('i').classList.add('fa-pause');
            currentPlayingElement = songElement;

            // Tăng lượt nghe
            $.post('/Songs/IncrementPlayCount', { id: songId });
        })
        .catch(error => {
            console.error('Error playing audio:', error);
            alert('Có lỗi xảy ra khi phát nhạc');
        });

    // Xử lý khi bài hát kết thúc
    audioPlayer.onended = function() {
        songElement.querySelector('i').classList.remove('fa-pause');
        songElement.querySelector('i').classList.add('fa-play');
        currentPlayingElement = null;
    };
} 