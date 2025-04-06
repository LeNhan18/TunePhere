function addSongRow() {
    const songsList = document.getElementById('songsList');
    const songCount = songsList.children.length;
    const newRow = document.createElement('div');
    newRow.className = 'spotify-song-row';
    newRow.setAttribute('data-song-index', songCount + 1);

    newRow.innerHTML = `
        <div class="spotify-song-number">${songCount + 1}</div>
        <div class="spotify-song-info">
            <div class="spotify-song-title">
                <input type="text" name="Songs[${songCount}].Title" class="form-control" placeholder="Tên bài hát" required />
            </div>
            <div class="spotify-song-track-number">
                <input type="number" name="Songs[${songCount}].TrackNumber" class="form-control" placeholder="Số thứ tự" min="1" value="${songCount + 1}" />
            </div>
            <div class="spotify-song-file">
                <input type="file" name="SongFiles" class="form-control" accept="audio/*" onchange="handleSongFile(this)" required />
                <label class="file-label">Chọn file nhạc</label>
            </div>
            <div class="spotify-song-image">
                <input type="file" name="ImageFiles" class="form-control" accept="image/*" onchange="handleImageFile(this)" />
                <label class="file-label">Chọn ảnh</label>
            </div>
        </div>
        <div class="spotify-song-duration">
            <span>0:00</span>
        </div>
        <div class="spotify-song-actions">
            <button type="button" class="btn-remove-song" onclick="removeSong(this)">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;

    songsList.appendChild(newRow);
    updateTrackNumbers();
}

function removeSong(button) {
    const row = button.closest('.spotify-song-row');
    row.remove();
    updateTrackNumbers();
}

function updateTrackNumbers() {
    const rows = document.querySelectorAll('.spotify-song-row');
    rows.forEach((row, index) => {
        // Cập nhật số thứ tự hiển thị
        row.querySelector('.spotify-song-number').textContent = index + 1;
        
        // Cập nhật data-song-index
        row.setAttribute('data-song-index', index + 1);
        
        // Cập nhật name của các input
        const titleInput = row.querySelector('input[name^="Songs"][name$="].Title"]');
        const trackNumberInput = row.querySelector('input[name^="Songs"][name$="].TrackNumber"]');
        
        if (titleInput) {
            titleInput.name = `Songs[${index}].Title`;
        }
        if (trackNumberInput) {
            trackNumberInput.name = `Songs[${index}].TrackNumber`;
            if (!trackNumberInput.value) {
                trackNumberInput.value = index + 1;
            }
        }
    });
} 