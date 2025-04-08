
function addToPlayHistory(songId) {
    if (!songId) return;

    fetch('/PlayHistories/AddToHistory', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ songId: songId })
    })
    .then(response => response.json())
    .then(data => {
        if (!data.success) {
            console.error('Không thể lưu lịch sử:', data.message);
        }
    })
    .catch(error => console.error('Lỗi:', error));
}

// Thêm event listener cho audio player
document.addEventListener('DOMContentLoaded', function() {
    const audioPlayer = document.querySelector('audio');
    if (audioPlayer) {
        audioPlayer.addEventListener('play', function() {
            const songId = this.dataset.songId; // Thêm data-song-id vào thẻ audio
            addToPlayHistory(songId);
        });
    }
});