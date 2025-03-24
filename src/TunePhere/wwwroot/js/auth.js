document.addEventListener('DOMContentLoaded', function () {
    const sign_in_btn = document.querySelector("#sign-in-btn");
    const sign_up_btn = document.querySelector("#sign-up-btn");
    const container = document.querySelector(".container");
    const sign_in_form = document.querySelector(".sign-in-form");
    const sign_up_form = document.querySelector(".sign-up-form");

    // Chuyển đổi form
    sign_up_btn.addEventListener("click", (e) => {
        e.preventDefault();
        container.classList.add("sign-up-mode");
    });

    sign_in_btn.addEventListener("click", (e) => {
        e.preventDefault();
        container.classList.remove("sign-up-mode");
    });

    // Animation cho các input fields khi focus
    const inputs = document.querySelectorAll(".input-field input");
    inputs.forEach(input => {
        input.addEventListener("focus", () => {
            input.parentNode.classList.add("active");
        });
        input.addEventListener("blur", () => {
            if (input.value === "") {
                input.parentNode.classList.remove("active");
            }
        });
    });

    // Animation cho nốt nhạc bay
    function createMusicNotes() {
        const notes = ['♪', '♫', '♬', '♩'];
        const container = document.querySelector('.music-notes');
        
        setInterval(() => {
            const note = document.createElement('span');
            note.className = 'music-note';
            note.textContent = notes[Math.floor(Math.random() * notes.length)];
            note.style.left = Math.random() * 100 + 'vw';
            note.style.animationDuration = Math.random() * 3 + 2 + 's';
            note.style.opacity = Math.random();
            note.style.fontSize = Math.random() * 20 + 10 + 'px';
            
            container.appendChild(note);
            
            // Xóa nốt nhạc sau khi animation kết thúc
            setTimeout(() => {
                note.remove();
            }, 5000);
        }, 300);
    }

    createMusicNotes();

    // Animation cho equalizer
    function animateEqualizer() {
        const bars = document.querySelectorAll('.bar');
        bars.forEach(bar => {
            const height = Math.random() * 20 + 5;
            bar.style.height = height + 'px';
        });
    }

    setInterval(animateEqualizer, 100);

    // Animation cho particles
    function createParticles() {
        const particleContainer = document.querySelector(".particle");
        for (let i = 0; i < 10; i++) {
            const span = document.createElement("span");
            particleContainer.appendChild(span);
        }
    }

    createParticles();

    // Xử lý validation
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        const inputs = form.querySelectorAll('input[required]');
        const validationSummary = form.querySelector('.validation-summary-errors');
        
        inputs.forEach(input => {
            const errorMessage = input.parentElement.nextElementSibling;
            
            // Kiểm tra khi input mất focus
            input.addEventListener('blur', function() {
                if (!this.value) {
                    input.parentElement.classList.add('error');
                    input.parentElement.classList.remove('success');
                } else {
                    input.parentElement.classList.remove('error');
                    input.parentElement.classList.add('success');
                }
            });

            // Xử lý khi bắt đầu nhập
            input.addEventListener('input', function() {
                input.parentElement.classList.remove('error');
            });
        });

        // Xử lý validation summary
        if (validationSummary) {
            form.addEventListener('submit', function(e) {
                const hasEmptyFields = Array.from(inputs).some(input => !input.value);
                if (hasEmptyFields) {
                    e.preventDefault();
                }
            });
        }
    });
});