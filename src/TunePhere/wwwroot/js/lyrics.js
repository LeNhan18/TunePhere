document.addEventListener('DOMContentLoaded', function () {
    // Hàm này sẽ chạy mỗi 100ms để đảm bảo không xuất hiện khung
    function removeBackgrounds() {
        document.querySelectorAll('.lyric-line, .lyric-line.active').forEach(line => {
            line.style.background = 'transparent';
            line.style.backgroundColor = 'transparent';
            line.style.borderRadius = '0';
            line.style.boxShadow = 'none';
            line.style.padding = '5px 0';
        });
    }

    // Chạy ngay lập tức và sau đó mỗi 100ms
    removeBackgrounds();
    setInterval(removeBackgrounds, 100);

    // Cũng xử lý khi audio cập nhật thời gian
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.addEventListener('timeupdate', removeBackgrounds);
    }
});