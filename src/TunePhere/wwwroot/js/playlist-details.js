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